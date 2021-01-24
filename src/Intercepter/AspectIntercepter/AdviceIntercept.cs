using System;
using System.Reflection;
using System.Threading.Tasks;
using Autofac.Annotation;
using Autofac.Annotation.Util;
using Autofac.AspectIntercepter.Advice;
using Castle.DynamicProxy;
using Castle.DynamicProxy.NoCoverage;

namespace Autofac.AspectIntercepter
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
        /// 
        /// </summary>
        /// <param name="invocation"></param>
        /// <exception cref="NotImplementedException"></exception>
        protected override void Intercept(IInvocation invocation)
        {
            #region 先从缓存里面拿到这个方法时候打了继承AspectInvokeAttribute的标签

            if (!_cache.CacheList.TryGetValue(invocation.MethodInvocationTarget, out var attribute))
            {
                //动态泛型类
                if (!invocation.MethodInvocationTarget.DeclaringType.GetTypeInfo().IsGenericType ||
                    (!_cache.DynamicCacheList.TryGetValue(invocation.MethodInvocationTarget.GetMethodInfoUniqueName(), out var AttributesDynamic)))
                {
                    invocation.Proceed();
                    return;
                }

                attribute = AttributesDynamic;
            }

            #endregion
            
            var catpture = invocation.CaptureProceedInfo();
            var aspectContext = new AspectContext(_component, invocation);
            aspectContext.Proceed =  () => { 
                catpture.Invoke();
                return new ValueTask();
            };
            
            var runTask = attribute.AspectFunc.Value;
            var task = runTask(aspectContext);
            // If the intercept task has yet to complete, wait for it.
            if (!task.IsCompleted)
            {
                // Need to use Task.Run() to prevent deadlock in .NET Framework ASP.NET requests.
                // GetAwaiter().GetResult() prevents a thrown exception being wrapped in a AggregateException.
                // See https://stackoverflow.com/a/17284612
                Task.Run(() => task).GetAwaiter().GetResult();
            }
            task.RethrowIfFaulted();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="invocation"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        protected override async ValueTask InterceptAsync(IAsyncInvocation invocation)
        {
            #region 先从缓存里面拿到这个方法时候打了继承AspectInvokeAttribute的标签

            if (!_cache.CacheList.TryGetValue(invocation.TargetMethod, out var attribute))
            {
                //动态泛型类
                if (!invocation.TargetMethod.DeclaringType.GetTypeInfo().IsGenericType ||
                    (!_cache.DynamicCacheList.TryGetValue(invocation.TargetMethod.GetMethodInfoUniqueName(), out var AttributesDynamic)))
                {
                     await invocation.ProceedAsync();
                     return;
                }

                attribute = AttributesDynamic;
            }

            #endregion

            var aspectContext = new AspectContext(_component, invocation);
            aspectContext.Proceed = async () =>
            {
                await invocation.ProceedAsync();
            };
            
            var runTask = attribute.AspectFunc.Value;
            await runTask(aspectContext);
        }
    }

}