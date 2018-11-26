using Xunit;

namespace Autofac.Annotation.Test
{
    public class AnotationTest
    {
        
        [Fact]
        public void Test_Type_01()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly));

            var container = builder.Build();

            var a1 = container.Resolve<A>();

            Assert.NotNull(a1);

        }

        [Fact]
        public void Test_Type_02()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly));

            var container = builder.Build();

            var a1 = container.Resolve<B>();

            Assert.NotNull(a1);

            Assert.Equal("A14", a1.GetSchool());

        }

        [Fact]
        public void Test_Type_03()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly));

            var container = builder.Build();

            var a1 = container.ResolveKeyed<B>("B1");
            var a2 = container.ResolveKeyed<B>("B2");

            Assert.NotNull(a1);
            Assert.NotNull(a2);

            Assert.Equal("测试1",a1.GetSchool());
            Assert.Equal("测试2",a2.GetSchool());

        }

        [Fact]
        public void Test_Type_04()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly));

            var container = builder.Build();

            var a1 = container.ResolveKeyed<A4>("a4");
            var ab = container.ResolveKeyed<B>("a5");
            var a6 = container.Resolve<B>();

            Assert.NotNull(a1);
            Assert.NotNull(ab);
            Assert.NotNull(a6);
            Assert.Equal("测试a5",ab.GetSchool());
        }

        
        [Fact]
        public void Test_Type_05()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly));

            var container = builder.Build();

            
            var a1 = container.Resolve<A8>();
            var a2 = container.Resolve<A9>();
            Assert.Equal(4,a2.list.Count);
            Assert.Equal(2,a2.dic.Keys.Count);
            Assert.Equal("a8",a1.GetSchool());
            Assert.Equal("aaaaaaaaa",a2.GetSchool());
           
        }
        [Fact]
        public void Test_Type_06()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly));

            var container = builder.Build();

            
            var a2 = container.Resolve<A10>();
            Assert.Equal(3,a2.list.Count);
            Assert.Single(a2.dic.Keys);
            Assert.Equal("aaaaaaaaa1",a2.GetSchool());
           
        }

        [Fact]
        public void Test_Type_07()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly));

            var container = builder.Build();

            
            var a2 = container.Resolve<A11>();
            Assert.Equal(3,a2.list.Count);
            Assert.Single(a2.dic.Keys);
            Assert.Equal("aaaaaaaaa1",a2.GetSchool());
           
        }

        [Fact]
        public void Test_Type_08()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly));

            var container = builder.Build();


            var a2 = container.Resolve<A12>();
            var a3 = container.ResolveKeyed<B>("A12");
            Assert.Equal("A12", a2.GetSchool());
            Assert.Equal("A12", a3.GetSchool());

        }

        [Fact]
        public void Test_Type_09()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly));

            var container = builder.Build();


            var a2 = container.ResolveKeyed<A13>("aa12");
            var a3 = container.ResolveKeyed<B>("A13");
            Assert.Equal("A13", a2.GetSchool());
            Assert.Equal("A13", a3.GetSchool());

        }


        [Fact]
        public void Test_Type_10()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly));

            var container = builder.Build();


            var a2 = container.ResolveKeyed<A14>("aa14");
            var a3 = container.Resolve<B>();
            container.TryResolveKeyed("aa14",typeof(B),out object aa);
            Assert.Null(aa);
            Assert.Equal("A14", a2.GetSchool());
            Assert.Equal("A14", a3.GetSchool());

        }

        [Fact]
        public void Test_Type_11()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly));

            var container = builder.Build();


            var a3 = container.Resolve<A15>();
            Assert.Equal("aaaaaaaaa", a3.School);

        }
    }
}
