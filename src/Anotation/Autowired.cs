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
            var autowired = parameter.GetCustomAttribute<Autowired>();
            if (autowired == null)
            {
                return null;
            }
            object obj = null;
            if (!string.IsNullOrEmpty(autowired.Name))
            {
                context.TryResolveKeyed(autowired.Name, parameter.ParameterType, out obj);
            }
            else
            {
                context.TryResolve(parameter.ParameterType, out obj);
            }
            if (obj == null && autowired.Required)
            {
                throw new DependencyResolutionException($"can not resolve type:{parameter.ParameterType.FullName} " + (!string.IsNullOrEmpty(autowired.Name) ? $" with key:{autowired.Name}" : ""));
            }
            return obj;
        }
    }
}
