using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac.Core;
using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Configuration;
using Spring.Core.TypeConversion;
using Spring.Expressions;

namespace Autofac.Annotation
{
    /// <summary>
    /// 注入值
    /// 只能打一个标签 可以继承父类
    /// ${xxx} 代表从配置源里面 获取属性名称为xxx的值
    /// #{xxx} 代表启动SPEL表达式，xxx里面可以嵌套${yyy}
    /// SPEL表达式具体用法可以查看：https://github.com/yuzd/Spring.EL
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Field)]
    public sealed class Value : ParameterFilterAttribute
    {
        /// <summary>
        /// The default placeholder prefix.
        /// </summary>
        private const string DefaultPlaceholderPrefix = "${";

        /// <summary>
        /// The default placeholder suffix.
        /// </summary>
        private const string DefaultPlaceholderSuffix = "}";

        /// <summary>
        /// 默认
        /// </summary>
        private const EnvironmentVariableMode DefaultEnvironmentVariableMode = EnvironmentVariableMode.Fallback;


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
        /// 是否启用SPEL 默认true
        /// 如果false，那么根据配置的value的值直接从PropertySource里面读取
        /// </summary>
        public bool UseSpel { get; set; } = true;

        /// <summary>
        /// 如果拿不到是否报错
        /// </summary>
        public bool IgnoreUnresolvablePlaceholders { get; set; }

        /// <summary>
        /// 设置是否从环境变量拿 默认是从文件里面拿不到 就从环境里面去拿
        /// </summary>
        public EnvironmentVariableMode EnvironmentVariableMode { get; set; } = DefaultEnvironmentVariableMode;

        /// <summary>
        /// 注入
        /// 只能支持值类型string int boolean dic list
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override object ResolveParameter(ParameterInfo parameter, IComponentContext context)
        {
            return parameter == null
                ? null
                : Resolve(context, parameter.Member.DeclaringType, parameter.ParameterType, parameter.Name);
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
        /// AutoConfiguration类的value注入
        /// </summary>
        /// <param name="detail"></param>
        /// <param name="parameter"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        internal object ResolveParameterWithConfiguration(AutoConfigurationDetail detail, ParameterInfo parameter,
            IComponentContext context)
        {
            return Resolve(context, parameter.Member.DeclaringType, parameter.ParameterType, parameter.Name, detail);
        }

        /// <summary>
        /// 注入Property的值
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public object ResolveProperty(PropertyInfo parameter, IComponentContext context)
        {
            return parameter == null ? null : Resolve(context, parameter.DeclaringType, parameter.PropertyType);
        }

        /// <summary>
        /// 注入Filed值
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public object ResolveFiled(FieldInfo parameter, IComponentContext context)
        {
            return parameter == null ? null : Resolve(context, parameter.DeclaringType, parameter.FieldType);
        }

        /// <summary>
        /// 对于memberInfo 或者  parameterInfo 进行设值
        /// </summary>
        /// <param name="context"></param>
        /// <param name="classType"></param>
        /// <param name="memberType"></param>
        /// <param name="parameterInfo"></param>
        /// <param name="autoConfigurationDetail"></param>
        /// <returns></returns>
        internal object Resolve(IComponentContext context, Type classType, Type memberType, string parameterInfo = null,
            AutoConfigurationDetail autoConfigurationDetail = null)
        {
            if (classType == null) return null;
            if (string.IsNullOrEmpty(this.value)) return null;
            try
            {
                //判断类型是否是IValue
                if ((typeof(IObjectFactory).IsAssignableFrom(memberType)))
                {
                    return context.Resolve<ObjectBeanFactory>().CreateValueFactory(this, memberType, classType,
                        parameterInfo, autoConfigurationDetail);
                }

                if (!UseSpel)
                {
                    // 原生的获取方式
                    var section = GetFromResource(context, classType, this.value, autoConfigurationDetail);
                    if (section != null)
                    {
                        return section.Get(memberType);
                    }
                    else if (IgnoreUnresolvablePlaceholders)
                    {
                        return null;
                    }
                    else
                    {
                        throw new Exception($"Could not resolve placeholder '{value}' with [UseSpel:false].");
                    }
                }

                //先把 ${} 的 placehoder 全部替换掉
                var parameterValue = ResolveSpel(context, classType, this.value, autoConfigurationDetail,
                    IgnoreUnresolvablePlaceholders, this.EnvironmentVariableMode);
                if (parameterValue == null) return null;
                return TypeConversionUtils.ConvertValueIfNecessary(memberType, parameterValue, null);
            }
            catch (Exception ex)
            {
                throw new DependencyResolutionException(
                    $"Value set error,can not resolve class type:{classType.FullName} =====>" +
                    $" {parameterInfo ?? memberType.Name} "
                    + (!string.IsNullOrEmpty(this.value) ? $",with value:[{this.value}]" : ""), ex);
            }
        }

        #region #{xxx} 代表启动SPEL表达式，xxx里面可以嵌套${yyy}

        /// <summary>
        /// #{xxx} 代表启动SPEL表达式，xxx里面可以嵌套${yyy}
        /// </summary>
        /// <param name="context">容器上下文</param>
        /// <param name="classType">当前使用的类</param>
        /// <param name="strVal">值</param>
        /// <param name="autoConfigurationDetail">propertySource配置</param>
        /// <param name="ignoreFail">如果需要替换但是没找到是否忽略报错</param>
        /// <param name="mode"><see cref="EnvironmentVariableMode"/></param>
        /// <returns></returns>
        internal static object ResolveSpel(
            IComponentContext context,
            Type classType,
            string strVal,
            AutoConfigurationDetail autoConfigurationDetail = null,
            bool ignoreFail = false,
            EnvironmentVariableMode mode = DefaultEnvironmentVariableMode)
        {
            //先把 ${} 的 placehoder 全部替换掉
            var parameterValue = ResolveEmbeddedValue(mode, context, classType, strVal, autoConfigurationDetail,
                ignoreFail);
            if (parameterValue == null)
            {
                return null;
            }

            int startIndex = parameterValue.ToString().IndexOf("#{", StringComparison.Ordinal);
            if (startIndex != -1)
            {
                int endIndex = parameterValue.ToString()
                    .LastIndexOf(DefaultPlaceholderSuffix, StringComparison.Ordinal);
                if (endIndex != -1)
                {
                    Dictionary<string, object> vars = new Dictionary<string, object>
                    {
                        ["_sprint_context_resove_"] = new SprintContextResove((type, name) =>
                        {
                            if (type != null && !string.IsNullOrEmpty(name)) return context.ResolveKeyed(name, type);
                            if (type != null) return context.Resolve(type);
                            if (!string.IsNullOrEmpty(name))
                            {
                                //从当前的Assembly去loadType
                                var arr = name.Split('>');
                                if (arr.Length == 2)
                                {
                                    var resolveType = classType.Assembly.GetType(classType.Namespace + "." + arr[0]);
                                    return resolveType == null ? null : context.ResolveKeyed(arr[1], resolveType);
                                }
                                else
                                {
                                    var resolveType = classType.Assembly.GetType(classType.Namespace + "." + name);
                                    return resolveType == null ? null : context.Resolve(resolveType);
                                }
                            }

                            return null;
                        })
                    };
                    //El表达式

                    int pos = startIndex + DefaultPlaceholderPrefix.Length;
                    string expression = parameterValue.ToString().Substring(pos, endIndex - pos);
                    parameterValue = ExpressionEvaluator.GetValue(null, expression, vars);
                }
            }

            return parameterValue;
        }

        #endregion

        #region ${xxx} 代表从配置源里面 获取属性名称为xxx的值

        static object ResolveEmbeddedValue(EnvironmentVariableMode mode,
            IComponentContext context, Type classType, string strVal,
            AutoConfigurationDetail autoConfigurationDetail = null, bool ignoreFail = false)
        {
            int startIndex = strVal.IndexOf(DefaultPlaceholderPrefix, StringComparison.Ordinal);
            while (startIndex != -1)
            {
                int endIndex = strVal.IndexOf(DefaultPlaceholderSuffix, startIndex + DefaultPlaceholderPrefix.Length,
                    StringComparison.Ordinal);
                if (endIndex != -1)
                {
                    int pos = startIndex + DefaultPlaceholderPrefix.Length;
                    string placeholder = strVal.Substring(pos, endIndex - pos);
                    string resolvedValue =
                        ResolvePlaceholder(mode, context, classType, placeholder, autoConfigurationDetail);
                    if (resolvedValue != null)
                    {
                        strVal = strVal.Substring(0, startIndex) + resolvedValue + strVal.Substring(endIndex + 1);
                        startIndex = strVal.IndexOf(DefaultPlaceholderPrefix, startIndex + resolvedValue.Length,
                            StringComparison.Ordinal);
                    }
                    else if (ignoreFail)
                    {
                        return null;
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


        /// <summary>
        /// 根据placeholder去数据源取
        /// </summary>
        /// <param name="mode">模式</param>
        /// <param name="context">di上下文</param>
        /// <param name="classType">需要返回的类型</param>
        /// <param name="placeholder">key</param>
        /// <param name="autoConfigurationDetail"></param>
        /// <returns></returns>
        static string ResolvePlaceholder(EnvironmentVariableMode mode, IComponentContext context, Type classType,
            string placeholder,
            AutoConfigurationDetail autoConfigurationDetail = null)
        {
            string propertyValue = null;
            if (mode == EnvironmentVariableMode.Override)
            {
                propertyValue = Environment.GetEnvironmentVariable(placeholder);
            }

            if (propertyValue == null)
            {
                var section = GetFromResource(context, classType, placeholder, autoConfigurationDetail);
                propertyValue = section?.Value;
            }

            if (propertyValue == null && mode == EnvironmentVariableMode.Fallback)
            {
                propertyValue = Environment.GetEnvironmentVariable(placeholder);
            }

            return propertyValue;
        }

        /// <summary>
        /// 从PropertySource源获取
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="classType">返回类型</param>
        /// <param name="placeholder">key</param>
        /// <param name="autoConfigurationDetail"></param>
        /// <returns></returns>
        static IConfigurationSection GetFromResource(IComponentContext context, Type classType, string placeholder,
            AutoConfigurationDetail autoConfigurationDetail = null)
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
                    if (metData == null || (string.IsNullOrEmpty(parameterValue) && !metData.GetChildren().Any()))
                    {
                        //表示key不存在 从下一个source里面去寻找
                        continue;
                    }

                    return metData;
                }
            }

            return null;
        }

        #endregion
    }
}