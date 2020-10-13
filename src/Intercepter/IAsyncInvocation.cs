// Copyright (c) 2020 stakx
// License available at https://github.com/stakx/DynamicProxy.AsyncInterceptor/blob/master/LICENSE.md.

using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Castle.DynamicProxy
{
    /// <summary>
    /// 
    /// </summary>
    public interface IAsyncInvocation
    {
        /// <summary>
        /// 被拦截的目标方法的参数
        /// </summary>
        IReadOnlyList<object> Arguments { get; }
        /// <summary>
        /// 被拦截的目标方法
        /// </summary>
        MethodInfo TargetMethod { get; }
        
        /// <summary>
        /// 被拦截的目标方法的proxy方法
        /// </summary>
        MethodInfo Method { get; }
        /// <summary>
        /// 被拦截的目标方法的返回
        /// </summary>
        object Result { get; set; }
        
        /// <summary>
        /// 代理方法
        /// </summary>
        /// <returns></returns>
        ValueTask ProceedAsync();
    }
}
