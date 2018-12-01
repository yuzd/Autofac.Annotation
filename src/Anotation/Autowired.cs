using AspectCore.Extensions.Reflection;
using Autofac.Core;
using Autofac.Features.AttributeFilters;
using System;
using System.Reflection;

namespace Autofac.Annotation
{
    /// <summary>
    /// 注入属性或者字段
    /// 只能打一个标签 可以继承父类
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class Autowired : ParameterFilterAttribute
    {
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
        /// <param name="parameter"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public object ResolveField(FieldInfo parameter, IComponentContext context)
        {
            return parameter == null ? null : Resolve(parameter.DeclaringType, parameter.FieldType, context, "field");
        }

        /// <summary>
        /// 装配属性
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public object ResolveProperty(PropertyInfo parameter, IComponentContext context)
        {
            return parameter == null ? null : Resolve(parameter.DeclaringType, parameter.PropertyType, context, "property");
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