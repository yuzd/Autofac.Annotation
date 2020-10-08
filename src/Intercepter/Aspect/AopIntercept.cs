using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AspectCore.Extensions.Reflection;
using Autofac.Annotation;
using Autofac.Aspect;
using Castle.DynamicProxy;
using Autofac.Annotation.Util;

namespace Autofac.Aspect
{
    /// <summary>
    /// AOP拦截器方法Attribute的缓存
    /// 在DI容器build的时候会触发这个实例new
    /// 然后解析所有打了Aspect标签的class进行解析打了有继承AspectInvokeAttribute的所有方法并且缓存起来
    /// </summary>
    [Component(AutofacScope = AutofacScope.SingleInstance, AutoActivate = true)]
    public class AopMethodInvokeCache
    {
        /// <summary>
        /// 构造方法
        /// </summary>
        public AopMethodInvokeCache(IComponentContext context)
        {
            CacheList = new ConcurrentDictionary<MethodInfo, AspectAttributeInfo>();
            DynamicCacheList = new ConcurrentDictionary<string, AspectAttributeInfo>();
            var componentModelCacheSingleton = context.Resolve<ComponentModelCacheSingleton>();
            var aspectClassList = componentModelCacheSingleton.ComponentModelCache.Values
                .Where(r => r.AspectAttribute != null).ToList();
            foreach (var aspectClass in aspectClassList)
            {
                var allAttributesinClass = aspectClass.CurrentType.GetReflector()
                    .GetCustomAttributes(typeof(AspectInvokeAttribute)).OfType<AspectInvokeAttribute>()
                    .Select(r => new {IsClass = true, Attribute = r, Index = r.OrderIndex}).ToList();

                var myArrayMethodInfo = aspectClass.CurrentType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(m => !m.IsSpecialName);

                foreach (var method in myArrayMethodInfo)
                {
                    var allAttributes = allAttributesinClass.Concat(method.GetReflector()
                        .GetCustomAttributes(typeof(AspectInvokeAttribute)).OfType<AspectInvokeAttribute>()
                        .Select(r => new {IsClass = false, Attribute = r, Index = r.OrderIndex}));

                    //如果class上也打了 method上也打了 优先用method上的
                    var attributes = allAttributes
                        .OrderBy(r => r.IsClass).ThenByDescending(r => r.Index)
                        .GroupBy(r => r.Attribute.GetType().FullName)
                        .Select(r => r.First().Attribute).ToList();

                    if (!attributes.Any()) continue;
                    var aspectAttributeInfo = new AspectAttributeInfo();
                    foreach (var attribute in attributes)
                    {
                        switch (attribute)
                        {
                            case AspectBeforeAttribute aspectBeforeAttribute:
                                aspectAttributeInfo.AspectBeforeAttributeList.Add(aspectBeforeAttribute);
                                break;
                            case AspectAfterAttribute aspectAfterAttribute:
                                aspectAttributeInfo.AspectAfterAttributeList.Add(aspectAfterAttribute);
                                break;
                            case AspectAfterReturningAttribute aspectAfterReturning:
                                aspectAttributeInfo.AspectAfterReturningAttributeList.Add(aspectAfterReturning);
                                break;
                            case AspectAfterThrowingAttribute aspectAfterThrowing:
                                aspectAttributeInfo.AspectAfterThrowingAttributeList.Add(aspectAfterThrowing);
                                break;
                            case AspectArroundAttribute aspectPointAttribute:
                                aspectAttributeInfo.AspectArroundAttributeList.Add(aspectPointAttribute);
                                break;
                        }
                    }

                    if (aspectClass.isDynamicGeneric)
                    {
                        DynamicCacheList.TryAdd(method.GetMethodInfoUniqueName(), aspectAttributeInfo);
                        continue;
                    }

                    CacheList.TryAdd(method, aspectAttributeInfo);
                }
            }
        }

        /// <summary>
        /// 缓存
        /// </summary>
        internal ConcurrentDictionary<MethodInfo, AspectAttributeInfo> CacheList { get; set; }

        /// <summary>
        /// 由于动态泛型的method是跟着泛型T变化的  所以需要单独缓存
        /// </summary>
        internal ConcurrentDictionary<string, AspectAttributeInfo> DynamicCacheList { get; set; }
    }


