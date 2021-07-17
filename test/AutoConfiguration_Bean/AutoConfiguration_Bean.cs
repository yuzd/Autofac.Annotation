using System.Linq;
using System.Threading;
using Autofac.Annotation;
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
            Assert.Equal("yuzd", a1.Name);
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
            Assert.Equal(2, a1.MyModel2.Count());
        }

        [Fact]
        public void Test_Type_04()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(MyConfig).Assembly));

            var container = builder.Build();

            var a1 = container.Resolve<MyModel4>();
            Thread.Sleep(1000);
            var a2 = container.Resolve<MyModel4>();
            Assert.False(a1.Name == a2.Name);
        }

        [Fact]
        public void Test_Type_05()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(MyConfig).Assembly));

            var container = builder.Build();

            var a1 = container.Resolve<MyModel5>();


            Assert.Equal("test", a1.Name);

            container.Dispose();
            Assert.Equal("end", a1.Name);
        }
    }
}