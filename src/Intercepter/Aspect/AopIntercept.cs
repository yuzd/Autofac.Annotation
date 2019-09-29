using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac.Annotation;
using Autofac.Aspect;
using Castle.DynamicProxy;

namespace Autofac.Aspect
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
        private async Task<Tuple<PointcutAttribute, List<object>>> BeforeInterceptAttribute(IInvocation invocation)
        {
            var Attributes = invocation.MethodInvocationTarget.GetCustomAttributes(true).ToList();

            foreach (var attribute in Attributes)
            {
                switch (attribute)
                {
                    case AspectAroundAttribute aspectAroundAttribute:
                        await aspectAroundAttribute.Before(_component, invocation);
                        break;
                    case AspectBeforeAttribute aspectBeforeAttribute:
                        await aspectBeforeAttribute.Before(_component, invocation);
                        break;
                }
            }

            return new Tuple<PointcutAttribute, List<object>>(Attributes.FirstOrDefault(r => r is PointcutAttribute) as PointcutAttribute, Attributes);
        }

        private async Task AfterInterceptAttribute(List<object> Attributes, IInvocation invocation, Exception exp)
        {
            foreach (var attribute in Attributes)
            {
                switch (attribute)
                {
                    case AspectAroundAttribute aspectAroundAttribute:
                        await aspectAroundAttribute.After(_component, invocation, exp);
                        break;
                    case AspectAfterAttribute aspectAfterAttribute:
                        await aspectAfterAttribute.After(_component, invocation, exp);
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
                if (attribute.Item1 == null)
                {
                    await proceed(invocation);
                }
                else
                {
                    await attribute.Item1.InterceptAsync(_component, invocation, proceed);
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
                TResult r;
                if (attribute.Item1 == null)
                {
                    r = await proceed(invocation);
                }
                else
                {
                    r = await attribute.Item1.InterceptAsync<TResult>(_component, invocation, proceed);
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
