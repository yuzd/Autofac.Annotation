using System.Threading.Tasks;
using Autofac.Annotation;

namespace Autofac.AspectIntercepter.Advice
{
    /// <summary>
    /// 前置通知 在某连接点——核心代码（类或者方法）之前执行的通知，但这个通知不能阻止连接点前的执行
    /// </summary>
    public abstract class AspectBefore : AspectInvokeAttribute
    {
        /// <summary>
        /// 前置执行
        /// </summary>
        /// <param name="aspectContext"></param>
        public abstract Task Before(AspectContext aspectContext);
    }
}