using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.Extensions.Reflection;
using Autofac.Annotation.Config;
using Autofac.Annotation.Util;
using Autofac.AspectIntercepter;
using Autofac.AspectIntercepter.Pointcut;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;
using Autofac.Core.Resolving.Pipeline;
using Autofac.Features.AttributeFilters;
using Autofac.Features.Variance;
using Castle.DynamicProxy;
using Microsoft.Extensions.Configuration;

#pragma warning disable CS0618

namespace Autofac.Annotation
{
    /// <inheritdoc />
    /// <summary>
    ///     autofac模块用注解的模式注册
    /// </summary>
    public partial class AutofacAnnotationModule : Module
    {
        internal const string _ALL_COMPOMENT = "_ALL_COMPOMENT";
        internal const string _AUTOFAC_SPRING = "autofac_spring";

        /// <summary>
        ///     默认的DataSource 只保存1个实例
        /// </summary>
        private readonly Lazy<MetaSourceData> DefaultMeaSourceData = new Lazy<MetaSourceData>(GetDefaultMetaSource);

        private List<Assembly> _assemblyList = new List<Assembly>();
        private ILifetimeScope autofacGlobalScope;


        /// <summary>
        ///     当前默认的Scope
        /// </summary>
        public AutofacScope DefaultAutofacScope { get; private set; } = AutofacScope.Default;

        /// <summary>
        ///     是否启用Autowired的循环注入
        /// </summary>
        public bool AllowCircularDependencies { get; private set; }

        /// <summary>
        ///     容器注册完开始找AutofacConfiguration标签的class 有多个的时候指定Key
        /// </summary>
        public string AutofacConfigurationKey { get; private set; }

        /// <summary>
        ///     是否开启EventBus
        /// </summary>
        public bool EnableAutofacEventBus { get; private set; } = true;

        /// <summary>
        ///     是否开启文件监听
        /// </summary>
        public bool EnableValueResourceReloadOnchange { get; private set; } = true;

        /// <summary>
        ///     自动注册父类
        /// </summary>
        public bool AutoRegisterParentClass { get; private set; } = true;

        /// <summary>
        ///     自动注册的时候如果父类是抽象class是否忽略
        /// </summary>
        public bool IgnoreAutoRegisterAbstractClass { get; private set; }

        /// <summary>
        ///     当調用autofac自帶的註冊方式也能识别Autowired和Value的注入
        /// </summary>
        public bool AutoAttachFromAutofacRegister { get; private set; } = true;

        /// <summary>
        ///     自动按实现的接口注册，默认为false
        /// </summary>
        public bool AutoRegisterInterface { get; private set; } = true;

        /// <summary>
        ///     自动 Component 探测，作为 Component 特性的补充。
        /// </summary>
        internal IComponentDetector ComponentDetector { get; private set; }


        /// <summary>
        ///     添加assembly自动被扫描
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public AutofacAnnotationModule RegisterAssembly(Assembly assembly)
        {
            _assemblyList.Add(assembly);
            return this;
        }

        /// <summary>
        ///     是否开启文件监听 默认true
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        public AutofacAnnotationModule SetValueResourceReloadOnChange(bool flag)
        {
            EnableValueResourceReloadOnchange = flag;
            DefaultMeaSourceData.Value.Reload = flag;
            var metaSource = DefaultMeaSourceData.Value;
            DefaultMeaSourceData.Value.ConfigurationLazy = new Lazy<IConfiguration>(() =>
                EmbeddedConfiguration.Load(null, metaSource.Path, metaSource.MetaSourceType, metaSource.Embedded, flag));
            return this;
        }


        /// <summary>
        ///     容器注册完开始找AutofacConfiguration标签的class 有多个的时候指定Key
        /// </summary>
        /// <returns></returns>
        public AutofacAnnotationModule SetAutofacConfigurationKey(string key)
        {
            AutofacConfigurationKey = key ?? throw new ArgumentException(nameof(key));
            return this;
        }

        /// <summary>
        ///     当調用autofac自帶的註冊方式也能识别Autowired和Value的注入
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        public AutofacAnnotationModule SetAutoAttachFromAutofacRegister(bool flag)
        {
            AutoAttachFromAutofacRegister = flag;
            return this;
        }

        /// <summary>
        ///     设置是否自动注册父类
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        public AutofacAnnotationModule SetAutoRegisterParentClass(bool flag)
        {
            AutoRegisterParentClass = flag;
            return this;
        }

        /// <summary>
        ///     设置是否自动注册接口
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        public AutofacAnnotationModule SetAutoRegisterInterface(bool flag)
        {
            AutoRegisterInterface = flag;
            return this;
        }

        /// <summary>
        ///     设置自动注册的时候如果父类是抽象class是否忽略
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        public AutofacAnnotationModule SetIgnoreAutoRegisterAbstractClass(bool flag)
        {
            IgnoreAutoRegisterAbstractClass = flag;
            return this;
        }

        /// <summary>
        ///     设置瞬时
        /// </summary>
        /// <returns></returns>
        public AutofacAnnotationModule SetDefaultAutofacScopeToInstancePerDependency()
        {
            DefaultAutofacScope = AutofacScope.InstancePerDependency;
            return this;
        }

        /// <summary>
        ///     设置单例
        /// </summary>
        /// <returns></returns>
        public AutofacAnnotationModule SetDefaultAutofacScopeToSingleInstance()
        {
            DefaultAutofacScope = AutofacScope.SingleInstance;
            return this;
        }

        /// <summary>
        ///     设置作用域
        /// </summary>
        /// <returns></returns>
        public AutofacAnnotationModule SetDefaultAutofacScopeToInstancePerLifetimeScope()
        {
            DefaultAutofacScope = AutofacScope.InstancePerLifetimeScope;
            return this;
        }

        /// <summary>
        ///     设置请求作用域
        /// </summary>
        /// <returns></returns>
        public AutofacAnnotationModule SetDefaultAutofacScopeToInstancePerRequest()
        {
            DefaultAutofacScope = AutofacScope.InstancePerRequest;
            return this;
        }

        /// <summary>
        ///     设置是否启用循环Autowired
        /// </summary>
        /// <returns></returns>
        public AutofacAnnotationModule SetAllowCircularDependencies(bool flag)
        {
            AllowCircularDependencies = flag;
            return this;
        }

        /// <summary>
        ///     设置是否启动eventBus
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        public AutofacAnnotationModule SetEnableAutofacEventBug(bool flag)
        {
            EnableAutofacEventBus = flag;
            return this;
        }

        /// <summary>
        ///     设置 <see cref="IComponentDetector" />
        /// </summary>
        /// <param name="componentDetector"></param>
        /// <returns></returns>
        public AutofacAnnotationModule SetComponentDetector(IComponentDetector componentDetector)
        {
            ComponentDetector = componentDetector;
            return this;
        }

        /// <summary>
        /// </summary>
        /// <param name="componentRegistry"></param>
        /// <param name="registration"></param>
        protected override void AttachToComponentRegistration(IComponentRegistryBuilder componentRegistry, IComponentRegistration registration)
        {
            //开关
            if (!AutoAttachFromAutofacRegister) return;

            //明确要求不需要被扫描
            if (registration.Metadata.ContainsKey(AutofacSpring.DISABLE_AUTO_INCLUE_INTO_COMPOMENT)) return;

            var currentType = registration.Activator.LimitType;

            if (typeof(IProxyTargetAccessor).IsAssignableFrom(currentType) && currentType.BaseType != null) currentType = currentType.BaseType;

            //过滤掉框架类
            if (currentType.Assembly == GetType().Assembly || currentType.Assembly == typeof(LifetimeScope).Assembly) return;

            if (!componentRegistry.Properties.Any() || !componentRegistry.Properties.ContainsKey(_ALL_COMPOMENT)) return;

            var allCompoment = componentRegistry.Properties[_ALL_COMPOMENT] as ComponentModelCacheSingleton;
            if (allCompoment == null) return;

            //过滤掉框架已经注册的类
            if (allCompoment.ComponentModelCache.ContainsKey(currentType)) return;

            var isGeneric = currentType.IsGenericType || currentType.GetTypeInfo().IsGenericTypeDefinition;
            if (isGeneric && allCompoment.DynamicComponentModelCache.ContainsKey(currentType.Namespace + currentType.Name)) return;

            //过滤掉框架的AutoConfiguration注册的类
            if (registration.Metadata.ContainsKey(_AUTOFAC_SPRING)) return;

            //剩下的就是自己注册的
            //检测自己注册的是否存在有Autowired 和 Value
            var component = CreateCompomentFromAutofac(currentType, registration, allCompoment, isGeneric);

            if (isGeneric)
                allCompoment.DynamicComponentModelCache.TryAdd(currentType.Namespace + currentType.Name, component.Item1);
            else
                allCompoment.ComponentModelCache.TryAdd(component.Item1.CurrentType, component.Item1);

            if (isGeneric || component.Item2 == null) return;

            componentRegistry.Register(component.Item2.CreateRegistration());
        }

