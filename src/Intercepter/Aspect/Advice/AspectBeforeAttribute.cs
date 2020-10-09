using System.Threading.Tasks;

namespace Autofac.Aspect.Advice
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// 前置通知
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