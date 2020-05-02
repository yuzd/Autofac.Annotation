using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac.Annotation.Util;
using Autofac.Aspect;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Activators.ProvidedInstance;
using Autofac.Core.Lifetime;
using Castle.DynamicProxy;

namespace Autofac.Annotation
{
    /// <summary>
    /// 自启动注册Configuration
    /// </summary>
    [Component(AutofacScope = AutofacScope.SingleInstance, AutoActivate = true, InitMethod = nameof(AutoConfigurationSource.Start))]
    public class AutoConfigurationSource
    {
        /// <summary>
        /// 执行自动注册
        /// </summary>
        /// <param name="context"></param>
        public void Start(IComponentContext context)
        {
            lock (this)
            {
                context.TryResolve<AutoConfigurationList>(out var autoConfigurationList);
                if (autoConfigurationList == null || autoConfigurationList.AutoConfigurationDetailList == null || !autoConfigurationList.AutoConfigurationDetailList.Any()) return;
                foreach (var autoConfigurationDetail in autoConfigurationList.AutoConfigurationDetailList)
                {
                    context.TryResolve(autoConfigurationDetail.AutoConfigurationClassType, out var autoConfigurationInstance);
                    if (autoConfigurationInstance == null) continue;


                    foreach (var beanMethod in autoConfigurationDetail.BeanMethodInfoList)
                    {
                        if (beanMethod.Item2.IsVirtual)
                        {
                            throw new InvalidOperationException(
                                $"The Configuration class `{autoConfigurationDetail.AutoConfigurationClassType.FullName}` method `{beanMethod.Item2.Name}` can not be virtual!");
                        }

                        if (!ProxyUtil.IsAccessible(beanMethod.Item2.ReturnType))
                        {
                            throw new InvalidOperationException(
                                $"The Configuration class `{autoConfigurationDetail.AutoConfigurationClassType.FullName}` method `{beanMethod.Item2.Name}` returnType is not accessible!");
                        }
                        if (beanMethod.Item2.ReturnType.IsValueType || beanMethod.Item2.ReturnType.IsEnum)
                        {
                            throw new InvalidOperationException(
                                $"The Configuration class `{autoConfigurationDetail.AutoConfigurationClassType.FullName}` method `{beanMethod.Item2.Name}` returnType is invalid!");
                        }

                        var result = AutoConfigurationHelper.InvokeInstanceMethod(context, autoConfigurationDetail, autoConfigurationInstance, beanMethod.Item2);
                        if (result == null) continue;
                        AutoConfigurationHelper.RegisterInstance(context.ComponentRegistry, beanMethod.Item2.ReturnType,
                            result, beanMethod.Item1.Key).CreateRegistration();
                    }

                }
            }
        }
    }