        /// <summary>
        ///     autofac加载
        /// </summary>
        /// <param name="builder">注意这个builder是被Module类new出来的 不是最外层的</param>
        protected override void Load(ContainerBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            if (_assemblyList == null || !_assemblyList.Any()) _assemblyList = getCurrentDomainAssemblies().ToList();

            _assemblyList.Add(typeof(AutofacAnnotationModule).Assembly);
            _assemblyList = _assemblyList.Distinct().ToList();

            if (EnableAutofacEventBus)
            {
                builder.RegisterSource(new ContravariantRegistrationSource());
                builder.RegisterEventing();
            }

            //注册自己 让后面的逻辑可以拿到配置参数
            builder.RegisterInstance(this).SingleInstance();
            builder.Properties[nameof(AutofacAnnotationModule)] = this;

            builder.RegisterBuildCallback(r =>
            {
                autofacGlobalScope = r;
                r.TryResolve<IEnumerable<BeanPostProcessor>>(out var beanPostProcessors);
                if (beanPostProcessors != null) r.ComponentRegistry.Properties[nameof(List<BeanPostProcessor>)] = beanPostProcessors?.ToList();
            });

            //解析程序集PointCut标签类和方法
            var pointCutCfg = GetPointCutConfiguration(builder);
            //解析程序集拿到打了pointcut的类 打了Compoment的类 解析Import的类
            var componetList = GetAllComponent(builder, pointCutCfg);
            var cache = builder.Properties[_ALL_COMPOMENT] as ComponentModelCacheSingleton;
            if (cache != null) cache.PointCutConfigurationList = pointCutCfg;
            foreach (var component in componetList)
            {
                //Conditional
                if (shouldSkip(builder.ComponentRegistryBuilder, component.CurrentType))
                {
                    if (component.isDynamicGeneric)
                        cache?.ComponentModelCache?.TryRemove(component.CurrentType, out _);
                    else
                        cache?.DynamicComponentModelCache?.TryRemove(component.CurrentType.Namespace + component.CurrentType.Name, out _);

                    continue;
                }

                //注册本身
                IRegistrationBuilder<object, ReflectionActivatorData, object> registrar = null;
                if (component.isDynamicGeneric)
                    registrar = builder.RegisterGeneric(component.CurrentType).WithAttributeFiltering();
                else
                    registrar = builder.RegisterType(component.CurrentType).WithAttributeFiltering();


                //如果没有指定的话就是注册本身类型 否则就是as注册
                //指定一个规则 像spring一样
                // 1. 如果指定了类型 那么不仅要as 也要注册本身类型
                // 2. 如果没有指定类型 那只需要注册本身类型
                //後置處理器
                RegisterBeforeBeanPostProcessor(component, registrar);

                //注册非本身类型 比如注册父类
                RegisterComponentServices(component, registrar);

                //构造方法的参数注入采用autofac原生支持的 ParameterFilterAttribute

                //property或者field的value标签注入动态值
                RegisterComponentValues(component, registrar);

                //设置生命周期
                SetLifetimeScope(component, registrar);

                //设置Ownership
                SetComponentOwnership(component, registrar);

                //属性依赖注入
                SetInjectProperties(component, registrar);

                //activation
                SetAutoActivate(component, registrar);

                //拦截器
                SetIntercept(component, registrar, pointCutCfg);

                //方法注册
                RegisterMethods(component, registrar);

                //後置處理器
                RegisterAfterBeanPostProcessor(component, registrar);

                //注册DependsOn
                RegisterDependsOn(component, registrar);
            }

            DoAutofacConfiguration(builder);
        }

        /// <summary>
        ///     init方法和Release方法
        /// </summary>
        /// <typeparam name="TReflectionActivatorData"></typeparam>
        /// <param name="component"></param>
        /// <param name="registrar"></param>
        internal static void RegisterMethods<TReflectionActivatorData>(ComponentModel component,
            IRegistrationBuilder<object, TReflectionActivatorData, object> registrar)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            if (registrar == null) throw new ArgumentNullException(nameof(registrar));

            {
                if (component.isDynamicGeneric)
                {
                    registrar.OnActivated(e =>
                    {
                        var postConstructs = ReflectionExtensions.AssertMethodDynamic<PostConstruct>(e.Instance.GetType());
                        var method = ReflectionExtensions.AssertMethod(e.Instance.GetType(), component.InitMethod);
                        if (postConstructs.Any()) postConstructs.ForEach(r => { AutoConfigurationHelper.InvokeInstanceMethod(e.Instance, r, e.Context); });

                        if (method != null) AutoConfigurationHelper.InvokeInstanceMethod(e.Instance, method, e.Context);
                    });
                }
                else
                {
                    var postConstructs = ReflectionExtensions.AssertMethodDynamic<PostConstruct>(component.CurrentType);
                    var method = ReflectionExtensions.AssertMethod(component.CurrentType, component.InitMethod);
                    registrar.OnActivated(e =>
                    {
                        if (postConstructs.Any()) postConstructs.ForEach(r => { AutoConfigurationHelper.InvokeInstanceMethod(e.Instance, r, e.Context); });

                        if (method != null) AutoConfigurationHelper.InvokeInstanceMethod(e.Instance, method, e.Context);
                    });
                }
            }

            {
                if (component.isDynamicGeneric)
                {
                    registrar.OnRelease(e =>
                    {
                        var destroys = ReflectionExtensions.AssertMethodDynamic<PreDestory>(component.CurrentType);
                        if (destroys.Any())
                            destroys.ForEach(r =>
                            {
                                if (r.GetParameters().Any())
                                    throw new DependencyResolutionException(
                                        $"class `{component.CurrentType.FullName}` DestroyMethod `PreDestory` must be no parameters");
                            });

                        var method = ReflectionExtensions.AssertMethod(e.GetType(), component.DestroyMethod);
                        if (method != null && method.GetParameters().Any())
                            throw new DependencyResolutionException(
                                $"class `{component.CurrentType.FullName}` DestroyMethod `{component.DestroyMethod}` must be no parameters");

                        if (destroys.Any()) destroys.ForEach(r => { r.Invoke(e, null); });

                        if (method != null) method.Invoke(e, null);
                    });
                }
                else
                {
                    var destroys = ReflectionExtensions.AssertMethodDynamic<PreDestory>(component.CurrentType);
                    if (destroys.Any())
                        destroys.ForEach(r =>
                        {
                            if (r.GetParameters().Any())
                                throw new DependencyResolutionException(
                                    $"class `{component.CurrentType.FullName}` DestroyMethod `PreDestory` must be no parameters");
                        });

                    var method = ReflectionExtensions.AssertMethod(component.CurrentType, component.DestroyMethod);
                    if (method != null && method.GetParameters().Any())
                        throw new DependencyResolutionException(
                            $"class `{component.CurrentType.FullName}` DestroyMethod `{component.DestroyMethod}` must be no parameters");

                    registrar.OnRelease(e =>
                    {
                        if (destroys.Any()) destroys.ForEach(r => { r.Invoke(e, null); });

                        if (method != null) method.Invoke(e, null);
                    });
                }
            }
        }

        /// <summary>
        ///     拦截器
        /// </summary>
        /// <param name="component"></param>
        /// <param name="builder"></param>
        /// <param name="needWarpPointcut"></param>
        private void SetIntercept(ComponentModel component,
            IRegistrationBuilder<object, ReflectionActivatorData, object> builder, bool needWarpPointcut)
        {
            if (needWarpPointcut)
            {
                if (component.CurrentType.GetCustomAttribute<ClassInterceptor>() != null)
                {
                    builder.EnableClassInterceptors().InterceptedBy(typeof(PointcutIntercept)).WithMetadata(_AUTOFAC_SPRING, true);
                    return;
                }

                if (component.CurrentType.GetCustomAttribute<InterfaceInterceptor>() != null)
                {
                    builder.EnableInterfaceInterceptors().InterceptedBy(typeof(PointcutIntercept)).WithMetadata(_AUTOFAC_SPRING, true);
                    return;
                }

                //找寻它的继承的接口列表下是否存在相同的namespace下的接口
                if (component.CurrentType.GetTypeInfo().ImplementedInterfaces
                    .Any(r => r.Assembly.Equals(component.CurrentType.Assembly)))
                {
                    builder.EnableInterfaceInterceptors().InterceptedBy(typeof(PointcutIntercept)).WithMetadata(_AUTOFAC_SPRING, true);
                    return;
                }

                builder.EnableClassInterceptors().InterceptedBy(typeof(PointcutIntercept)).WithMetadata(_AUTOFAC_SPRING, true);
            }
            else if (component.EnableAspect)
            {
                if (component.isDynamicGeneric)
                    //动态泛型类的话 只能用 interface拦截器 
                    builder.EnableInterfaceInterceptors().InterceptedBy(typeof(AdviceIntercept)).WithMetadata(_AUTOFAC_SPRING, true);
                else if (component.CurrentType.GetCustomAttribute<InterfaceInterceptor>() != null)
                    //打了[InterfaceInterceptor]标签
                    builder.EnableInterfaceInterceptors().InterceptedBy(typeof(AdviceIntercept)).WithMetadata(_AUTOFAC_SPRING, true);
                else if (component.InterceptorType == InterceptorType.Interface)
                    //指定了 interface 拦截器 或
                    builder.EnableInterfaceInterceptors().InterceptedBy(typeof(AdviceIntercept)).WithMetadata(_AUTOFAC_SPRING, true);
                else
                    //默认是class + virtual 拦截器
                    builder.EnableClassInterceptors().InterceptedBy(typeof(AdviceIntercept)).WithMetadata(_AUTOFAC_SPRING, true);
            }
        }

