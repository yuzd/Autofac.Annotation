using Autofac.Annotation;

namespace Autofac.AspectIntercepter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// 拦截node组装
    /// </summary>
    internal class AspectMiddlewareComponentNode
    {
        /// <summary>
        /// 下一个
        /// </summary>
        public AspectDelegate Next;
        /// <summary>
        /// 执行器
        /// </summary>
        public AspectDelegate Process;
        /// <summary>
        /// 组件
        /// </summary>
        public Func<AspectDelegate, AspectDelegate> Component;
    }

}