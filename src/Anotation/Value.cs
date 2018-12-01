using AspectCore.Extensions.Reflection;
using Autofac.Annotation.Util;
using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Reflection;
using Autofac.Core;

namespace Autofac.Annotation
{
    /// <summary>
    /// 注入值
    /// 只能打一个标签 可以继承父类
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Field)]
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
            return parameter == null ? null : Resolve(parameter.Member.DeclaringType, parameter.ParameterType, null, parameter);
        }

        /// <summary>
        /// 注入Property的值
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public object ResolveProperty(PropertyInfo parameter, IComponentContext context)
        {
            return parameter == null ? null : Resolve(parameter.DeclaringType, parameter.PropertyType, parameter);
        }

        /// <summary>
        /// 注入Filed值
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public object ResolveFiled(FieldInfo parameter, IComponentContext context)
        {
            return parameter == null ? null : Resolve(parameter.DeclaringType, parameter.FieldType, parameter);
        }

        /// <summary>
        /// 对于memberInfo 或者  parameterInfo 进行设值
        /// </summary>
        /// <param name="classType"></param>
        /// <param name="memberType"></param>
        /// <param name="memberInfo"></param>
        /// <param name="parameterInfo"></param>
        /// <returns></returns>
        private object Resolve(Type classType, Type memberType, MemberInfo memberInfo, ParameterInfo parameterInfo = null)
        {
            if (classType == null) return null;
            if (string.IsNullOrEmpty(this.value)) return null;
            try
            {
                if (!this.value.StartsWith("#{") || !this.value.EndsWith("}"))
                {
                    var parameterValue = this.value;
                    var parseValue = parameterInfo == null
                        ? TypeManipulation.ChangeToCompatibleType(parameterValue, memberType, memberInfo)
                        : TypeManipulation.ChangeToCompatibleType(parameterValue, memberType, parameterInfo);
                    return parseValue;
                }
                else
                {
                    var key = this.value.Substring(2, this.value.Length - 3)?.Trim();
                    
                    if (AutofacAnnotationModule.ComponentModelCache.TryGetValue(classType, out var component))
                    {
                        foreach (var metaSource in component.MetaSourceList)
                        {
                            if (metaSource.Configuration == null)
                            {
                                continue;
                            }
                            IConfigurationSection metData = metaSource.Configuration.GetSection(key);
                            var parameterValue = ConfigurationUtil.GetConfiguredParameterValue(metData);

                            if (parameterValue == null)
                            {
                                //表示key不存在 从下一个source里面去寻找
                                continue;
                            }

                            var parseValue = parameterInfo == null
                                ? TypeManipulation.ChangeToCompatibleType(parameterValue, memberType, memberInfo)
                                : TypeManipulation.ChangeToCompatibleType(parameterValue, memberType, parameterInfo);
                            return parseValue;
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new DependencyResolutionException($"Value set error,can not resolve class type:{classType.FullName} =====>" +
                                                        $" {(parameterInfo == null?memberType.Name:parameterInfo.Name)} "
                                                        + (!string.IsNullOrEmpty(this.value) ? $",with value:[{this.value}]" : ""),ex);
            }
        }
    }
}