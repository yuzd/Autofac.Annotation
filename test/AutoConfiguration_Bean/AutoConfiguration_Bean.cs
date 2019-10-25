using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Annotation;
using Autofac.Annotation.Anotation;
using Autofac.Configuration.Test.test3;
using Xunit;

namespace Autofac.Configuration.Test.AutoConfiguration_Bean
{
    
    public class AutoConfiguration_Bean
    {
        [Fact]
        public void Test_Type_01()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(MyConfig).Assembly).SetAutofacConfigurationKey("Test"));

            var container = builder.Build();

            var a1 = container.Resolve<MyModel>();
            Assert.NotNull(a1);
            Assert.Equal("yuzd",a1.Name);
        }

        [Fact]
        public void Test_Type_02()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(MyConfig).Assembly).SetAutofacConfigurationKey("Test2"));

            var container = builder.Build();

            var a1 = container.Resolve<MyModel>();
            Assert.NotNull(a1);
            Assert.Equal(a1, container.Resolve<MyModel>());
            Assert.Equal("yuzd2", a1.Name);
        }

        [Fact]
        public void Test_Type_03()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(MyConfig).Assembly));

            var container = builder.Build();

            var a1 = container.Resolve<MyModel3>();
            Assert.NotNull(a1);
            Assert.Equal(2,a1.MyModel2.Count());
        }
    }
}
