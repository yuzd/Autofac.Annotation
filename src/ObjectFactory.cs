using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using AspectCore.Extensions.Reflection;
using Autofac.Core;

namespace Autofac.Annotation
{
    /// <summary>
    /// 获取Bean的泛型接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ObjectFactory<T> : IObjectFactory
    {
        /// <summary>
        /// Bean工厂
        /// </summary>
        T GetObject();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AutowiredObjectFactory<T> : ObjectFactory<T>
    {
        /// <summary>
        /// 获取value的包装
        /// </summary>
        /// <returns></returns>
        internal Func<object> function;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="function"></param>
        public AutowiredObjectFactory(Func<object> function)
        {
            this.function = function;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public T GetObject()
        {
            return (T) function();
        }
    }

    internal class LazyAutowiredFactory<T>
    {
        /// <summary>
        /// 获取value的包装
        /// </summary>
        /// <returns></returns>
        internal Func<object> function;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="function"></param>
        public LazyAutowiredFactory(Func<object> function)
        {
            this.function = function;
        }

        public Lazy<T> CreateLazy()
        {
            return new Lazy<T>(() => (T) function());
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IObjectFactory
    {
    }

    /// <summary>
    /// 获取Value的泛型接口 保证每次获取到最新的
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IValue<T> : IObjectFactory
    {
        /// <summary>
        /// 包装
        /// </summary>
        T Value { get; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ValueObjectFactory<T> : IValue<T>
    {
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="function"></param>
        public ValueObjectFactory(Func<object> function)
        {
            this.GetObject = function;
        }

        /// <summary>
        /// 获取value的包装
        /// </summary>
        /// <returns></returns>
        internal Func<object> GetObject;

        /// <summary>
        /// 
        /// </summary>
        public T Value => (T) GetObject();
    }

    /// <summary>
    /// IValue工厂
    /// </summary>
    [Component(AutofacScope = AutofacScope.SingleInstance, AutoActivate = true, NotUseProxy = true)]
    public sealed class ObjectBeanFactory
    {
        private readonly IComponentContext _context;

        /// <summary>
        /// 存储lazy的 CreateLazy 的方法缓存
        /// </summary>
        private readonly ConcurrentDictionary<Type, MethodReflector> _lazyMethodCache = new ConcurrentDictionary<Type, MethodReflector>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public ObjectBeanFactory(IComponentContext context)
        {
            _context = context;
        }

        /// <summary>
        ///  创建Value的包装器
        /// </summary>
        /// <param name="value"></param>
        /// <param name="memberType"></param>
        /// <param name="classType"></param>
        /// <param name="parameterInfo"></param>
        /// <param name="autoConfigurationDetail"></param>
        /// <returns></returns>
        internal object CreateValueFactory(Value value, Type memberType, Type classType, string parameterInfo, AutoConfigurationDetail autoConfigurationDetail)
        {
            var targetType = memberType.GenericTypeArguments[0];
            var valueType = typeof(ValueObjectFactory<>);
            var valueFactoryType = valueType.MakeGenericType(targetType);
            Func<object> function = () => value.Resolve(_context, classType, targetType, parameterInfo, autoConfigurationDetail);
            return Activator.CreateInstance(valueFactoryType, new object[] {function});
        }


        // /// <summary>
        // ///  创建Autowired的包装器
        // /// </summary>
        // /// <param name="autowired"></param>
        // /// <param name="type"></param>
        // /// <param name="classType"></param>
        // /// <param name="fieldOrPropertyName"></param>
        // /// <param name="typeDescription"></param>
        // /// <returns></returns>
        // public object CreateAutowiredFactory(Autowired autowired, Type type, Type classType,string fieldOrPropertyName, string typeDescription)
        // {
        //     var targetType = type.GenericTypeArguments[0];
        //     var valueType = typeof(AutowiredObjectFactory<>);
        //     var valueFactoryType = valueType.MakeGenericType(targetType);
        //     Func<object> function = () => autowired.Resolve(classType, targetType,fieldOrPropertyName, _context, typeDescription);
        //     return Activator.CreateInstance(valueFactoryType, new object[] {function});
        // }

        /// <summary>
        /// 创建Autowired的包装器
        /// </summary>
        /// <param name="autowired"></param>
        /// <param name="type"></param>
        /// <param name="classType"></param>
        /// <param name="fieldOrPropertyName"></param>
        /// <param name="instance"></param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        internal object CreateAutowiredFactory(Autowired autowired, Type type, Type classType, string fieldOrPropertyName, object instance,
            List<Parameter> Parameters)
        {
            var targetType = type.GenericTypeArguments[0];
            var valueType = typeof(AutowiredObjectFactory<>);
            var valueFactoryType = valueType.MakeGenericType(targetType);
            Func<object> function = () => autowired.Resolve(_context, instance, classType, targetType, fieldOrPropertyName, Parameters);
            return Activator.CreateInstance(valueFactoryType, new object[] {function});
        }

        /// <summary>
        /// 创建Lazy的包装器
        /// </summary>
        /// <param name="autowired"></param>
        /// <param name="type"></param>
        /// <param name="classType"></param>
        /// <param name="fieldOrPropertyName"></param>
        /// <param name="instance"></param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        internal object CreateLazyFactory(Autowired autowired, Type type, Type classType, string fieldOrPropertyName, object instance,
            List<Parameter> Parameters)
        {
            var targetType = type.GenericTypeArguments[0];
            var valueType = typeof(LazyAutowiredFactory<>);
            var valueFactoryType = valueType.MakeGenericType(targetType);
            Func<object> function = () => autowired.Resolve(_context, instance, classType, targetType, fieldOrPropertyName, Parameters);
            var lazyFactory = Activator.CreateInstance(valueFactoryType, new object[] {function});
            if (!this._lazyMethodCache.TryGetValue(valueFactoryType, out var _cache))
            {
                _cache = lazyFactory.GetType().GetTypeInfo().GetMethod("CreateLazy").GetReflector();
                this._lazyMethodCache.TryAdd(valueFactoryType, _cache);
            }

            return _cache.Invoke(lazyFactory);
        }
    }
}