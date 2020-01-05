using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Autofac.Events
{
    /// <summary>
    /// 消息发送器
    /// </summary>
    public class EventPublisher : IEventPublisher, IAsyncEventPublisher
    {
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="scope"></param>
        public EventPublisher(ILifetimeScope scope)
        {
            _scope = scope;
        }

        private readonly ILifetimeScope _scope;
        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="event"></param>

        public void Publish(object @event)
        {
            _scope.PublishEvent(@event);
        }
        
        /// <summary>
        /// 发布消息并且拿到返回值
        /// </summary>
        /// <param name="event"></param>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <returns></returns>
        public List<T> Send<T>(object @event)
        {
            return _scope.SendEvent<T>(@event);
        }
        
        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public Task PublishAsync(object @event)
        {
            return _scope.PublishEventAsync(@event);
        }

        /// <summary>
        /// 发布消息并且拿到返回值
        /// </summary>
        /// <param name="event"></param>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <returns></returns>
        public Task<List<T>> SendAsync<T>(object @event)
        {
            return _scope.SendEventAsync<T>(@event);
        }
    }
}