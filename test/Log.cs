using System;
using System.Threading.Tasks;
using Castle.DynamicProxy;

namespace Autofac.Annotation.Test
{
    [Bean(typeof(AsyncInterceptor))]
    public class Log : AsyncInterceptor
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
                var tt =  (TResult) Activator.CreateInstance(typeof(String),new char[]{'a'});
                invocation.ReturnValue = tt;
                return tt;
            }
            return result;
        }
    }

    [Bean(typeof(AsyncInterceptor),"log2")]
    public class Log2 : AsyncInterceptor
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
                var tt =  (TResult) Activator.CreateInstance(typeof(String),new char[]{'b'});
                invocation.ReturnValue = tt;
                return tt;
            }
            return result;
        }
    }
}