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
        /// 默认是false
        /// </summary>
        internal bool? AllowCircularDependencies { get; set; }


        /// <summary>
        /// 是否支持循环注入 默认是false 不支持
        /// </summary>
        public bool CircularDependencies
        {
            get => AllowCircularDependencies != null && AllowCircularDependencies.Value;
            set => AllowCircularDependencies = value;
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
            // return parameter == null ? null : Resolve(parameter.Member.DeclaringType, parameter.ParameterType,parameter.Name, context, "parameter");
            return Resolve(context, null, parameter.Member.DeclaringType, parameter.ParameterType,parameter.Name, null);
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
        public object ResolveField(FieldInfo property, IComponentContext context, IEnumerable<Parameter> Parameters, object instance)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            // if (this.AllowCircularDependencies == null || !this.AllowCircularDependencies.Value) return Resolve(property.DeclaringType, property.FieldType,property.Name, context, "field");
            return Resolve(context, instance, property.DeclaringType, property.FieldType,property.Name, Parameters);
        }

        /// <summary>
        /// 装配属性
        /// </summary>
        /// <param name="property"></param>
        /// <param name="context"></param>
        /// <param name="Parameters"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public object ResolveProperty(PropertyInfo property, IComponentContext context, IEnumerable<Parameter> Parameters, object instance)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            // if (this.AllowCircularDependencies == null || !this.AllowCircularDependencies.Value) return Resolve(property.DeclaringType, property.PropertyType,property.Name, context, "property");
            return Resolve(context, instance, property.DeclaringType, property.PropertyType,property.Name, Parameters);
        }

        internal object Resolve(IComponentContext context, object instance, Type classType, Type memberType,string fieldOrPropertyName, IEnumerable<Parameter> Parameters)
        {
            object returnObj = null;
            
            //针对继承 IObjectFactory 的 需要动态创建
            if ((typeof(IObjectFactory).IsAssignableFrom(memberType)))
            {
                return context.Resolve<ObjectBeanFactory>().CreateAutowiredFactory(this, memberType, classType,fieldOrPropertyName, instance, Parameters);
            }
            else if (memberType.IsGenericType && memberType.GetGenericTypeDefinition() == typeof(Lazy<>))
            {
                return context.Resolve<ObjectBeanFactory>().CreateLazyFactory(this, memberType, classType,fieldOrPropertyName, instance, Parameters);
            }
            else if (string.IsNullOrEmpty(this.Name) && memberType.IsGenericEnumerableInterfaceType())
            {
                var genericType = memberType.GenericTypeArguments[0];
                if (genericType.FullName != null && genericType.FullName.StartsWith("System.Lazy`1"))
                {
                    genericType = genericType.GenericTypeArguments[0];
                }
                context.TryResolveKeyed("`1System.Collections.Generic.IEnumerable`1" + genericType.FullName, memberType, out returnObj);
                
                if (returnObj == null && this.Required)
                {
                    throw new DependencyResolutionException($"Autowire error,can not resolve class type:{classType.FullName}:{fieldOrPropertyName} "
                                                            + (!string.IsNullOrEmpty(this.Name) ? $",with key:[{this.Name}]" : ""));
                }
                return returnObj;
            }
            
           
            Service propertyService = null;
            if (!string.IsNullOrEmpty(this.Name))
            {
                propertyService = new KeyedService(this.Name, memberType);
            }
            else
            {
                //如果指定Name查找
                propertyService = new TypedService(memberType);
            }

            //如果没有开启支持循环注入的话
            // ReSharper disable once PossibleMultipleEnumeration
            if (Parameters != null && Parameters.Count() == 1)
            {
                // ReSharper disable once PossibleMultipleEnumeration
                if (!(Parameters.First() is AutowiredParmeter AutowiredParmeter))
                {
                    //error
                }
                // ReSharper disable once AssignNullToNotNullAttribute
                else if (AutowiredParmeter.TryGet(memberType.Namespace+"."+memberType.Name, out returnObj))
                {
                    if (this.AllowCircularDependencies == null || !this.AllowCircularDependencies.Value)
                    {
                        var err = AutowiredParmeter.GetCircualrChains() + ($"->{classType.Namespace}.{classType.Name}");
                        AutowiredParmeter.Dispose();
                        throw new DependencyResolutionException(err);
                    }
                    AutowiredParmeter.Dispose();
                }
                else
                {
                    // ReSharper disable once PossibleNullReferenceException
                    AutowiredParmeter.TryAdd(classType.Namespace+"."+classType.Name, instance);
                    if (context.TryResolveService(propertyService, new Parameter[] {AutowiredParmeter}, out returnObj))
                    {
                        //success
                    } //先判断根据Type来找是否能找得到 发现Type没有就尝试用当前的属性定义的名称去找
                    else if(string.IsNullOrEmpty(this.Name) && context.TryResolveService( new KeyedService(fieldOrPropertyName, memberType), new Parameter[] {AutowiredParmeter}, out returnObj))
                    {
                        //success
                    }
                }
            }
            else
            {
                var instanceTypeParameter = new AutowiredParmeter();
                // ReSharper disable once PossibleNullReferenceException
                instanceTypeParameter.TryAdd(classType.Namespace+"."+classType.Name, instance);
                if (context.TryResolveService(propertyService, new Parameter[] {instanceTypeParameter},out returnObj))
                {
                    //success
                }//先判断根据Type来找是否能找得到 发现Type没有就尝试用当前的属性定义的名称去找
                else if(string.IsNullOrEmpty(this.Name) && context.TryResolveService( new KeyedService(fieldOrPropertyName, memberType), new Parameter[] {instanceTypeParameter}, out returnObj))
                {
                    //success
                }
            }
            
            if (returnObj == null && this.Required)
            {
                throw new DependencyResolutionException($"Autowire error,can not resolve class type:{classType.FullName}.{fieldOrPropertyName} "
                                                        + (!string.IsNullOrEmpty(this.Name) ? $",with key:[{this.Name}]" : ""));
            }
            return returnObj;
        }

        // /// <summary>
        // /// 装配 parameter参数
        // /// </summary>
        // /// <param name="classType"></param>
        // /// <param name="type"></param>
        // /// <param name="fieldOrPropertyName"></param>
        // /// <param name="context"></param>
        // /// <param name="typeDescription"></param>
        // /// <returns></returns>
        // /// <exception cref="DependencyResolutionException"></exception>
        // internal object Resolve(Type classType, Type type,string fieldOrPropertyName, IComponentContext context, string typeDescription)
        // {
        //     if ((typeof(IObjectFactory).IsAssignableFrom(type)))
        //     {
        //         return context.Resolve<ObjectBeanFactory>().CreateAutowiredFactory(this, type, classType,fieldOrPropertyName, typeDescription);
        //     }
        //
        //     object obj = null;
        //     if (!string.IsNullOrEmpty(this.Name))
        //     {
        //         context.TryResolveKeyed(this.Name, type, out obj);
        //     }
        //     else
        //     {
        //         if (type.IsGenericEnumerableInterfaceType())
        //         {
        //             var genericType = type.GenericTypeArguments[0];
        //             if (genericType.FullName != null && genericType.FullName.StartsWith("System.Lazy`1"))
        //             {
        //                 genericType = genericType.GenericTypeArguments[0];
        //             }
        //
        //             context.TryResolveKeyed("`1System.Collections.Generic.IEnumerable`1" + genericType.FullName, type, out obj);
        //         }
        //         else
        //         {
        //             context.TryResolve(type, out obj);
        //         }
        //     }
        //
        //     if (obj == null && this.Required)
        //     {
        //         throw new DependencyResolutionException($"Autowire error,can not resolve class type:{classType.FullName},${typeDescription} name:{type.Name}.{fieldOrPropertyName} "
        //                                                 + (!string.IsNullOrEmpty(this.Name) ? $",with key:[{this.Name}]" : ""));
        //     }
        //
        //     return obj;
        // }

    }
}