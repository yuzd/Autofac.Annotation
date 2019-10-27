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

namespace Autofac.Aspect
{
    /// <summary>
    /// AOP拦截器方法Attribute的缓存
    /// </summary>
    [Component(AutofacScope = AutofacScope.SingleInstance,AutoActivate = true)]
    public class AopMethodInvokeCache
    {

        /// <summary>
        /// 构造方法
        /// </summary>
        public AopMethodInvokeCache(IComponentContext context)
        {
            CacheList = new ConcurrentDictionary<MethodInfo, List<AspectInvokeAttribute>>();
            var componentModelCacheSingleton = context.Resolve<ComponentModelCacheSingleton>();
            var aspectClassList = componentModelCacheSingleton.ComponentModelCache.Values
                .Where(r => r.AspectAttribute != null).ToList();
            foreach (var aspectClass in aspectClassList)
            {
                var allAttributes = aspectClass.CurrentType.GetReflector()
                    .GetCustomAttributes(typeof(AspectInvokeAttribute)).OfType<AspectInvokeAttribute>()
                    .Select(r => new {IsClass = true, Attribute = r, Index = r.OrderIndex});

                var myArrayMethodInfo = aspectClass.CurrentType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName);

                foreach (var method in myArrayMethodInfo)
                {
                    allAttributes = allAttributes.Concat(method.GetReflector()
                            .GetCustomAttributes(typeof(AspectInvokeAttribute)).OfType<AspectInvokeAttribute>()
                            .Select(r => new { IsClass = false, Attribute = r, Index = r.OrderIndex }));

                    var attributes = allAttributes
                        .OrderBy(r => r.IsClass).ThenByDescending(r => r.Index)
                        .GroupBy(r => r.Attribute.GetType().FullName)
                        .Select(r => r.First().Attribute).ToList();

                    CacheList.TryAdd(method, attributes);
                }
            }
        }
        /// <summary>
        /// 缓存
        /// </summary>
        public ConcurrentDictionary<MethodInfo,List<AspectInvokeAttribute>> CacheList { get; set; }
    }


    /// <summary>
    /// AOP拦截器
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

        /// <summary>
        /// 拦截器
        /// </summary>
        /// <param name="invocation"></param>
        private async Task<Tuple<List<PointcutAttribute>, List<AspectInvokeAttribute>,Exception>> BeforeInterceptAttribute(IInvocation invocation)
        {
            if(!_cache.CacheList.TryGetValue(invocation.MethodInvocationTarget,out var Attributes))
            {
                var allAttributes = invocation.TargetType.GetReflector()
                    .GetCustomAttributes(typeof(AspectInvokeAttribute)).OfType<AspectInvokeAttribute>()
                    .Select(r => new { IsClass = true, Attribute = r, Index = r.OrderIndex })
                    .Concat(invocation.MethodInvocationTarget.GetReflector()
                        .GetCustomAttributes(typeof(AspectInvokeAttribute)).OfType<AspectInvokeAttribute>()
                        .Select(r => new { IsClass = false, Attribute = r, Index = r.OrderIndex }));

                Attributes = allAttributes
                    .OrderBy(r => r.IsClass).ThenByDescending(r => r.Index)
                    .GroupBy(r => r.Attribute.GetType().FullName)
                    .Select(r => r.First().Attribute).ToList();

                _cache.CacheList.TryAdd(invocation.MethodInvocationTarget, Attributes);
            }
           
            var aspectContext = new AspectContext(_component, invocation);
            Exception ex = null;
            try
            {
                foreach (var attribute in Attributes)
                {
                    switch (attribute)
                    {
                        case AspectAroundAttribute aspectAroundAttribute:
                            await aspectAroundAttribute.Before(aspectContext);
                            break;
                        case AspectBeforeAttribute aspectBeforeAttribute:
                            await aspectBeforeAttribute.Before(aspectContext);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                ex = e;
            }
            return new Tuple<List<PointcutAttribute>, List<AspectInvokeAttribute>,Exception>(Attributes.OfType<PointcutAttribute>().ToList(), Attributes,ex);
        }

        private async Task AfterInterceptAttribute(List<AspectInvokeAttribute> Attributes, IInvocation invocation, Exception exp)
        {
            var aspectContext = new AspectContext(_component, invocation, exp);
            foreach (var attribute in Attributes)
            {
                switch (attribute)
                {
                    case AspectAroundAttribute aspectAroundAttribute:
                        await aspectAroundAttribute.After(aspectContext);
                        break;
                    case AspectAfterAttribute aspectAfterAttribute:
                        await aspectAfterAttribute.After(aspectContext);
                        break;
                }
            }
        }

        /// <summary>
        /// 无返回值拦截器
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="proceed"></param>
        /// <returns></returns>
        protected override async Task InterceptAsync(IInvocation invocation, Func<IInvocation, Task> proceed)
        {
            var attribute = await BeforeInterceptAttribute(invocation);
            try
            {
                if (attribute.Item3 != null)
                {
                    await AfterInterceptAttribute(attribute.Item2, invocation, attribute.Item3);
                    throw attribute.Item3;
                }
                
                if (attribute.Item1 == null || !attribute.Item1.Any())
                {
                    await proceed(invocation);
                }
                else
                {
                    AspectMiddlewareBuilder builder = new AspectMiddlewareBuilder();
                    foreach (var pointAspect in attribute.Item1)
                    {
                        builder.Use(next => async ctx => { await pointAspect.OnInvocation(ctx, next); });
                    }

                    builder.Use(next => async ctx => { await proceed(invocation); });

                    var aspectfunc = builder.Build();
                    await aspectfunc(new AspectContext(_component, invocation));
                }
                await AfterInterceptAttribute(attribute.Item2, invocation, null);
            }
            catch (Exception e)
            {
                await AfterInterceptAttribute(attribute.Item2, invocation, e);
                throw;
            }
        }

        /// <summary>
        /// 有返回值拦截器
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="invocation"></param>
        /// <param name="proceed"></param>
        /// <returns></returns>
        protected override async Task<TResult> InterceptAsync<TResult>(IInvocation invocation, Func<IInvocation, Task<TResult>> proceed)
        {
            var attribute = await BeforeInterceptAttribute(invocation);
            try
            {
                if (attribute.Item3 != null)
                {
                    await AfterInterceptAttribute(attribute.Item2, invocation, attribute.Item3);
                    throw attribute.Item3;
                }
                
                TResult r;
                if (attribute.Item1 == null || !attribute.Item1.Any())
                {
                    r = await proceed(invocation);
                }
                else
                {
                    AspectMiddlewareBuilder builder = new AspectMiddlewareBuilder();
                    foreach (var pointAspect in attribute.Item1)
                    {
                        builder.Use(next => async ctx => { await pointAspect.OnInvocation(ctx, next); });
                    }


                    builder.Use(next => async ctx =>
                     {
                         ctx.Result = await proceed(invocation);
                     });

                    var aspectfunc = builder.Build();
                    var aspectContext = new AspectContext(_component, invocation);
                    await aspectfunc(aspectContext);
                    r = (TResult)aspectContext.Result;
                }

                await AfterInterceptAttribute(attribute.Item2, invocation, null);
                return r;
            }
            catch (Exception e)
            {
                await AfterInterceptAttribute(attribute.Item2, invocation, e);
                throw;
            }
        }
    }
}
