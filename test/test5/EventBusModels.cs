using System;
using Autofac.Events;

namespace Autofac.Annotation.Test.test5
{
    public class WorkModel1
    {
        public string Name { get; set; } = nameof(WorkModel1);
    }
    
    [Component]
    public class WorkListener1:IHandleEvent<WorkModel1>
    {
        public void Handle(WorkModel1 @event)
        {
            Console.WriteLine(@event.Name);
        }
    }
    
    [Component]
    public class WorkListener2:IHandleEvent<WorkModel1>
    {
        public void Handle(WorkModel1 @event)
        {
            Console.WriteLine(@event.Name);
        }
    }

    [Component]
    public class WorkPublisher
    {
        [Autowired]
        public IEventPublisher EventPublisher { get; set; }
    }
}