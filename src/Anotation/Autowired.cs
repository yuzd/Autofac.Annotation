using AspectCore.Extensions.Reflection;
using Autofac.Core;
using Autofac.Features.AttributeFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac.Annotation.Util;

namespace Autofac.Annotation
{
    /// <summary>
    /// 注入属性或者字段
    /// 只能打一个标签 可以继承父类
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class Autowired : ParameterFilterAttribute
    {
        //     
        /// <summary>
        /// 默认的
        /// </summary>
        public Autowired()
        {
        }

        /// <summary>
        /// 按照名称来注入
        /// </summary>
        /// <param name="name"></param>
        public Autowired(string name)
        {
            Name = name;
        }

        /// <summary>
        /// 设置是否装载失败报错
        /// </summary>
        /// <param name="required"></param>
        public Autowired(bool required)
        {
            Required = required;
        }

        /// <summary>
        /// 对应的值
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 默认装载失败会报错 设置为false装载失败不会报错
        /// </summary>
        public bool Required { get; set; } = true;

        /// <summary>
        /// 作为ParameterInfo自动装载
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override object ResolveParameter(ParameterInfo parameter, IComponentContext context)
        {
            return parameter == null ? null : Resolve(parameter.Member.DeclaringType, parameter.ParameterType, context, "parameter");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override bool CanResolveParameter(ParameterInfo parameter, IComponentContext context)
        {
            return true;
        }

        /// <summary>
        /// 装配字段
        /// </summary>
        /// <param name="property"></param>
        /// <param name="context"></param>
        /// <param name="Parameters"></param>
        /// <param name="instance"></param>
        /// <param name="allowCircle"></param>
        /// <returns></returns>
        public object ResolveField(FieldInfo property, IComponentContext context, IEnumerable<Parameter> Parameters, object instance, bool allowCircle)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            if (!allowCircle) return Resolve(property.DeclaringType, property.FieldType, context, "field");
            return Resolve(context, instance, property.DeclaringType, property.FieldType, Parameters);
        }

        /// <summary>
        /// 装配属性
        /// </summary>
        /// <param name="property"></param>
        /// <param name="context"></param>
        /// <param name="Parameters"></param>
        /// <param name="instance"></param>
        /// <param name="alowCircle"></param>
        /// <returns></returns>
        public object ResolveProperty(PropertyInfo property, IComponentContext context, IEnumerable<Parameter> Parameters, object instance, bool alowCircle)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            if (!alowCircle) return Resolve(property.DeclaringType, property.PropertyType, context, "property");
            return Resolve(context, instance, property.DeclaringType, property.PropertyType, Parameters);
        }

        internal object Resolve(IComponentContext context, object instance, Type classType, Type memberType, IEnumerable<Parameter> Parameters)
        {
            if ((typeof(IObjectFactory).IsAssignableFrom(memberType)))
            {
                return context.Resolve<ObjectBeanFactory>().CreateAutowiredFactory(this, memberType, classType, instance, Parameters);
            }

            Service propertyService = null;
            if (!string.IsNullOrEmpty(this.Name))
            {
                propertyService = new KeyedService(this.Name, memberType);
            }
            else
            {
                propertyService = new TypedService(memberType);
            }

            // ReSharper disable once PossibleMultipleEnumeration
            if (Parameters != null && Parameters.Count() == 1)
            {
                // ReSharper disable once PossibleMultipleEnumeration
                if (!(Parameters.First() is AutowiredParmeter AutowiredParmeter))
                {
                    return null;
                }

                // ReSharper disable once AssignNullToNotNullAttribute
                if (AutowiredParmeter.TryGet(getAutowiredParmeterKey(memberType), out var objectInstance))
                {
                    return objectInstance;
                }
                else
                {
                    // ReSharper disable once PossibleNullReferenceException
                    AutowiredParmeter.TryAdd(getAutowiredParmeterKey(classType), instance);
                    if (context.TryResolveService(propertyService, new Parameter[] {AutowiredParmeter}, out var propertyValue))
                    {
                        return propertyValue;
                    }
                }
            }
            else
            {
                var instanceTypeParameter = new AutowiredParmeter();
                // ReSharper disable once PossibleNullReferenceException
                instanceTypeParameter.TryAdd(getAutowiredParmeterKey(classType), instance);
                if (context.TryResolveService(propertyService, new Parameter[] {instanceTypeParameter}, out var propertyValue))
                {
                    return propertyValue;
                }
            }

            return null;
        }

        /// <summary>
        /// 装配
        /// </summary>
        /// <param name="classType"></param>
        /// <param name="type"></param>
        /// <param name="context"></param>
        /// <param name="typeDescription"></param>
        /// <returns></returns>
        /// <exception cref="DependencyResolutionException"></exception>
        internal object Resolve(Type classType, Type type, IComponentContext context, string typeDescription)
        {
            if ((typeof(IObjectFactory).IsAssignableFrom(type)))
            {
                return context.Resolve<ObjectBeanFactory>().CreateAutowiredFactory(this, type, classType, typeDescription);
            }

            object obj = null;
            if (!string.IsNullOrEmpty(this.Name))
            {
                context.TryResolveKeyed(this.Name, type, out obj);
            }
            else
            {
                if (type.IsGenericEnumerableInterfaceType())
                {
                    var genericType = type.GenericTypeArguments[0];
                    if (genericType.FullName != null && genericType.FullName.StartsWith("System.Lazy`1"))
                    {
                        genericType = genericType.GenericTypeArguments[0];
                    }

                    context.TryResolveKeyed("`1System.Collections.Generic.IEnumerable`1" + genericType.FullName, type, out obj);
                }
                else
                {
                    context.TryResolve(type, out obj);
                }
            }

            if (obj == null && this.Required)
            {
                throw new DependencyResolutionException($"Autowire error,can not resolve class type:{classType.FullName},${typeDescription} name:{type.Name} "
                                                        + (!string.IsNullOrEmpty(this.Name) ? $",with key:[{this.Name}]" : ""));
            }

            return obj;
        }

        private static string getAutowiredParmeterKey(Type type)
        {
            return "`1CircularDependencies`1" + type.FullName;
        }
    }
}