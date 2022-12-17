using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Castle.DynamicProxy;

namespace Autofac.Annotation
{
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
            AdditionalData = new Dictionary<string, object>();
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="context"></param>
        /// <param name="invocation"></param>
        public AspectContext(IComponentContext context, IInvocation invocation) : this()
        {
            this.ComponentContext = context;
            this.InvocationContext = invocation;
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="context"></param>
        /// <param name="invocation"></param>
        public AspectContext(IComponentContext context, IAsyncInvocation invocation)
        {
            this.ComponentContext = context;
            this.IAsyncnvocationContext = invocation;
        }

        /// <summary>
        /// autofac容器
        /// </summary>
        public IComponentContext ComponentContext { get; set; }


        /// <summary>
        /// 执行环节上下文
        /// </summary>

        internal IInvocation InvocationContext { get; set; }

        /// <summary>
        /// 异步执行环节上下文
        /// </summary>
        internal IAsyncInvocation IAsyncnvocationContext { get; set; }

        /// <summary>
        ///  临时存储,比如在多个Interceptor中传递
        /// </summary>
        public IDictionary<string, object> AdditionalData { get; }

        /// <summary>
        /// 被拦截的目标方法的参数
        /// </summary>
        public IReadOnlyList<object> Arguments
        {
            get
            {
                if (InvocationContext != null)
                {
                    return InvocationContext.Arguments;
                }

                return IAsyncnvocationContext.Arguments;
            }
        }

        /// <summary>
        /// 被拦截的目标方法
        /// </summary>
        public MethodInfo TargetMethod
        {
            get
            {
                if (InvocationContext != null)
                {
                    return InvocationContext.MethodInvocationTarget;
                }

                return IAsyncnvocationContext.TargetMethod;
            }
        }

        /// <summary>
        /// 被拦截的目标方法的proxy方法
        /// </summary>
        public MethodInfo Method
        {
            get
            {
                if (InvocationContext != null)
                {
                    return InvocationContext.Method;
                }

                return IAsyncnvocationContext.Method;
            }
        }

        /// <summary>
        /// 设置返回值或者获取返回值
        /// </summary>
        public object ReturnValue
        {
            get
            {
                if (InvocationContext != null)
                {
                    return InvocationContext.ReturnValue;
                }

                return IAsyncnvocationContext?.Result;
            }
            set
            {
                if (InvocationContext != null)
                {
                    InvocationContext.ReturnValue = value;
                    return;
                }

                if (IAsyncnvocationContext != null)
                {
                    IAsyncnvocationContext.Result = value;
                }
            }
        }


        /// <summary>
        /// 实际真正的方法用在拦截器链的执行过程中
        /// </summary>
        internal Func<ValueTask> Proceed { get; set; }


        /// <summary>
        /// 有返回Exception
        /// </summary>
        internal Exception Exception { get; set; }
    }
}