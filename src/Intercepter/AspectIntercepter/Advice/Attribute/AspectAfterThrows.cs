using System;
using System.Threading.Tasks;
using Autofac.Annotation;

namespace Autofac.AspectIntercepter.Advice
{
    /// <summary>
    /// 异常通知 在方法抛出异常退出时执行的通知。
    /// </summary>
    public abstract class AspectAfterThrows : AspectInvokeAttribute
    {
        /// <summary>
        /// 异常的类型 根据下面的方法解析泛型
        /// </summary>
        public virtual Type ExceptionType { get; }

        /// <summary>
        /// 后置执行
        /// </summary>
        /// <param name="aspectContext">上下文</param>
        /// <param name="exception">异常</param>
        public abstract Task AfterThrows(AspectContext aspectContext, Exception exception);
    }
}