using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private async Task<Tuple<List<PointcutAttribute>, List<object>>> BeforeInterceptAttribute(IInvocation invocation)
        {
            var Attributes = invocation.MethodInvocationTarget.GetCustomAttributes(true).ToList();

            var aspectContext = new AspectContext(_component, invocation);
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

            return new Tuple<List<PointcutAttribute>, List<object>>(Attributes.OfType<PointcutAttribute>().ToList(), Attributes);
        }

        private async Task AfterInterceptAttribute(List<object> Attributes, IInvocation invocation, Exception exp)
        {
            var aspectContext = new AspectContext(_component, invocation,exp);
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
                if (attribute.Item1 == null || !attribute.Item1.Any())
                {
                    await proceed(invocation);
                }
                else
                {
                    AspectMiddlewareBuilder builder = new AspectMiddlewareBuilder();
                    foreach (var pointAspect in attribute.Item1)
                    {
                        builder.Use(next =>  async  ctx => {  await pointAspect.OnInvocation(ctx, next); });
                    }
                    
                    builder.Use(next=> async ctx => { await proceed(invocation); });

                    var aspectfunc  = builder.Build();
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
                        builder.Use(next =>  async  ctx => {  await pointAspect.OnInvocation(ctx, next); });
                    }
                    
                    
                    builder.Use(next=> async ctx => 
                    {
                        ctx.Result  =  await proceed(invocation);
                    });

                    var aspectfunc  = builder.Build();
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
