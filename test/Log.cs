using Castle.DynamicProxy;
using System;
using System.Threading.Tasks;

namespace Autofac.Annotation.Test
{
    [Component(typeof(AsyncInterceptor))]
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

    [Component(typeof(AsyncInterceptor),"log2")]
    public class Log2 : AsyncInterceptor
    {
        [Value("test")]
        public string Test { get; set; }

        [Autowired]
        public A21 A21 { get; set; }

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