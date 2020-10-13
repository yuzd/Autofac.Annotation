using System.Threading.Tasks;
using Autofac.Annotation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Autofac.AspectIntercepter.Advice
{

    /// <summary>
    /// 后置通知 当某连接点退出的时候执行的通知（不论是正常返回还是异常退出）。
    /// </summary>
    public abstract class AspectAfter: AspectInvokeAttribute
    {
        /// <summary>
        /// 后置执行
        /// </summary>
        /// <param name="aspectContext"></param>
        /// <param name="result"></param>
        public abstract Task After(AspectContext aspectContext,object result);

    }

}