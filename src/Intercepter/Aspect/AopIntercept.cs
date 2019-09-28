using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac.Aspect;
using Castle.DynamicProxy;

namespace Autofac.Annotation.Intercepter.Aspect
{
    /// <summary>
    /// AOP拦截器
    /// </summary>
    [Component(typeof(AopIntercept))]
    public class AopIntercept : AsyncInterceptor
    {
        private readonly IComponentContext _component;
        /// <summary>
        /// 构造方法
        /// </summary>
        public AopIntercept(IComponentContext context)
        {
            _component = context;
        }
        /// <summary>
        /// 拦截器
        /// </summary>
        /// <param name="invocation"></param>
        private async Task<List<object>> BeforeInterceptAttribute(IInvocation invocation)
        {
            var Attributes = invocation.MethodInvocationTarget.GetCustomAttributes(true).ToList();

            foreach (var attribute in Attributes)
            {
                if (attribute is AspectAroundAttribute aspectAroundAttribute)
                {
                    await aspectAroundAttribute.Before(_component,invocation);
                }
                else  if (attribute is AspectBeforeAttribute aspectBeforeAttribute)
                {
                    await aspectBeforeAttribute.Before(_component, invocation);
                }
            }

            return Attributes;
        }

        private async Task AfterInterceptAttribute(List<object> Attributes, IInvocation invocation, Exception exp)
        {
            foreach (var attribute in Attributes)
            {
                if (attribute is AspectAroundAttribute aspectAroundAttribute)
                {
                    await aspectAroundAttribute.After(_component, invocation, exp);
                }
                else if (attribute is AspectAfterAttribute aspectAfterAttribute)
                {
                    await aspectAfterAttribute.After(_component, invocation, exp);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="proceed"></param>
        /// <returns></returns>
        protected override async Task InterceptAsync(IInvocation invocation, Func<IInvocation, Task> proceed)
        {
            var attribute = await BeforeInterceptAttribute(invocation);
            try
            {
                await proceed(invocation);
                await AfterInterceptAttribute(attribute, invocation, null);
            }
            catch (Exception e)
            {
                await AfterInterceptAttribute(attribute, invocation, e);
                throw;
            }
        }

        /// <summary>
        /// 
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
                var r = await proceed(invocation);
                await AfterInterceptAttribute(attribute, invocation, null);
                return r;
            }
            catch (Exception e)
            {
                await AfterInterceptAttribute(attribute, invocation, e);
                throw;
            }
        }
    }
}
