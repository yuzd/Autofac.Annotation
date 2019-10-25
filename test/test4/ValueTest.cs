using System;
using System.Collections.Generic;
using System.Text;
using Autofac.Annotation;
using Autofac.Configuration.Test.test3;
using Xunit;

namespace Autofac.Configuration.Test.test4
{
    public class ValueTest
    {
        [Fact]
        public void Test_Type_01()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(ValueTest).Assembly));

            var ioc = builder.Build();

            var a1 = ioc.Resolve<ValueModel2>();

            Assert.NotNull(a1.ValueModel1);
            Assert.NotNull(a1.CallA9);

        }

        [Fact]
        public void Test_Type_02()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(ValueTest).Assembly));

            var ioc = builder.Build();

            var a1 = ioc.Resolve<ValueModel4>();

            Assert.Equal("aaaaaaaaa_ValueModel32_override", a1.CallA9);

        }

        [Fact]
        public void Test_Type_03()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(ValueTest).Assembly));

            var ioc = builder.Build();

            var a1 = ioc.Resolve<ValueModel5>();

            Assert.Equal("yuzd", a1.ParentName);

        }
    }
}
