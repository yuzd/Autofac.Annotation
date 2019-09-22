using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Autofac.Annotation;
using Castle.DynamicProxy;

namespace Autofac.Configuration.Test.test2
{
    [Component]
    public class Test2Interceptor : AsyncInterceptor
    {
        protected override async Task InterceptAsync(IInvocation invocation, Func<IInvocation, Task> proceed)
        {
            await proceed(invocation).ConfigureAwait(false);
        }

        protected override async Task<TResult> InterceptAsync<TResult>(IInvocation invocation, Func<IInvocation, Task<TResult>> proceed)
        {
            TResult result = await proceed(invocation).ConfigureAwait(false);
            if (result is string)
            {
                var tt = (TResult)Activator.CreateInstance(typeof(String), new char[] { 'a' });
                invocation.ReturnValue = tt;
                return tt;
            }
            return result;
        }
    }

    [Component("Test2Interceptor2")]
    public class Test2Interceptor2 : AsyncInterceptor
    {
        protected override async Task InterceptAsync(IInvocation invocation, Func<IInvocation, Task> proceed)
        {
            await proceed(invocation).ConfigureAwait(false);
        }

        protected override async Task<TResult> InterceptAsync<TResult>(IInvocation invocation, Func<IInvocation, Task<TResult>> proceed)
        {
            TResult result = await proceed(invocation).ConfigureAwait(false);
            if (result is string)
            {
                var tt = (TResult)Activator.CreateInstance(typeof(String), new char[] { 'b' });
                invocation.ReturnValue = tt;
                return tt;
            }
            return result;
        }
    }

}
