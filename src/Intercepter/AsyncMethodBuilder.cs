// Copyright (c) 2020 stakx
// License available at https://github.com/stakx/DynamicProxy.AsyncInterceptor/blob/master/LICENSE.md.

using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Castle.DynamicProxy
{
    internal static class AsyncMethodBuilder
    {
        public static object TryCreate(Type returnType)
        {
            var builderType = GetAsyncMethodBuilderType(returnType);
            if (builderType != null)
            {
                var createMethod = builderType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
                var builder = createMethod.Invoke(null, null);
                return builder;
            }
            else
            {
                return null;
            }
        }

        private static Type GetAsyncMethodBuilderType(Type returnType)
        {
            var asyncMethodBuilderAttribute =
                (AsyncMethodBuilderAttribute)Attribute.GetCustomAttribute(returnType, typeof(AsyncMethodBuilderAttribute), inherit: false);
            if (asyncMethodBuilderAttribute != null)
            {
                var builderType = asyncMethodBuilderAttribute.BuilderType;
                if (builderType.IsGenericTypeDefinition)
                {
                    Debug.Assert(returnType.IsConstructedGenericType);
                    return builderType.MakeGenericType(returnType.GetGenericArguments());
                }
                else
                {
                    return builderType;
                }
            }
            else if (returnType == typeof(ValueTask))
            {
                return typeof(AsyncValueTaskMethodBuilder);
            }
            else if (returnType == typeof(Task))
            {
                return typeof(AsyncTaskMethodBuilder);
            }
            else if (returnType.IsGenericType)
            {
                var returnTypeDefinition = returnType.GetGenericTypeDefinition();
                if (returnTypeDefinition == typeof(ValueTask<>))
                {
                    return typeof(AsyncValueTaskMethodBuilder<>).MakeGenericType(returnType.GetGenericArguments()[0]);
                }
                else if (returnTypeDefinition == typeof(Task<>))
                {
                    return typeof(AsyncTaskMethodBuilder<>).MakeGenericType(returnType.GetGenericArguments()[0]);
                }
            }

            // NOTE: `AsyncVoidMethodBuilder` is intentionally excluded here because we want to end up in a synchronous
            // `Intercept` callback for non-awaitable methods.
            return null;
        }

        public static void AwaitOnCompleted(this object builder, object awaiter, object stateMachine)
        {
            var awaitOnCompletedMethod = builder.GetType().GetMethod("AwaitOnCompleted", BindingFlags.Public | BindingFlags.Instance)
                .MakeGenericMethod(awaiter.GetType(), stateMachine.GetType());
            awaitOnCompletedMethod.Invoke(builder, new object[] { awaiter, stateMachine });
        }

        public static void SetException(this object builder, Exception exception)
        {
            var setExceptionMethod = builder.GetType().GetMethod("SetException", BindingFlags.Public | BindingFlags.Instance);
            setExceptionMethod.Invoke(builder, new object[] { exception });
        }

        public static void SetResult(this object builder, object result)
        {
            var setResultMethod = builder.GetType().GetMethod("SetResult", BindingFlags.Public | BindingFlags.Instance);
            if (setResultMethod.GetParameters().Length == 0)
            {
                setResultMethod.Invoke(builder, null);
            }
            else
            {
                setResultMethod.Invoke(builder, new object[] { result });
            }
        }

        public static void Start(this object builder, object stateMachine)
        {
            var startMethod = builder.GetType().GetMethod("Start", BindingFlags.Public | BindingFlags.Instance).MakeGenericMethod(stateMachine.GetType());
            startMethod.Invoke(builder, new object[] { stateMachine });
        }

        public static object Task(this object builder)
        {
            var taskProperty = builder.GetType().GetProperty("Task", BindingFlags.Public | BindingFlags.Instance);
            return taskProperty.GetValue(builder);
        }
    }
}