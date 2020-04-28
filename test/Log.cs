using Castle.DynamicProxy;
using System;
using System.Threading.Tasks;

namespace Autofac.Annotation.Test
{
    [Component]
    public class Log : AsyncInterceptor
    {
        protected override async Task InterceptAsync(
            IInvocation invocation,
            IInvocationProceedInfo proceedInfo,
            Func<IInvocation, IInvocationProceedInfo, Task> proceed)
        {
            await proceed(invocation, proceedInfo).ConfigureAwait(false);
        }

        protected override async Task<TResult> InterceptAsync<TResult>(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed)
        {
            TResult result = await proceed(invocation, proceedInfo).ConfigureAwait(false);
            if (result is string)
            {
                var tt = (TResult)Activator.CreateInstance(typeof(String), new char[] { 'a' });
                invocation.ReturnValue = tt;
                return tt;
            }
            return result;
        }

    }

    [Component("log2")]
    public class Log2 : AsyncInterceptor
    {
        //[Value("test")]
        public string Test { get; set; }

        //[Autowired]
        public A21 A21 { get; set; }

        protected override async Task InterceptAsync(
            IInvocation invocation,
            IInvocationProceedInfo proceedInfo,
            Func<IInvocation, IInvocationProceedInfo, Task> proceed)
        {
            await proceed(invocation, proceedInfo).ConfigureAwait(false);
        }

        protected override async Task<TResult> InterceptAsync<TResult>(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed)
        {
            TResult result = await proceed(invocation, proceedInfo).ConfigureAwait(false);
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