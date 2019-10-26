using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Events
{
    /// <summary>
    /// Handle an event
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public interface IHandleEvent<in TEvent>
    {
        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="event"></param>
        void Handle(TEvent @event);
    }
}