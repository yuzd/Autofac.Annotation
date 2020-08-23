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
    //[Component(AutofacScope = AutofacScope.SingleInstance, AutoActivate = true, InitMethod = nameof(AutoConfigurationSource.Start))]
    internal class AutoConfigurationSource
    {
        /// <summary>
        /// 注册AutoConfiguration
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="list"></param>
        public static void Register(ContainerBuilder builder,AutoConfigurationList list)
        {

            //注册Configuration
            foreach (var configuration in list.AutoConfigurationDetailList)
            {
                RegisterConfiguration(builder, configuration);
            }
            
        }

        private static void RegisterConfiguration(ContainerBuilder builder,AutoConfigurationDetail autoConfigurationDetail)
        {
            //注册为工厂
            foreach (var beanMethod in autoConfigurationDetail.BeanMethodInfoList)
            {
                if (!beanMethod.Item2.IsVirtual)
                {
                    throw new InvalidOperationException(
                        $"The Configuration class `{autoConfigurationDetail.AutoConfigurationClassType.FullName}` method `{beanMethod.Item2.Name}` must be virtual!");
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
                
                //包装这个方法成功工厂
                //先拿到AutoConfiguration class的实例
                //注册一个方法到容器 拿到并传入IComponentContext 
                
                builder.RegisterCallback(cr =>
                {
                    var instanceType = beanMethod.Item3;//返回类型
                    
                    var rb = RegistrationBuilder.ForDelegate(instanceType, ((context, parameters) =>
                    {
                        var autoConfigurationInstance = context.Resolve(autoConfigurationDetail.AutoConfigurationClassType);
                        var instance = AutoConfigurationHelper.InvokeInstanceMethod(context, autoConfigurationDetail, autoConfigurationInstance, beanMethod.Item2);
                        if (typeof(Task).IsAssignableFrom(instance.GetType()))
                        {
                             return typeof(Task<>).MakeGenericType(instanceType).GetProperty("Result").GetValue(instance);
                        }
                        return instance;
                    }));
                    
                    if (!string.IsNullOrEmpty(beanMethod.Item1.Key))
                    {
                        rb.Keyed(beanMethod.Item1.Key, instanceType).Named("`1System.Collections.Generic.IEnumerable`1" + instanceType.FullName, instanceType);
                    }
                    else
                    {
                        rb.As(instanceType).Named("`1System.Collections.Generic.IEnumerable`1" + instanceType.FullName, instanceType);
                    }
                    
                    rb.SingleInstance();
                    RegistrationBuilder.RegisterSingleComponent(cr, rb);
                });
               
            }
        }
        
        
        
        // /// <summary>
        // /// 执行自动注册
        // /// </summary>
        // /// <param name="context"></param>
        // internal void Start(IComponentContext context)
        // {
        //     lock (this)
        //     {
        //         context.TryResolve<AutoConfigurationList>(out var autoConfigurationList);
        //         if (autoConfigurationList == null || autoConfigurationList.AutoConfigurationDetailList == null || !autoConfigurationList.AutoConfigurationDetailList.Any()) return;
        //         foreach (var autoConfigurationDetail in autoConfigurationList.AutoConfigurationDetailList)
        //         {
        //             context.TryResolve(autoConfigurationDetail.AutoConfigurationClassType, out var autoConfigurationInstance);
        //             if (autoConfigurationInstance == null) continue;
        //
        //
        //             foreach (var beanMethod in autoConfigurationDetail.BeanMethodInfoList)
        //             {
        //                 if (beanMethod.Item2.IsVirtual)
        //                 {
        //                     throw new InvalidOperationException(
        //                         $"The Configuration class `{autoConfigurationDetail.AutoConfigurationClassType.FullName}` method `{beanMethod.Item2.Name}` can not be virtual!");
        //                 }
        //
        //                 if (!ProxyUtil.IsAccessible(beanMethod.Item2.ReturnType))
        //                 {
        //                     throw new InvalidOperationException(
        //                         $"The Configuration class `{autoConfigurationDetail.AutoConfigurationClassType.FullName}` method `{beanMethod.Item2.Name}` returnType is not accessible!");
        //                 }
        //                 if (beanMethod.Item2.ReturnType.IsValueType || beanMethod.Item2.ReturnType.IsEnum)
        //                 {
        //                     throw new InvalidOperationException(
        //                         $"The Configuration class `{autoConfigurationDetail.AutoConfigurationClassType.FullName}` method `{beanMethod.Item2.Name}` returnType is invalid!");
        //                 }
        //
        //                 var result = AutoConfigurationHelper.InvokeInstanceMethod(context, autoConfigurationDetail, autoConfigurationInstance, beanMethod.Item2);
        //                 if (result == null) continue;
        //                 AutoConfigurationHelper.RegisterInstance(context.ComponentRegistry, beanMethod.Item2.ReturnType,
        //                     result, beanMethod.Item1.Key).CreateRegistration();
        //             }
        //
        //         }
        //     }
        // }
    }


    internal static class AutoConfigurationHelper
    {
        public static object InvokeInstanceMethod(object instance, MethodInfo methodInfo,IComponentContext context,PointcutContext invocation = null)
        {
            try
            {
                var parameters = methodInfo.GetParameters();
                if (parameters.Length == 0)
                { 
                    return methodInfo.Invoke(instance, null);
                }

                //自动类型注入

                List<object> parameterObj = new List<object>();
                foreach (var parameter in parameters)
                {
                    if (invocation != null && parameter.ParameterType == typeof(PointcutContext))
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
                
                return methodInfo.Invoke(instance, parameterObj.ToArray());

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

        // public static IRegistrationBuilder<object, SimpleActivatorData, SingleRegistrationStyle> RegisterInstance(IComponentRegistry cr, Type returnType, object instance, string key)
        // {
        //     if (instance == null) throw new ArgumentNullException(nameof(instance));
        //
        //     var activator = new ProvidedInstanceActivator(instance);
        //     var instanceType = instance.GetType();
        //     var rb = RegistrationBuilder.ForDelegate(instanceType, ((context, parameters) => instance));
        //     if (returnType != instanceType)
        //     {
        //         if (!string.IsNullOrEmpty(key))
        //         {
        //             rb.Keyed(key, returnType).Named("`1System.Collections.Generic.IEnumerable`1" + returnType.FullName, returnType); 
        //         }
        //         else
        //         {
        //             rb.As(returnType).Named("`1System.Collections.Generic.IEnumerable`1" + returnType.FullName, returnType);
        //         }
        //     }
        //     else
        //     {
        //         if (!string.IsNullOrEmpty(key))
        //         {
        //             rb.Keyed(key, instanceType).Named("`1System.Collections.Generic.IEnumerable`1" + instanceType.FullName, instanceType);
        //         }
        //         else
        //         {
        //             rb.As(instanceType).Named("`1System.Collections.Generic.IEnumerable`1" + instanceType.FullName, instanceType);
        //         }
        //     }
        //
        //     rb.SingleInstance();
        //     if (!(rb.RegistrationData.Lifetime is RootScopeLifetime) ||
        //         rb.RegistrationData.Sharing != InstanceSharing.Shared)
        //     {
        //         throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "The instance  registration `{0}` can support SingleInstance() sharing only", instance));
        //     }
        //
        //     activator.DisposeInstance = rb.RegistrationData.Ownership == InstanceOwnership.OwnedByLifetimeScope;
        //
        //     RegistrationBuilder.RegisterSingleComponent(cr, rb);
        //
        //     return rb;
        // }
    }

    /// <summary>
    /// AutoConfiguration的类代理
    /// </summary>
    [Component(typeof(AutoConfigurationIntercept))]
    public class AutoConfigurationIntercept: AsyncInterceptor
    {
        /// <summary>
        /// 单例对象缓存
        /// </summary>
        private ConcurrentDictionary<MethodInfo,object> _instanceCache = new ConcurrentDictionary<MethodInfo, object>();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="proceedInfo"></param>
        /// <param name="proceed"></param>
        /// <returns></returns>
        protected override async Task InterceptAsync(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task> proceed)
        {
            await proceed(invocation,proceedInfo);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="proceedInfo"></param>
        /// <param name="proceed"></param>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        protected override async Task<TResult> InterceptAsync<TResult>(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed)
        {
            //单例的
            if (_instanceCache.TryGetValue(invocation.MethodInvocationTarget, out var instance))
            {
                invocation.ReturnValue = instance;
                return (TResult)instance;
            }

            var result = await proceed(invocation, proceedInfo);

            _instanceCache.TryAdd(invocation.MethodInvocationTarget, result);
            return result;
        }
        
    }
    /// <summary>
    /// AutoConfiguration装配集合数据源
    /// </summary>
    public class AutoConfigurationList
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
        /// 针对动态泛型类的method目标集合
        /// </summary>
        public ConcurrentDictionary<string,PointcutConfigurationInfo> DynamicPointcutTargetInfoList { get; set; }
        
        /// <summary>
        /// 对应的class目标集合
        /// </summary>
        public ConcurrentDictionary<Type,bool> PointcutTypeInfoList { get; set; }

    }

    /// <summary>
    /// AutoConfiguration装配集合数据源
    /// </summary>
    public class AutoConfigurationDetail
    {
        /// <summary>
        /// Configuration 所在的类的类型
        /// </summary>
        public Type AutoConfigurationClassType { get; set; }

        /// <summary>
        /// Configuration 所在的类的里面有Bean标签的所有方法
        /// </summary>
        public List<Tuple<Bean, MethodInfo,Type>> BeanMethodInfoList { get; set; }


        /// <summary>
        /// 数据源
        /// </summary>
        public List<MetaSourceData> MetaSourceDataList { get; set; }

    }
}
