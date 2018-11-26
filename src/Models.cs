using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using Autofac.Configuration;

namespace Autofac.Annotation
{
    /// <summary>
    /// 根据注解解析
    /// </summary>
    public class ComponentModel
    {
        /// <summary>
        /// 当前类所在类型
        /// </summary>
        public Type CurrentType { get; set; }

        /// <summary>
        /// 注册的对应类型
        /// </summary>
        public List<ComponentServiceModel> ComponentServiceList { get; set; }


        /// <summary>
        /// 注册Meta
        /// </summary>
        public List<MetaData> MetaDataList { get; set; }

        /// <summary>
        /// PropertySource
        /// </summary>
        public List<MetaSourceData> MetaSourceList { get; set; }

        /// <summary>
        /// 构造方法对应的
        /// </summary>
        public List<ComponentParameter> ComponentParameterList { get; set; }


        /// <summary>
        /// A Boolean indicating if the component should auto-activate.
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
        public string Ownership { get; set; }

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
        /// 资源格式类型
        /// </summary>
        public MetaSourceType MetaSourceType { get; set; }

        /// <summary>
        /// Configuration
        /// </summary>
        public IConfiguration Configuration { get; set; }
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
    /// MetaData
    /// </summary>
    public class MetaData
    {
        /// <summary>
        /// 字段
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public object Value { get; set; }

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
        /// </summary>
        public string Key { get; set; }
    }

    /// <summary>
    /// 注册构造方法的参数
    /// </summary>
    public class ComponentParameter
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public object Value { get; set; }
    }

    /// <summary>
    /// 注册属性注入
    /// </summary>
    public class ComponentProperty
    {  /// <summary>
       /// 名称
       /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public object Value { get; set; }
    }
}
