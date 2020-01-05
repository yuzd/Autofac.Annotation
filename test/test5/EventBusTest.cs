using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Autofac.Annotation.Test.test5
{
    public class EventBusTest
    {
        [Fact]
        public async Task Test_Type_01()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(WorkPublisher).Assembly));

            var ioc = builder.Build();

            var a1 = ioc.Resolve<WorkPublisher>();

            Assert.NotNull(a1);
            a1.EventPublisher.Publish(new WorkModel1());

            List<WorkReturnListener2Model> sendResult = a1.EventPublisher.Publish<WorkReturnListener2Model>(new WorkModel1());

            await a1.AsyncEventPublisher.PublishAsync(new WorkModel1());
            
            List<WorkReturnListener2Model> sendAsyncResult = await a1.AsyncEventPublisher.PublishAsync<WorkReturnListener2Model>(new WorkModel1());
        }
    }
}