using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac.Annotation.Util;
using Autofac.AspectIntercepter.Advice;
using Autofac.AspectIntercepter.Pointcut;
using Microsoft.Extensions.Configuration;

namespace Autofac.Annotation
{
    /// <summary>
    ///     根据注解解析
    /// </summary>
    internal class ComponentModel
    {
        /// <summary>
        ///     构造方法
        /// </summary>
        public ComponentModel()
        {
            MetaSourceList = new List<MetaSourceData>();
            ComponentServiceList = new List<ComponentServiceModel>();
            AutowiredFieldInfoList = new List<Tuple<FieldInfo, Autowired, FieldInfo>>();
            AutowiredPropertyInfoList = new List<Tuple<PropertyInfo, Autowired, PropertyInfo>>();
            AutowiredMethodInfoList = new List<Tuple<MethodInfo, Autowired>>();
            ValueFieldInfoList = new List<Tuple<FieldInfo, Value, FieldInfo>>();
            ValuePropertyInfoList = new List<Tuple<PropertyInfo, Value, PropertyInfo>>();
        }

        /// <summary>
        ///     当前类所在类型
        /// </summary>
        public Type CurrentType { get; set; }

        /// <summary>
        ///     注册的对应类型
        /// </summary>
        public List<ComponentServiceModel> ComponentServiceList { get; set; }

        /// <summary>
        ///     需要装配的Autowired的字段集合
        /// </summary>
        public List<Tuple<FieldInfo, Autowired, FieldInfo>> AutowiredFieldInfoList { get; set; }

        /// <summary>
        ///     需要装配的Autowired的属性集合
        /// </summary>
        public List<Tuple<PropertyInfo, Autowired, PropertyInfo>> AutowiredPropertyInfoList { get; set; }

        /// <summary>
        ///     需要装配的Autowired的public方法集合
        /// </summary>
        public List<Tuple<MethodInfo, Autowired>> AutowiredMethodInfoList { get; set; }

        /// <summary>
        ///     需要装配的Value的字段集合
        /// </summary>
        public List<Tuple<FieldInfo, Value, FieldInfo>> ValueFieldInfoList { get; set; }

        /// <summary>
        ///     需要装配的Value的属性集合
        /// </summary>
        public List<Tuple<PropertyInfo, Value, PropertyInfo>> ValuePropertyInfoList { get; set; }

        /// <summary>
        ///     PropertySource
        /// </summary>
        public List<MetaSourceData> MetaSourceList { get; set; }


        /// <summary>
        ///     当前Class的所有的标签 包括父类 包括接口
        /// </summary>
        internal List<Attribute> CurrentClassTypeAttributes { get; set; }


        /// <summary>
        ///     A Boolean indicating if the component should auto-activate.
        ///     true的话 autofac容器在构建后自动触发该类的初始化
        /// </summary>
        public bool AutoActivate { get; set; }

        /// <summary>
        ///     A Boolean indicating whether property (setter) injection for the component should be enabled.
        /// </summary>
        public bool InjectProperties { get; set; }

        /// <summary>
        ///     属性自动装配的类型
        /// </summary>
        public InjectPropertyType InjectPropertyType { get; set; }

        /// <summary>
        ///     作用域
        /// </summary>
        public AutofacScope AutofacScope { get; set; }

        /// <summary>
        ///     如果设置值为external代表需要自己管理dispose
        /// </summary>
        public Ownership Ownership { get; set; }

        /// <summary>
        ///     指定拦截器类型
        /// </summary>
        public Type Interceptor { get; set; }

        /// <summary>
        ///     如果同一个类型的拦截器有多个 可以指定Key
        /// </summary>
        public string InterceptorKey { get; set; }

        /// <summary>
        ///     拦截器类型
        /// </summary>
        public InterceptorType InterceptorType { get; set; }

        public bool EnableAspect { get; set; }

        /// <summary>
        ///     被创建后执行的方法
        /// </summary>
        public string InitMethod { get; set; }

        /// <summary>
        ///     被Release时执行的方法
        /// </summary>
        public string DestroyMethod { get; set; }

        /// <summary>
        ///     自定义注册顺序
        /// </summary>
        public int OrderIndex { get; set; }

        /// <summary>
        ///     不允许被切面扫描到
        /// </summary>
        internal bool NotUseProxy { get; set; }

        /// <summary>
        ///     是否要注册为后置处理器
        /// </summary>
        internal bool IsBenPostProcessor { get; set; }


        /// <summary>
        ///     当前注册的类型是否是动态泛型 https://github.com/yuzd/Autofac.Annotation/issues/13
        /// </summary>
        internal bool isDynamicGeneric
        {
            get
            {
                var currentTypeInfo = CurrentType.GetTypeInfo();
                if (currentTypeInfo.IsGenericTypeDefinition) return true;

                return false;
            }
        }

        /// <summary>
        /// 是否是泛型类
        /// </summary>
        internal bool isGeneric
        {
            get
            {
                var currentTypeInfo = CurrentType.GetTypeInfo();
                return currentTypeInfo.IsGenericType;
            }
        }

        /// <summary>
        ///     注册类型
        /// </summary>
        internal RegisterType RegisterType { get; set; } = RegisterType.Compoment;

        #region DependsOn

        /// <summary>
        ///     依赖的 是用来表示一个bean A的实例化依赖另一个bean B的实例化， 但是A并不需要持有一个B的对象
        /// </summary>
        internal DependsOn DependsOn { get; set; }

        #endregion


