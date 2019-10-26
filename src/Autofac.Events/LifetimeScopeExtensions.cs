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
                    handler.Handle((dynamic)@event);
                }
                catch (Exception exception)
                {
                    exceptions.Add(exception);
                }
            }
            foreach(dynamic asyncHandler in scope.ResolveAsyncHandlers(@event))
            {
                try
                {
                    asyncHandler.HandleAsync((dynamic)@event).Wait();
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
        /// <exception cref="AggregateException"></exception>
        public static async Task PublishEventAsync(this ILifetimeScope scope, object @event)
        {
            var exceptions = new List<Exception>();
            //TODO: Investigate synchronous vs. synchronous handling here, ConfigureAwait()?  Seems problematic given reliance on scope
            foreach (dynamic handler in scope.ResolveHandlers(@event))
            {
                try
                {
                    handler.Handle((dynamic)@event);
                }
                catch (Exception exception)
                {
                    exceptions.Add(exception);
                }
            }

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


        private static IEnumerable<dynamic> ResolveHandlers<TEvent>(this ILifetimeScope scope, TEvent @event)
        {
            var eventType = @event.GetType();
            return scope.ResolveConcreteHandlers(eventType, MakeHandlerType)
                .Union(scope.ResolveInterfaceHandlers(eventType, MakeHandlerType));
        }

        private static IEnumerable<dynamic> ResolveAsyncHandlers<TEvent>(this ILifetimeScope scope, TEvent @event)
        {
            var eventType = @event.GetType();
            return scope.ResolveConcreteHandlers(eventType, MakeAsyncHandlerType)
                .Union(scope.ResolveInterfaceHandlers(eventType, MakeAsyncHandlerType));
        }
        
        private static IEnumerable<dynamic> ResolveConcreteHandlers(this ILifetimeScope scope, Type eventType, Func<Type, Type> handlerFactory)
        {
            return (IEnumerable<dynamic>)scope.Resolve(handlerFactory(eventType));
        }

        private static IEnumerable<dynamic> ResolveInterfaceHandlers(this ILifetimeScope scope, Type eventType, Func<Type, Type> handlerFactory)
        {
            return eventType.GetTypeInfo().ImplementedInterfaces.SelectMany(i => (IEnumerable<dynamic>)scope.Resolve(handlerFactory(i))).Distinct();
        }

        private static Type MakeHandlerType(Type type)
        {
            return typeof(IEnumerable<>).MakeGenericType(typeof(IHandleEvent<>).MakeGenericType(type));
        }

        private static Type MakeAsyncHandlerType(Type type)
        {
            return typeof(IEnumerable<>).MakeGenericType(typeof(IHandleEventAsync<>).MakeGenericType(type));
        }
    }
}