using AspectCore.Extensions.Reflection;
using Autofac.Annotation.Util;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Extras.DynamicProxy;
using Autofac.Features.AttributeFilters;
using Castle.DynamicProxy;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Autofac.Annotation
{
    /// <inheritdoc />
    /// <summary>
    /// autofac模块用注解的模式注册
    /// </summary>
    public class AutofacAnnotationModule : Module
    {
        /// <summary>
        /// ComponentModel缓存
        /// </summary>
        internal static ConcurrentDictionary<Type, ComponentModel> ComponentModelCache = new ConcurrentDictionary<Type, ComponentModel>();

        private readonly Assembly[] _assemblyList;

        /// <summary>
        /// 当前默认的Scope
        /// </summary>
        public AutofacScope AutofacScope { get; private set; } = AutofacScope.Default;

        /// <summary>
        /// 是否启用Autowired的循环注入
        /// </summary>
        public bool AllowCircularDependencies { get; private set; }

        /// <summary>
        /// 根据程序集来实例化
        /// </summary>
        /// <param name="assemblyList"></param>
        public AutofacAnnotationModule(params Assembly[] assemblyList)
        {
            if (assemblyList.Length < 1)
            {
                throw new ArgumentException(nameof(assemblyList));
            }

            _assemblyList = assemblyList;
        }

        /// <summary>
        /// 根据程序集的名称来实例化
        /// </summary>
        /// <param name="assemblyNameList"></param>
        public AutofacAnnotationModule(params string[] assemblyNameList)
        {
            if (assemblyNameList.Length < 1)
            {
                throw new ArgumentException(nameof(assemblyNameList));
            }

            _assemblyList = GetAssemblies().Where(assembly => assemblyNameList.Contains(assembly.GetName().Name)).ToArray();
        }

        /// <summary>
        /// 设置瞬时
        /// </summary>
        /// <returns></returns>
        public AutofacAnnotationModule InstancePerDependency()
        {
            this.AutofacScope = AutofacScope.InstancePerDependency;
            return this;
        }

        /// <summary>
        /// 设置单例
        /// </summary>
        /// <returns></returns>
        public AutofacAnnotationModule SingleInstance()
        {
            this.AutofacScope = AutofacScope.SingleInstance;
            return this;
        }

        /// <summary>
        /// 设置作用域
        /// </summary>
        /// <returns></returns>
        public AutofacAnnotationModule InstancePerLifetimeScope()
        {
            this.AutofacScope = AutofacScope.InstancePerLifetimeScope;
            return this;
        }

        /// <summary>
        /// 设置请求作用域
        /// </summary>
        /// <returns></returns>
        public AutofacAnnotationModule InstancePerRequest()
        {
            this.AutofacScope = AutofacScope.InstancePerRequest;
            return this;
        }

        /// <summary>
        /// 设置是否启用循环Autowired
        /// </summary>
        /// <returns></returns>
        public AutofacAnnotationModule SetAllowCircularDependencies(bool flag)
        {
            this.AllowCircularDependencies = flag;
            return this;
        }

        /// <summary>
        /// autofac加载
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var componetList = GetAllComponent();

            foreach (var component in componetList)
            {
                //注册本身
                var registrar = builder.RegisterType(component.CurrentType).WithAttributeFiltering();

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
                SetIntercept(component, registrar);

                //方法注册
                RegisterMethods(component, registrar);
            }
        }

        /// <summary>
        /// init方法和Release方法
        /// </summary>
        /// <typeparam name="TReflectionActivatorData"></typeparam>
        /// <typeparam name="TSingleRegistrationStyle"></typeparam>
        /// <param name="component"></param>
        /// <param name="registrar"></param>
        protected virtual void RegisterMethods<TReflectionActivatorData, TSingleRegistrationStyle>(ComponentModel component,
            IRegistrationBuilder<object, TReflectionActivatorData, TSingleRegistrationStyle> registrar)
            where TReflectionActivatorData : ReflectionActivatorData
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            Tuple<MethodInfo, bool> AssertMethod(Type type, string methodName, bool havaParams = false)
            {
                MethodInfo method = null;
                try
                {
                    BindingFlags flags = BindingFlags.Public |
                                         BindingFlags.NonPublic |
                                         BindingFlags.Static |
                                         BindingFlags.Instance |
                                         BindingFlags.DeclaredOnly;
                    method = type.GetMethod(methodName, flags);
                }
                catch (Exception)
                {
                    //如果有多个就抛出异常
                    throw new DependencyResolutionException($"find method: {methodName} in type:{type.FullName} have more then one");
                }

                if (method == null)
                {
                    throw new DependencyResolutionException($"find method: {methodName} in type:{type.FullName} error");
                }

                var withParameter = false;
                var parameters = method.GetParameters();
                if (havaParams && parameters.Any())
                {
                    if (parameters.Length > 1)
                        throw new DependencyResolutionException($"method: {methodName} in type:{type.FullName} must without any parameters");
                    if (parameters.First().ParameterType != typeof(IComponentContext))
                    {
                        throw new DependencyResolutionException($"method: {methodName} in type:{type.FullName} ,the parameter type must be IComponentContext");
                    }

                    withParameter = true;
                }

                return new Tuple<MethodInfo, bool>(method, withParameter);
            }

            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            if (registrar == null)
            {
                throw new ArgumentNullException(nameof(registrar));
            }

            if (!string.IsNullOrEmpty(component.InitMethod))
            {
                var method = AssertMethod(component.CurrentType, component.InitMethod, true);
                if (!method.Item2)
                {
                    registrar.OnActivated(e => { method.Item1.Invoke(e.Instance, null); });
                }
                else
                {
                    registrar.OnActivated(e => { method.Item1.Invoke(e.Instance, new object[] { e.Context }); });
                }
            }

            if (!string.IsNullOrEmpty(component.DestroyMetnod))
            {
                var method = AssertMethod(component.CurrentType, component.DestroyMetnod);
                registrar.OnRelease(e => { method.Item1.Invoke(e, null); });
            }
        }

        /// <summary>
        /// 拦截器
        /// </summary>
        /// <typeparam name="TLimit"></typeparam>
        /// <typeparam name="TConcreteReflectionActivatorData"></typeparam>
        /// <typeparam name="TRegistrationStyle"></typeparam>
        /// <param name="component"></param>
        /// <param name="registrar"></param>
        protected virtual void SetIntercept<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle>(ComponentModel component,
            IRegistrationBuilder<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle> registrar)
            where TConcreteReflectionActivatorData : ConcreteReflectionActivatorData
        {
            if (registrar == null)
            {
                throw new ArgumentNullException(nameof(registrar));
            }

            if (component.Interceptor != null)
            {
                switch (component.InterceptorType)
                {
                    case InterceptorType.Interface:
                        if (!string.IsNullOrEmpty(component.InterceptorKey))
                        {
                            registrar.EnableInterfaceInterceptors().InterceptedBy(new KeyedService(component.InterceptorKey, component.Interceptor));
                            return;
                        }

                        registrar.EnableInterfaceInterceptors().InterceptedBy(component.Interceptor);
                        return;
                    case InterceptorType.Class:
                        if (!string.IsNullOrEmpty(component.InterceptorKey))
                        {
                            registrar.EnableClassInterceptors().InterceptedBy(new KeyedService(component.InterceptorKey, component.Interceptor));
                            return;
                        }

                        registrar.EnableClassInterceptors().InterceptedBy(component.Interceptor);
                        return;
                }
            }
        }

        /// <summary>
        /// Sets the auto activation mode for the component.
        /// </summary>
        /// <typeparam name="TReflectionActivatorData"></typeparam>
        /// <typeparam name="TSingleRegistrationStyle"></typeparam>
        /// <param name="component"></param>
        /// <param name="registrar"></param>
        protected virtual void SetAutoActivate<TReflectionActivatorData, TSingleRegistrationStyle>(ComponentModel component,
            IRegistrationBuilder<object, TReflectionActivatorData, TSingleRegistrationStyle> registrar)
            where TReflectionActivatorData : ReflectionActivatorData
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            if (registrar == null)
            {
                throw new ArgumentNullException(nameof(registrar));
            }

            if (component.AutoActivate)
            {
                registrar.AutoActivate();
            }
        }

        /// <summary>
        /// 设置属性自动注入
        /// </summary>
        /// <typeparam name="TReflectionActivatorData"></typeparam>
        /// <typeparam name="TSingleRegistrationStyle"></typeparam>
        /// <param name="component"></param>
        /// <param name="registrar"></param>
        protected virtual void SetInjectProperties<TReflectionActivatorData, TSingleRegistrationStyle>(ComponentModel component,
            IRegistrationBuilder<object, TReflectionActivatorData, TSingleRegistrationStyle> registrar)
            where TReflectionActivatorData : ReflectionActivatorData
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            if (registrar == null)
            {
                throw new ArgumentNullException(nameof(registrar));
            }

            if (component.InjectProperties)
            {
                if (component.InjectPropertyType.Equals(InjectPropertyType.ALL))
                {
                    //保留autofac原本的方式
                    registrar.PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);
                    return;
                }

                component.AutowiredPropertyInfoList = (from p in component.CurrentType.GetAllProperties()
                                                       let propertyType = p.GetType()
                                                       let typeInfo = p.GetType().GetTypeInfo()
                                                       let va = p.GetCustomAttribute<Autowired>()
                                                       where va != null && !typeInfo.IsValueType
                                                             && (!propertyType.IsArray || !propertyType.GetElementType().GetTypeInfo().IsValueType)
                                                             && (!propertyType.IsGenericEnumerableInterfaceType()
                                                                 || !typeInfo.GenericTypeArguments[0].GetTypeInfo().IsValueType)
                                                       select new Tuple<PropertyInfo, Autowired>(p, va))
                                                          .ToList();


                component.AutowiredFieldInfoList = (from p in component.CurrentType.GetAllFields()
                                                    let propertyType = p.GetType()
                                                    let typeInfo = p.GetType().GetTypeInfo()
                                                    let va = p.GetCustomAttribute<Autowired>()
                                                    where va != null && !typeInfo.IsValueType
                                                                     && (!propertyType.IsArray
                                                                         || !propertyType.GetElementType().GetTypeInfo().IsValueType)
                                                                     && (!propertyType.IsGenericEnumerableInterfaceType()
                                                                         || !typeInfo.GenericTypeArguments[0].GetTypeInfo().IsValueType)
                                                    select new Tuple<FieldInfo, Autowired>(p, va))
                                                    .ToList();

                if (!component.AutowiredPropertyInfoList.Any() && !component.AutowiredFieldInfoList.Any())
                {
                    return;
                }

                if (!AllowCircularDependencies)
                {
                    //不支持循环
                    registrar.OnActivating((e) =>
                    {
                        DoAutoWired(e.Context, e.Parameters, e.Instance, false);
                    });
                }
                else
                {
                    //支持循环
                    registrar.RegistrationData.ActivatedHandlers.Add((s, e) =>
                    {
                        DoAutoWired(e.Context, e.Parameters, e.Instance, true);
                    });
                }
            }
        }


        /// <summary>
        /// 设置Ownership
        /// </summary>
        /// <typeparam name="TReflectionActivatorData"></typeparam>
        /// <typeparam name="TSingleRegistrationStyle"></typeparam>
        /// <param name="component"></param>
        /// <param name="registrar"></param>
        protected virtual void SetComponentOwnership<TReflectionActivatorData, TSingleRegistrationStyle>(ComponentModel component,
            IRegistrationBuilder<object, TReflectionActivatorData, TSingleRegistrationStyle> registrar)
            where TReflectionActivatorData : ReflectionActivatorData
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            if (registrar == null)
            {
                throw new ArgumentNullException(nameof(registrar));
            }

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
        /// 设置scope
        /// </summary>
        /// <typeparam name="TReflectionActivatorData"></typeparam>
        /// <typeparam name="TSingleRegistrationStyle"></typeparam>
        /// <param name="component"></param>
        /// <param name="registrar"></param>
        protected virtual void SetLifetimeScope<TReflectionActivatorData, TSingleRegistrationStyle>(ComponentModel component,
            IRegistrationBuilder<object, TReflectionActivatorData, TSingleRegistrationStyle> registrar)
            where TReflectionActivatorData : ReflectionActivatorData
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            if (registrar == null)
            {
                throw new ArgumentNullException(nameof(registrar));
            }

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
        /// 动态注入打了value标签的值
        /// </summary>
        /// <typeparam name="TReflectionActivatorData"></typeparam>
        /// <typeparam name="TSingleRegistrationStyle"></typeparam>
        /// <param name="component"></param>
        /// <param name="registrar"></param>
        protected virtual void RegisterComponentValues<TReflectionActivatorData, TSingleRegistrationStyle>(ComponentModel component,
            IRegistrationBuilder<object, TReflectionActivatorData, TSingleRegistrationStyle> registrar)
            where TReflectionActivatorData : ReflectionActivatorData
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            if (registrar == null)
            {
                throw new ArgumentNullException(nameof(registrar));
            }

            component.ValuePropertyInfoList = (from p in component.CurrentType.GetAllProperties()
                                               let va = p.GetCustomAttribute<Value>()
                                               where va != null
                                               select new Tuple<PropertyInfo, Value>(p, va)).ToList();

            component.ValueFieldInfoList = (from p in component.CurrentType.GetAllFields()
                                            let va = p.GetCustomAttribute<Value>()
                                            where va != null
                                            select new Tuple<FieldInfo, Value>(p, va)).ToList();

            if (!component.ValueFieldInfoList.Any() && !component.ValuePropertyInfoList.Any())
            {
                return;
            }

            //创建对象之后调用
            registrar.OnActivated(e =>
            {
                var instance = e.Instance;
                if (instance == null) return;
                Type instanceType = instance.GetType();
                object RealInstance = instance;
                if (ProxyUtil.IsProxy(instance))
                {
                    RealInstance = ProxyUtil.GetUnproxiedInstance(instance);
                    instanceType = ProxyUtil.GetUnproxiedType(instance);
                }

                if (RealInstance == null) return;
                if (!ComponentModelCache.TryGetValue(instanceType, out ComponentModel model)) return;
                foreach (var field in model.ValueFieldInfoList)
                {
                    var value = field.Item2.ResolveFiled(field.Item1, e.Context);
                    if (value == null) continue;
                    try
                    {
                        field.Item1.GetReflector().SetValue(RealInstance, value);
                    }
                    catch (Exception ex)
                    {
                        throw new DependencyResolutionException($"Value set error,can not resolve class type:{instanceType.FullName} =====>" +
                                                                $" ,fail resolve field value:{field.Item1.Name} "
                                                                + (!string.IsNullOrEmpty(field.Item2.value) ? $",with value:[{field.Item2.value}]" : ""), ex);
                    }
                }

                foreach (var property in model.ValuePropertyInfoList)
                {
                    var value = property.Item2.ResolveProperty(property.Item1, e.Context);
                    if (value == null) continue;
                    try
                    {
                        property.Item1.GetReflector().SetValue(RealInstance, value);
                    }
                    catch (Exception ex)
                    {
                        throw new DependencyResolutionException($"Value set error,can not resolve class type:{instanceType.FullName} =====>" +
                                                                $" ,fail resolve property value:{property.Item1.Name} "
                                                                + (!string.IsNullOrEmpty(property.Item2.value) ? $",with value:[{property.Item2.value}]" : ""),
                            ex);
                    }
                }
            });
        }

        /// <summary>
        /// 注册Component
        /// </summary>
        /// <typeparam name="TReflectionActivatorData"></typeparam>
        /// <typeparam name="TSingleRegistrationStyle"></typeparam>
        /// <param name="component"></param>
        /// <param name="registrar"></param>
        protected virtual void RegisterComponentServices<TReflectionActivatorData, TSingleRegistrationStyle>(ComponentModel component,
            IRegistrationBuilder<object, TReflectionActivatorData, TSingleRegistrationStyle> registrar)
            where TReflectionActivatorData : ReflectionActivatorData
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            if (registrar == null)
            {
                throw new ArgumentNullException(nameof(registrar));
            }

            if (component.ComponentServiceList != null && component.ComponentServiceList.Any())
            {
                foreach (var componentServiceModel in component.ComponentServiceList)
                {
                    if (!string.IsNullOrEmpty(componentServiceModel.Key))
                    {
                        registrar.As(componentServiceModel.Type).Keyed(componentServiceModel.Key, componentServiceModel.Type);
                    }
                    else
                    {
                        registrar.As(componentServiceModel.Type);
                    }
                }
            }
        }


        /// <summary>
        /// 解析程序集
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private List<ComponentModel> GetAllComponent()
        {
            if (_assemblyList == null || _assemblyList.Length < 1)
            {
                throw new ArgumentNullException(nameof(_assemblyList));
            }

            var result = new List<ComponentModel>();
            var assemblyList = _assemblyList.Distinct();
            foreach (var assembly in assemblyList)
            {
                var types = assembly.GetExportedTypes();
                //找到类型中含有 Component 标签的类 排除掉抽象类
                var beanTypeList = (from type in types
                                    let bean = type.GetCustomAttribute<Bean>()
                                    where type.IsClass && !type.IsAbstract && bean != null
                                    select new
                                    {
                                        Type = type,
                                        Bean = bean
                                    }).ToList();

                foreach (var bean in beanTypeList)
                {
                    var component = EnumerateComponentServices(bean.Bean, bean.Type);
                    EnumerateMetaSourceAttributes(component);
                    result.Add(component);
                    ComponentModelCache[bean.Type] = component;
                }
            }

            return result;
        }

        /// <summary>
        /// 根据注解解析
        /// </summary>
        /// <param name="bean"></param>
        /// <param name="currentType"></param>
        /// <returns></returns>
        private ComponentModel EnumerateComponentServices(Bean bean, Type currentType)
        {
            if (bean == null)
            {
                throw new ArgumentNullException(nameof(bean));
            }

            var result = new ComponentModel
            {
                AutoActivate = bean.AutoActivate,
                AutofacScope = this.AutofacScope.Equals(AutofacScope.Default) ? bean.AutofacScope : this.AutofacScope,
                CurrentType = currentType,
                InjectProperties = bean.InjectProperties,
                InjectPropertyType = bean.InjectPropertyType,
                Ownership = bean.Ownership,
                Interceptor = bean.Interceptor,
                InterceptorKey = bean.InterceptorKey,
                InterceptorType = bean.InterceptorType,
                InitMethod = bean.InitMethod,
                DestroyMetnod = bean.DestroyMetnod
            };

            #region 解析注册对应的类的列表

            var re = new List<ComponentServiceModel>();


            if (bean.Service != null)
            {
                //本身有指定注册类型
                re.Add(new ComponentServiceModel
                {
                    Type = bean.Service,
                    Key = bean.Key
                });
            }
            else if (!string.IsNullOrEmpty(bean.Key))
            {
                //指定了key 默认为本身
                re.Add(new ComponentServiceModel
                {
                    Type = currentType,
                    Key = bean.Key
                });
            }

            if (bean.Services != null && bean.Services.Length > 0)
            {
                var keyList = new string[bean.Services.Length];
                if (bean.Keys != null && bean.Keys.Length > 0 && bean.Keys.Length <= bean.Services.Length)
                {
                    for (int i = 0; i < bean.Keys.Length; i++)
                    {
                        keyList[i] = bean.Keys[i];
                    }
                }

                for (int i = 0; i < bean.Services.Length; i++)
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
        /// 设置source源
        /// </summary>
        /// <param name="componentModel"></param>
        private void EnumerateMetaSourceAttributes(ComponentModel componentModel)
        {
            #region PropertySource

            componentModel.MetaSourceList = new List<MetaSourceData>();
            var metaSourceAttributes = componentModel.CurrentType.GetCustomAttributes<PropertySource>().ToList();
            if (metaSourceAttributes.Any())
            {
                metaSourceAttributes = metaSourceAttributes.OrderBy(r => r.Order).ToList();
                foreach (var metaSourceAttribute in metaSourceAttributes)
                {
                    MetaSourceData metaSource = new MetaSourceData
                    {
                        Origin = metaSourceAttribute.Path,
                        Embedded = metaSourceAttribute.Embedded,
                        MetaSourceType = metaSourceAttribute.MetaSourceType,
                        Order = metaSourceAttribute.Order
                    };

                    if (string.IsNullOrEmpty(metaSourceAttribute.Path))
                    {
                        metaSource.Origin = "appsettings.json";
                        metaSource.Embedded = false;
                        metaSource.MetaSourceType = MetaSourceType.JSON;
                        metaSource.Path = Path.Combine(GetAssemblyLocation(), metaSource.Origin);
                        metaSource.Configuration = EmbeddedConfiguration.Load(null, metaSource.Path, metaSource.MetaSourceType, metaSource.Embedded);
                        componentModel.MetaSourceList.Add(metaSource);
                        continue;
                    }


                    if (metaSourceAttribute.Embedded)
                    {
                        metaSource.Path = metaSource.Origin;
                    }
                    else
                    {
                        metaSource.Path = metaSource.Origin.StartsWith("/")
                            ? Path.Combine(GetAssemblyLocation(), metaSource.Origin.Substring(1, metaSource.Origin.Length - 1))
                            : metaSource.Origin;
                    }

                    metaSource.Configuration = EmbeddedConfiguration.Load(componentModel.CurrentType, metaSource.Path, metaSource.MetaSourceType,
                        metaSourceAttribute.Embedded);

                    componentModel.MetaSourceList.Add(metaSource);
                }
            }
            else
            {
                MetaSourceData metaSource = new MetaSourceData
                {
                    Origin = "appsettings.json",
                    Embedded = false,
                    MetaSourceType = MetaSourceType.JSON,
                    Order = 0
                };
                metaSource.Path = Path.Combine(GetAssemblyLocation(), metaSource.Origin);
                metaSource.Configuration = EmbeddedConfiguration.Load(null, metaSource.Path, metaSource.MetaSourceType, metaSource.Embedded);
                componentModel.MetaSourceList.Add(metaSource);
            }

            #endregion
        }

        /// <summary>
        /// 查询指定程序集下引用的所有程序集
        /// </summary>
        /// <returns></returns>
        private string GetAssemblyLocation()
        {
            //var entry = Assembly.GetEntryAssembly();
            //if (entry != null)
            //{
            //    return new FileInfo(entry.Location)?.Directory?.Parent?.FullName;
            //}

            return AppDomain.CurrentDomain.BaseDirectory;
        }

        /// <summary>
        /// 获取当前Domain所有的程序集
        /// </summary>
        /// <returns></returns>
        private IEnumerable<Assembly> GetAssemblies()
        {
            var list = new List<string>();
            var stack = new Stack<Assembly>();

            stack.Push(Assembly.GetEntryAssembly());

            do
            {
                var asm = stack.Pop();

                yield return asm;

                foreach (var reference in asm.GetReferencedAssemblies())
                    if (!list.Contains(reference.FullName))
                    {
                        stack.Push(Assembly.Load(reference));
                        list.Add(reference.FullName);
                    }
            } while (stack.Count > 0);
        }


        /// <summary>
        /// 属性注入
        /// </summary>
        /// <param name="context">容器</param>
        /// <param name="Parameters">参数</param>
        /// <param name="instance">实例</param>
        /// <param name="allowCircle">是否可以循环</param>
        private void DoAutoWired(IComponentContext context, IEnumerable<Parameter> Parameters, object instance, bool allowCircle)
        {
            if (instance == null) return;
            Type instanceType = instance.GetType();
            object RealInstance = instance;
            if (ProxyUtil.IsProxy(instance))
            {
                RealInstance = ProxyUtil.GetUnproxiedInstance(instance);
                instanceType = ProxyUtil.GetUnproxiedType(instance);
            }

            if (RealInstance == null) return;
            if (!ComponentModelCache.TryGetValue(instanceType, out ComponentModel model)) return;
            //字段注入
            foreach (var field in model.AutowiredFieldInfoList)
            {
                var obj = field.Item2.ResolveField(field.Item1, context, Parameters, RealInstance, allowCircle);
                if (obj == null)
                {
                    if (field.Item2.Required)
                    {
                        throw new DependencyResolutionException(
                            $"Autowire error,can not resolve class type:{instanceType.FullName},field name:{field.Item1.Name} "
                            + (!string.IsNullOrEmpty(field.Item2.Name) ? $",with key:{field.Item2.Name}" : ""));
                    }
                    continue;
                }
                try
                {
                    field.Item1.GetReflector().SetValue(RealInstance, obj);
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
                var obj = property.Item2.ResolveProperty(property.Item1, context, Parameters, RealInstance, allowCircle);
                if (obj == null)
                {
                    if (property.Item2.Required)
                    {
                        throw new DependencyResolutionException(
                            $"Autowire error,can not resolve class type:{instanceType.FullName},property name:{property.Item1.Name} "
                            + (!string.IsNullOrEmpty(property.Item2.Name) ? $",with key:{property.Item2.Name}" : ""));
                    }
                    continue;
                }
                try
                {
                    property.Item1.GetReflector().SetValue(RealInstance, obj);
                }
                catch (Exception ex)
                {
                    throw new DependencyResolutionException(
                        $"Autowire error,can not resolve class type:{instanceType.FullName},property name:{property.Item1.Name} "
                        + (!string.IsNullOrEmpty(property.Item2.Name) ? $",with key:{property.Item2.Name}" : ""), ex);
                }
            }
        }
    }
}