        /// <summary>
        ///     一个class下所有的打了aspect标签的方法缓存
        /// </summary>
        internal readonly ConcurrentDictionary<MethodInfo, List<AspectInvokeAttribute>> AspectAttributeCache =
            new ConcurrentDictionary<MethodInfo, List<AspectInvokeAttribute>>();

        /// <summary>
        /// 是否Pointcut要拦截继承(比如抽象父类)的方法 默认为true
        /// </summary>
        public bool EnablePointcutInherited { get; set; } = true;

        /// <summary>
        /// 针对泛型 在resovle的时候动态注册的时候 检查哪些method需要pointcut
        /// </summary>
        internal readonly ConcurrentBag<string> DynamicGenricMethodsNeedPointcuts = new ConcurrentBag<string>();

        /// <summary>
        /// 针对飞泛型 在resovle的时候动态注册的时候 检查哪些method需要pointcut
        /// </summary>
        internal readonly ConcurrentBag<ObjectKey> MethodsNeedPointcuts = new ConcurrentBag<ObjectKey>();
    }

    /// <summary>
    ///     MetaSourceData
    /// </summary>
    internal class MetaSourceData
    {
        /// <summary>
        ///     Configuration
        /// </summary>
        public Lazy<IConfiguration> ConfigurationLazy;

        /// <summary>
        ///     原来的值
        /// </summary>
        public string Origin { get; set; }

        /// <summary>
        ///     转换成路径
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        ///     是否是内嵌资源
        /// </summary>
        public bool Embedded { get; set; }

        /// <summary>
        ///     当类型为Dynamic的时候使用
        /// </summary>
        public Type DynamicSource { get; set; }

        /// <summary>
        ///     排序
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        ///     是否开启监听重新加载
        /// </summary>
        public bool Reload { get; set; }

        /// <summary>
        ///     资源格式类型
        /// </summary>
        public MetaSourceType MetaSourceType { get; set; }

        /// <summary>
        ///     Configuration
        /// </summary>
        public IConfiguration Configuration => ConfigurationLazy?.Value;
    }

    /// <summary>
    ///     MetaSource源文件的类型
    /// </summary>
    public enum MetaSourceType
    {
        /// <summary>
        ///     根据后缀自动判断
        /// </summary>
        Auto,

        /// <summary>
        ///     json文件
        /// </summary>
        JSON,

        /// <summary>
        ///     xml格式
        /// </summary>
        XML,

        /// <summary>
        ///     动态自定义类型
        /// </summary>
        Dynamic
    }


    /// <summary>
    ///     注册对应的类型
    /// </summary>
    internal class ComponentServiceModel
    {
        /// <summary>
        ///     类型
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        ///     别名
        ///     1. 如果同一个类型注册多个相同别名 那么根据别名会获取到最后一个注册的
        ///     2. 如果一个类型 既注册了别名 又注册了非别名 那么根据别名只能获取到别名注册的，要想获取非别名注册就不能带Key去Autowired
        /// </summary>
        public string Key { get; set; }
    }


    /// <summary>
    ///     注册信息
    /// </summary>
    public class BeanDefination
    {
        /// <summary>
        ///     ctor
        /// </summary>
        public BeanDefination()
        {
        }

        /// <summary>
        ///     ctor
        /// </summary>
        public BeanDefination(Type _type)
        {
            Type = _type;
            Bean = new Component();
        }

        /// <summary>
        ///     ctor
        /// </summary>
        /// <param name="_type">要注册的类型</param>
        /// <param name="asType">注册成为的类型</param>
        public BeanDefination(Type _type, Type asType)
        {
            Type = _type;
            Bean = new Component(asType);
        }

        /// <summary>
        ///     ctor
        /// </summary>
        /// <param name="_type">要注册的类型</param>
        /// <param name="asKey">注册的key</param>
        public BeanDefination(Type _type, string asKey)
        {
            Type = _type;
            Bean = new Component(asKey);
        }

        /// <summary>
        ///     当前类型
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        ///     注册定义
        /// </summary>
        public Component Bean { get; set; }

        /// <summary>
        ///     按照从小到大的顺序注册 如果同一个Type被处理多次会被覆盖！
        /// </summary>
        internal int OrderIndex { get; set; }

        /// <summary>
        ///     toString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Type.Namespace + "." + Type.Name + "(" + OrderIndex + ")";
        }
    }

    /// <summary>
    ///     注册缓存
    /// </summary>
    internal class ComponentModelCacheSingleton
    {
        public ConcurrentDictionary<Type, ComponentModel> ComponentModelCache { get; set; }

        /// <summary>
        ///     存储动态泛型类
        /// </summary>
        public ConcurrentDictionary<string, ComponentModel> DynamicComponentModelCache { get; set; }

        /// <summary>
        ///     存储所有PointCut信息
        /// </summary>
        public PointCutConfigurationList PointCutConfigurationList { get; set; }
    }

    /// <summary>
    ///     注册类型 内部维护
    /// </summary>
    internal enum RegisterType
    {
        Compoment,
        Bean,
        Import,
        PointCut,
        AutoConfiguration
    }

    internal class ObjectKey
    {
        public ObjectKey(Type type, MethodInfo method)
        {
            this.Type = type;
            this.Method = method;
        }

        public Type Type { get; set; }
        public MethodInfo Method { get; set; }

        public override int GetHashCode()
        {
            return this.Type.GetHashCode() + this.Method.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var item = obj as ObjectKey;
            if (item == null)
            {
                return false;
            }

            return this.Type == item.Type && this.Method == item.Method;
        }
    }
}