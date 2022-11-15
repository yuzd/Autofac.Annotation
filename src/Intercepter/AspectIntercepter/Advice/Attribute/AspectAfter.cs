using System.Threading.Tasks;
using Autofac.Annotation;


namespace Autofac.AspectIntercepter.Advice
{
    /// <summary>
    /// 后置通知 当某连接点退出的时候执行的通知（不论是正常返回还是异常退出）。
    /// </summary>
    public abstract class AspectAfter : AspectInvokeAttribute
    {
        /// <summary>
        /// 后置执行
        /// </summary>
        /// <param name="aspectContext">上下文</param>
        /// <param name="result">方法运行结果，如果没有的话就是null</param>
        public abstract Task After(AspectContext aspectContext, object result);
    }
}