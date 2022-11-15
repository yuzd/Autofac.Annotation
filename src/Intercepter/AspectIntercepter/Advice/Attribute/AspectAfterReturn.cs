using System.Threading.Tasks;
using Autofac.Annotation;

namespace Autofac.AspectIntercepter.Advice
{
    /// <summary>
    /// 后置通知 在某连接点正常完成后执行的通知，不包括抛出异常的情况。
    /// </summary>
    public abstract class AspectAfterReturn : AspectInvokeAttribute
    {
        /// <summary>
        /// 后置执行，没有出现异常
        /// </summary>
        /// <param name="aspectContext">上下文</param>
        /// <param name="result">方法运行结果，如果没有的话就是null</param>
        public abstract Task AfterReturn(AspectContext aspectContext, object result);
    }
}