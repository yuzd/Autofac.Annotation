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


    /// <summary>
    /// Handle an event and return with response of T
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    /// <typeparam name="T"></typeparam>
    public interface IReturnEvent<in TEvent, out T>
    {
        /// <summary>
        /// 接收消息 并处理返回值
        /// </summary>
        /// <param name="event"></param>
        T Handle(TEvent @event);
    }
}