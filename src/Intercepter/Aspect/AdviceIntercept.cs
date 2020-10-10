using System;
using System.Reflection;
using System.Threading.Tasks;
using Autofac.Annotation;
using Autofac.Annotation.Util;
using Autofac.Aspect.Advice;
using Castle.DynamicProxy;

namespace Autofac.Aspect
{
     /// <summary>
    /// AOP拦截器 配合打了 Aspect标签的class 和 里面打了 继承AspectInvokeAttribute 标签的 方法
    /// </summary>
    [Component(typeof(AdviceIntercept),NotUseProxy = true)]
    public class AdviceIntercept : AsyncInterceptor
    {
        private readonly IComponentContext _component;
        private readonly ApsectAdviceMethodInvokeCache _cache;


        /// <summary>
        /// 构造方法
        /// </summary>
        public AdviceIntercept(IComponentContext context, ApsectAdviceMethodInvokeCache cache)
        {
            _component = context;
            _cache = cache;
        }
        
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
            #region 先从缓存里面拿到这个方法时候打了继承AspectInvokeAttribute的标签

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

            #endregion

            var aspectContext = new AspectContext(_component, invocation,proceedInfo);
            aspectContext.Proceed = async () => { await proceed(invocation, proceedInfo); };
            var runTask = attribute.AspectFunc.Value;
            await runTask(aspectContext);
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

            #region 先从缓存里面拿到这个方法时候打了继承AspectInvokeAttribute的标签

            if (!_cache.CacheList.TryGetValue(invocation.MethodInvocationTarget, out var attribute))
            {
                //动态泛型类
                if (!invocation.MethodInvocationTarget.DeclaringType.GetTypeInfo().IsGenericType ||
                    (!_cache.DynamicCacheList.TryGetValue(invocation.MethodInvocationTarget.GetMethodInfoUniqueName(), out var AttributesDynamic)))
                {
                    return await proceed(invocation, proceedInfo);
                }

                attribute = AttributesDynamic;
            }

            #endregion

            var aspectContext = new AspectContext(_component, invocation,proceedInfo);
            aspectContext.Proceed = async () =>
            {
                aspectContext.Result = await proceed(invocation, proceedInfo);
                aspectContext.InvocationContext.ReturnValue = aspectContext.Result; //原方法的执行返回值
            };
            var runTask = attribute.AspectFunc.Value;
            await runTask(aspectContext);
            var r = (TResult) aspectContext.Result;
            return r;
        }

    }

}