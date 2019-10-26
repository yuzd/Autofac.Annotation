using System;
using Autofac.Events;

namespace Autofac.Annotation.Test.test5
{
    public class WorkModel1
    {
        public string Name { get; set; } = nameof(WorkModel1);
    }

    [Component]
    public class School
    {
        public string Name { get; set; } = nameof(School);
    }
    
    [Component]
    public class WorkListener1:IHandleEvent<WorkModel1>
    {
        [Autowired]
        public School School { get; set; }
        
        public void Handle(WorkModel1 @event)
        {
            Console.WriteLine(@event.Name + School.Name);
        }
    }
    
    [Component]
    public class WorkListener2:IHandleEvent<WorkModel1>
    {
        [Autowired]
        public School School { get; set; }
        public void Handle(WorkModel1 @event)
        {
            
            Console.WriteLine(@event.Name + School.Name);
        }
    }

    [Component]
    public class WorkPublisher
    {
        [Autowired]
        public IEventPublisher EventPublisher { get; set; }
    }
}