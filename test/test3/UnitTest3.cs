using System;
using System.Collections.Generic;
using System.Text;
using Autofac.Annotation;
using Autofac.Configuration.Test.test2;
using Xunit;

namespace Autofac.Configuration.Test.test3
{
    public class UnitTest3
    {
        [Fact]
        public void Test_Type_01()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(UnitTest3).Assembly));

            var container = builder.Build();

            var a1 = container.Resolve<TestModel3>();
            var a12 = container.Resolve<TestModel3>();
            Assert.Equal("aa", a1.Name);
            Assert.Equal(a1,a12);

        }

        [Fact]
        public void Test_Type_02()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(UnitTest3).Assembly));

            var container = builder.Build();

            var a1 = container.ResolveKeyed<TestModel3>("getTest31");
            var a12 = container.ResolveKeyed<TestModel3>("getTest31");
            Assert.Equal("aa1",a1.Name);
            Assert.Equal(a1, a12);

        }

        [Fact]
        public void Test_Type_03()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(UnitTest3).Assembly));

            var container = builder.Build();

            var a1 = container.Resolve<ITestModel4>();
            var a12 = container.Resolve<TestModel4Parent>();
            Assert.Equal("getTest5", a1.Name);
            Assert.NotEqual(a1, a12);

        }

        [Fact]
        public void Test_Type_04()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(UnitTest3).Assembly));

            var container = builder.Build();

            var a1 = container.ResolveKeyed<ITestModel5>("getTest61");
            var a12 = container.ResolveKeyed<ITestModel5>("getTest62");
            Assert.Equal("getTest61", a1.Name);
            Assert.Equal("getTest62", a12.Name);

        }
    }
}
