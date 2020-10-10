using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Extensions.Reflection;
using Autofac.Aspect;

namespace Autofac.Annotation
{
    /// <summary>
    /// 根据注解解析
    /// </summary>
    public class ComponentModel
    {
        /// <summary>
        /// 构造方法
        /// </summary>
        public ComponentModel()
        {
            MetaSourceList = new List<MetaSourceData>();
            ComponentServiceList = new List<ComponentServiceModel>();
            AutowiredFieldInfoList = new List<Tuple<FieldInfo, Autowired,FieldReflector>>();
            AutowiredPropertyInfoList = new List<Tuple<PropertyInfo, Autowired,PropertyReflector>>();
            ValueFieldInfoList = new List<Tuple<FieldInfo, Value,FieldReflector>>();
            ValuePropertyInfoList = new List<Tuple<PropertyInfo, Value,PropertyReflector>>();
        }
        /// <summary>
        /// 当前类所在类型
        /// </summary>
        public Type CurrentType { get; set; }

        /// <summary>
        /// 注册的对应类型
        /// </summary>
        public List<ComponentServiceModel> ComponentServiceList { get; set; }

        /// <summary>
        /// 需要装配的Autowired的字段集合
        /// </summary>
        public List<Tuple<FieldInfo,Autowired,FieldReflector>> AutowiredFieldInfoList { get; set; }
        
        /// <summary>
        /// 需要装配的Autowired的属性集合
        /// </summary>
        public List<Tuple<PropertyInfo,Autowired,PropertyReflector>> AutowiredPropertyInfoList { get; set; }
        
        
        /// <summary>
        /// 需要装配的Value的字段集合
        /// </summary>
        public List<Tuple<FieldInfo,Value,FieldReflector>> ValueFieldInfoList { get; set; }
        
        /// <summary>
        /// 需要装配的Value的属性集合
        /// </summary>
        public List<Tuple<PropertyInfo,Value,PropertyReflector>> ValuePropertyInfoList { get; set; }

        /// <summary>
        /// PropertySource
        /// </summary>
        public List<MetaSourceData> MetaSourceList { get; set; }

        /// <summary>
        /// Aspect标签
        /// </summary>
        internal Aspect AspectAttribute { get; set; }



        /// <summary>
        /// A Boolean indicating if the component should auto-activate.
        /// SingleInstance Scope default is true
        /// </summary>
        public bool AutoActivate { get; set; }

        /// <summary>
        /// A Boolean indicating whether property (setter) injection for the component should be enabled.
        /// </summary>
        public bool InjectProperties { get; set; }

        /// <summary>
        /// 属性自动装配的类型
        /// </summary>
        public InjectPropertyType InjectPropertyType { get; set; }

        /// <summary>
        /// 作用域
        /// </summary>
        public AutofacScope AutofacScope { get; set; }

        /// <summary>
        /// 如果设置值为external代表需要自己管理dispose
        /// </summary>
        public Ownership Ownership { get; set; }

        /// <summary>
        /// 指定拦截器类型
        /// </summary>
        public Type Interceptor { get; set; }

        /// <summary>
        /// 如果同一个类型的拦截器有多个 可以指定Key
        /// </summary>
        public string InterceptorKey { get; set; }

        /// <summary>
        /// 拦截器类型
        /// </summary>
        public InterceptorType InterceptorType { get; set; } 
        
        /// <summary>
        /// 被创建后执行的方法
        /// </summary>
        public string InitMethod { get; set; }
        /// <summary>
        /// 被Release时执行的方法
        /// </summary>
        public string DestroyMethod { get; set; }

        /// <summary>
        /// 自定义注册顺序
        /// </summary>
        public int OrderIndex { get; set; }
        
        /// <summary>
        /// 不允许被切面扫描到
        /// </summary>
        internal bool NotUseProxy { get; set; }
        
         
        /// <summary>
        /// 当前注册的类型是否是动态泛型 https://github.com/yuzd/Autofac.Annotation/issues/13
        /// </summary>
        internal bool isDynamicGeneric {
            get
            {
                var currentTypeInfo = this.CurrentType.GetTypeInfo();
                if (currentTypeInfo.IsGenericTypeDefinition)
                {
                    return true;
                }
                return false;
            } 
        }
    }

    /// <summary>
    /// MetaSourceData
    /// </summary>
    public class MetaSourceData
    {

        /// <summary>
        /// 原来的值
        /// </summary>
        public string Origin { get; set; }

        /// <summary>
        /// 转换成路径
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 是否是内嵌资源
        /// </summary>
        public bool Embedded { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// 是否开启监听重新加载
        /// </summary>
        public bool Reload { get; set; }

        /// <summary>
        /// 资源格式类型
        /// </summary>
        public MetaSourceType MetaSourceType { get; set; }


        /// <summary>
        /// Configuration
        /// </summary>
        public Lazy<IConfiguration> ConfigurationLazy;
        
        /// <summary>
        /// Configuration
        /// </summary>
        public IConfiguration Configuration => ConfigurationLazy?.Value;
    }

    /// <summary>
    /// MetaSource源文件的类型
    /// </summary>
    public enum MetaSourceType
    {
        /// <summary>
        /// 根据后缀自动判断
        /// </summary>
        Auto,
        /// <summary>
        /// json文件
        /// </summary>
        JSON,
        /// <summary>
        /// xml格式
        /// </summary>
        XML
    }


    /// <summary>
    /// 注册对应的类型
    /// </summary>
    public class ComponentServiceModel
    {
        /// <summary>
        /// 类型
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// 别名
        ///  1. 如果同一个类型注册多个相同别名 那么根据别名会获取到最后一个注册的
        ///  2. 如果一个类型 既注册了别名 又注册了非别名 那么根据别名只能获取到别名注册的，要想获取非别名注册就不能带Key去Autowired
        /// </summary>
        public string Key { get; set; }
    }


    /// <summary>
    /// 注册信息
    /// </summary>
    public class BeanDefination
    {
        /// <summary>
        /// ctor
        /// </summary>
        public BeanDefination()
        {
            
        }
        
        /// <summary>
        /// ctor
        /// </summary>
        public BeanDefination(Type _type)
        {
            this.Type = _type;
            this.Bean = new Component();
        }
        
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="_type">要注册的类型</param>
        /// <param name="asType">注册成为的类型</param>
        public BeanDefination(Type _type,Type asType)
        {
            this.Type = _type;
            this.Bean = new Component(asType);
        }
        
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="_type">要注册的类型</param>
        /// <param name="asKey">注册的key</param>
        public BeanDefination(Type _type,string asKey)
        {
            this.Type = _type;
            this.Bean = new Component(asKey);
        }
        
        /// <summary>
        /// 当前类型
        /// </summary>
        public Type Type { get; set; }
        /// <summary>
        /// 注册定义
        /// </summary>
        public Component Bean { get; set; }
        /// <summary>
        /// 按照从小到大的顺序注册 如果同一个Type被处理多次会被覆盖！
        /// </summary>
        internal int OrderIndex { get; set; }
    }
}
