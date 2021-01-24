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
using Castle.DynamicProxy;
using Autofac.Annotation.Util;
using Autofac.AspectIntercepter.Pointcut;
using Castle.DynamicProxy.NoCoverage;

namespace Autofac.AspectIntercepter
{
  

    /// <summary>
    /// AOP Pointcut拦截器
    /// </summary>
    [Component(typeof(PointcutIntercept),NotUseProxy = true)]
    public class PointcutIntercept : AsyncInterceptor
    {
        private readonly IComponentContext _component;
        private readonly PointcutMethodInvokeCache _configuration;


        /// <summary>
        /// 构造方法
        /// </summary>
        public PointcutIntercept(IComponentContext context, PointcutMethodInvokeCache configurationList)
        {
            _component = context;
            _configuration = configurationList;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="invocation"></param>
        protected override void Intercept(IInvocation invocation)
        {
            if (!_configuration.CacheList.TryGetValue(invocation.MethodInvocationTarget, out var pointCut))
            {
                if (!invocation.MethodInvocationTarget.DeclaringType.GetTypeInfo().IsGenericType ||
                    !_configuration.DynamicCacheList.TryGetValue(invocation.MethodInvocationTarget.GetMethodInfoUniqueName(),
                        out var pointCutDynamic))
                {
                    //该方法不需要拦截
                    invocation.Proceed();
                    return;
                }

                pointCut = pointCutDynamic;
            }

            var catpture = invocation.CaptureProceedInfo();
            var aspectContext = new AspectContext(_component, invocation);
            aspectContext.Proceed =  () => { 
                catpture.Invoke();
                return new ValueTask();
            };
            
            var runTask = pointCut.AspectFunc.Value;
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
        protected override async ValueTask InterceptAsync(IAsyncInvocation invocation)
        {
            if (!_configuration.CacheList.TryGetValue(invocation.TargetMethod, out var pointCut))
            {
                if (!invocation.TargetMethod.DeclaringType.GetTypeInfo().IsGenericType ||
                    !_configuration.DynamicCacheList.TryGetValue(invocation.TargetMethod.GetMethodInfoUniqueName(),
                        out var pointCutDynamic))
                {
                    //该方法不需要拦截
                    await invocation.ProceedAsync();
                    return;
                }

                pointCut = pointCutDynamic;
            }

             
            var aspectContext = new AspectContext(_component, invocation);
            aspectContext.Proceed = async () =>
            {
                await invocation.ProceedAsync();
            };
            var runTask = pointCut.AspectFunc.Value;
            await runTask(aspectContext);
        }
    }
}