    internal static class AutoConfigurationHelper
    {
        public static void InvokeInstanceMethod(object instance, MethodInfo methodInfo,IComponentContext context,AspectContext invocation = null)
        {
            try
            {
                var parameters = methodInfo.GetParameters();
                if (parameters.Length == 0)
                { 
                    methodInfo.Invoke(instance, null);
                    return;
                }

                //自动类型注入

                List<object> parameterObj = new List<object>();
                foreach (var parameter in parameters)
                {
                    if (invocation != null && parameter.ParameterType == typeof(AspectContext))
                    {
                        parameterObj.Add(invocation);
                        continue;
                    }
                    
                    var autowired = parameter.GetCustomAttribute<Autowired>();
                    if (autowired != null)
                    {
                        parameterObj.Add(autowired.ResolveParameter(parameter, context));
                        continue;
                    }

                    var value = parameter.GetCustomAttribute<Value>();
                    if (value != null)
                    {
                        parameterObj.Add(value.ResolveParameter(parameter, context));
                        continue;
                    }

                    if (parameter.HasDefaultValue)
                    {
                        parameterObj.Add(parameter.RawDefaultValue);
                        continue;
                    }

                    if (parameter.IsOptional)
                    {
                        parameterObj.Add(Type.Missing);
                        continue;
                    }

                    if (parameter.IsOut)
                    {
                        parameterObj.Add(Type.Missing);
                        continue;
                    }

                    if (parameter.ParameterType.IsValueType || parameter.ParameterType.IsEnum)
                    {
                        parameterObj.Add(parameter.RawDefaultValue);
                        continue;
                    }



                    parameterObj.Add(context.Resolve(parameter.ParameterType));

                }
                
                methodInfo.Invoke(instance, parameterObj.ToArray());

            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"The class `{methodInfo.DeclaringType.FullName}` method `{methodInfo.Name}` invoke fail!", e);
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="methodInfo"></param>
        /// <param name="context"></param>
        /// <param name="invocation"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static T InvokeInstanceMethodReturn<T>(object instance, MethodInfo methodInfo,IComponentContext context,AspectContext invocation = null)
        {
            try
            {
                var parameters = methodInfo.GetParameters();
                if (parameters.Length == 0)
                { 
                    return (T)methodInfo.Invoke(instance, null);
                }

                //自动类型注入

                List<object> parameterObj = new List<object>();
                foreach (var parameter in parameters)
                {
                    if (invocation != null && parameter.ParameterType == typeof(AspectContext))
                    {
                        parameterObj.Add(invocation);
                        continue;
                    }
                    
                    var autowired = parameter.GetCustomAttribute<Autowired>();
                    if (autowired != null)
                    {
                        parameterObj.Add(autowired.ResolveParameter(parameter, context));
                        continue;
                    }

                    var value = parameter.GetCustomAttribute<Value>();
                    if (value != null)
                    {
                        parameterObj.Add(value.ResolveParameter(parameter, context));
                        continue;
                    }

                    if (parameter.HasDefaultValue)
                    {
                        parameterObj.Add(parameter.RawDefaultValue);
                        continue;
                    }

                    if (parameter.IsOptional)
                    {
                        parameterObj.Add(Type.Missing);
                        continue;
                    }

                    if (parameter.IsOut)
                    {
                        parameterObj.Add(Type.Missing);
                        continue;
                    }

                    if (parameter.ParameterType.IsValueType || parameter.ParameterType.IsEnum)
                    {
                        parameterObj.Add(parameter.RawDefaultValue);
                        continue;
                    }



                    parameterObj.Add(context.Resolve(parameter.ParameterType));

                }
                
                return (T)methodInfo.Invoke(instance, parameterObj.ToArray());

            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"The class `{methodInfo.DeclaringType.FullName}` method `{methodInfo.Name}` invoke fail!", e);
            }
        }
        
        public static object InvokeInstanceMethod(IComponentContext context, AutoConfigurationDetail autoConfigurationDetail, object autoConfigurationInstance, MethodInfo methodInfo)
        {
            try
            {
                var parameters = methodInfo.GetParameters();
                if (parameters.Length == 0)
                {
                    return methodInfo.Invoke(autoConfigurationInstance, null);
                }

                //自动类型注入

                List<object> parameterObj = new List<object>();
                foreach (var parameter in parameters)
                {
                    var autowired = parameter.GetCustomAttribute<Autowired>();
                    if (autowired != null)
                    {
                        parameterObj.Add(autowired.ResolveParameter(parameter, context));
                        continue;
                    }

                    var value = parameter.GetCustomAttribute<Value>();
                    if (value != null)
                    {
                        parameterObj.Add(value.ResolveParameterWithConfiguration(autoConfigurationDetail,parameter, context));
                        continue;
                    }

                    if (parameter.HasDefaultValue)
                    {
                        parameterObj.Add(parameter.RawDefaultValue);
                        continue;
                    }

                    if (parameter.IsOptional)
                    {
                        parameterObj.Add(Type.Missing);
                        continue;
                    }

                    if (parameter.IsOut)
                    {
                        parameterObj.Add(Type.Missing);
                        continue;
                    }

                    if (parameter.ParameterType.IsValueType || parameter.ParameterType.IsEnum)
                    {
                        parameterObj.Add(parameter.RawDefaultValue);
                        continue;
                    }


                    parameterObj.Add(context.Resolve(parameter.ParameterType));

                }

                return methodInfo.Invoke(autoConfigurationInstance, parameterObj.ToArray());

            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"The Configuration class `{autoConfigurationDetail.AutoConfigurationClassType.FullName}` method `{methodInfo.Name}` invoke fail!", e);
            }

        }

