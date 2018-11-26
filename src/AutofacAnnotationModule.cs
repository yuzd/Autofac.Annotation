using Autofac.Builder;
using Autofac.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Autofac.Configuration;
using Autofac.Features.AttributeFilters;

namespace Autofac.Annotation
{
    /// <summary>
    /// autofac模块用注解的模式注册
    /// </summary>
    public class AutofacAnnotationModule : Module
    {
        /// <summary>
        /// ComponentModel缓存
        /// </summary>
        public static Dictionary<Type, ComponentModel> ComponentModelCache = new Dictionary<Type, ComponentModel>();

        private readonly Assembly[] _assemblyList;

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

            _assemblyList = GetAssemblies().Where(assembly => assemblyNameList.Equals(assembly.GetName().Name)).ToArray();
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




            }
        }

        /// <summary>
        /// 注册Component
        /// </summary>
        /// <typeparam name="TReflectionActivatorData"></typeparam>
        /// <typeparam name="TSingleRegistrationStyle"></typeparam>
        /// <param name="component"></param>
        /// <param name="registrar"></param>
        protected virtual void RegisterComponentServices<TReflectionActivatorData, TSingleRegistrationStyle>(ComponentModel component, IRegistrationBuilder<object, TReflectionActivatorData, TSingleRegistrationStyle> registrar)
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
                        registrar.As(new KeyedService(componentServiceModel.Key, componentServiceModel.Type));
                    }
                    else
                    {
                        registrar.As(new TypedService(componentServiceModel.Type));
                    }
                }
            }
        }



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
                                    let bean = type.GetCustomAttribute<Component>()
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
        /// <param name="component"></param>
        /// <param name="currentType"></param>
        /// <returns></returns>
        private ComponentModel EnumerateComponentServices(Component component, Type currentType)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }
            var result = new ComponentModel
            {
                AutoActivate = component.AutoActivate,
                AutofacScope = component.AutofacScope,
                CurrentType = currentType,
                InjectProperties = component.InjectProperties,
                InjectPropertyType = component.InjectPropertyType,
                Ownership = component.Ownership
            };

            #region 解析注册对应的类的列表
            var re = new List<ComponentServiceModel>();
            if (!string.IsNullOrEmpty(component.Key) && component.Service == null)
            {
                if (component.Services == null)
                {
                    component.Services = new Type[] {currentType};
                }
                else
                {
                    var _li = component.Services.ToList();
                    _li.Add(currentType);
                    component.Services = _li.ToArray();
                }
            }
            if (component.Service != null)
            {
                if (component.Services == null)
                {
                    component.Services = new Type[] {component.Service};
                }
                else
                {
                    var _li = component.Services.ToList();
                    _li.Add(component.Service);
                    component.Services = _li.ToArray();
                }
            }

            if (component.Services != null && component.Services.Length > 0)
            {
                component.Services = component.Services.Distinct().ToArray();
                var keyList = new string[component.Services.Length];
                if (!string.IsNullOrEmpty(component.Key))
                {
                    if (component.Keys == null)
                    {
                        component.Keys = new string[] {component.Key};
                    }
                    else
                    {
                        var _li = component.Keys.ToList();
                        _li.Add(component.Key);
                        component.Keys = _li.ToArray();
                    }
                }
                if (component.Keys != null && component.Keys.Length > 0 && component.Keys.Length <= component.Services.Length)
                {
                    for (int i = 0; i < component.Keys.Length; i++)
                    {
                        keyList[i] = component.Keys[i];
                    }
                }

                for (int i = 0; i < component.Services.Length; i++)
                {
                    var serviceKey = keyList[i];
                    re.Add(new ComponentServiceModel
                    {
                        Type = component.Services[i],
                        Key = serviceKey
                    });
                }
            }
            result.ComponentServiceList = re;

            #endregion

            return result;
        }


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
                        metaSource.Path = metaSource.Origin.StartsWith("/") ? Path.Combine(GetAssemblyLocation(), metaSource.Origin.Substring(1,metaSource.Origin.Length-1)) : metaSource.Origin;
                    }

                    metaSource.Configuration = EmbeddedConfiguration.Load(componentModel.CurrentType, metaSource.Path, metaSource.MetaSourceType, metaSourceAttribute.Embedded);

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

            }
            while (stack.Count > 0);

        }
    }
}
