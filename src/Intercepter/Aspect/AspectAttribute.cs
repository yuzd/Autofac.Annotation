using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac.Annotation;
using Castle.DynamicProxy;

namespace Autofac.Aspect
{

    /// <summary>
    /// AOP拦截器
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class AspectMethodAttribute : Attribute
    {


    }

    /// <summary>
    /// 开启AOP拦截器
    /// 配合 AspectAroundAttribute  AspectBeforeAttribute AspectAfterAttribute 使用
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AspectAttribute : Attribute
    {
        /// <summary>
        /// ctor
        /// </summary>
        public AspectAttribute()
        {
            
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="type"></param>
        public AspectAttribute(InterceptorType type)
        {
            AspectType = type;
        }

        /// <summary>
        /// 拦截器类型 默认是当前class的拦截器
        /// </summary>
        public InterceptorType AspectType { get; set; } = InterceptorType.Class;
    }
    /// <summary>
    /// AOP环绕拦截器
    /// </summary>
    public abstract class AspectAroundAttribute : AspectMethodAttribute
    {


        /// <summary>
        /// 后置执行
        /// </summary>
        /// <param name="context"></param>
        /// <param name="invocation"></param>
        /// <param name="exp"></param>
        public abstract Task After(IComponentContext context, IInvocation invocation, Exception exp);


        /// <summary>
        /// 前置执行
        /// </summary>
        /// <param name="context"></param>
        /// <param name="invocation"></param>
        public abstract Task Before(IComponentContext context,IInvocation invocation);

    }

   

    /// <summary>
    /// AOP前置拦截器
    /// </summary>
    public abstract class AspectBeforeAttribute : AspectMethodAttribute
    {

        /// <summary>
        /// 前置执行
        /// </summary>
        /// <param name="context"></param>
        /// <param name="invocation"></param>
        public abstract Task Before(IComponentContext context, IInvocation invocation);

    }

  

    /// <summary>
    /// AOP后置拦截器
    /// </summary>
    public abstract class AspectAfterAttribute : AspectMethodAttribute
    {

        /// <summary>
        /// 后置执行
        /// </summary>
        /// <param name="context"></param>
        /// <param name="invocation"></param>
        /// <param name="exp"></param>
        public abstract Task After(IComponentContext context, IInvocation invocation, Exception exp);

    }

    /// <summary>
    /// 切入点拦截器
    /// </summary>
    public abstract class PointcutAttribute : AspectMethodAttribute
    {

        /// <summary>
        /// 没有返回值的拦截器
        /// </summary>
        /// <param name="context"></param>
        /// <param name="invocation"></param>
        /// <param name="proceed"></param>
        public abstract Task InterceptAsync(IComponentContext context, IInvocation invocation,
            Func<IInvocation, Task> proceed);


        /// <summary>
        /// 有返回值的拦截器
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="context"></param>
        /// <param name="invocation"></param>
        /// <param name="proceed"></param>
        /// <returns></returns>
        public abstract Task<TResult> InterceptAsync<TResult>(IComponentContext context, IInvocation invocation,
            Func<IInvocation, Task<TResult>> proceed);

    }

}
