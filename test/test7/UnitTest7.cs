using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Autofac.Annotation;
using Autofac.Annotation.Test.test6;
using Xunit;

namespace Autofac.Configuration.Test.test7
{
    public class UnitTest7
    {
        [Fact]
        public async Task Test_Type_01()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(UnitTest7).Assembly));

            var container = builder.Build();

            var a1 = container.Resolve<TestCacheAop>();

            var result = await a1.TestInterceptor2();
            var result2 = await a1.TestInterceptor2();
            Assert.Equal(result2,result);
        }
    }
}
