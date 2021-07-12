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
    }
}