    /// <summary>
    /// AOP拦截器 配合打了 Aspect标签的class 和 里面打了 继承AspectInvokeAttribute 标签的 方法
    /// </summary>
    [Component(typeof(AopIntercept))]
    public class AopIntercept : AsyncInterceptor
    {
        private readonly IComponentContext _component;
        private readonly AopMethodInvokeCache _cache;


        /// <summary>
        /// 构造方法
        /// </summary>
        public AopIntercept(IComponentContext context, AopMethodInvokeCache cache)
        {
            _component = context;
            _cache = cache;
        }

        // try{
        //     try{
        //         //@Before
        //         method.invoke(..);
        //     }finally{
        //         //@After
        //     }
        //     //@AfterReturning
        // }catch(){
        //     //@AfterThrowing
        // }


        /// <summary>
        /// 无返回值拦截器
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="proceedInfo"></param>
        /// <param name="proceed"></param>
        /// <returns></returns>
        protected override async Task InterceptAsync(IInvocation invocation, IInvocationProceedInfo proceedInfo,
            Func<IInvocation, IInvocationProceedInfo, Task> proceed)
        {
            //先从缓存里面拿到这个方法时候打了继承AspectInvokeAttribute的标签
            if (!_cache.CacheList.TryGetValue(invocation.MethodInvocationTarget, out var attribute))
            {
                //动态泛型类
                if (!invocation.MethodInvocationTarget.DeclaringType.GetTypeInfo().IsGenericType ||
                    (!_cache.DynamicCacheList.TryGetValue(invocation.MethodInvocationTarget.GetMethodInfoUniqueName(), out var AttributesDynamic)))
                {
                    await proceed(invocation, proceedInfo);
                    return;
                }

                attribute = AttributesDynamic;
            }

            var aspectContext = new AspectContext(_component, invocation);
            try
            {
                try
                {
                    #region Before

                    foreach (var beforeAttribute in attribute.AspectBeforeAttributeList)
                    {
                        await beforeAttribute.Before(aspectContext);
                    }

                    #endregion

                    #region method.invoke(..)

                    if (attribute.AspectArroundAttributeList.Any())
                    {
                        AspectMiddlewareBuilder builder = new AspectMiddlewareBuilder();
                        foreach (var pointAspect in attribute.AspectArroundAttributeList)
                        {
                            builder.Use(next => async ctx => { await pointAspect.OnInvocation(ctx, next); });
                        }

                        builder.Use(next => async ctx => { await proceed(invocation, proceedInfo); });

                        var aspectfunc = builder.Build();
                        await aspectfunc(new AspectContext(_component, invocation));
                    }
                    else
                    {
                        await proceed(invocation, proceedInfo);
                    }

                    #endregion
                }
                finally
                {
                    #region After

                    foreach (var afterAttribute in attribute.AspectAfterAttributeList)
                    {
                        await afterAttribute.After(aspectContext);
                    }

                    #endregion
                }

                #region AfterReurning

                foreach (var afterReturning in attribute.AspectAfterReturningAttributeList)
                {
                    await afterReturning.AfterReturning(aspectContext, null);
                }

                #endregion
            }
            catch (Exception e)
            {
                #region AfterThrow

                foreach (var aspectAfterThrowing in attribute.AspectAfterThrowingAttributeList)
                {
                    await aspectAfterThrowing.AfterThrowing(aspectContext, e);
                }

                #endregion

                throw;
            }
        }

