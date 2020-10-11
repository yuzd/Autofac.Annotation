using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac.Configuration.Test.test7;
using Xunit;

namespace Autofac.Annotation.Test.test6
{
    public class UnitTest6
    {
        [Fact]
        public void Test_Type_01()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(UnitTest6).Assembly).SetAllowCircularDependencies(true));

            var container = builder.Build();
                
            var a1 = container.Resolve<LogAspectTest1>();
            var a2 = container.Resolve<LogAspectTest2>();
            var a3 = container.Resolve<LogAspectT1est>();
           
            //被拦截
            a1.Test1();
            //不会被拦截
            a1.Test2();
            
            //被拦截
            a2.Test1();
            //不会被拦截
            a2.Test2();

            //不会拦截
            a3.Test1();
            a3.Test2();


        }
        
        [Fact]
        public void Test_Type_02()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(UnitTest6).Assembly).SetAllowCircularDependencies(true));

            var container = builder.Build();
                
            var a1 = container.Resolve<IAspectA>();
            var a2 = container.Resolve<IAspecB>();
           
            a1.Hello("ddd");
            var a111 = a1.Hello2("ssss");
            
            a2.Hello("ddd2");
            var a1112 = a2.Hello2("ssss2");

        }
        
        [Fact]
        public void Test_Type_03()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(UnitTest6).Assembly).SetAllowCircularDependencies(true));

            var container = builder.Build();
                
            var a1 = container.Resolve<LogAroundTest>();
           
            a1.Hello("ddd");
            var a111 = a1.Hello2("ssss");
            

        }
        
        [Fact]
        public void Test_Type_04()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(UnitTest6).Assembly).SetAllowCircularDependencies(true));

            var container = builder.Build();
                
            var a1 = container.Resolve<LogTaskTest>();
           
            a1.Hello("ddd");
            var a111 = a1.Hello2("ssss");
            

        }
        
        [Fact]
        public async Task Test_Type_05()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(UnitTest6).Assembly).SetAllowCircularDependencies(true));

            var container = builder.Build();
                
            var a1 = container.Resolve<ICacheAop23<Model1>>();
           
            await a1.TestInterceptor2();
        }
        
        
        [Fact]
        public void Test_Type_06()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(UnitTest6).Assembly).SetAllowCircularDependencies(true));

            var container = builder.Build();
                
            Pointcut1Controller.testResult = new List<string>();
            var a1 = container.Resolve<Pointcut1Controller>();
            a1.TestSuccess();
            Assert.Equal(9,Pointcut1Controller.testResult.Count);
            Assert.Equal("PointcutTest2.Around-start",Pointcut1Controller.testResult[0]);
            Assert.Equal("PointcutTest2.Before",Pointcut1Controller.testResult[1]);
            Assert.Equal("PointcutTest1.Around-start",Pointcut1Controller.testResult[2]);
            Assert.Equal("PointcutTest1.Before",Pointcut1Controller.testResult[3]);
            Assert.Equal("Pointcut1Controller.TestSuccess",Pointcut1Controller.testResult[4]);
            Assert.Equal("PointcutTest1.Around-end",Pointcut1Controller.testResult[5]);
            Assert.Equal("PointcutTest1.After",Pointcut1Controller.testResult[6]);
            Assert.Equal("PointcutTest2.Around-end",Pointcut1Controller.testResult[7]);
            Assert.Equal("PointcutTest2.After",Pointcut1Controller.testResult[8]);
        }
        
        [Fact]
        public void Test_Type_07()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(UnitTest6).Assembly).SetAllowCircularDependencies(true));

            var container = builder.Build();
                
            Pointcut2Controller.testResult = new List<string>();
            var a1 = container.Resolve<Pointcut2Controller>();
            a1.TestSuccess();
            Assert.Equal(9,Pointcut2Controller.testResult.Count);
            Assert.Equal("PointcutTest2.Around-start",Pointcut2Controller.testResult[0]);
            Assert.Equal("PointcutTest2.Before",Pointcut2Controller.testResult[1]);
            Assert.Equal("PointcutTest1.Around-start",Pointcut2Controller.testResult[2]);
            Assert.Equal("PointcutTest1.Before",Pointcut2Controller.testResult[3]);
            Assert.Equal("Pointcut2Controller.TestSuccess",Pointcut2Controller.testResult[4]);
            Assert.Equal("PointcutTest1.Around-end",Pointcut2Controller.testResult[5]);
            Assert.Equal("PointcutTest1.After",Pointcut2Controller.testResult[6]);
            Assert.Equal("PointcutTest2.Around-end",Pointcut2Controller.testResult[7]);
            Assert.Equal("PointcutTest2.After",Pointcut2Controller.testResult[8]);
        }
        
        
        [Fact]
        public void Test_Type_08()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(UnitTest6).Assembly).SetAllowCircularDependencies(true));

            var container = builder.Build();
                
            Pointcut1Controller.testResult = new List<string>();
            var a1 = container.Resolve<Pointcut1Controller>();
            Assert.Throws<ArgumentException>(() => a1.TestThrow());
            Assert.Equal(7,Pointcut1Controller.testResult.Count);
            Assert.Equal("PointcutTest2.Around-start",Pointcut1Controller.testResult[0]);
            Assert.Equal("PointcutTest2.Before",Pointcut1Controller.testResult[1]);
            Assert.Equal("PointcutTest1.Around-start",Pointcut1Controller.testResult[2]);
            Assert.Equal("PointcutTest1.Before",Pointcut1Controller.testResult[3]);
            Assert.Equal("Pointcut1Controller.TestThrow",Pointcut1Controller.testResult[4]);
            Assert.Equal("PointcutTest1.Throwing",Pointcut1Controller.testResult[5]);
            Assert.Equal("PointcutTest2.Throwing",Pointcut1Controller.testResult[6]);
        }
        
        
        [Fact]
        public void Test_Type_09()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(UnitTest6).Assembly).SetAllowCircularDependencies(true));

            var container = builder.Build();
                
            Pointcut2Controller.testResult = new List<string>();
            var a1 = container.Resolve<Pointcut2Controller>();
            Assert.Throws<ArgumentException>(() => a1.TestThrow());
            Assert.Equal(7,Pointcut2Controller.testResult.Count);
            Assert.Equal("PointcutTest2.Around-start",Pointcut2Controller.testResult[0]);
            Assert.Equal("PointcutTest2.Before",Pointcut2Controller.testResult[1]);
            Assert.Equal("PointcutTest1.Around-start",Pointcut2Controller.testResult[2]);
            Assert.Equal("PointcutTest1.Before",Pointcut2Controller.testResult[3]);
            Assert.Equal("Pointcut2Controller.TestThrow",Pointcut2Controller.testResult[4]);
            Assert.Equal("PointcutTest1.Throwing",Pointcut2Controller.testResult[5]);
            Assert.Equal("PointcutTest2.Throwing",Pointcut2Controller.testResult[6]);
        }
        
        [Fact]
        public void Test_Type_10()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(UnitTest6).Assembly).SetAllowCircularDependencies(true));

            var container = builder.Build();
                
            var a1 = container.Resolve<PointcutAnotationTest1>();
            PointcutAnotationTest1.testResult = new List<string>();
            a1.TestSuccess();
            Assert.Equal(13,PointcutAnotationTest1.testResult.Count);
            Assert.Equal("PointcutTest2.Around-start",PointcutAnotationTest1.testResult[0]);
            Assert.Equal("PointcutTest2.Before",PointcutAnotationTest1.testResult[1]);
            Assert.Equal("PointcutTest4.Around-start",PointcutAnotationTest1.testResult[2]);
            Assert.Equal("PointcutTest1.Around-start",PointcutAnotationTest1.testResult[3]);
            Assert.Equal("PointcutTest1.Before",PointcutAnotationTest1.testResult[4]);
            Assert.Equal("PointcutTest3.Around-start",PointcutAnotationTest1.testResult[5]);
            Assert.Equal("PointcutAnotationTest1.TestSuccess",PointcutAnotationTest1.testResult[6]);
            Assert.Equal("PointcutTest3.Around-end",PointcutAnotationTest1.testResult[7]);
            Assert.Equal("PointcutTest1.Around-end",PointcutAnotationTest1.testResult[8]);
            Assert.Equal("PointcutTest1.After",PointcutAnotationTest1.testResult[9]);
            Assert.Equal("PointcutTest4.Around-end",PointcutAnotationTest1.testResult[10]);
            Assert.Equal("PointcutTest2.Around-end",PointcutAnotationTest1.testResult[11]);
            Assert.Equal("PointcutTest2.After",PointcutAnotationTest1.testResult[12]);
            
        }
        
        [Fact]
        public void Test_Type_11()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(UnitTest6).Assembly).SetAllowCircularDependencies(true));

            var container = builder.Build();
                
            var a1 = container.Resolve<PointcutAnotationTest1>();
            PointcutAnotationTest1.testResult = new List<string>();
            Assert.Throws<ArgumentException>(() => a1.TestThrow());
            Assert.Equal(9,PointcutAnotationTest1.testResult.Count);
            Assert.Equal("PointcutTest2.Around-start",PointcutAnotationTest1.testResult[0]);
            Assert.Equal("PointcutTest2.Before",PointcutAnotationTest1.testResult[1]);
            Assert.Equal("PointcutTest4.Around-start",PointcutAnotationTest1.testResult[2]);
            Assert.Equal("PointcutTest1.Around-start",PointcutAnotationTest1.testResult[3]);
            Assert.Equal("PointcutTest1.Before",PointcutAnotationTest1.testResult[4]);
            Assert.Equal("PointcutTest3.Around-start",PointcutAnotationTest1.testResult[5]);
            Assert.Equal("PointcutAnotationTest1.TestThrow",PointcutAnotationTest1.testResult[6]);
            Assert.Equal("PointcutTest1.Throwing",PointcutAnotationTest1.testResult[7]);
            Assert.Equal("PointcutTest2.Throwing",PointcutAnotationTest1.testResult[8]);
            
        }
        
        [Fact]
        public async Task Test_Type_12()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(UnitTest6).Assembly).SetAllowCircularDependencies(true));

            var container = builder.Build();
                
            var ddd = container.Resolve<PointcutAnotationTest3>();

            var r2 = await ddd.Test("ddd");
            
            Assert.Equal("PointcutTest2.Around-start",PointcutAnotationTest1.testResult[0]);
        }
        
        [Fact]
        public async Task Test_Type_13()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(UnitTest6).Assembly).SetAllowCircularDependencies(true));

            var container = builder.Build();
                
            var ddd = container.Resolve<ValueTaskAnotationTest4>();

            var r2 = await ddd.Test("ddd");
             await ddd.Test2("ddd");
            
        }
    }
}