        public static IRegistrationBuilder<object, SimpleActivatorData, SingleRegistrationStyle> RegisterInstance(IComponentRegistry cr, Type returnType, object instance, string key)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            var activator = new ProvidedInstanceActivator(instance);
            var instanceType = instance.GetType();
            var rb = RegistrationBuilder.ForDelegate(instanceType, ((context, parameters) => instance));
            if (returnType != instanceType)
            {
                if (!string.IsNullOrEmpty(key))
                {
                    rb.Keyed(key, returnType).Named("`1System.Collections.Generic.IEnumerable`1" + returnType.FullName, returnType); 
                }
                else
                {
                    rb.As(returnType).Named("`1System.Collections.Generic.IEnumerable`1" + returnType.FullName, returnType);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(key))
                {
                    rb.Keyed(key, instanceType).Named("`1System.Collections.Generic.IEnumerable`1" + instanceType.FullName, instanceType);
                }
                else
                {
                    rb.As(instanceType).Named("`1System.Collections.Generic.IEnumerable`1" + instanceType.FullName, instanceType);
                }
            }

            rb.SingleInstance();
            if (!(rb.RegistrationData.Lifetime is RootScopeLifetime) ||
                rb.RegistrationData.Sharing != InstanceSharing.Shared)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "The instance  registration `{0}` can support SingleInstance() sharing only", instance));
            }

            activator.DisposeInstance = rb.RegistrationData.Ownership == InstanceOwnership.OwnedByLifetimeScope;

            RegistrationBuilder.RegisterSingleComponent(cr, rb);

            return rb;
        }
    }

    /// <summary>
    /// AutoConfiguration装配集合数据源
    /// </summary>
    internal class AutoConfigurationList
    {

        /// <summary>
        /// AutoConfiguration装配集合数据源
        /// </summary>
        public List<AutoConfigurationDetail> AutoConfigurationDetailList { get; set; }

    }
    
    /// <summary>
    /// PointCut装配集合数据源
    /// </summary>
    public class PointCutConfigurationList
    {

        /// <summary>
        /// PointCut装配集合数据源
        /// </summary>
        public List<PointcutConfigurationInfo> PointcutConfigurationInfoList { get; set; }
        
        /// <summary>
        /// 对应的method目标集合
        /// </summary>
        public ConcurrentDictionary<MethodInfo,PointcutConfigurationInfo> PointcutTargetInfoList { get; set; }
        
        /// <summary>
        /// 对应的class目标集合
        /// </summary>
        public ConcurrentDictionary<Type,bool> PointcutTypeInfoList { get; set; }

    }

    /// <summary>
    /// AutoConfiguration装配集合数据源
    /// </summary>
    internal class AutoConfigurationDetail
    {
        /// <summary>
        /// Configuration 所在的类的类型
        /// </summary>
        public Type AutoConfigurationClassType { get; set; }

        /// <summary>
        /// Configuration 所在的类的里面有Bean标签的所有方法
        /// </summary>
        public List<Tuple<Bean, MethodInfo>> BeanMethodInfoList { get; set; }


        /// <summary>
        /// 数据源
        /// </summary>
        public List<MetaSourceData> MetaSourceDataList { get; set; }

    }
}
