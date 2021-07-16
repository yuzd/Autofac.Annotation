using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac.Events;

// ReSharper disable once CheckNamespace
namespace Autofac
{
    /// <summary>
    /// 
    /// </summary>
    internal static class LifetimeScopeExtensions
    {
        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="event"></param>
        /// <exception cref="AggregateException"></exception>
        public static void PublishEvent(this ILifetimeScope scope, object @event)
        {
            var exceptions = new List<Exception>();
            foreach (dynamic handler in scope.ResolveHandlers(@event))
            {
                try
                {
                    handler.Handle((dynamic) @event);
                }
                catch (Exception exception)
                {
                    exceptions.Add(exception);
                }
            }

            if (exceptions.Count > 0)
                throw new AggregateException(exceptions);
        }

        /// <summary>
        /// 发布消息 如果接收消息处理器有返回值则拿到返回值
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="event"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> SendEvent<T>(this ILifetimeScope scope, object @event)
        {
            var exceptions = new List<Exception>();
            foreach (dynamic handler in scope.ResolveHandlers(@event))
            {
                try
                {
                    handler.Handle((dynamic) @event);
                }
                catch (Exception exception)
                {
                    exceptions.Add(exception);
                }
            }

            var returnType = typeof(T);
            var result = new List<T>();
            foreach (dynamic handler in scope.ResolveReturnHandlers(@event, returnType))
            {
                try
                {
                    result.Add((T) handler.Handle((dynamic) @event));
                }
                catch (Exception exception)
                {
                    exceptions.Add(exception);
                }
            }

            if (exceptions.Count > 0)
                throw new AggregateException(exceptions);

            return result;
        }

        /// <summary>
        /// 异步发布消息 如果接收消息处理器有返回值则拿到返回值
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="event"></param>
        /// <returns></returns>
        /// <exception cref="AggregateException"></exception>
        public static async Task PublishEventAsync(this ILifetimeScope scope, object @event)
        {
            var exceptions = new List<Exception>();
            foreach (dynamic asyncHandler in scope.ResolveAsyncHandlers(@event))
            {
                try
                {
                    await asyncHandler.HandleAsync((dynamic) @event);
                }
                catch (Exception exception)
                {
                    exceptions.Add(exception);
                }
            }

            if (exceptions.Count > 0)
                throw new AggregateException(exceptions);
        }

        /// <summary>
        /// 异步发布消息
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="event"></param>
        /// <returns></returns>
        public static async Task<List<T>> SendEventAsync<T>(this ILifetimeScope scope, object @event)
        {
            var exceptions = new List<Exception>();
            foreach (dynamic asyncHandler in scope.ResolveAsyncHandlers(@event))
            {
                try
                {
                    await asyncHandler.HandleAsync((dynamic) @event);
                }
                catch (Exception exception)
                {
                    exceptions.Add(exception);
                }
            }

            var returnType = typeof(T);
            var result = new List<T>();
            foreach (dynamic asyncHandler in scope.ResolveAsyncReturnHandlers(@event, returnType))
            {
                try
                {
                    result.Add((T) (await asyncHandler.HandleAsync((dynamic) @event)));
                }
                catch (Exception exception)
                {
                    exceptions.Add(exception);
                }
            }

            if (exceptions.Count > 0)
                throw new AggregateException(exceptions);
            return result;
        }

        /// <summary>
        /// 找到所有注册为 IHandleEvent《T》的所有类型  
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="event"></param>
        /// <typeparam name="TEvent"></typeparam>
        /// <returns></returns>
        private static IEnumerable<dynamic> ResolveHandlers<TEvent>(this ILifetimeScope scope, TEvent @event)
        {
            var eventType = @event.GetType();
            return scope.ResolveConcreteHandlers(eventType, MakeHandlerType)
                .Union(scope.ResolveInterfaceHandlers(eventType, MakeHandlerType));
        }

