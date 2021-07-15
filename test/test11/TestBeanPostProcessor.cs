using Xunit;

namespace Autofac.Annotation.Test.test11
{

    /// <summary>
    /// 
    /// </summary>
    public class TestBeanPostProcessor
    {
        [Fact]
        public void Test1()
        {
            var builder = new ContainerBuilder();
            builder.RegisterSpring(r => r.RegisterAssembly(typeof(TestBeanPostProcessor).Assembly));
            var container = builder.Build();
            var isRegisterd = container.TryResolve(out Test11Models1 model1);
            Assert.True(isRegisterd);
            Assert.Equal("SoaTest1",model1.getSoa1());
            Assert.Equal("SoaTest2",model1.getSoa2());
        }

        
    }
}