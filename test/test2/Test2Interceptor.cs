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
        protected override void Intercept(IInvocation invocation)
        {
            invocation.Proceed();
        }

        protected override async ValueTask InterceptAsync(IAsyncInvocation invocation)
        {
            await invocation.ProceedAsync();
            if (invocation.Result is string)
            {
                invocation.Result = "a";
            }
        }
    }

    [Component("Test2Interceptor2")]
    public class Test2Interceptor2 : AsyncInterceptor
    {

        protected override void Intercept(IInvocation invocation)
        {
            invocation.Proceed();
        }

        protected override async ValueTask InterceptAsync(IAsyncInvocation invocation)
        {
            await invocation.ProceedAsync();
            if (invocation.Result is string)
            {
                invocation.Result = "b";
            }
        }
    }

}
