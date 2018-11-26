using Autofac.Annotation.Util;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.AttributeFilters;
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

                //property注入
                RegisterComponentProperties(component, registrar);




            }
        }

        /// <summary>
        /// 注册property注入
        /// </summary>
        /// <typeparam name="TReflectionActivatorData"></typeparam>
        /// <typeparam name="TSingleRegistrationStyle"></typeparam>
        /// <param name="component"></param>
        /// <param name="registrar"></param>
        protected virtual void RegisterComponentProperties<TReflectionActivatorData, TSingleRegistrationStyle>(ComponentModel component, IRegistrationBuilder<object, TReflectionActivatorData, TSingleRegistrationStyle> registrar)
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

            //foreach (var prop in component.GetProperties("properties"))
            //{
            //    registrar.WithProperty(prop);
            //}


            var properties = (from p in component.CurrentType.GetAllProperties()
                             let va = p.GetCustomAttribute<Value>()
                             where va != null
                             select new
                             {
                                 Property = p,
                                 Value = va
                             }).ToList();

            foreach (var property in properties)
            {
                var resolvedParameter = new ResolvedParameter(
                    (pi, c) =>
                    {
                        PropertyInfo prop;
                        return pi.TryGetDeclaringProperty(out prop) && string.Equals(prop.Name, property.Property.Name, StringComparison.OrdinalIgnoreCase);
                    },
                    (pi, c) => property.Value.ResolveParameter(pi, c));
                registrar.WithProperty(resolvedParameter);
            }

            var fields = (from p in component.CurrentType.GetAllFields()
                let va = p.GetCustomAttribute<Value>()
                where va != null
                select new
                {
                    Property = p,
                    Value = va
                }).ToList();

            foreach (var property in fields)
            {
                var resolvedParameter = new ResolvedParameter(
                    (pi, c) =>
                    {
                        PropertyInfo prop;
                        return pi.TryGetDeclaringProperty(out prop) && string.Equals(prop.Name, property.Property.Name, StringComparison.OrdinalIgnoreCase);
                    },
                    (pi, c) => property.Value.ResolveParameter(pi, c));
                registrar.WithProperty(resolvedParameter);
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
                AutofacScope = bean.AutofacScope,
                CurrentType = currentType,
                InjectProperties = bean.InjectProperties,
                InjectPropertyType = bean.InjectPropertyType,
                Ownership = bean.Ownership
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
