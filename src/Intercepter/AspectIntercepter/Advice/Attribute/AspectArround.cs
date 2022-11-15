using System.Threading.Tasks;
using Autofac.Annotation;

namespace Autofac.AspectIntercepter.Advice
{

    /// <summary>
    /// 环绕通知 包围一个连接点的通知
    /// </summary>
    public abstract class AspectArround : AspectInvokeAttribute
    {
        /// <summary>
        /// 拦截器
        /// </summary>
        /// <param name="aspectContext">拦截上下文</param>
        /// <param name="_next">下一个拦截器 最后一个是执行被拦截的方法</param>
        /// <returns></returns>
        public abstract Task OnInvocation(AspectContext aspectContext, AspectDelegate _next);
    }
}