        /// <summary>
        /// 找到所有注册为 IReturnEvent《T》的所有类型  
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="event"></param>
        /// <param name="returnType"></param>
        /// <typeparam name="TEvent"></typeparam>
        /// <returns></returns>
        private static IEnumerable<dynamic> ResolveReturnHandlers<TEvent>(this ILifetimeScope scope, TEvent @event, Type returnType)
        {
            var eventType = @event.GetType();
            return scope.ResolveConcreteReturnHandlers(eventType, returnType, MakeReturnType)
                .Union(scope.ResolveInterfaceReturnHandlers(eventType, returnType, MakeReturnType));
        }

        /// <summary>
        /// 找到所有注册为 IHandleEventAsync《T》的所有类型
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="event"></param>
        /// <typeparam name="TEvent"></typeparam>
        /// <returns></returns>
        private static IEnumerable<dynamic> ResolveAsyncHandlers<TEvent>(this ILifetimeScope scope, TEvent @event)
        {
            var eventType = @event.GetType();
            return scope.ResolveConcreteHandlers(eventType, MakeAsyncHandlerType)
                .Union(scope.ResolveInterfaceHandlers(eventType, MakeAsyncHandlerType));
        }

        /// <summary>
        /// 找到所有注册为 IReturnEventAsync《T》的所有类型
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="event"></param>
        /// <param name="returnType"></param>
        /// <typeparam name="TEvent"></typeparam>
        /// <returns></returns>
        private static IEnumerable<dynamic> ResolveAsyncReturnHandlers<TEvent>(this ILifetimeScope scope, TEvent @event, Type returnType)
        {
            var eventType = @event.GetType();
            return scope.ResolveConcreteReturnHandlers(eventType, returnType, MakeAsyncReturnType)
                .Union(scope.ResolveInterfaceReturnHandlers(eventType, returnType, MakeAsyncReturnType));
        }

        private static IEnumerable<dynamic> ResolveConcreteHandlers(this ILifetimeScope scope, Type eventType, Func<Type, Type> handlerFactory)
        {
            return (IEnumerable<dynamic>) scope.Resolve(handlerFactory(eventType));
        }

        private static IEnumerable<dynamic> ResolveConcreteReturnHandlers(this ILifetimeScope scope, Type eventType, Type returnType,
            Func<Type, Type, Type> handlerFactory)
        {
            return (IEnumerable<dynamic>) scope.Resolve(handlerFactory(eventType, returnType));
        }

        private static IEnumerable<dynamic> ResolveInterfaceHandlers(this ILifetimeScope scope, Type eventType, Func<Type, Type> handlerFactory)
        {
            return eventType.GetTypeInfo().ImplementedInterfaces.SelectMany(i => (IEnumerable<dynamic>) scope.Resolve(handlerFactory(i))).Distinct();
        }

        private static IEnumerable<dynamic> ResolveInterfaceReturnHandlers(this ILifetimeScope scope, Type eventType, Type returnType,
            Func<Type, Type, Type> handlerFactory)
        {
            var interfaces = eventType.GetTypeInfo().ImplementedInterfaces;
            var tuplerList = (from iInterface in interfaces
                let inType = iInterface.GenericTypeArguments[0]
                let outType = iInterface.GenericTypeArguments[1]
                select new Tuple<Type, Type>(inType, outType)).ToList();
            return tuplerList.Select(i => (IEnumerable<dynamic>) scope.Resolve(handlerFactory(i.Item1, i.Item2))).Distinct();
        }

        private static Type MakeHandlerType(Type type)
        {
            return typeof(IEnumerable<>).MakeGenericType(typeof(IHandleEvent<>).MakeGenericType(type));
        }

        private static Type MakeReturnType(Type type, Type returnType)
        {
            return typeof(IEnumerable<>).MakeGenericType(typeof(IReturnEvent<,>).MakeGenericType(type, returnType));
        }

        private static Type MakeAsyncHandlerType(Type type)
        {
            return typeof(IEnumerable<>).MakeGenericType(typeof(IHandleEventAsync<>).MakeGenericType(type));
        }

        private static Type MakeAsyncReturnType(Type type, Type returnType)
        {
            return typeof(IEnumerable<>).MakeGenericType(typeof(IReturnEventAsync<,>).MakeGenericType(type, returnType));
        }
    }
}