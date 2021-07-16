using System.Threading.Tasks;

namespace Autofac.Events
{
    /// <summary>
    /// Handle an event asynchronously
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public interface IHandleEventAsync<in TEvent>
    {
        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        Task HandleAsync(TEvent @event);
    }

    /// <summary>
    /// Handle an event and return with response of T
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    /// <typeparam name="T"></typeparam>
    public interface IReturnEventAsync<in TEvent, T>
    {
        /// <summary>
        /// 接收消息 并处理返回值
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        Task<T> HandleAsync(TEvent @event);
    }
}