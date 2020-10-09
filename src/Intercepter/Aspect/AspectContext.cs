using Castle.DynamicProxy;

namespace Autofac.Aspect
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    /// <summary>
    /// 拦截器上下文
    /// </summary>
    public class AspectContext
    {
        /// <summary>
        /// 空的构造方法
        /// </summary>
        public AspectContext()
        {
            
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="context"></param>
        /// <param name="invocation"></param>
        /// <param name="proceedInfo"></param>
        public AspectContext(IComponentContext context,IInvocation invocation,IInvocationProceedInfo proceedInfo)
        {
            this.ComponentContext = context;
            this.InvocationContext = invocation;
            this.CaptureProceedInfo = proceedInfo;
        }


        /// <summary>
        /// autofac容器
        /// </summary>
        public IComponentContext ComponentContext { get; set; }
        
        /// <summary>
        /// 执行快照
        /// </summary>
        public IInvocationProceedInfo CaptureProceedInfo { get; set; }
        
        /// <summary>
        /// 执行环节上下文
        /// </summary>

        public IInvocation InvocationContext { get; set; }
       
        
        /// <summary>
        /// 有返回结果的
        /// </summary>
        internal object Result { get; set; }
        
        /// <summary>
        /// 有返回Exception
        /// </summary>
        internal Exception Exception { get; set; }
    }

}