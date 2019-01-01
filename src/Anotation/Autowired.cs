using AspectCore.Extensions.Reflection;
using Autofac.Core;
using Autofac.Features.AttributeFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac.Annotation.Util;
using Autofac.Features.Metadata;

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
        /// 装配字段
        /// </summary>
        /// <param name="property"></param>
        /// <param name="args"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public object ResolveField(FieldInfo property, IActivatedEventArgs<object> args,object instance)
        {
            //return parameter == null ? null : Resolve(parameter.DeclaringType, parameter.FieldType, context, "field");
            var context = args.Context;
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }
           
            
            Service propertyService = null;
            if (!string.IsNullOrEmpty(this.Name))
            {
                propertyService = new KeyedService(this.Name,property.FieldType);
            }
            else
            {
                propertyService = new TypedService(property.FieldType);
            }

            if (args.Parameters != null && args.Parameters.Count() == 1)
            {
                if (!(args.Parameters.First() is AutowiredParmeter AutowiredParmeter)) return null;
                if (AutowiredParmeter.AutowiredChains.TryGetValue(property.FieldType.FullName,out var objectInstance))
                {
                    return objectInstance;
                }
                else
                {
                    AutowiredParmeter.Add(property.DeclaringType.FullName,instance);
                    if (context.TryResolveService(propertyService, new Parameter[] { AutowiredParmeter }, out var propertyValue))
                    {
                        return propertyValue;
                    }
                }
            }
            else
            {
                var instanceTypeParameter = new AutowiredParmeter();
                instanceTypeParameter.Add(property.DeclaringType.FullName,instance);
                if (context.TryResolveService(propertyService, new Parameter[] { instanceTypeParameter }, out var propertyValue))
                {
                    return propertyValue;
                }
            }
            return null;
        }

        /// <summary>
        /// 装配属性
        /// </summary>
        /// <param name="property"></param>
        /// <param name="args"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public object ResolveProperty(PropertyInfo property, IActivatedEventArgs<object> args,object instance)
        {
            //return property == null ? null : Resolve(property.DeclaringType, property.PropertyType, args.Context, "property");
            
            var context = args.Context;
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }
           
            var instanceType = instance.GetType();

            
            Service propertyService = null;
            if (!string.IsNullOrEmpty(this.Name))
            {
                propertyService = new KeyedService(this.Name,property.PropertyType);
            }
            else
            {
                propertyService = new TypedService(property.PropertyType);
            }

            if (args.Parameters != null && args.Parameters.Count() == 1)
            {
                if (!(args.Parameters.First() is AutowiredParmeter AutowiredParmeter)) return null;
                if (AutowiredParmeter.AutowiredChains.TryGetValue(property.PropertyType.FullName,out var objectInstance))
                {
                    return objectInstance;
                }
                else
                {
                    AutowiredParmeter.Add(property.DeclaringType.FullName,instance);
                    if (context.TryResolveService(propertyService, new Parameter[] { AutowiredParmeter }, out var propertyValue))
                    {
                        return propertyValue;
                    }
                }
            }
            else
            {
                var instanceTypeParameter = new AutowiredParmeter();
                instanceTypeParameter.Add(property.DeclaringType.FullName,instance);
                if (context.TryResolveService(propertyService, new Parameter[] { instanceTypeParameter }, out var propertyValue))
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
        private object Resolve(Type classType, Type type, IComponentContext context, string typeDescription)
        {

            object obj = null;
            if (!string.IsNullOrEmpty(this.Name))
            {
                context.TryResolveKeyed(this.Name, type, out obj);
            }
            else
            {
                context.TryResolve(type, out obj);
            }

            if (obj == null && this.Required)
            {
                throw new DependencyResolutionException($"Autowire error,can not resolve class type:{classType.FullName},${typeDescription} name:{type.Name} "
                                                        + (!string.IsNullOrEmpty(this.Name) ? $",with key:[{this.Name}]" : ""));
            }

            return obj;
        }

    }
}