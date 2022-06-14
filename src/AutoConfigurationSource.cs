using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac.Builder;
using Autofac.Core.Resolving.Pipeline;
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
        public static void Register(ContainerBuilder builder, AutoConfigurationList list)
        {
            //注册Configuration
            foreach (var configuration in list.AutoConfigurationDetailList)
            {
                RegisterConfiguration(builder, configuration);
            }
        }

        private static void RegisterConfiguration(ContainerBuilder builder, AutoConfigurationDetail autoConfigurationDetail)
        {
            var cache = builder.Properties[AutofacAnnotationModule._ALL_COMPOMENT] as ComponentModelCacheSingleton;
            //注册为工厂
            foreach (var beanMethod in autoConfigurationDetail.BeanMethodInfoList.OrderBy(r => r.Item3.Name))
            {
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
                    var instanceType = beanMethod.Item3; //返回类型
                    //Condition
                    if (AutofacAnnotationModule.shouldSkip(cr, instanceType, beanMethod.Item2))
                    {
                        return;
                    }

                    //注册到统一缓存
                    var compoment = new ComponentModel
                    {
                        CurrentType = beanMethod.Item3,
                        InjectProperties = true,
                        InjectPropertyType = InjectPropertyType.Autowired,
                        AutofacScope = beanMethod.Item1.AutofacScope,
                        InitMethod = beanMethod.Item1.InitMethod,
                        DestroyMethod = beanMethod.Item1.DestroyMethod,
                        RegisterType = RegisterType.Bean,
                        DependsOn = beanMethod.Item1.DependsOn
                    };
                    cache?.ComponentModelCache?.TryAdd(beanMethod.Item3, compoment);

                    var rb = RegistrationBuilder.ForDelegate(instanceType, ((context, parameters) =>
                    {
                        var autoConfigurationInstance = context.Resolve(autoConfigurationDetail.AutoConfigurationClassType);
                        var instance = MethodInvokeHelper.InvokeInstanceMethod(context, autoConfigurationDetail, autoConfigurationInstance,
                            beanMethod.Item2);
                        if (instance is Task)
                        {
                            return typeof(Task<>).MakeGenericType(instanceType).GetProperty("Result")?.GetValue(instance);
                        }

                        return instance;
                    })).ConfigurePipeline(p =>
                    {
                        //DepondsOn注入
                        p.Use(PipelinePhase.RegistrationPipelineStart, (context, next) =>
                        {
                            if (compoment.DependsOn != null && compoment.DependsOn.Any())
                            {
                                foreach (var dependsType in compoment.DependsOn)
                                {
                                    new Autowired(false).Resolve(context, compoment.CurrentType, dependsType, dependsType.Name, context.Parameters.ToList());
                                }
                            }

                            next(context);
                        });
                    });
                    rb.WithMetadata(AutofacAnnotationModule._AUTOFAC_SPRING, true);
                    if (!string.IsNullOrEmpty(beanMethod.Item1.Key))
                    {
                        rb.Keyed(beanMethod.Item1.Key, instanceType).Named("`1System.Collections.Generic.IEnumerable`1" + instanceType.FullName, instanceType);
                    }
                    else
                    {
                        rb.As(instanceType).Named("`1System.Collections.Generic.IEnumerable`1" + instanceType.FullName, instanceType);
                    }

                    //bean也可以注册多例 默认是单例
                    AutofacAnnotationModule.SetLifetimeScope(compoment, rb);
                    //bean也可以支持initMethod 和 DestroyMethod
                    AutofacAnnotationModule.RegisterMethods(compoment, rb);
                    RegistrationBuilder.RegisterSingleComponent(cr, rb);
                });
            }
        }
    }


    internal static class MethodInvokeHelper
    {
        /// <summary>
        /// 调用切面的拦截方法
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="methodInfo"></param>
        /// <param name="parameters"></param>
        /// <param name="context"></param>
        /// <param name="invocation"></param>
        /// <param name="_next"></param>
        /// <param name="returnValue"></param>
        /// <param name="returnParam"></param>
        /// <param name="injectAnotation"></param>
        /// <returns></returns>
        public static object InvokeInstanceMethod(object instance, MethodInfo methodInfo, ParameterInfo[] parameters, IComponentContext context,
            AspectContext invocation = null, AspectDelegate _next = null, object returnValue = null, string returnParam = null,
            Attribute injectAnotation = null)
        {
            if (parameters == null || parameters.Length == 0)
            {
                return methodInfo.Invoke(instance, null);
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

                if (_next != null && parameter.ParameterType == typeof(AspectDelegate))
                {
                    parameterObj.Add(_next);
                    continue;
                }

                if (injectAnotation != null && parameter.ParameterType == injectAnotation.GetType())
                {
                    parameterObj.Add(injectAnotation);
                    continue;
                }

                if (returnValue != null && !string.IsNullOrWhiteSpace(returnParam) && parameter.Name.Equals(returnParam))
                {
                    //如果指定的类型会出错
                    parameterObj.Add(returnValue);
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


                //如果拿不到就默认
                context.TryResolve(parameter.ParameterType, out var obj);
                parameterObj.Add(obj);
            }

            return methodInfo.Invoke(instance, parameterObj.ToArray());
        }


        public static object InvokeInstanceMethod(object instance, MethodInfo methodInfo, IComponentContext context,
            AspectContext invocation = null, AspectDelegate _next = null, object returnValue = null, string returnParam = null,
            Attribute injectAnotation = null)
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
                    if (invocation != null && parameter.ParameterType == typeof(AspectContext))
                    {
                        parameterObj.Add(invocation);
                        continue;
                    }

                    if (_next != null && parameter.ParameterType == typeof(AspectDelegate))
                    {
                        parameterObj.Add(_next);
                        continue;
                    }

                    if (injectAnotation != null && parameter.ParameterType == injectAnotation.GetType())
                    {
                        parameterObj.Add(injectAnotation);
                        continue;
                    }

                    if (returnValue != null && !string.IsNullOrWhiteSpace(returnParam) && parameter.Name.Equals(returnParam))
                    {
                        //如果指定的类型会出错
                        parameterObj.Add(returnValue);
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


                    //如果拿不到就默认
                    context.TryResolve(parameter.ParameterType, out var obj);
                    parameterObj.Add(obj);
                }

                return methodInfo.Invoke(instance, parameterObj.ToArray());
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"The class `{methodInfo.DeclaringType.FullName}` method `{methodInfo.Name}` invoke fail!", e);
            }
        }

        public static object InvokeInstanceMethod(IComponentContext context, AutoConfigurationDetail autoConfigurationDetail, object autoConfigurationInstance,
            MethodInfo methodInfo)
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
                        parameterObj.Add(value.ResolveParameterWithConfiguration(autoConfigurationDetail, parameter, context));
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
                throw new InvalidOperationException(
                    $"The Configuration class `{autoConfigurationDetail.AutoConfigurationClassType.FullName}` method `{methodInfo.Name}` invoke fail!", e);
            }
        }
    }

    /// <summary>
    /// AutoConfiguration的类代理
    /// </summary>
    [Component(typeof(AutoConfigurationIntercept), NotUseProxy = true)]
    public class AutoConfigurationIntercept : AsyncInterceptor
    {
#pragma warning disable 649
        [Autowired] private ComponentModelCacheSingleton cache;
#pragma warning restore 649

        /// <summary>
        /// 单例对象缓存
        /// </summary>
        private ConcurrentDictionary<MethodInfo, object> _instanceCache = new ConcurrentDictionary<MethodInfo, object>();


        /// <summary>
        /// 
        /// </summary>
        /// <param name="invocation"></param>
        /// <exception cref="NotImplementedException"></exception>
        protected override void Intercept(IInvocation invocation)
        {
            cache.ComponentModelCache.TryGetValue(invocation.MethodInvocationTarget.ReturnType, out var componentModel);
            if (invocation.MethodInvocationTarget.ReturnType == typeof(void))
            {
                invocation.Proceed();
                return;
            }


            //单例的
            if (componentModel?.AutofacScope == AutofacScope.SingleInstance && _instanceCache.TryGetValue(invocation.Method, out var instance))
            {
                invocation.ReturnValue = instance;
                return;
            }

            invocation.Proceed();

            _instanceCache.TryAdd(invocation.Method, invocation.ReturnValue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="invocation"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        protected override async ValueTask InterceptAsync(IAsyncInvocation invocation)
        {
            cache.ComponentModelCache.TryGetValue(invocation.TargetMethod.ReturnType, out var componentModel);

            //单例的
            if (componentModel?.AutofacScope == AutofacScope.SingleInstance && _instanceCache.TryGetValue(invocation.TargetMethod, out var instance))
            {
                invocation.Result = instance;
                return;
            }

            await invocation.ProceedAsync();

            _instanceCache.TryAdd(invocation.TargetMethod, invocation.Result);
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
        public List<Tuple<Bean, MethodInfo, Type>> BeanMethodInfoList { get; set; }


        /// <summary>
        /// 数据源
        /// </summary>
        public List<MetaSourceData> MetaSourceDataList { get; set; }
    }
}