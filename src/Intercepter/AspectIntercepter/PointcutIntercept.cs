using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Annotation;
using Autofac.Annotation.Util;
using Autofac.AspectIntercepter.Pointcut;
using Castle.DynamicProxy;
using Castle.DynamicProxy.NoCoverage;

namespace Autofac.AspectIntercepter
{
    /// <summary>
    ///     AOP Pointcut拦截器
    /// </summary>
    [Component(typeof(PointcutIntercept), NotUseProxy = true)]
    public class PointcutIntercept : AsyncInterceptor
    {
        private readonly AdviceIntercept _adviceIntercept;
        private readonly IComponentContext _component;
        private readonly PointcutMethodInvokeCache _configuration;

        /// <summary>
        ///     构造方法
        /// </summary>
        public PointcutIntercept(IComponentContext context, PointcutMethodInvokeCache configurationList, AdviceIntercept adviceIntercept)
        {
            _component = context;
            _configuration = configurationList;
            _adviceIntercept = adviceIntercept;
        }


        /// <summary>
        /// </summary>
        /// <param name="invocation"></param>
        protected override void Intercept(IInvocation invocation)
        {
            if (!_configuration.CacheList.TryGetValue(new ObjectKey(invocation.TargetType, invocation.Method), out var pointCut))
            {
                if (!_configuration.CacheList.TryGetValue(new ObjectKey(invocation.TargetType, invocation.MethodInvocationTarget), out var pointCutInherited))
                {
                    if (!invocation.MethodInvocationTarget.DeclaringType.GetTypeInfo().IsGenericType ||
                        !_configuration.DynamicCacheList.TryGetValue(invocation.MethodInvocationTarget.GetMethodInfoUniqueName(),
                            out var pointCutDynamic))
                    {
                        //该方法不需要拦截
                        _adviceIntercept.InterceptInternal(invocation);
                        // invocation.Proceed();
                        return;
                    }

                    pointCutInherited = pointCutDynamic;
                }

                pointCut = pointCutInherited;
            }

            var catpture = invocation.CaptureProceedInfo();
            var aspectContext = new AspectContext(_component, invocation)
            {
                Proceed = () =>
                {
                    catpture.Invoke();
                    return new ValueTask();
                }
            };

            var runTask = pointCut.AspectFunc.Value;
            var task = runTask(aspectContext);
            // If the intercept task has yet to complete, wait for it.
            if (!task.IsCompleted)
            {
                if (SynchronizationContext.Current == null)
                {
                    // 针对console 或者 aspnetcore类型的应用
                    task.GetAwaiter().GetResult();
                }
                else
                {
                    // 针对winform wpf aspnetframework类型的应用
                    Task.Run(() => task.ConfigureAwait(false)).ConfigureAwait(false).GetAwaiter().GetResult();
                }
            }

            task.RethrowIfFaulted();
        }


        /// <summary>
        /// </summary>
        /// <param name="invocation"></param>
        /// <returns></returns>
        protected override async ValueTask InterceptAsync(IAsyncInvocation invocation)
        {
            if (!_configuration.CacheList.TryGetValue(new ObjectKey(invocation.TargetType, invocation.Method), out var pointCut))
            {
                if (!_configuration.CacheList.TryGetValue(new ObjectKey(invocation.TargetType, invocation.TargetMethod), out var pointCutInherited))
                {
                    if (!invocation.TargetMethod.DeclaringType.GetTypeInfo().IsGenericType ||
                        !_configuration.DynamicCacheList.TryGetValue(invocation.TargetMethod.GetMethodInfoUniqueName(),
                            out var pointCutDynamic))
                    {
                        //该方法不需要拦截
                        await _adviceIntercept.InterceptInternalAsync(invocation);
                        // await invocation.ProceedAsync();
                        return;
                    }

                    pointCutInherited = pointCutDynamic;
                }

                pointCut = pointCutInherited;
            }


            var aspectContext = new AspectContext(_component, invocation)
            {
                Proceed = async () => { await invocation.ProceedAsync(); }
            };
            var runTask = pointCut.AspectFunc.Value;
            await runTask(aspectContext);
        }
    }
}