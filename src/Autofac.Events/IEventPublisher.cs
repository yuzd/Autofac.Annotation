using System;
using System.Collections.Generic;
using System.Text;

namespace Autofac.Events
{
    /// <summary>
    /// Publishes an event
    /// </summary>
    public interface IEventPublisher
    {
        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="event"></param>
        void Publish(object @event);
        
        
        /// <summary>
        /// 发布消息并且拿到返回值
        /// </summary>
        /// <param name="event"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        List<T> Send<T>(object @event);
    }
}