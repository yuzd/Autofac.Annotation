using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Autofac.Annotation;
using Castle.DynamicProxy;
using Xunit;

namespace Autofac.Configuration.Test.test2
{
    public class Test2
    {
        [Fact]
        public void Test_Type_01()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(Test2).Assembly));

            var container = builder.Build();

            var a1 = container.Resolve<Model2>();
            Assert.Equal(typeof(Model2), a1.GetType());

        }


        [Fact]
        public void Test_Type_02()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(Test2).Assembly));

            var container = builder.Build();

            //注册顺序
            var a1 = container.Resolve<Model1>();
            Assert.Equal(typeof(Model2), a1.GetType());

        }

        [Fact]
        public void Test_Type_03()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(Test2).Assembly));

            var container = builder.Build();

            var a1 = container.Resolve<Model11>();
            Assert.Equal(typeof(Model22), a1.GetType());

        }

        [Fact]
        public void Test_Type_04()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(Test2).Assembly));

            var container = builder.Build();

            var a1 = container.ResolveKeyed<Model11>(nameof(Model11));
            Assert.Equal(typeof(Model11), a1.GetType());

        }

        [Fact]
        public void Test_Type_05()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(Test2).Assembly));

            var container = builder.Build();

            var a1 = container.Resolve<Model12>();
            Assert.Equal(typeof(Model222), a1.GetType());

        }

        [Fact]
        public void Test_Type_06()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(Test2).Assembly));

            var container = builder.Build();

            var a1 = container.Resolve<Imodel1>();
            Assert.Equal(typeof(Model32), a1.GetType());

        }

        [Fact]
        public void Test_Type_07()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(Test2).Assembly));

            var container = builder.Build();

            var a1 = container.Resolve<Imodel1>();
            var a2 = container.Resolve<Model3>();
            Assert.Equal(typeof(Model32), a1.GetType());
            Assert.Equal(typeof(Model32), a2.GetType());

        }

        [Fact]
        public void Test_Type_08()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(Test2).Assembly));

            var container = builder.Build();

            var a1 = container.Resolve<Imodel2>();
            var a2 = container.Resolve<Model4>();
            Assert.Equal(typeof(Model42), a1.GetType());
            Assert.Equal(typeof(Model42), a2.GetType());

        }


        [Fact]
        public async Task Test_Type_09()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(Test2).Assembly));

            var container = builder.Build();

            var a1 = container.Resolve<Imodel3>();
            var r = await a1.hello();
            container.TryResolve<Model5>(out var obj);
            Assert.Null(obj);
            Assert.Equal("a",r);

        }

        [Fact]
        public async Task Test_Type_10()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(Test2).Assembly));

            var container = builder.Build();

            var a1 = container.Resolve<Model55>();
            var r = await a1.hello();
            Assert.Equal("a", r);

        }

        [Fact]
        public async Task Test_Type_11()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(Test2).Assembly));

            var container = builder.Build();

            var a1 = container.Resolve<Model6>();
            var r = await a1.hello();
            Assert.Equal("b", r);

        }

        [Fact]
        public async Task Test_Type_12()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(Test2).Assembly));

            var container = builder.Build();

            var a1 = container.Resolve<Model61>();
            var r = await a1.hello();
            Assert.Equal("b", r);

        }

        [Fact]
        public void Test_Type_13()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(Test2).Assembly));

            var container = builder.Build();

            var a1 = container.Resolve<IList<AsyncInterceptor>>();
            Assert.True(a1.Count == 2); //用autofac本身的Resolve方法只能获取到2个 因为另外2个是加了key

        }

        [Fact]
        public void Test_Type_14()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(Test2).Assembly));

            var container = builder.Build();

            var a1 = container.Resolve<Model7>();//Autowired的方式可以获取到所有的
            Assert.True(a1.Interceptors.Count  == 4);

        }
    }
}
