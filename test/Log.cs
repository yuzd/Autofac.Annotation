using Castle.DynamicProxy;
using System;
using System.Threading.Tasks;

namespace Autofac.Annotation.Test
{
    [Component]
    [Order(int.MaxValue)]
    public class Log : AsyncInterceptor
    {

        protected override void Intercept(IInvocation invocation)
        {
            invocation.Proceed();
            if (invocation.ReturnValue is string)
            {
                invocation.ReturnValue = "a";
            }
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

    [Component("log2")]
    public class Log2 : AsyncInterceptor
    {
        //[Value("test")]
        public string Test { get; set; }

        //[Autowired]
        public A21 A21 { get; set; }

        protected override void Intercept(IInvocation invocation)
        {
            invocation.Proceed();
            if (invocation.ReturnValue is string)
            {
                invocation.ReturnValue = "b";
            }
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