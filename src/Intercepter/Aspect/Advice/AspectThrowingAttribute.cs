using System.Threading.Tasks;

namespace Autofac.Aspect.Advice
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// 异常通知
    /// </summary>
    public abstract class AspectThrowingAttribute : AspectInvokeAttribute
    {
        /// <summary>
        /// 异常的类型 根据下面的方法解析泛型
        /// </summary>
        internal Type ExceptionType { get; set; }

        /// <summary>
        /// 后置执行
        /// </summary>
        /// <param name="aspectContext"></param>
        /// <param name="exception"></param>
        public abstract Task Throwing<E>(AspectContext aspectContext,E exception) where E:Exception;

    }


}