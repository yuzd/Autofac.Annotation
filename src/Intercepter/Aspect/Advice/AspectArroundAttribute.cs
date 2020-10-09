using System.Threading.Tasks;

namespace Autofac.Aspect.Advice
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// 环绕通知
    /// </summary>
    public abstract class AspectArroundAttribute : AspectInvokeAttribute
    {

        /// <summary>
        /// 拦截器
        /// </summary>
        /// <param name="aspectContext">拦截上下文</param>
        /// <param name="_next">下一个拦截器 最后一个是执行被拦截的方法</param>
        /// <returns></returns>
        public abstract Task OnInvocation(AspectContext aspectContext,AspectDelegate _next);

    }

}