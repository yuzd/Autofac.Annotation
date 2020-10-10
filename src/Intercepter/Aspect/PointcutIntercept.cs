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
using Autofac.Aspect.Advice;
using Autofac.Aspect.Pointcut;

namespace Autofac.Aspect
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
        /// 一个目标方法只会适 Before After Arround 其中的一个切面
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="proceedInfo"></param>
        /// <param name="proceed"></param>
        /// <returns></returns>
        protected override async Task InterceptAsync(IInvocation invocation, IInvocationProceedInfo proceedInfo,
            Func<IInvocation, IInvocationProceedInfo, Task> proceed)
        {
            if (!_configuration.CacheList.TryGetValue(invocation.MethodInvocationTarget, out var pointCut))
            {
                if (!invocation.MethodInvocationTarget.DeclaringType.GetTypeInfo().IsGenericType ||
                    !_configuration.DynamicCacheList.TryGetValue(invocation.MethodInvocationTarget.GetMethodInfoUniqueName(),
                        out var pointCutDynamic))
                {
                    //该方法不需要拦截
                    await proceed(invocation, proceedInfo);
                    return;
                }

                pointCut = pointCutDynamic;
            }
            
            var aspectContext = new AspectContext(_component, invocation,proceedInfo);
            aspectContext.Proceed = async () => { await proceed(invocation, proceedInfo); };
            var runTask = pointCut.AspectFunc.Value;
            await runTask(aspectContext);
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
            if (!_configuration.CacheList.TryGetValue(invocation.MethodInvocationTarget, out var pointCut))
            {
                if (!invocation.MethodInvocationTarget.DeclaringType.GetTypeInfo().IsGenericType ||
                    !_configuration.DynamicCacheList.TryGetValue(invocation.MethodInvocationTarget.GetMethodInfoUniqueName(),
                        out var pointCutDynamic))
                {
                    //该方法不需要拦截
                    return await proceed(invocation, proceedInfo);
                }

                pointCut = pointCutDynamic;
            }

             
            var aspectContext = new AspectContext(_component, invocation,proceedInfo);
            aspectContext.Proceed = async () =>
            {
                aspectContext.Result = await proceed(invocation, proceedInfo);
                aspectContext.InvocationContext.ReturnValue = aspectContext.Result; //原方法的执行返回值
            };
            var runTask = pointCut.AspectFunc.Value;
            await runTask(aspectContext);
            var r = (TResult) aspectContext.Result;
            return r;
        }
    }
}