        /// <summary>
        /// 有返回值拦截器
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="invocation"></param>
        /// <param name="proceedInfo"></param>
        /// <param name="proceed"></param>
        /// <returns></returns>
        protected override async Task<TResult> InterceptAsync<TResult>(IInvocation invocation, IInvocationProceedInfo proceedInfo,
            Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed)
        {
            TResult r;

            //先从缓存里面拿到这个方法时候打了继承AspectInvokeAttribute的标签
            if (!_cache.CacheList.TryGetValue(invocation.MethodInvocationTarget, out var attribute))
            {
                //动态泛型类
                if (!invocation.MethodInvocationTarget.DeclaringType.GetTypeInfo().IsGenericType ||
                    (!_cache.DynamicCacheList.TryGetValue(invocation.MethodInvocationTarget.GetMethodInfoUniqueName(), out var AttributesDynamic)))
                {
                    r = await proceed(invocation, proceedInfo);
                    return r;
                }

                attribute = AttributesDynamic;
            }

            var aspectContext = new AspectContext(_component, invocation);
            try
            {
                try
                {
                    #region Before

                    foreach (var beforeAttribute in attribute.AspectBeforeAttributeList)
                    {
                        await beforeAttribute.Before(aspectContext);
                    }

                    #endregion

                    #region method.invoke(..)

                    if (attribute.AspectArroundAttributeList.Any())
                    {
                        AspectMiddlewareBuilder builder = new AspectMiddlewareBuilder();
                        foreach (var pointAspect in attribute.AspectArroundAttributeList)
                        {
                            builder.Use(next => async ctx =>
                            {
                                await pointAspect.OnInvocation(ctx, next);
                                //如果有拦截器设置 ReturnValue 那么就直接拿这个作为整个拦截器的方法返回值
                                if (ctx.InvocationContext.ReturnValue != null)
                                {
                                    ctx.Result = ctx.InvocationContext.ReturnValue;
                                }
                            });
                        }

                        builder.Use(next => async ctx =>
                        {
                            ctx.Result = await proceed(invocation, proceedInfo);
                            invocation.ReturnValue = ctx.Result; //原方法的执行返回值
                        });

                        var aspectfunc = builder.Build();
                        await aspectfunc(aspectContext);
                        r = (TResult) aspectContext.Result;
                    }
                    else
                    {
                        r = await proceed(invocation, proceedInfo);
                    }

                    #endregion
                }
                finally
                {
                    #region After

                    foreach (var afterAttribute in attribute.AspectAfterAttributeList)
                    {
                        await afterAttribute.After(aspectContext);
                    }

                    #endregion
                }

                #region AfterReurning

                foreach (var afterReturning in attribute.AspectAfterReturningAttributeList)
                {
                    await afterReturning.AfterReturning(aspectContext, r);
                }

                #endregion

                return r;
            }
            catch (Exception e)
            {
                #region AfterThrow

                foreach (var aspectAfterThrowing in attribute.AspectAfterThrowingAttributeList)
                {
                    await aspectAfterThrowing.AfterThrowing(aspectContext, e);
                }

                #endregion

                throw;
            }
        }
    }


    /// <summary>
    /// AOP Pointcut拦截器
    /// </summary>
    [Component(typeof(AspectJIntercept))]
    public class AspectJIntercept : AsyncInterceptor
    {
        private readonly IComponentContext _component;
        private readonly PointCutConfigurationList _configuration;


        /// <summary>
        /// 构造方法
        /// </summary>
        public AspectJIntercept(IComponentContext context, PointCutConfigurationList configurationList)
        {
            _component = context;
            _configuration = configurationList;
        }

