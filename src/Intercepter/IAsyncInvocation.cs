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
        /// 
        /// </summary>
        IReadOnlyList<object> Arguments { get; }
        /// <summary>
        /// 
        /// </summary>
        MethodInfo Method { get; }
        /// <summary>
        /// 
        /// </summary>
        object Result { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ValueTask ProceedAsync();
    }
}
