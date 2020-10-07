using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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
            var a13 = container.ResolveKeyed<ITestModel5>("getTest63");
            Assert.Equal("getTest61", a1.Name);
            Assert.Equal("getTest62", a12.Name);
            Assert.Equal("getTest63", a13.Name);

        }

        [Fact]
        public void Test_Type_05()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(UnitTest3).Assembly));

            var container = builder.Build();

            var a1 = container.Resolve<ITestModel6>();
            var a12 = container.Resolve<TestModel61>();

            a1.Hello("test");
            a12.Hello("test");
            
        }


        [Fact]
        public void Test_Type_06()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(UnitTest3).Assembly).SetAllowCircularDependencies(true));

            var container = builder.Build();

            var a12 = container.Resolve<TestModel7>();
            
            Assert.NotNull(a12);
            Assert.NotNull(a12.TestModel71);
            Assert.NotNull(a12.TestModel71.TestModel7);

        }

        [Fact]
        public void Test_Type_07()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(UnitTest3).Assembly).SetAllowCircularDependencies(true));

            var container = builder.Build();

            var a12 = container.Resolve<TestModel8>();

            Assert.NotNull(a12);
            Assert.NotNull(a12.TestModel6);
            Assert.NotNull(a12.TestModel81.TestModel8);

        }

        [Fact]
        public void Test_Type_08()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(UnitTest3).Assembly).SetAllowCircularDependencies(true));

            var container = builder.Build();

            var a12 = container.Resolve<TestModel9>();

            a12.Say();
            a12.SayAfter();

        }


        [Fact]
        public void Test_Type_09()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(UnitTest3).Assembly).SetAllowCircularDependencies(true));

            var container = builder.Build();

            var a12 = container.Resolve<TestModel911>();

            Assert.Throws<Exception>(() => a12.Say());
            Assert.Throws<Exception>(() => a12.SayAfter());

        }

        [Fact]
        public async Task Test_Type_10()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(UnitTest3).Assembly).SetAllowCircularDependencies(true));

            var container = builder.Build();

            var a12 = container.Resolve<TestModel91>();

            await a12.Say();
            await a12.SayAfter();

        }

        [Fact]
        public async Task Test_Type_11()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(UnitTest3).Assembly).SetAllowCircularDependencies(true));

            var container = builder.Build();

            var a12 = container.Resolve<TestModel912>();

            await Assert.ThrowsAsync<Exception>(  () =>  a12.Say());
            await Assert.ThrowsAsync<Exception>(  () =>  a12.SayAfter());
            

        }

        //[Fact]
        //public void Test_Type_12()
        //{
        //    var builder = new ContainerBuilder();

        //    // autofac打标签模式
        //    builder.RegisterModule(new AutofacAnnotationModule(typeof(UnitTest3).Assembly).SetAllowCircularDependencies(true));
        //    builder.Build();
        //    //Assert.Throws<InvalidOperationException>(() => );

        //}

        [Fact]
        public async Task Test_Type_13()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(UnitTest3).Assembly).SetAllowCircularDependencies(true));

            var container = builder.Build();

            var a12 = container.Resolve<TestModel88>();
            
            Assert.NotEmpty(a12.Name);
            Assert.NotEqual("TestModel88", a12.Name);


        }
        
        [Fact]
        public async Task Test_Type_14()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(UnitTest3).Assembly).SetAllowCircularDependencies(true));

            var container = builder.Build();

            var a12 = container.Resolve<TestModel101>();
            
            a12.TestInterceptor();
            a12.TestNoInterceptor();
            Assert.NotEmpty(a12.Name);

           var ss1 = await a12.TestInterceptor2();
           
           Assert.NotEmpty(ss1);


        }


        [Fact]
        public async Task Test_Type_15()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(UnitTest3).Assembly));

            var container = builder.Build();

            var a12 = container.Resolve<AopModel2>();

            a12.AopModel1.SayHello();
        }
        
        [Fact]
        public void Test_Type_16()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(UnitTest3).Assembly));

            var container = builder.Build();

            var a12 = container.Resolve<TestModel1000>();
            
            var a13 = container.Resolve<TestModel1001>();
            Assert.Equal(a12,a13.TestModel1000);
            
            var a132 = container.Resolve<TestModel1002>();
            Assert.Equal(a132.Name,"1002");
            
        }
        
        [Fact]
        public void Test_Type_17()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(UnitTest3).Assembly));

            var container = builder.Build();

            var a132 = container.Resolve<TestImport1>();
            var a1322= container.Resolve<ITestImport>();
            Assert.Equal("TestImport1", a132.Name);
            a1322.Test();
            
        }
    }
}
