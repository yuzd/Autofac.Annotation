using System;
using System.Threading.Tasks;
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
    public class WorkReturnListener2:IReturnEvent<WorkModel1,WorkReturnListener2Model>
    {
        [Autowired]
        public School School { get; set; }
        public WorkReturnListener2Model Handle(WorkModel1 @event)
        {
            return new WorkReturnListener2Model
            {
                Name = @event.Name + School.Name
            };
        }
    }
    
    [Component]
    public class AsyncWorkListener2:IHandleEventAsync<WorkModel1>
    {
        [Autowired]
        public School School { get; set; }
        public async Task HandleAsync(WorkModel1 @event)
        {
            Console.WriteLine(@event.Name + School.Name);
            await Task.Delay(1000);
        }
    }
    
    [Component]
    public class AsyncWorkReturnListener2:IReturnEventAsync<WorkModel1,WorkReturnListener2Model>
    {
        [Autowired]
        public School School { get; set; }

        public async Task<WorkReturnListener2Model> HandleAsync(WorkModel1 @event)
        {
            return await Task.FromResult(new WorkReturnListener2Model
            {
                Name = @event.Name + School.Name
            });
        }
    }

    [Component]
    public class WorkPublisher
    {
        [Autowired]
        public IEventPublisher EventPublisher { get; set; }
        
        [Autowired]
        public IAsyncEventPublisher AsyncEventPublisher { get; set; }
    }

    public class WorkReturnListener2Model
    {
        public string Name { get; set; }
    }
}