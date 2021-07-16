using System;
using Microsoft.Extensions.Configuration;

namespace Autofac.Annotation
{
    /// <summary>
    /// 打在class上面
    /// 可以存在多个 如果有key重复会覆盖
    /// 如果不打则默认获取目录下的appsetting.json文件里面的
    /// An array of metadata values to associate with the component. Each item specifies the name, type, and value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class PropertySource : Attribute
    {
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public PropertySource()
        {
        }

        /// <summary>
        /// 传入指定的路径是相对于工程目录的地址
        /// </summary>
        /// <param name="path"></param>
        public PropertySource(string path)
        {
            this.Path = path;
        }

        /// <summary>
        /// 对应的值
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 排序 越小的越优先加载
        /// </summary>
        public int OrderIndex { get; set; }

        /// <summary>
        /// 是否是内嵌资源
        /// </summary>
        public bool Embedded { get; set; }

        /// <summary>
        /// 动态数据源类型 必须继承接口：IDynamicSourceProvider
        /// </summary>
        public Type Dynamic { get; set; }

        /// <summary>
        /// 当动态数据源类型有多个注册的时候根据key来获取唯一
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 
        /// </summary>
        internal bool? _reload;

        /// <summary>
        /// 是否重新加载 只针对文件配置有效 对于Embedded=true的不生效
        /// </summary>
        public bool ReloadOnChange
        {
            get => _reload ?? false;
            set => _reload = value;
        }

        /// <summary>
        /// 资源格式类型
        /// </summary>
        public MetaSourceType MetaSourceType { get; set; } = MetaSourceType.Auto;
    }

    /// <summary>
    /// 动态数据源
    /// </summary>
    public interface IDynamicSourceProvider : IConfigurationProvider
    {
        /// <summary>
        /// 配置信息传递
        /// </summary>
        /// <param name="source"></param>
        void setPropertySource(PropertySource source);
    }
}