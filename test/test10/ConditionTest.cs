using System;
using System.Runtime.InteropServices;
using Xunit;

namespace Autofac.Annotation.Test.test10
{
    public class ConditionTest
    {
        [Fact]
        public void Test1()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return;
            }

            var builder = new ContainerBuilder();
            builder.RegisterSpring(r => r.RegisterAssembly(typeof(ConditionTest).Assembly));
            var container = builder.Build();
            //mac系统下不被注册
            var isRegisterd = container.TryResolve(out Test10Model1 model1);
            Assert.False(isRegisterd);
        }
        
        [Fact]
        public void Test2()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return;
            }

            var builder = new ContainerBuilder();
            builder.RegisterSpring(r => r.RegisterAssembly(typeof(ConditionTest).Assembly));
            var container = builder.Build();
            //mac系统下被注册
            var isRegisterd = container.TryResolve(out Test10Model2 model1);
            Assert.True(isRegisterd);
        }
        
        [Fact]
        public void Test3()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return;
            }

            var builder = new ContainerBuilder();
            builder.RegisterSpring(r => r.RegisterAssembly(typeof(ConditionTest).Assembly));
            var container = builder.Build();
            //mac系统下被注册
            var isRegisterd = container.TryResolve(out Test10Model3 model1);
            Assert.True(isRegisterd);
        }
        
        [Fact]
        public void Test4()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return;
            }

            var builder = new ContainerBuilder();
            builder.RegisterSpring(r => r.RegisterAssembly(typeof(ConditionTest).Assembly));
            var container = builder.Build();
            //mac系统下不被注册
            var isRegisterd = container.TryResolve(out Test10Model4 model1);
            Assert.False(isRegisterd);
        }
        
        [Fact]
        public void Test5()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return;
            }

            var builder = new ContainerBuilder();
            builder.RegisterSpring(r => r.RegisterAssembly(typeof(ConditionTest).Assembly));
            var container = builder.Build();
            //mac系统下被注册
            var isRegisterd = container.TryResolve(out Test10Model5 model1);
            Assert.True(isRegisterd);
        }
        
        [Fact]
        public void Test6()
        {

            var builder = new ContainerBuilder();
            builder.RegisterSpring(r => r.RegisterAssembly(typeof(ConditionTest).Assembly));
            var container = builder.Build();
            var type = Type.GetType("Autofac.Annotation.Test.test10.Test10Model2,Autofac.Configuration.Test");
            //mac系统下被注册
            var isRegisterd = container.TryResolve(out Test10Model6 model1);
            Assert.True(isRegisterd);
        }
        
        [Fact]
        public void Test7()
        {

            var builder = new ContainerBuilder();
            builder.RegisterSpring(r => r.RegisterAssembly(typeof(ConditionTest).Assembly));
            var container = builder.Build();
            //mac系统下被注册
            var isRegisterd = container.TryResolve(out Test10Model7 model1);
            Assert.True(isRegisterd);
        }
        
        [Fact]
        public void Test8()
        {
            var builder = new ContainerBuilder();
            builder.RegisterSpring(r => r.RegisterAssembly(typeof(ConditionTest).Assembly));
            var container = builder.Build();
            var isRegisterd = container.TryResolve(out Test10Model8 model1);
            Assert.True(isRegisterd);
        }
        
        [Fact]
        public void Test9()
        {
            var builder = new ContainerBuilder();
            builder.RegisterSpring(r => r.RegisterAssembly(typeof(ConditionTest).Assembly));
            var container = builder.Build();
            var isRegisterd = container.TryResolve(out Test10Model9 model1);
            Assert.False(isRegisterd);
        }
        
        [Fact]
        public void Test10()
        {
            var builder = new ContainerBuilder();
            builder.RegisterSpring(r => r.RegisterAssembly(typeof(ConditionTest).Assembly));
            var container = builder.Build();
            var isRegisterd = container.TryResolve(out Test10Model10 model1);
            Assert.True(isRegisterd);
        }
        
        [Fact]
        public void Test11()
        {
            var builder = new ContainerBuilder();
            builder.RegisterSpring(r => r.RegisterAssembly(typeof(ConditionTest).Assembly));
            var container = builder.Build();
            var isRegisterd = container.TryResolve(out Test10Model11 model1);
            Assert.False(isRegisterd);
        }
        
        [Fact]
        public void Test12()
        {
            var builder = new ContainerBuilder();
            builder.RegisterSpring(r => r.RegisterAssembly(typeof(ConditionTest).Assembly));
            var container = builder.Build();
            var isRegisterd = container.TryResolve(out AutoConfTest1 model1);
            var isRegisterd2 = container.TryResolve(out AutoConfTest12 model12);
            Assert.False(isRegisterd);
            Assert.False(isRegisterd2);
        }
    }
}