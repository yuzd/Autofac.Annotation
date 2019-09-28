using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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


    }
    /// <summary>
    /// AOP环绕拦截器
    /// </summary>
    public abstract class AspectAroundAttribute : AspectMethodAttribute
    {


        /// <summary>
        /// 后置执行
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="exp"></param>
        public abstract Task After(IInvocation invocation, Exception exp);


        /// <summary>
        /// 前置执行
        /// </summary>
        /// <param name="invocation"></param>
        public abstract Task Before(IInvocation invocation);

    }

   

    /// <summary>
    /// AOP前置拦截器
    /// </summary>
    public abstract class AspectBeforeAttribute : AspectMethodAttribute
    {

        /// <summary>
        /// 前置执行
        /// </summary>
        /// <param name="invocation"></param>
        public abstract Task Before(IInvocation invocation);

    }

  

    /// <summary>
    /// AOP后置拦截器
    /// </summary>
    public abstract class AspectAfterAttribute : AspectMethodAttribute
    {

        /// <summary>
        /// 后置执行
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="exp"></param>
        public abstract Task After(IInvocation invocation, Exception exp);

    }

}
