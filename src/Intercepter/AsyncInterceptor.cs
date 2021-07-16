// Copyright (c) 2020 stakx
// License available at https://github.com/stakx/DynamicProxy.AsyncInterceptor/blob/master/LICENSE.md.

using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Castle.DynamicProxy;

namespace Castle.DynamicProxy
{
    /// <summary>
    /// 
    /// </summary>
    public abstract partial class AsyncInterceptor : IInterceptor
    {
        void IInterceptor.Intercept(IInvocation invocation)
        {
            var returnType = invocation.Method.ReturnType;
            var builder = AsyncMethodBuilder.TryCreate(returnType);
            if (builder != null)
            {
                var asyncInvocation = new AsyncInvocation(invocation);
                var stateMachine = new AsyncStateMachine(asyncInvocation, builder, task: this.InterceptAsync(asyncInvocation));
                builder.Start(stateMachine);
                invocation.ReturnValue = builder.Task();
            }
            else
            {
                this.Intercept(invocation);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="invocation"></param>
        protected abstract void Intercept(IInvocation invocation);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="invocation"></param>
        /// <returns></returns>
        protected abstract ValueTask InterceptAsync(IAsyncInvocation invocation);
    }
}