        /// <summary>
        ///     拦截器
        /// </summary>
        /// <typeparam name="TLimit"></typeparam>
        /// <typeparam name="TConcreteReflectionActivatorData"></typeparam>
        /// <param name="component"></param>
        /// <param name="registrar"></param>
        /// <param name="pointCutCfg"></param>
        private void SetIntercept<TLimit, TConcreteReflectionActivatorData>(ComponentModel component,
            IRegistrationBuilder<TLimit, TConcreteReflectionActivatorData, object> registrar, PointCutConfigurationList pointCutCfg)
            where TConcreteReflectionActivatorData : ReflectionActivatorData
        {
            if (registrar == null) throw new ArgumentNullException(nameof(registrar));

            if (component.EnableAspect && component.Interceptor != null)
                throw new InvalidOperationException(
                    $"'{component.CurrentType.FullName}' can not interceptor by both EnableAspect and Interceptor:'{component.Interceptor.FullName}' ");
            if (component.Interceptor != null)
                //配置拦截器
                switch (component.InterceptorType)
                {
                    case InterceptorType.Interface:
                        if (!string.IsNullOrEmpty(component.InterceptorKey))
                        {
                            registrar.EnableInterfaceInterceptors().InterceptedBy(new KeyedService(component.InterceptorKey, component.Interceptor))
                                .WithMetadata(_AUTOFAC_SPRING, true);
                            return;
                        }

                        registrar.EnableInterfaceInterceptors().InterceptedBy(component.Interceptor).WithMetadata(_AUTOFAC_SPRING, true);
                        return;
                    case InterceptorType.Class:
                        if (!string.IsNullOrEmpty(component.InterceptorKey))
                        {
                            registrar.EnableClassInterceptors().InterceptedBy(new KeyedService(component.InterceptorKey, component.Interceptor))
                                .WithMetadata(_AUTOFAC_SPRING, true);
                            return;
                        }

                        registrar.EnableClassInterceptors().InterceptedBy(component.Interceptor).WithMetadata(_AUTOFAC_SPRING, true);
                        return;
                    default:
                        throw new InvalidOperationException(
                            $"'{component.CurrentType.FullName}' can not interceptor by both EnableAspect and Interceptor:'{component.Interceptor.FullName}' ");
                }

            registrar.As(component.CurrentType);

            //某些不能被设置为代理 防止出错
            if (component.NotUseProxy) return;

            //PointCut 包含 Aspect 。反之不可以
            if (NeedWarpForPointcut(component, pointCutCfg))
            {
                if (component.CurrentType.GetCustomAttribute<ClassInterceptor>() != null)
                {
                    registrar.EnableClassInterceptors().InterceptedBy(typeof(PointcutIntercept)).WithMetadata(_AUTOFAC_SPRING, true);
                    return;
                }

                if (component.CurrentType.GetCustomAttribute<InterfaceInterceptor>() != null)
                {
                    registrar.EnableInterfaceInterceptors().InterceptedBy(typeof(PointcutIntercept)).WithMetadata(_AUTOFAC_SPRING, true);
                    return;
                }

                //找寻它的继承的接口列表下是否存在相同的namespace下的接口
                if (component.CurrentType.GetTypeInfo().ImplementedInterfaces
                    .Any(r => r.Assembly.Equals(component.CurrentType.Assembly)))
                {
                    registrar.EnableInterfaceInterceptors().InterceptedBy(typeof(PointcutIntercept)).WithMetadata(_AUTOFAC_SPRING, true);
                    return;
                }

                registrar.EnableClassInterceptors().InterceptedBy(typeof(PointcutIntercept)).WithMetadata(_AUTOFAC_SPRING, true);
            }
            else if (component.EnableAspect)
            {
                if (component.isDynamicGeneric)
                    //动态泛型类的话 只能用 interface拦截器 
                    registrar.EnableInterfaceInterceptors().InterceptedBy(typeof(AdviceIntercept)).WithMetadata(_AUTOFAC_SPRING, true);
                else if (component.CurrentType.GetCustomAttribute<InterfaceInterceptor>() != null)
                    //打了[InterfaceInterceptor]标签
                    registrar.EnableInterfaceInterceptors().InterceptedBy(typeof(AdviceIntercept)).WithMetadata(_AUTOFAC_SPRING, true);
                else if (component.InterceptorType == InterceptorType.Interface)
                    //指定了 interface 拦截器 或
                    registrar.EnableInterfaceInterceptors().InterceptedBy(typeof(AdviceIntercept)).WithMetadata(_AUTOFAC_SPRING, true);
                else
                    //默认是class + virtual 拦截器
                    registrar.EnableClassInterceptors().InterceptedBy(typeof(AdviceIntercept)).WithMetadata(_AUTOFAC_SPRING, true);
            }
        }


        /// <summary>
        ///     查看是否存在需要拦截的
        /// </summary>
        /// <param name="component"></param>
        /// <param name="pointCutCfg"></param>
        /// <returns></returns>
        private bool NeedWarpForPointcut(ComponentModel component, PointCutConfigurationList pointCutCfg)
        {
            var targetClass = component.CurrentType;
            if (targetClass.IsAbstract || targetClass.IsInterface) return false;

            var result = false;

            if (pointCutCfg == null || pointCutCfg.PointcutConfigurationInfoList == null || !pointCutCfg.PointcutConfigurationInfoList.Any())
            {
                return false;
            }

            if (pointCutCfg.PointcutTypeInfoList.ContainsKey(targetClass)) return true;

            foreach (var aspectClass in pointCutCfg.PointcutConfigurationInfoList)
            {
                //切面 不能 切自己
                if (aspectClass.PointClass == component.CurrentType)
                {
                    pointCutCfg.PointcutTypeInfoList.TryAdd(targetClass, true);
                    return false;
                }

                //先检查class是否满足  pointCutClassInjectAnotation指的是 这个切面 在这个class上有没有对应的
                if (!aspectClass.Pointcut.IsVaildClass(component, out var pointCutClassInjectAnotation)) continue;

                //查看里面的method是否有满足的 包含查询继承的方法
                foreach (var method in targetClass.GetAllInstanceMethod(component.EnablePointcutInherited))
                {
                    //pointCutMethodInjectAnotation 指的是 这个切面 在method 有没有对应的 如果method没有 class有就用class的
                    if (!aspectClass.Pointcut.IsVaild(component, method, pointCutClassInjectAnotation, out var pointCutMethodInjectAnotation)) continue;

                    if (component.isDynamicGeneric)
                    {
                        var uniqKey = method.GetMethodInfoUniqueName();
                        if (pointCutCfg.DynamicPointcutTargetInfoList.ContainsKey(uniqKey))
                            pointCutCfg.DynamicPointcutTargetInfoList[uniqKey]
                                .Add(new RunTimePointCutConfiguration(aspectClass, pointCutMethodInjectAnotation));
                        else
                            pointCutCfg.DynamicPointcutTargetInfoList.TryAdd(uniqKey,
                                new List<RunTimePointCutConfiguration> { new RunTimePointCutConfiguration(aspectClass, pointCutMethodInjectAnotation) });
                    }
                    else
                    {
                        var key = new ObjectKey(targetClass, method);
                        if (pointCutCfg.PointcutTargetInfoList.ContainsKey(key))
                            pointCutCfg.PointcutTargetInfoList[key].Add(new RunTimePointCutConfiguration(aspectClass, pointCutMethodInjectAnotation));
                        else
                            pointCutCfg.PointcutTargetInfoList.TryAdd(key,
                                new List<RunTimePointCutConfiguration> { new RunTimePointCutConfiguration(aspectClass, pointCutMethodInjectAnotation) });
                    }

                    result = true;
                }
            }

            pointCutCfg.PointcutTypeInfoList.TryAdd(targetClass, true);
            return result;
        }


        /// <summary>
        ///     Sets the auto activation mode for the component.
        /// </summary>
        /// <typeparam name="TReflectionActivatorData"></typeparam>
        /// <param name="component"></param>
        /// <param name="registrar"></param>
        private void SetAutoActivate<TReflectionActivatorData>(ComponentModel component,
            IRegistrationBuilder<object, TReflectionActivatorData, object> registrar)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            if (registrar == null) throw new ArgumentNullException(nameof(registrar));

            if (!component.AutoActivate) return;

