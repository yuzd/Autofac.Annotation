using AspectCore.Extensions.Reflection;
using Autofac.Annotation.Util;
using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
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
        /// The default placeholder prefix.
        /// </summary>
        public static readonly string DefaultPlaceholderPrefix = "${";

        /// <summary>
        /// The default placeholder suffix.
        /// </summary>
        public static readonly string DefaultPlaceholderSuffix = "}";


        /// <summary>
        /// 构造方法
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
        /// 如果拿不到是否报错
        /// </summary>
        public bool IgnoreUnresolvablePlaceholders { get; set; }

        /// <summary>
        /// 设置是否从环境变量拿 默认是从文件里面拿不到 就从环境里面去拿
        /// </summary>
        public EnvironmentVariableMode EnvironmentVariableMode { get; set; } = EnvironmentVariableMode.Fallback;

        /// <summary>
        /// 注入
        /// 只能支持值类型string int boolean dic list
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override object ResolveParameter(ParameterInfo parameter, IComponentContext context)
        {
            return parameter == null ? null : Resolve(context, parameter.Member.DeclaringType, parameter.ParameterType, null, parameter);
        }

        /// <summary>
        /// AutoConfiguration类的value注入
        /// </summary>
        /// <param name="detail"></param>
        /// <param name="parameter"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        internal object ResolveParameterWithConfiguration(AutoConfigurationDetail detail, ParameterInfo parameter, IComponentContext context)
        {
            return Resolve(context, parameter.Member.DeclaringType, parameter.ParameterType, null, parameter, detail);
        }

        /// <summary>
        /// 注入Property的值
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public object ResolveProperty(PropertyInfo parameter, IComponentContext context)
        {
            return parameter == null ? null : Resolve(context, parameter.DeclaringType, parameter.PropertyType, parameter);
        }

        /// <summary>
        /// 注入Filed值
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public object ResolveFiled(FieldInfo parameter, IComponentContext context)
        {
            return parameter == null ? null : Resolve(context, parameter.DeclaringType, parameter.FieldType, parameter);
        }

        /// <summary>
        /// 对于memberInfo 或者  parameterInfo 进行设值
        /// </summary>
        /// <param name="context"></param>
        /// <param name="classType"></param>
        /// <param name="memberType"></param>
        /// <param name="memberInfo"></param>
        /// <param name="parameterInfo"></param>
        /// <param name="autoConfigurationDetail"></param>
        /// <returns></returns>
        private object Resolve(IComponentContext context, Type classType, Type memberType, MemberInfo memberInfo, ParameterInfo parameterInfo = null, AutoConfigurationDetail autoConfigurationDetail = null)
        {
            if (classType == null) return null;
            if (string.IsNullOrEmpty(this.value)) return null;
            try
            {
                var parameterValue = ResolveEmbeddedValue(context,classType,this.value,autoConfigurationDetail);
                if (this.value.Equals(parameterValue))
                {
                    //El表达式

                    
                }

                if (parameterValue == null) return null;
                var parseValue = parameterInfo == null
                    ? TypeManipulation.ChangeToCompatibleType(parameterValue, memberType, memberInfo)
                    : TypeManipulation.ChangeToCompatibleType(parameterValue, memberType, parameterInfo);
                return parseValue;
            }
            catch (Exception ex)
            {
                throw new DependencyResolutionException($"Value set error,can not resolve class type:{classType.FullName} =====>" +
                                                        $" {(parameterInfo == null ? memberType.Name : parameterInfo.Name)} "
                                                        + (!string.IsNullOrEmpty(this.value) ? $",with value:[{this.value}]" : ""), ex);
            }
        }


       
        private string ResolveEmbeddedValue(IComponentContext context, Type classType, string strVal, AutoConfigurationDetail autoConfigurationDetail = null)
        {
            int startIndex = strVal.IndexOf(DefaultPlaceholderPrefix, StringComparison.Ordinal);
            while (startIndex != -1)
            {
                int endIndex = strVal.IndexOf(DefaultPlaceholderSuffix, startIndex + DefaultPlaceholderPrefix.Length, StringComparison.Ordinal);
                if (endIndex != -1)
                {
                    int pos = startIndex + DefaultPlaceholderPrefix.Length;
                    string placeholder = strVal.Substring(pos, endIndex - pos);
                    string resolvedValue = ResolvePlaceholder(context,classType,placeholder, autoConfigurationDetail);
                    if (resolvedValue != null)
                    {
                        strVal = strVal.Substring(0, startIndex) + resolvedValue + strVal.Substring(endIndex + 1);
                        startIndex = strVal.IndexOf(DefaultPlaceholderPrefix, startIndex + resolvedValue.Length, StringComparison.Ordinal);
                    }
                    else if (IgnoreUnresolvablePlaceholders)
                    {
                        return strVal;
                    }
                    else
                    {
                        throw new Exception(string.Format("Could not resolve placeholder '{0}'.", placeholder));
                    }
                }
                else
                {
                    startIndex = -1;
                }
            }
            return strVal;
        }


        private string ResolvePlaceholder(IComponentContext context, Type classType, string placeholder, AutoConfigurationDetail autoConfigurationDetail = null)
        {
            string propertyValue = null;
            if (this.EnvironmentVariableMode == EnvironmentVariableMode.Override)
            {
                propertyValue = Environment.GetEnvironmentVariable(placeholder);
            }

            if (propertyValue == null)
            {
                List<MetaSourceData> MetaSourceList = null;
                if (autoConfigurationDetail != null)
                {
                    MetaSourceList = autoConfigurationDetail.MetaSourceDataList;
                }
                else
                {
                    var componentModelCacheSingleton = context.Resolve<ComponentModelCacheSingleton>();
                    if (componentModelCacheSingleton.ComponentModelCache.TryGetValue(classType, out var component))
                    {
                        MetaSourceList = component.MetaSourceList;
                    }
                }

                if (MetaSourceList != null)
                {
                    foreach (var metaSource in MetaSourceList)
                    {
                        if (metaSource.Configuration == null)
                        {
                            continue;
                        }

                        IConfigurationSection metData = metaSource.Configuration.GetSection(placeholder);
                        var parameterValue = metData?.Value;
                        if (parameterValue == null)
                        {
                            //表示key不存在 从下一个source里面去寻找
                            continue;
                        }
                        propertyValue = parameterValue;
                        break;
                    }
                }
            }
            if (propertyValue == null && EnvironmentVariableMode == EnvironmentVariableMode.Fallback)
            {
                propertyValue = Environment.GetEnvironmentVariable(placeholder);
            }
            return propertyValue;
        }
    }


}