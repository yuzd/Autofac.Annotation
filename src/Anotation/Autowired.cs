using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac.Annotation.Util;
using Autofac.Core;
using Autofac.Features.AttributeFilters;

namespace Autofac.Annotation
{
    /// <summary>
    /// 注入属性或者字段
    /// 只能打一个标签 可以继承父类
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Method)]
    public sealed class Autowired : ParameterFilterAttribute
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
        /// AutoConfiguration类的自动装载
        /// </summary>
        /// <param name="detail"></param>
        /// <param name="parameter"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        internal object ResolveParameterWithConfiguration(AutoConfigurationDetail detail, ParameterInfo parameter, IComponentContext context)
        {
            return Resolve(context, parameter.Member.DeclaringType, parameter.ParameterType, parameter.Name, null, detail);
        }

        /// <summary>
        /// 作为ParameterInfo自动装载
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override object ResolveParameter(ParameterInfo parameter, IComponentContext context)
        {
            if (parameter == null) throw new ArgumentNullException(nameof(parameter));
            return Resolve(context, parameter.Member.DeclaringType, parameter.ParameterType, parameter.Name, null);
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
        /// <returns></returns>
        public object ResolveField(FieldInfo property, IComponentContext context, List<Parameter> Parameters, object instance)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            return Resolve(context, property.DeclaringType, property.FieldType, property.Name, Parameters);
        }

        /// <summary>
        /// 装配属性
        /// </summary>
        /// <param name="property"></param>
        /// <param name="context"></param>
        /// <param name="Parameters"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public object ResolveProperty(PropertyInfo property, IComponentContext context, List<Parameter> Parameters, object instance)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            return Resolve(context, property.DeclaringType, property.PropertyType, property.Name, Parameters);
        }

        internal object Resolve(IComponentContext context, Type classType, Type memberType, string fieldOrPropertyName,
            List<Parameter> Parameters, AutoConfigurationDetail autoConfigurationDetail = null)
        {
            object returnObj = null;

            //针对继承 IObjectFactory 的 需要动态创建
            if (typeof(IObjectFactory).IsAssignableFrom(memberType))
            {
                return context.Resolve<ObjectBeanFactory>()
                    .CreateAutowiredFactory(this, memberType, classType, fieldOrPropertyName, null, autoConfigurationDetail);
            }
            else if (memberType.IsGenericType && memberType.GetGenericTypeDefinition() == typeof(Lazy<>))
            {
                return context.Resolve<ObjectBeanFactory>()
                    .CreateLazyFactory(this, memberType, classType, fieldOrPropertyName, null, autoConfigurationDetail);
            }
            else if (string.IsNullOrEmpty(Name) && memberType.IsGenericEnumerableInterfaceType())
            {
                var genericType = memberType.GenericTypeArguments[0];
                if (genericType.FullName != null && genericType.FullName.StartsWith("System.Lazy`1"))
                    genericType = genericType.GenericTypeArguments[0];

                context.TryResolveKeyed("`1System.Collections.Generic.IEnumerable`1" + genericType.FullName, memberType, out returnObj);

                if (returnObj == null && Required)
                    throw new DependencyResolutionException($"Autowire error,can not resolve class type:{classType.FullName}:{fieldOrPropertyName} "
                                                            + (!string.IsNullOrEmpty(Name) ? $",with key:[{Name}]" : ""));

                return returnObj;
            }

            var spelName = Name;
            Service propertyService = null;
            if (!string.IsNullOrEmpty(spelName))
            {
                // spel判断
                spelName = Value.ResolveSpel(context, classType, spelName, autoConfigurationDetail).ToString();
                propertyService = new KeyedService(spelName, memberType);
            }
            else
            {
                //如果指定Name查找
                propertyService = new TypedService(memberType);
            }

            if (Parameters != null && Parameters.Any() && Parameters.Last() is AutowiredParmeterStack AutowiredParmeter)
            {
                //先检查是否已注册过
                if (AutowiredParmeter.CircularDetected(propertyService, out returnObj))
                {
                }
                else if (context.TryResolveService(propertyService, new Parameter[] { AutowiredParmeter }, out returnObj))
                {
                }
                //先判断根据Type来找是否能找得到 发现Type没有就尝试用当前的属性定义的名称去找
                else if (string.IsNullOrEmpty(Name))
                {
                    propertyService = new KeyedService(fieldOrPropertyName, memberType);
                    if (AutowiredParmeter.CircularDetected(propertyService, out returnObj))
                    {
                    }
                    else if (context.TryResolveService(propertyService, new Parameter[] { AutowiredParmeter }, out returnObj))
                    {
                    }
                }
            }
            else
            {
                //构造方法注入的分支
                if (context.TryResolveService(propertyService, new Parameter[] { }, out returnObj))
                {
                    //success
                }
                else if (string.IsNullOrEmpty(spelName) &&
                         context.TryResolveService(new KeyedService(fieldOrPropertyName, memberType), new Parameter[] { }, out returnObj))
                {
                    //success
                }
            }

            if (returnObj == null && Required)
                throw new DependencyResolutionException($"Autowire error,can not resolve class type:{classType.FullName}.{fieldOrPropertyName} "
                                                        + (!string.IsNullOrEmpty(spelName) ? $",with key:[{spelName}]" : ""));

            return returnObj;
        }
    }
}