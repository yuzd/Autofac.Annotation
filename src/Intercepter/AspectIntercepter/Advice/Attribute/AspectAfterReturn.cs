using System.Threading.Tasks;
using Autofac.Annotation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Autofac.AspectIntercepter.Advice
{

    /// <summary>
    /// 后置通知 在某连接点正常完成后执行的通知，不包括抛出异常的情况。
    /// </summary>
    public abstract class AspectAfterReturn : AspectInvokeAttribute
    {
        /// <summary>
        /// 后置执行
        /// </summary>
        /// <param name="aspectContext"></param>
        /// <param name="result"></param>
        public abstract Task AfterReturn(AspectContext aspectContext,object result);

    }

}