            if (component.AutoActivate && component.AutofacScope == AutofacScope.SingleInstance)
                //默认单例注册完成后自动装载
                registrar.AutoActivate();
        }

        /// <summary>
        ///     设置Ownership
        /// </summary>
        /// <typeparam name="TReflectionActivatorData"></typeparam>
        /// <param name="component"></param>
        /// <param name="registrar"></param>
        private void SetComponentOwnership<TReflectionActivatorData>(ComponentModel component,
            IRegistrationBuilder<object, TReflectionActivatorData, object> registrar)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            if (registrar == null) throw new ArgumentNullException(nameof(registrar));

            switch (component.Ownership)
            {
                case Ownership.External:
                    registrar.ExternallyOwned();
                    return;
                case Ownership.LifetimeScope:
                    registrar.OwnedByLifetimeScope();
                    return;
            }
        }

        /// <summary>
        ///     设置scope
        /// </summary>
        /// <typeparam name="TReflectionActivatorData"></typeparam>
        /// <param name="component"></param>
        /// <param name="registrar"></param>
        internal static void SetLifetimeScope<TReflectionActivatorData>(ComponentModel component,
            IRegistrationBuilder<object, TReflectionActivatorData, object> registrar)
        {
            if (registrar == null) throw new ArgumentNullException(nameof(registrar));

            switch (component.AutofacScope)
            {
                case AutofacScope.Default:
                    registrar.InstancePerDependency();
                    return;
                case AutofacScope.InstancePerDependency:
                    registrar.InstancePerDependency();
                    return;
                case AutofacScope.SingleInstance:
                    registrar.SingleInstance();
                    return;
                case AutofacScope.InstancePerLifetimeScope:
                    registrar.InstancePerLifetimeScope();
                    return;
                case AutofacScope.InstancePerRequest:
                    registrar.InstancePerRequest();
                    return;
            }
        }


        /// <summary>
        ///     注册Component
        /// </summary>
        /// <typeparam name="TReflectionActivatorData"></typeparam>
        /// <param name="component"></param>
        /// <param name="registrar"></param>
        private void RegisterComponentServices<TReflectionActivatorData>(ComponentModel component,
            IRegistrationBuilder<object, TReflectionActivatorData, object> registrar)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            if (registrar == null) throw new ArgumentNullException(nameof(registrar));

            if (component.ComponentServiceList != null && component.ComponentServiceList.Any())
                foreach (var componentServiceModel in component.ComponentServiceList)
                    if (!string.IsNullOrEmpty(componentServiceModel.Key))
                    {
                        registrar.Keyed(componentServiceModel.Key, componentServiceModel.Type)
                            .Named("`1System.Collections.Generic.IEnumerable`1" + componentServiceModel.Type.FullName,
                                componentServiceModel.Type); //通过集合注入Autowired拿到所有
                    }
                    else
                    {
                        if (component.isDynamicGeneric && !componentServiceModel.Type.IsGenericTypeDefinition)
                            throw new InvalidOperationException(
                                $"The class `{component.CurrentType.FullName}` must register as genericTypeDefinition, please use `[Component(typeOf(xxx<>))]` ");

                        registrar.As(componentServiceModel.Type)
                            .Named("`1System.Collections.Generic.IEnumerable`1" + componentServiceModel.Type.FullName,
                                componentServiceModel.Type); //通过集合注入Autowired拿到所有
                    }
        }


        /// <summary>
        ///     解析程序集
        ///     打了pointcut的类
        ///     打了Compoment的类
        ///     解析Import的类
        /// </summary>
        /// <returns></returns>
        private List<ComponentModel> GetAllComponent(ContainerBuilder builder, PointCutConfigurationList pointCutConfigurationList)
        {
            if (_assemblyList == null || _assemblyList.Count < 1) throw new ArgumentNullException(nameof(_assemblyList));

            var singleton = new ComponentModelCacheSingleton
            {
                ComponentModelCache = new ConcurrentDictionary<Type, ComponentModel>(),
                DynamicComponentModelCache = new ConcurrentDictionary<string, ComponentModel>()
            };
            try
            {
                var result = new List<ComponentModel>();
                var beanTypeList = new List<BeanDefination>();

                //获取打了PointCut的标签的class注册到DI
                foreach (var pointcutConfig in pointCutConfigurationList.PointcutConfigurationInfoList
                             .GroupBy(r => r.PointClass)
                             .ToDictionary(r => r.Key,
                                 y => y.ToList()))
                {
                    var pointCutAtt = pointcutConfig.Value.First();
                    beanTypeList.Add(new BeanDefination
                    {
                        Type = pointcutConfig.Key,
                        Bean = new Component(pointcutConfig.Key)
                        {
                            AutofacScope = AutofacScope.SingleInstance,
                            InitMethod = pointCutAtt.Pointcut.InitMethod,
                            DestroyMethod = pointCutAtt.Pointcut.DestroyMethod,
                            RegisterType = RegisterType.PointCut
                        },
                        OrderIndex = -1147483648
                    });
                }

                var importList = new List<Tuple<Import, Type>>();

                //从assembly里面解析打了Compoment标签的 或者 自定义设置了 ComponentDetector的采用ComponentDetector的方式去解析生产的Compoment
                foreach (var assembly in _assemblyList)
                {
                    if (assembly.IsDynamic) continue;
                    var types = assembly.GetExportedTypes();
                    //找到类型中含有 Component 标签的类 排除掉抽象类
                    var assemblBeanTypeList = (from type in types
                        let bean = type.GetComponent(ComponentDetector)
                        let order = type.GetCustomAttribute<Order>()
                        where type.IsClass && !type.IsAbstract && bean != null
                        select new BeanDefination
                        {
                            Type = type,
                            Bean = bean,
                            OrderIndex = order?.Index ?? bean.OrderIndex
                        }).ToList();
                    beanTypeList.AddRange(assemblBeanTypeList);


                    //找到类型中含有 Import 标签的类 排除掉抽象类
                    var importBeanTypeList = (from type in types
                        let bean = type.GetCustomAttribute<Import>()
                        where type.IsClass && !type.IsAbstract && bean != null
                        select new Tuple<Import, Type>(bean, type)).ToList();
                    importList.AddRange(importBeanTypeList);
                }

                beanTypeList.AddRange(DoImportComponent(importList));

                //拿到了所有的BenDefinition之后注册到DI容器里面去
                //和Spring一致优先使用类名排序再按照从小到大的顺序注册 如果同一个Type被处理多次会被覆盖！
                foreach (var bean in beanTypeList.OrderBy(r => r.OrderIndex).ThenBy(r => r.Type.Name))
                {
                    var component = EnumerateComponentServices(bean.Bean, bean.Type);
                    component.MetaSourceList = new List<MetaSourceData>();
                    EnumerateMetaSourceAttributes(component.CurrentType, component.MetaSourceList);
                    result.Add(component);
                    if (component.isDynamicGeneric)
                    {
                        if (component.AutoActivate) //动态泛型注册类 没法容器完成就实例化
                            throw new InvalidOperationException(
                                $"The class `{component.CurrentType.FullName}` register as genericTypeDefinition, can not set AutoActivate:true ");

                        singleton.DynamicComponentModelCache[bean.Type.Namespace + bean.Type.Name] = component;
                    }

                    singleton.ComponentModelCache[bean.Type] = component;
                }

                return result;
            }
            finally
            {
                builder.RegisterInstance(singleton).SingleInstance();
                builder.Properties.Add(_ALL_COMPOMENT, singleton);
            }
        }


        /// <summary>
        ///     解析程序集的Import标签并解析得到结果注册到DI容器
        /// </summary>
        /// <returns></returns>
        private List<BeanDefination> DoImportComponent(List<Tuple<Import, Type>> importList)
        {
            var result = new List<BeanDefination>();
            foreach (var import in importList)
            {
                //查看当前的Type的类型是否是继承了ImportSelector
                if (typeof(ImportSelector).IsAssignableFrom(import.Item2))
                    if (Activator.CreateInstance(import.Item2) is ImportSelector importSelectorInstance)
                    {
                        var temp = importSelectorInstance.SelectImports();
                        if (temp == null || !temp.Any()) continue;

                        foreach (var beanDefination in temp) beanDefination.Bean.RegisterType = RegisterType.Import;

                        result.AddRange(temp);
                    }

                if (import.Item1.ImportTypes == null || !import.Item1.ImportTypes.Any()) continue;

                //直接注册进来
                foreach (var item in import.Item1.ImportTypes)
                    if (typeof(ImportSelector).IsAssignableFrom(item))
                    {
                        if (!(Activator.CreateInstance(item) is ImportSelector importSelectorInstance)) continue;
                        var temp = importSelectorInstance.SelectImports();
                        if (temp == null || !temp.Any()) continue;

                        foreach (var beanDefination in temp) beanDefination.Bean.RegisterType = RegisterType.Import;

                        result.AddRange(temp);
                    }
                    else
                    {
                        var temp = new BeanDefination(item);
                        temp.Bean.RegisterType = RegisterType.Import;
                        result.Add(temp);
                    }
            }

            return result;
        }


        /// <summary>
        ///     解析程序集AutoConfiguration标签并解析里面打了Bean标签的方法并注册到DI容器
        /// </summary>
        /// <param name="builder"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private void DoAutofacConfiguration(ContainerBuilder builder)
        {
            var list = new AutoConfigurationList
            {
                AutoConfigurationDetailList = new List<AutoConfigurationDetail>()
            };
            var cache = builder.Properties[_ALL_COMPOMENT] as ComponentModelCacheSingleton;
            try
            {
                var allConfiguration = GetAllAutofacConfiguration();
                if (!allConfiguration.Any()) return;
                if (!string.IsNullOrEmpty(AutofacConfigurationKey))
                    allConfiguration = allConfiguration.Where(r => r.Key.Equals(AutofacConfigurationKey)).ToList();

                foreach (var configuration in allConfiguration)
                {
                    if (configuration.GetType().IsGenericType)
                        throw new InvalidOperationException(
                            $"The Configuration class `{configuration.Type.FullName}` must not be genericType!");

                    //Conditional
                    if (shouldSkip(builder.ComponentRegistryBuilder, configuration.Type)) continue;

                    var beanTypeMethodList = configuration.Type.GetAllInstanceMethod(false);
                    var bean = new AutoConfigurationDetail
                    {
                        AutoConfigurationClassType = configuration.Type,
                        BeanMethodInfoList = new List<Tuple<Bean, MethodInfo, Type>>(),
                        MetaSourceDataList = new List<MetaSourceData>()
                    };
                    foreach (var beanTypeMethod in beanTypeMethodList)
                    {
                        var beanAttribute = beanTypeMethod.GetCustomAttribute<Bean>();
                        if (beanAttribute == null) continue;

                        var returnType = beanTypeMethod.ReturnType;
                        if (returnType == typeof(void))
                            throw new InvalidOperationException(
                                $"The Configuration class `{configuration.Type.FullName}` method `{beanTypeMethod.Name}` returnType must not be void!");

                        //查看是否是Task
                        if (typeof(Task).IsAssignableFrom(returnType))
                        {
                            if (!returnType.GetTypeInfo().IsGenericType)
                                throw new InvalidOperationException(
                                    $"The Configuration class `{configuration.Type.FullName}` method `{beanTypeMethod.Name}` returnType can not be Task!");

                            if (returnType.GetTypeInfo().GenericTypeArguments.Length > 1)
                                throw new InvalidOperationException(
                                    $"The Configuration class `{configuration.Type.FullName}` method `{beanTypeMethod.Name}` returnType can not be Task<?,?>!");

                            returnType = returnType.GetTypeInfo().GenericTypeArguments.First();
                        }

                        if (returnType.IsValueType || returnType.IsEnum)
                            throw new InvalidOperationException(
                                $"The Configuration class `{configuration.Type.FullName}` method `{beanTypeMethod.Name}` returnType is invalid!");

                        if (!ProxyUtil.IsAccessible(returnType))
                            throw new InvalidOperationException(
                                $"The Configuration class `{configuration.Type.FullName}` method `{beanTypeMethod.Name}` returnType is not accessible!");
                        var dependsOnAttribute = beanTypeMethod.GetCustomAttribute<DependsOn>();
                        if (dependsOnAttribute != null)
                        {
                            beanAttribute.DependsOn = dependsOnAttribute.dependsOn;
                            if (beanAttribute.DependsOn.Contains(returnType))
                                throw new InvalidOperationException(
                                    $"The Configuration class `{configuration.Type.FullName}` method `{beanTypeMethod.Name}` returnType can not dependency on itself!");
                        }

                        bean.BeanMethodInfoList.Add(new Tuple<Bean, MethodInfo, Type>(beanAttribute, beanTypeMethod, returnType));
                    }

                    var compoment = new ComponentModel
                    {
                        CurrentType = configuration.Type,
                        InjectProperties = true,
                        InjectPropertyType = InjectPropertyType.Autowired,
                        AutoActivate = true,
                        EnableAspect = true,
                        Interceptor = typeof(AutoConfigurationIntercept),
                        AutofacScope = AutofacScope.SingleInstance,
                        InterceptorType = InterceptorType.Class,
                        InitMethod = configuration.AutofacConfiguration.InitMethod,
                        DestroyMethod = configuration.AutofacConfiguration.DestroyMethod,
                        RegisterType = RegisterType.AutoConfiguration
                    };
                    cache?.ComponentModelCache.TryAdd(configuration.Type, compoment);
                    //注册为代理类
                    var rb = builder.RegisterType(configuration.Type).EnableClassInterceptors().InterceptedBy(typeof(AutoConfigurationIntercept))
                        .SingleInstance().WithMetadata(_AUTOFAC_SPRING, true); //注册为单例模式
                    list.AutoConfigurationDetailList.Add(bean);

                    RegisterMethods(compoment, rb);

                    EnumerateMetaSourceAttributes(bean.AutoConfigurationClassType, bean.MetaSourceDataList);
                }
            }
            finally
            {
                builder.RegisterInstance(list).SingleInstance();
            }

            AutoConfigurationSource.Register(builder, list);
        }

        /// <summary>
        ///     解析程序集PointCut标签类和方法
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        private PointCutConfigurationList GetPointCutConfiguration(ContainerBuilder builder)
        {
            var list = new PointCutConfigurationList
            {
                PointcutConfigurationInfoList = new List<PointcutConfigurationInfo>(),
                PointcutTargetInfoList = new ConcurrentDictionary<ObjectKey, List<RunTimePointCutConfiguration>>(),
                DynamicPointcutTargetInfoList = new ConcurrentDictionary<string, List<RunTimePointCutConfiguration>>(),
                PointcutTypeInfoList = new ConcurrentDictionary<Type, bool>()
            };
            try
            {
                var allConfiguration = DoPointcutConfiguration();
                if (!allConfiguration.Any()) return list;

                list.PointcutConfigurationInfoList = allConfiguration;
            }
            finally
            {
                builder.RegisterInstance(list).SingleInstance();
            }

            return list;
        }

        /// <summary>
        ///     解析程序集的AutofacConfiguration
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private List<AutofacConfigurationInfo> GetAllAutofacConfiguration()
        {
            if (_assemblyList == null || _assemblyList.Count < 1) throw new ArgumentNullException(nameof(_assemblyList));

            var result = new List<AutofacConfigurationInfo>();
            foreach (var assembly in _assemblyList)
            {
                if (assembly.IsDynamic) continue;
                var types = assembly.GetExportedTypes();
                //找到类型中含有 AutofacConfiguration 标签的类 排除掉抽象类
                var typeList = (from type in types
                    let bean = type.GetCustomAttribute<AutoConfiguration>()
                    let order = type.GetCustomAttribute<Order>()
                    where type.IsClass && !type.IsAbstract && bean != null
                    select new
                    {
                        Type = type,
                        Bean = bean,
                        Order = order
                    }).ToList();

                foreach (var configuration in typeList)
                    result.Add(new AutofacConfigurationInfo
                    {
                        Type = configuration.Type,
                        AutofacConfiguration = configuration.Bean,
                        Key = configuration.Bean.Key,
                        OrderIndex = configuration.Order?.Index ?? configuration.Bean.OrderIndex
                    });
            }

            return result.OrderBy(r => r.OrderIndex).ThenBy(r => r.Type.Name).ToList();
        }

        /// <summary>
        ///     获取所有打了切面的信息
        /// </summary>
        /// <returns></returns>
        private List<PointcutConfigurationInfo> DoPointcutConfiguration()
        {
            if (_assemblyList == null || _assemblyList.Count < 1) throw new ArgumentNullException(nameof(_assemblyList));

            //一个pointcut 对应 一个class 对应多个group method
            var result = new List<PointcutConfigurationInfo>();
            foreach (var assembly in _assemblyList)
            {
                if (assembly.IsDynamic) continue;
                var types = assembly.GetExportedTypes();
                //找到类型中含有 AutofacConfiguration 标签的类 排除掉抽象类
                var typeList = (from type in types
                    let bean = type.GetCustomAttributes<Pointcut>()
                    let order = type.GetCustomAttribute<Order>()
                    where type.IsClass && !type.IsAbstract && bean != null && bean.Any()
                    select new
                    {
                        order?.Index,
                        Type = type,
                        Bean = bean.Where(r => !string.IsNullOrEmpty(r.Class) || r.AttributeType != null).ToList()
                    }).OrderBy(r => r.Index).ThenBy(r => r.Type.Name).ToList();

                foreach (var configuration in typeList)
                {
                    //解析方法 pointcut配置类不支持继承的方法
                    var beanTypeMethodList = configuration.Type.GetAllInstanceMethod(false);

                    //一个point标签下 class里面 最多一组 
                    var beforeMethodInfos = new Dictionary<string, Tuple<Before, MethodInfo>>();
                    var afterReturnMethodInfos = new Dictionary<string, Tuple<AfterReturn, MethodInfo>>();
                    var afterMethodInfos = new Dictionary<string, Tuple<After, MethodInfo>>();
                    var aroundMethodInfos = new Dictionary<string, Tuple<Around, MethodInfo>>();
                    var throwMethodInfos = new Dictionary<string, Tuple<AfterThrows, MethodInfo>>();
                    foreach (var beanTypeMethod in beanTypeMethodList)
                    {
                        var beforeAttribute = beanTypeMethod.GetCustomAttribute<Before>();
                        var afterReturnAttribute = beanTypeMethod.GetCustomAttribute<AfterReturn>();
                        var afterAttribute = beanTypeMethod.GetCustomAttribute<After>();
                        var aroundAttribute = beanTypeMethod.GetCustomAttribute<Around>();
                        var throwAttribute = beanTypeMethod.GetCustomAttribute<AfterThrows>();
                        if (beforeAttribute == null && afterReturnAttribute == null && aroundAttribute == null && throwAttribute == null &&
                            afterAttribute == null) continue;

                        if (aroundAttribute != null)
                        {
                            //检查方法的参数是否对了
                            var parameters = beanTypeMethod.GetParameters();
                            if (parameters.All(r => r.ParameterType != typeof(AspectContext)))
                                throw new InvalidOperationException(
                                    $"The Pointcut class `{configuration.Type.FullName}` arround method `{beanTypeMethod.Name}` can not be register without parameter of `AspectContext`!");

                            if (parameters.All(r => r.ParameterType != typeof(AspectDelegate)))
                                throw new InvalidOperationException(
                                    $"The Pointcut class `{configuration.Type.FullName}` arround method `{beanTypeMethod.Name}` can not be register without parameter of `AspectDelegate`!");

                            //必须是异步的 返回类型是Task才行
                            if (beanTypeMethod.ReturnType != typeof(Task))
                                throw new InvalidOperationException(
                                    $"The Pointcut class `{configuration.Type.FullName}` arround method `{beanTypeMethod.Name}` must returnType of `Task`!");

                            var key = aroundAttribute.GroupName ?? "";
                            if (aroundMethodInfos.ContainsKey(key))
                                throw new InvalidOperationException(
                                    $"The Pointcut class `{configuration.Type.FullName}` arround method `{beanTypeMethod.Name}` can not be register multi${(!string.IsNullOrEmpty(key) ? " with key:`" + key + "`" : "")}!");

                            aroundMethodInfos.Add(key, new Tuple<Around, MethodInfo>(aroundAttribute, beanTypeMethod));
                        }

                        //返回类型只能是void和Task
                        if (beanTypeMethod.ReturnType != typeof(void) && beanTypeMethod.ReturnType != typeof(Task))
                            throw new InvalidOperationException(
                                $"The Configuration class `{configuration.Type.FullName}` method `{beanTypeMethod.Name}` returnType invaild");

                        if (beforeAttribute != null)
                        {
                            var key = beforeAttribute.GroupName ?? "";
                            if (beforeMethodInfos.ContainsKey(key))
                                throw new InvalidOperationException(
                                    $"The Pointcut class `{configuration.Type.FullName}` method `{beanTypeMethod.Name}` can not be register multi${(!string.IsNullOrEmpty(key) ? " with key:`" + key + "`" : "")}!");

                            beforeMethodInfos.Add(key, new Tuple<Before, MethodInfo>(beforeAttribute, beanTypeMethod));
                        }

                        if (afterAttribute != null)
                        {
                            var key = afterAttribute.GroupName ?? "";
                            if (!string.IsNullOrEmpty(afterAttribute.Returing))
                            {
                                //查看这个指定的参数有没有在这个方法里面
                                var parameters = beanTypeMethod.GetParameters();
                                var returnIngParam = parameters.FirstOrDefault(r => r.Name == afterAttribute.Returing);
                                if (returnIngParam == null)
                                    throw new InvalidOperationException(
                                        $"The Pointcut class `{configuration.Type.FullName}` after method `{beanTypeMethod.Name}` can not be register without special parameter of `{afterAttribute.Returing}`!");
                            }

                            if (afterMethodInfos.ContainsKey(key))
                                throw new InvalidOperationException(
                                    $"The Pointcut class `{configuration.Type.FullName}` method `{beanTypeMethod.Name}` can not be register multi${(!string.IsNullOrEmpty(key) ? " with key:`" + key + "`" : "")}!");

                            afterMethodInfos.Add(key, new Tuple<After, MethodInfo>(afterAttribute, beanTypeMethod));
                        }

                        if (afterReturnAttribute != null)
                        {
                            var key = afterReturnAttribute.GroupName ?? "";
                            if (!string.IsNullOrEmpty(afterReturnAttribute.Returing))
                            {
                                //查看这个指定的参数有没有在这个方法里面
                                var parameters = beanTypeMethod.GetParameters();
                                var returnIngParam = parameters.FirstOrDefault(r => r.Name == afterReturnAttribute.Returing);
                                if (returnIngParam == null)
                                    throw new InvalidOperationException(
                                        $"The Pointcut class `{configuration.Type.FullName}` after method `{beanTypeMethod.Name}` can not be register without special parameter of `{afterReturnAttribute.Returing}`!");
                            }

                            if (afterReturnMethodInfos.ContainsKey(key))
                                throw new InvalidOperationException(
                                    $"The Pointcut class `{configuration.Type.FullName}` method `{beanTypeMethod.Name}` can not be register multi${(!string.IsNullOrEmpty(key) ? " with key:`" + key + "`" : "")}!");

                            afterReturnMethodInfos.Add(key, new Tuple<AfterReturn, MethodInfo>(afterReturnAttribute, beanTypeMethod));
                        }

                        if (throwAttribute != null)
                        {
                            var key = throwAttribute.GroupName ?? "";
                            if (!string.IsNullOrEmpty(throwAttribute.Throwing))
                            {
                                //查看这个指定的参数有没有在这个方法里面
                                var parameters = beanTypeMethod.GetParameters();
                                var returnIngParam = parameters.FirstOrDefault(r => r.Name == throwAttribute.Throwing);
                                if (returnIngParam == null)
                                    throw new InvalidOperationException(
                                        $"The Pointcut class `{configuration.Type.FullName}` throwing method `{beanTypeMethod.Name}` can not be register without special parameter of `{throwAttribute.Throwing}`!");
                            }

                            if (throwMethodInfos.ContainsKey(key))
                                throw new InvalidOperationException(
                                    $"The Pointcut class `{configuration.Type.FullName}` method `{beanTypeMethod.Name}` can not be register multi${(!string.IsNullOrEmpty(key) ? " with key:`" + key + "`" : "")}!");

                            throwMethodInfos.Add(key, new Tuple<AfterThrows, MethodInfo>(throwAttribute, beanTypeMethod));
                        }
                    }

                    // PointCut 看下是否有配置name 如果有配置name就要check有没有该name对应的methodinfo
                    foreach (var pc in configuration.Bean)
                    {
                        if (!beforeMethodInfos.ContainsKey(pc.GroupName)
                            && !afterReturnMethodInfos.ContainsKey(pc.GroupName)
                            && !aroundMethodInfos.ContainsKey(pc.GroupName)
                            && !throwMethodInfos.ContainsKey(pc.GroupName)
                            && !afterMethodInfos.ContainsKey(pc.GroupName)
                           )
                            continue;

                        var rr = new PointcutConfigurationInfo
                        {
                            PointClass = configuration.Type,
                            Pointcut = pc,
                            GroupName = pc.GroupName
                        };
                        if (beforeMethodInfos.TryGetValue(pc.GroupName, out var be)) rr.BeforeMethod = be;

                        if (afterMethodInfos.TryGetValue(pc.GroupName, out var af1)) rr.AfterMethod = af1;

                        if (afterReturnMethodInfos.TryGetValue(pc.GroupName, out var af)) rr.AfterReturnMethod = af;

                        if (aroundMethodInfos.TryGetValue(pc.GroupName, out var ar)) rr.AroundMethod = ar;

                        if (throwMethodInfos.TryGetValue(pc.GroupName, out var trr)) rr.AfterThrows = trr;

                        result.Add(rr);
                    }
                }
            }

            return result;
        }

        /// <summary>
        ///     根据注解解析
        /// </summary>
        /// <param name="bean"></param>
        /// <param name="currentType"></param>
        /// <returns></returns>
        private ComponentModel EnumerateComponentServices(Component bean, Type currentType)
        {
            if (bean == null) bean = new Component();

            var result = new ComponentModel
            {
                AutoActivate = bean.AutoActivate,
                CurrentType = currentType,
                InjectProperties = bean.InjectProperties,
                InjectPropertyType = bean.InjectPropertyType,
                Ownership = bean.Ownership,
                Interceptor = bean.Interceptor,
                InterceptorKey = bean.InterceptorKey,
                InterceptorType = bean.InterceptorType,
                InitMethod = bean.InitMethod,
                DestroyMethod = bean.DestroyMethod,
                OrderIndex = bean.OrderIndex,
                NotUseProxy = bean.NotUseProxy,
                EnableAspect = bean.EnableAspect,
                EnablePointcutInherited = bean.EnablePointcutInherited,
                IsBenPostProcessor = typeof(BeanPostProcessor).IsAssignableFrom(currentType),
                CurrentClassTypeAttributes = currentType.GetCustomAttributes().OfType<Attribute>().ToList(),
                DependsOn = currentType.GetCustomAttribute<DependsOn>()?.dependsOn
            };

            if (bean.AutofacScope == AutofacScope.Default)
                //说明没有特别指定
                result.AutofacScope = DefaultAutofacScope.Equals(AutofacScope.Default) ? AutofacScope.InstancePerDependency : DefaultAutofacScope;
            else
                result.AutofacScope = bean.AutofacScope;

            if (result.IsBenPostProcessor)
            {
                result.AutoActivate = false;
                result.Interceptor = null;
                result.AutofacScope = AutofacScope.SingleInstance;
                result.OrderIndex = int.MinValue;
                result.NotUseProxy = true;
                result.EnableAspect = false;
            }
            else
            {
                //自动识别EnableAspect
                NeedWarpForAspect(result);
            }

            #region 解析注册对应的类的列表

            var re = new List<ComponentServiceModel>();

            if (bean.Service == null)
            {
                //接口自动注册 父类自动注册
                var typeInterfaces = currentType.GetParentTypes();
                foreach (var iInterface in typeInterfaces)
                {
                    //自动注册的时候如果父类是抽象class是否忽略
                    if (iInterface.IsAbstract && IgnoreAutoRegisterAbstractClass) continue;
                    //自动按实现的接口注册 但保留对BeanPostProcessor的注册
                    if (iInterface.IsInterface && !result.IsBenPostProcessor && !AutoRegisterInterface) continue;
                    //自动注册父类
                    if (iInterface.IsClass && !AutoRegisterParentClass) continue;

                    if (iInterface.IsValueType || iInterface.IsEnum || iInterface == typeof(object) || iInterface.IsGenericEnumerableInterfaceType() ||
                        !ProxyUtil.IsAccessible(iInterface)) continue;

                    if (bean.Services == null || !bean.Services.Contains(iInterface))
                    {
                        if (bean.Interceptor != null)
                        {
                            //有配置接口拦截器 但是当前不是注册成接口
                            if (bean.InterceptorType == InterceptorType.Interface && !iInterface.IsInterface) continue;
                            //有配置class拦截器 但是当前不是注册成class
                            if (bean.InterceptorType == InterceptorType.Class && !iInterface.IsClass) continue;
                        }

                        //如果父类直接是接口 也没有特别指定 Service 或者 Services集合
                        re.Add(new ComponentServiceModel
                        {
                            Type = iInterface,
                            Key = bean.Key
                        });
                    }
                }
            }

            if (bean.Service != null)
                //本身有指定注册类型
                re.Add(new ComponentServiceModel
                {
                    Type = bean.Service,
                    Key = bean.Key
                });
            else if (!string.IsNullOrEmpty(bean.Key))
                //指定了key 默认为本身
                re.Add(new ComponentServiceModel
                {
                    Type = currentType,
                    Key = bean.Key
                });

            if (bean.Services != null && bean.Services.Length > 0)
            {
                var keyList = new string[bean.Services.Length];
                if (bean.Keys != null && bean.Keys.Length > 0)
                {
                    if (bean.Keys.Length > bean.Services.Length)
                        throw new ArgumentOutOfRangeException(currentType.FullName + ":" + nameof(bean.Keys) + "`length is not eq to " + nameof(bean.Services) +
                                                              "`length");

                    //Keys和Services是按照一对一的  如果长度不对
                    for (var i = 0; i < bean.Keys.Length; i++) keyList[i] = bean.Keys[i];
                }

                for (var i = 0; i < bean.Services.Length; i++)
                {
                    var serviceKey = keyList[i];
                    re.Add(new ComponentServiceModel
                    {
                        Type = bean.Services[i],
                        Key = serviceKey
                    });
                }
            }

            result.ComponentServiceList = re;

            #endregion

            return result;
        }

        /// <summary>
        ///     构建默认的datasource
        /// </summary>
        /// <returns></returns>
        private static MetaSourceData GetDefaultMetaSource()
        {
            var metaSource = new MetaSourceData();
            metaSource.Origin = "appsettings.json";
            metaSource.Embedded = false;
            metaSource.MetaSourceType = MetaSourceType.JSON;
            metaSource.Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, metaSource.Origin);
            metaSource.ConfigurationLazy =
                new Lazy<IConfiguration>(() => EmbeddedConfiguration.Load(null, metaSource.Path, metaSource.MetaSourceType, metaSource.Embedded));
            metaSource.Reload = true; //默认
            return metaSource;
        }

        /// <summary>
        ///     设置source源
        /// </summary>
        private void EnumerateMetaSourceAttributes(Type classType, List<MetaSourceData> MetaSourceList)
        {
            #region PropertySource

            var metaSourceAttributes = classType.GetCustomAttributes<PropertySource>().ToList();
            if (metaSourceAttributes.Any())
            {
                metaSourceAttributes = metaSourceAttributes.OrderBy(r => r.OrderIndex).ToList();
                foreach (var metaSourceAttribute in metaSourceAttributes)
                {
                    var metaSource = new MetaSourceData
                    {
                        Origin = metaSourceAttribute.Path,
                        Embedded = metaSourceAttribute.Embedded,
                        DynamicSource = metaSourceAttribute.Dynamic,
                        MetaSourceType = metaSourceAttribute.MetaSourceType,
                        Order = metaSourceAttribute.OrderIndex,
                        Reload = metaSourceAttribute._reload ?? EnableValueResourceReloadOnchange
                    };

                    if (metaSource.DynamicSource != null)
                    {
                        //校验必须实现接口IDynamicSourceProvider
                        if (!typeof(IConfigurationProvider).IsAssignableFrom(metaSource.DynamicSource))
                            throw new InvalidOperationException(
                                $"'{classType.FullName}' can not load DynamicSource:'{metaSource.DynamicSource.FullName}',Must implement interface:IConfigurationProvider");

                        metaSource.MetaSourceType = MetaSourceType.Dynamic;
                        metaSource.ConfigurationLazy = new Lazy<IConfiguration>(() =>
                        {
                            //单例还是多例交给外面自己控制
                            object sourceProvider = null;
                            if (!string.IsNullOrEmpty(metaSourceAttribute.Key))
                                autofacGlobalScope?.TryResolveService(new KeyedService(metaSourceAttribute.Key, metaSource.DynamicSource), out sourceProvider);
                            else
                                autofacGlobalScope?.TryResolveService(new TypedService(metaSource.DynamicSource), out sourceProvider);

                            if (sourceProvider == null)
                                //如果没有配置那么只能是多例
                                sourceProvider = Activator.CreateInstance(metaSource.DynamicSource);

                            if (sourceProvider == null)
                                throw new InvalidOperationException(
                                    $"'{classType.FullName}' can not load DynamicSource:'{metaSource.DynamicSource.FullName}',Must implement interface:IConfigurationProvider");

                            if (sourceProvider is IDynamicSourceProvider dynamicSource) dynamicSource.setPropertySource(metaSourceAttribute);

                            if (sourceProvider is IConfigurationProvider configurationProvider)
                            {
                                var config = new ConfigurationRoot(new List<IConfigurationProvider> { configurationProvider });
                                return config;
                            }

                            throw new InvalidOperationException(
                                $"'{classType.FullName}' can not load DynamicSource:'{metaSource.DynamicSource.FullName}',Must implement interface:IConfigurationProvider");
                        });

                        MetaSourceList.Add(metaSource);
                        continue;
                    }

                    if (string.IsNullOrEmpty(metaSourceAttribute.Path))
                    {
                        MetaSourceList.Add(DefaultMeaSourceData.Value);
                        continue;
                    }


                    if (metaSourceAttribute.Embedded)
                        metaSource.Path = metaSource.Origin;
                    else
                        metaSource.Path = metaSource.Origin.StartsWith("/")
                            ? Path.Combine(GetAssemblyLocation(), metaSource.Origin.Substring(1, metaSource.Origin.Length - 1))
                            : metaSource.Origin;

                    metaSource.ConfigurationLazy = new Lazy<IConfiguration>(() => EmbeddedConfiguration.Load(classType, metaSource.Path,
                        metaSource.MetaSourceType,
                        metaSourceAttribute.Embedded, metaSource.Reload));

                    MetaSourceList.Add(metaSource);
                }
            }
            else
            {
                MetaSourceList.Add(DefaultMeaSourceData.Value);
            }

            #endregion
        }

        /// <summary>
        ///     查询指定程序集下引用的所有程序集
        /// </summary>
        /// <returns></returns>
        private string GetAssemblyLocation()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }


        /// <summary>
        ///     获取当前工程下所有要用到的dll
        /// </summary>
        /// <returns></returns>
        private List<Assembly> getCurrentDomainAssemblies()
        {
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            var loadedPaths = loadedAssemblies.Select(
                a =>
                {
                    // prevent exception accessing Location
                    try
                    {
                        return a.Location;
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
            ).ToArray();
            var referencedPaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");
            var toLoad = referencedPaths.Where(r => !loadedPaths.Contains(r, StringComparer.InvariantCultureIgnoreCase)).ToList();
            toLoad.ForEach(
                path =>
                {
                    // prevent exception loading some assembly
                    try
                    {
                        loadedAssemblies.Add(AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(path)));
                    }
                    catch (Exception)
                    {
                        ; // DO NOTHING
                    }
                }
            );

            // prevent loading of dynamic assembly, autofac doesn't support dynamic assembly
            loadedAssemblies.RemoveAll(i => i.IsDynamic);

            return loadedAssemblies;
        }


        /// <summary>
        ///     自己注册的 不是用注解来注入的 也能识别 Autowire 和 Value注入
        /// </summary>
        /// <returns></returns>
        private (ComponentModel, IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle>)
            CreateCompomentFromAutofac(Type currentType, IComponentRegistration registration
                , ComponentModelCacheSingleton allCompoment, bool isGeneric)
        {
            var component = new ComponentModel
            {
                AutoActivate = false,
                CurrentType = currentType,
                InjectProperties = true,
                InjectPropertyType = InjectPropertyType.Autowired,
                IsBenPostProcessor = typeof(BeanPostProcessor).IsAssignableFrom(currentType),
                CurrentClassTypeAttributes = isGeneric
                    ? currentType.GetGenericTypeDefinition().GetCustomAttributes().OfType<Attribute>().ToList()
                    : currentType.GetCustomAttributes().OfType<Attribute>().ToList()
            };

            component.MetaSourceList = new List<MetaSourceData>();
            EnumerateMetaSourceAttributes(component.CurrentType, component.MetaSourceList);

            var needPointCut = isGeneric || NeedWarpForPointcut(component, allCompoment.PointCutConfigurationList);
            if (isGeneric || !(NeedWarpForAspect(component) || needPointCut))
            {
                RegisterBeforeBeanPostProcessor(component, registration);
                RegisterComponentValues(component, registration);
                SetInjectProperties(component, registration);
                RegisterAfterBeanPostProcessor(component, registration);
                return (component, null);
            }

            var builder = RegistrationBuilder.ForType(component.CurrentType);
            RegisterBeforeBeanPostProcessor(component, builder);
            RegisterComponentValues(component, builder);
            SetInjectProperties(component, builder);
            SetIntercept(component, builder, needPointCut);
            RegisterAfterBeanPostProcessor(component, builder);
            return (component, builder);
        }


        #region 构造方法

        /// <summary>
        ///     根据程序集来实例化
        /// </summary>
        /// <param name="assemblyList"></param>
        public AutofacAnnotationModule(params Assembly[] assemblyList)
        {
            if (assemblyList.Length < 1) throw new ArgumentException(nameof(assemblyList));

            _assemblyList = assemblyList.ToList();
        }

        /// <summary>
        ///     根据程序集的名称来实例化
        /// </summary>
        /// <param name="assemblyNameList"></param>
        public AutofacAnnotationModule(params string[] assemblyNameList)
        {
            if (assemblyNameList == null || assemblyNameList.Length == 0) throw new ArgumentException(nameof(assemblyNameList));

            _assemblyList = getCurrentDomainAssemblies().Where(assembly => assemblyNameList.Contains(assembly.GetName().Name)).ToList();
        }


        /// <summary>
        ///     加载当前domain所有用到的dll
        /// </summary>
        public AutofacAnnotationModule()
        {
        }

        #endregion


        #region Autowired

        /// <summary>
        ///     设置属性自动注入
        /// </summary>
        /// <typeparam name="TReflectionActivatorData"></typeparam>
        /// <param name="component"></param>
        /// <param name="registrar"></param>
        internal void SetInjectProperties<TReflectionActivatorData>(ComponentModel component,
            IRegistrationBuilder<object, TReflectionActivatorData, object> registrar)
        {
            if (registrar == null) throw new ArgumentNullException(nameof(registrar));

            if (!component.InjectProperties) return;

            if (component.InjectPropertyType.Equals(InjectPropertyType.ALL))
            {
                //保留autofac原本的方式
                registrar.PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);
                return;
            }

            if (!prepareSetInjectProperties(component)) return;

            //支持循环
            registrar.ConfigurePipeline(p =>
                p.Use(PipelinePhase.Activation, MiddlewareInsertionMode.StartOfPhase, (ctxt, next) =>
                {
                    next(ctxt);
                    DoAutoWired(ctxt, ctxt.Parameters.ToList(), ctxt.Instance);
                }));
        }

        internal bool prepareSetInjectProperties(ComponentModel component)
        {
            if (!component.InjectProperties) return false;

            component.AutowiredPropertyInfoList = (from p in component.CurrentType.GetAllProperties()
                    let propertyType = p.GetType()
                    let typeInfo = p.GetType().GetTypeInfo()
                    let va = p.GetCustomAttribute<Autowired>()
                    where va != null && !typeInfo.IsValueType && !typeInfo.IsEnum
                          && (!propertyType.IsArray || !propertyType.GetElementType().GetTypeInfo().IsValueType)
                          && (!propertyType.IsGenericEnumerableInterfaceType() || !typeInfo.GenericTypeArguments[0].GetTypeInfo().IsValueType)
                    select new Tuple<PropertyInfo, Autowired, PropertyReflector>(p, va, p.GetReflector()))
                .ToList();

            component.AutowiredFieldInfoList = (from p in component.CurrentType.GetAllFields()
                    let propertyType = p.GetType()
                    let typeInfo = p.GetType().GetTypeInfo()
                    let va = p.GetCustomAttribute<Autowired>()
                    where va != null && !typeInfo.IsValueType && !typeInfo.IsEnum
                          && (!propertyType.IsArray || !propertyType.GetElementType().GetTypeInfo().IsValueType)
                          && (!propertyType.IsGenericEnumerableInterfaceType() || !typeInfo.GenericTypeArguments[0].GetTypeInfo().IsValueType)
                    select new Tuple<FieldInfo, Autowired, FieldReflector>(p, va, p.GetReflector()))
                .ToList();

            if (!component.AutowiredPropertyInfoList.Any() && !component.AutowiredFieldInfoList.Any()) return false;

            //初始化 AllowCircularDependencies 参数
            component.AutowiredPropertyInfoList.ForEach(r =>
            {
                if (r.Item2.AllowCircularDependencies == null) //如果自己指定了的话 就不管了
                    r.Item2.AllowCircularDependencies = AllowCircularDependencies;
            });

            component.AutowiredFieldInfoList.ForEach(r =>
            {
                if (r.Item2.AllowCircularDependencies == null) //如果自己指定了的话 就不管了
                    r.Item2.AllowCircularDependencies = AllowCircularDependencies;
            });
            return true;
        }

        private void SetInjectProperties(ComponentModel component,
            IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> registrar)
        {
            if (registrar == null) throw new ArgumentNullException(nameof(registrar));

            if (!prepareSetInjectProperties(component)) return;

            //支持循环
            registrar.ConfigurePipeline(p =>
                p.Use(PipelinePhase.Activation, MiddlewareInsertionMode.StartOfPhase, (ctxt, next) =>
                {
                    next(ctxt);
                    DoAutoWired(ctxt, ctxt.Parameters.ToList(), ctxt.Instance);
                }));
        }

        private void SetInjectProperties(ComponentModel component,
            IComponentRegistration registrar)
        {
            if (registrar == null) throw new ArgumentNullException(nameof(registrar));

            if (!prepareSetInjectProperties(component)) return;

            //支持循环
            registrar.ConfigurePipeline(p =>
                p.Use(PipelinePhase.Activation, MiddlewareInsertionMode.StartOfPhase, (ctxt, next) =>
                {
                    next(ctxt);
                    DoAutoWired(ctxt, ctxt.Parameters.ToList(), ctxt.Instance);
                }));
        }

        /// <summary>
        ///     属性注入
        /// </summary>
        /// <param name="context">容器</param>
        /// <param name="Parameters">参数</param>
        /// <param name="instance">实例</param>
        internal static void DoAutoWired(ResolveRequestContext context, List<Parameter> Parameters, object instance)
        {
            if (instance == null) return;
            var instanceType = instance.GetType();
            var RealInstance = instance;
            if (ProxyUtil.IsProxy(instance))
            {
                RealInstance = ProxyUtil.GetUnproxiedInstance(instance);
                instanceType = ProxyUtil.GetUnproxiedType(instance);
            }

            if (RealInstance == null) return;
            var componentModelCacheSingleton = context.Resolve<ComponentModelCacheSingleton>();
            if (!componentModelCacheSingleton.ComponentModelCache.TryGetValue(instanceType, out var model))
            {
                if (!componentModelCacheSingleton.DynamicComponentModelCache.TryGetValue(instanceType.Namespace + instanceType.Name, out var mode1)) return;

                model = mode1;
            }

            AutowiredParmeterStack firstStack = null;
            // context.Service
            if (Parameters != null)
            {
                if (!Parameters.Any() || !(Parameters.First() is AutowiredParmeterStack AutowiredParmeter))
                {
                    firstStack = new AutowiredParmeterStack();
                    firstStack.Push(context.Service, RealInstance);
                    Parameters.Add(firstStack);
                }
                else
                {
                    AutowiredParmeter.Push(context.Service, RealInstance);
                }
            }

            try
            {
                //字段注入
                foreach (var field in model.AutowiredFieldInfoList)
                {
                    // ReSharper disable once PossibleMultipleEnumeration
                    var obj = field.Item2.ResolveField(field.Item1, context, Parameters, RealInstance);
                    if (obj == null)
                    {
                        if (field.Item2.Required)
                            throw new DependencyResolutionException(
                                $"Autowire error,can not resolve class type:{instanceType.FullName},field name:{field.Item1.Name} "
                                + (!string.IsNullOrEmpty(field.Item2.Name) ? $",with key:{field.Item2.Name}" : ""));

                        continue;
                    }

                    try
                    {
                        if (model.isDynamicGeneric) //如果是动态泛型的话
                        {
                            instanceType.GetTypeInfo().GetField(field.Item1.Name).GetReflector().SetValue(RealInstance, obj);
                            continue;
                        }

                        field.Item3.SetValue(RealInstance, obj);
                    }
                    catch (Exception ex)
                    {
                        throw new DependencyResolutionException(
                            $"Autowire error,can not resolve class type:{instanceType.FullName},field name:{field.Item1.Name} "
                            + (!string.IsNullOrEmpty(field.Item2.Name) ? $",with key:{field.Item2.Name}" : ""), ex);
                    }
                }

                //属性注入
                foreach (var property in model.AutowiredPropertyInfoList)
                {
                    // ReSharper disable once PossibleMultipleEnumeration
                    var obj = property.Item2.ResolveProperty(property.Item1, context, Parameters, RealInstance);
                    if (obj == null)
                    {
                        if (property.Item2.Required)
                            throw new DependencyResolutionException(
                                $"Autowire error,can not resolve class type:{instanceType.FullName},property name:{property.Item1.Name} "
                                + (!string.IsNullOrEmpty(property.Item2.Name) ? $",with key:{property.Item2.Name}" : ""));

                        continue;
                    }

                    try
                    {
                        if (model.isDynamicGeneric) //如果是动态泛型的话
                        {
                            instanceType.GetTypeInfo().GetProperty(property.Item1.Name).GetReflector().SetValue(RealInstance, obj);
                            continue;
                        }

                        property.Item3.SetValue(RealInstance, obj);
                    }
                    catch (Exception ex)
                    {
                        throw new DependencyResolutionException(
                            $"Autowire error,can not resolve class type:{instanceType.FullName},property name:{property.Item1.Name} "
                            + (!string.IsNullOrEmpty(property.Item2.Name) ? $",with key:{property.Item2.Name}" : ""), ex);
                    }
                }
            }
            finally
            {
                firstStack?.Dispose();
            }
        }

        #endregion


        #region Value

        /// <summary>
        ///     动态注入打了value标签的值
        /// </summary>
        /// <typeparam name="TReflectionActivatorData"></typeparam>
        /// <param name="component"></param>
        /// <param name="registrar"></param>
        private void RegisterComponentValues<TReflectionActivatorData>(ComponentModel component,
            IRegistrationBuilder<object, TReflectionActivatorData, object> registrar)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            if (registrar == null) throw new ArgumentNullException(nameof(registrar));

            if (!prepareSetInjectValue(component)) return;

            //创建对象之后调用
            registrar.ConfigurePipeline(p =>
                p.Use(PipelinePhase.Activation, MiddlewareInsertionMode.StartOfPhase, (ctxt, next) =>
                {
                    next(ctxt);
                    DoValueInject(ctxt);
                }));
        }

        /// <summary>
        ///     自己注册的 不是用注解来注入的 也能识别 Autowire 和 Value注入
        /// </summary>
        /// <param name="component"></param>
        /// <param name="registrar"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DependencyResolutionException"></exception>
        private void RegisterComponentValues(ComponentModel component,
            IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> registrar)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            if (registrar == null) throw new ArgumentNullException(nameof(registrar));

            if (!prepareSetInjectValue(component)) return;

            //创建对象之后调用
            registrar.ConfigurePipeline(p =>
                p.Use(PipelinePhase.Activation, MiddlewareInsertionMode.StartOfPhase, (ctxt, next) =>
                {
                    next(ctxt);
                    DoValueInject(ctxt);
                }));
        }

        private void RegisterComponentValues(ComponentModel component,
            IComponentRegistration registrar)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            if (registrar == null) throw new ArgumentNullException(nameof(registrar));

            if (!prepareSetInjectValue(component)) return;

            //创建对象之后调用
            registrar.ConfigurePipeline(p =>
                p.Use(PipelinePhase.Activation, MiddlewareInsertionMode.StartOfPhase, (ctxt, next) =>
                {
                    next(ctxt);
                    DoValueInject(ctxt);
                }));
        }

        private void DoValueInject(ResolveRequestContext e)
        {
            var instance = e.Instance;
            if (instance == null) return;
            var instanceType = instance.GetType();
            var RealInstance = instance;
            if (ProxyUtil.IsProxy(instance))
            {
                RealInstance = ProxyUtil.GetUnproxiedInstance(instance);
                instanceType = ProxyUtil.GetUnproxiedType(instance);
            }

            if (RealInstance == null) return;
            var componentModelCacheSingleton = e.Resolve<ComponentModelCacheSingleton>();
            if (!componentModelCacheSingleton.ComponentModelCache.TryGetValue(instanceType, out var model))
            {
                if (!componentModelCacheSingleton.DynamicComponentModelCache.TryGetValue(instanceType.Namespace + instanceType.Name,
                        out var mode1))
                    return;

                model = mode1;
            }

            foreach (var field in model.ValueFieldInfoList)
            {
                var value = field.Item2.ResolveFiled(field.Item1, e);
                if (value == null) continue;
                try
                {
                    if (model.isDynamicGeneric) //如果是动态泛型的话
                    {
                        instanceType.GetTypeInfo().GetField(field.Item1.Name).GetReflector().SetValue(RealInstance, value);
                        continue;
                    }

                    field.Item3.SetValue(RealInstance, value);
                }
                catch (Exception ex)
                {
                    throw new DependencyResolutionException(
                        $"Value set error,can not resolve class type:{instanceType.FullName} =====>" + $" ,fail resolve field value:{field.Item1.Name} " +
                        (!string.IsNullOrEmpty(field.Item2.value) ? $",with value:[{field.Item2.value}]" : ""), ex);
                }
            }

            foreach (var property in model.ValuePropertyInfoList)
            {
                var value = property.Item2.ResolveProperty(property.Item1, e);
                if (value == null) continue;
                try
                {
                    if (model.isDynamicGeneric) //如果是动态泛型的话
                    {
                        instanceType.GetTypeInfo().GetProperty(property.Item1.Name).GetReflector().SetValue(RealInstance, value);
                        continue;
                    }

                    property.Item3.SetValue(RealInstance, value);
                }
                catch (Exception ex)
                {
                    throw new DependencyResolutionException(
                        $"Value set error,can not resolve class type:{instanceType.FullName} =====>" +
                        $" ,fail resolve property value:{property.Item1.Name} " +
                        (!string.IsNullOrEmpty(property.Item2.value) ? $",with value:[{property.Item2.value}]" : ""), ex);
                }
            }
        }

        private bool prepareSetInjectValue(ComponentModel component)
        {
            component.ValuePropertyInfoList = (from p in component.CurrentType.GetAllProperties()
                let va = p.GetCustomAttribute<Value>()
                where va != null
                select new Tuple<PropertyInfo, Value, PropertyReflector>(p, va, p.GetReflector())).ToList();

            component.ValueFieldInfoList = (from p in component.CurrentType.GetAllFields()
                let va = p.GetCustomAttribute<Value>()
                where va != null
                select new Tuple<FieldInfo, Value, FieldReflector>(p, va, p.GetReflector())).ToList();

            if (!component.ValueFieldInfoList.Any() && !component.ValuePropertyInfoList.Any()) return false;

            return true;
        }

        #endregion
    }
}