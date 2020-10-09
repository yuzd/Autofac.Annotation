using System.Threading.Tasks;

namespace Autofac.Aspect.Advice
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// 后置通知
    /// </summary>
    public abstract class AspectAfterAttribute : AspectInvokeAttribute
    {
        /// <summary>
        /// 后置执行
        /// </summary>
        /// <param name="aspectContext"></param>
        /// <param name="result"></param>
        public abstract Task After(AspectContext aspectContext,object result);

    }

}