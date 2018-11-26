using Autofac.Annotation.Util;
using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Reflection;

namespace Autofac.Annotation
{
    /// <summary>
    /// 注入值
    /// 只能打一个标签 可以继承父类
    /// </summary>
    [AttributeUsage(AttributeTargets.Property| AttributeTargets.Parameter| AttributeTargets.Field)]
    public class Value : ParameterFilterAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_value"></param>
        public Value(string _value)
        {
            value = _value;
        }

        /// <summary>
        /// 对应的值
        /// </summary>
        public string value { get; set; }

        /// <summary>
        /// 注入
        /// 只能支持值类型string int boolean dic list
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override object ResolveParameter(ParameterInfo parameter, IComponentContext context)
        {
            var valueAttr = parameter.GetCustomAttribute<Value>();
            if (valueAttr == null)
            {
                var prop = (PropertyInfo)null;
                parameter.TryGetDeclaringProperty(out prop);
                if (prop == null)
                {
                    return null;
                }
                else
                {
                    valueAttr = prop.GetCustomAttribute<Value>();
                    if (valueAttr == null)
                    {
                        return null;
                    }
                }
            }
            if (string.IsNullOrEmpty(valueAttr.value)) return null;
            if (!valueAttr.value.StartsWith("#{") || !valueAttr.value.EndsWith("}"))
            {
                var parameterValue = valueAttr.value;
                var parseValue = TypeManipulation.ChangeToCompatibleType(parameterValue, parameter.ParameterType, parameter);
                return parseValue;
            }
            else
            {
                var key = valueAttr.value.Substring(2, valueAttr.value.Length - 3)?.Trim();
                var classType = parameter.Member.DeclaringType;
                if (classType == null) return null;
                if (AutofacAnnotationModule.ComponentModelCache.TryGetValue(classType, out var component))
                {
                    if (component.MetaSourceList != null && component.MetaSourceList.Any())
                    {
                        foreach (var metaSource in component.MetaSourceList)
                        {
                            if (metaSource.Configuration != null)
                            {
                                IConfigurationSection metData = metaSource.Configuration.GetSection(key);
                                var parameterValue = ConfigurationUtil.GetConfiguredParameterValue(metData);
                                
                                if (parameterValue == null)
                                {
                                    //表示key不存在 从下一个source里面去寻找
                                    continue;
                                }
                                var parseValue = TypeManipulation.ChangeToCompatibleType(parameterValue, parameter.ParameterType, parameter);
                                return parseValue;
                            }
                        }
                    }
                }
            }
            return null;
        }

    }
}
