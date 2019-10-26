using Xunit;

namespace Autofac.Annotation.Test.test5
{
    public class EventBusTest
    {
        [Fact]
        public void Test_Type_01()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(WorkPublisher).Assembly));

            var ioc = builder.Build();

            var a1 = ioc.Resolve<WorkPublisher>();

            Assert.NotNull(a1);
            a1.EventPublisher.Publish(new WorkModel1());
            

        }
    }
}