        /// <summary>
        /// 一个目标方法只会适 Before After Arround 其中的一个切面
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="proceedInfo"></param>
        /// <param name="proceed"></param>
        /// <returns></returns>
        protected override async Task InterceptAsync(IInvocation invocation, IInvocationProceedInfo proceedInfo,
            Func<IInvocation, IInvocationProceedInfo, Task> proceed)
        {
            if (!_configuration.PointcutTargetInfoList.TryGetValue(invocation.MethodInvocationTarget, out var pointCut))
            {
                if (!invocation.MethodInvocationTarget.DeclaringType.GetTypeInfo().IsGenericType ||
                    !_configuration.DynamicPointcutTargetInfoList.TryGetValue(invocation.MethodInvocationTarget.GetMethodInfoUniqueName(),
                        out var pointCutDynamic))
                {
                    //该方法不需要拦截
                    await proceed(invocation, proceedInfo);
                    return;
                }

                pointCut = pointCutDynamic;
            }

            //pointcut定义所在对象
            var instance = _component.Resolve(pointCut.PointClass);

            PointcutContext aspectContext = new PointcutContext
            {
                ComponentContext = _component,
                InvocationMethod = invocation.MethodInvocationTarget,
            };

            if (pointCut.AroundMethod != null)
            {
                aspectContext.Proceed = async () => { await proceed(invocation, proceedInfo); };

                var rt = AutoConfigurationHelper.InvokeInstanceMethod(instance, pointCut.AroundMethod, _component, aspectContext);
                if (typeof(Task).IsAssignableFrom(pointCut.AroundMethod.ReturnType))
                {
                    await ((Task) rt).ConfigureAwait(false);
                }

                return;
            }

            try
            {
                if (pointCut.BeforeMethod != null)
                {
                    var rtBefore = AutoConfigurationHelper.InvokeInstanceMethod(instance, pointCut.BeforeMethod, _component, aspectContext);
                    if (typeof(Task).IsAssignableFrom(pointCut.BeforeMethod.ReturnType))
                    {
                        await ((Task) rtBefore).ConfigureAwait(false);
                    }
                }

                await proceed(invocation, proceedInfo);
                if (pointCut.AfterMethod != null)
                {
                    var rtAfter = AutoConfigurationHelper.InvokeInstanceMethod(instance, pointCut.AfterMethod, _component, aspectContext);
                    if (typeof(Task).IsAssignableFrom(pointCut.AfterMethod.ReturnType))
                    {
                        await ((Task) rtAfter).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception e)
            {
                aspectContext.Exception = e;
                if (pointCut.AfterMethod != null)
                {
                    var rtAfter = AutoConfigurationHelper.InvokeInstanceMethod(instance, pointCut.AfterMethod, _component, aspectContext);
                    if (typeof(Task).IsAssignableFrom(pointCut.AfterMethod.ReturnType))
                    {
                        await ((Task) rtAfter).ConfigureAwait(false);
                    }
                }

                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="proceedInfo"></param>
        /// <param name="proceed"></param>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        protected override async Task<TResult> InterceptAsync<TResult>(IInvocation invocation, IInvocationProceedInfo proceedInfo,
            Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed)
        {
            if (!_configuration.PointcutTargetInfoList.TryGetValue(invocation.MethodInvocationTarget, out var pointCut))
            {
                if (!invocation.MethodInvocationTarget.DeclaringType.GetTypeInfo().IsGenericType ||
                    !_configuration.DynamicPointcutTargetInfoList.TryGetValue(invocation.MethodInvocationTarget.GetMethodInfoUniqueName(),
                        out var pointCutDynamic))
                {
                    //该方法不需要拦截
                    return await proceed(invocation, proceedInfo);
                }

                pointCut = pointCutDynamic;
            }

            //pointcut定义所在对象
            var instance = _component.Resolve(pointCut.PointClass);

            PointcutContext aspectContext = new PointcutContext
            {
                ComponentContext = _component,
                InvocationMethod = invocation.MethodInvocationTarget,
            };

            if (pointCut.AroundMethod != null)
            {
                aspectContext.Proceed = async () => { invocation.ReturnValue = await proceed(invocation, proceedInfo); };

                var rt = AutoConfigurationHelper.InvokeInstanceMethod(instance, pointCut.AroundMethod, _component, aspectContext);
                if (typeof(Task).IsAssignableFrom(pointCut.AroundMethod.ReturnType))
                {
                    await ((Task) rt).ConfigureAwait(false);
                }

                return (TResult) invocation.ReturnValue;
            }

            try
            {
                if (pointCut.BeforeMethod != null)
                {
                    var rtBefore = AutoConfigurationHelper.InvokeInstanceMethod(instance, pointCut.BeforeMethod, _component, aspectContext);
                    if (typeof(Task).IsAssignableFrom(pointCut.BeforeMethod.ReturnType))
                    {
                        await ((Task) rtBefore).ConfigureAwait(false);
                    }
                }

                var rt = await proceed(invocation, proceedInfo);


                if (pointCut.AfterMethod != null)
                {
                    var rtAfter = AutoConfigurationHelper.InvokeInstanceMethod(instance, pointCut.AfterMethod, _component, aspectContext);
                    if (typeof(Task).IsAssignableFrom(pointCut.AfterMethod.ReturnType))
                    {
                        await ((Task) rtAfter).ConfigureAwait(false);
                    }
                }

                return rt;
            }
            catch (Exception e)
            {
                aspectContext.Exception = e;
                if (pointCut.AfterMethod != null)
                {
                    var rtAfter = AutoConfigurationHelper.InvokeInstanceMethod(instance, pointCut.AfterMethod, _component, aspectContext);
                    if (typeof(Task).IsAssignableFrom(pointCut.AfterMethod.ReturnType))
                    {
                        await ((Task) rtAfter).ConfigureAwait(false);
                    }
                }

                throw;
            }
        }
    }
}