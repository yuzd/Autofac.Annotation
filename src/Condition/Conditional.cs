using System;

namespace Autofac.Annotation.Condition
{
    /// <summary>
    /// attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class Conditional : Attribute
    {
        /// <summary>
        /// ctor
        /// </summary>
        public Conditional()
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="type"></param>
        public Conditional(Type type)
        {
            this.Type = type;
        }

        /// <summary>
        /// type必须是继承了Condition的接口
        /// </summary>
        public Type Type { get; set; }
    }

    /// <summary>
    /// 只能打在标有Bean的方法上面
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class ConditionOnMissingBean : Conditional
    {
        /// <summary>
        /// ctor
        /// </summary>
        public ConditionOnMissingBean()
        {
            this.Type = typeof(OnMissingBean);
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="type"></param>
        public ConditionOnMissingBean(Type type) : this()
        {
            this.type = type;
        }


        /// <summary>
        /// ctor
        /// </summary>
        public ConditionOnMissingBean(Type type, string name) : this(type)
        {
            this.name = name;
        }


        /// <summary>
        /// keyname
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 类型判断
        /// </summary>
        public Type type { get; set; }
    }

    /// <summary>
    /// 只能打在标有Bean的方法上面
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class ConditionOnBean : Conditional
    {
        /// <summary>
        /// ctor
        /// </summary>
        public ConditionOnBean()
        {
            this.Type = typeof(OnBean);
        }


        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="type"></param>
        public ConditionOnBean(Type type) : this()
        {
            this.type = type;
        }


        /// <summary>
        /// ctor
        /// </summary>
        public ConditionOnBean(Type type, string name) : this(type)
        {
            this.name = name;
        }

        /// <summary>
        /// keyname
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 类型判断
        /// </summary>
        public Type type { get; set; }
    }

    /// <summary>
    /// 根据class的全路径(namespace.classname)如果没有找到对应的class就加载
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class ConditionOnMissingClass : Conditional
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="classPath"></param>
        public ConditionOnMissingClass(string classPath)
        {
            this.name = classPath;
            this.Type = typeof(OnMissingClass);
        }

        /// <summary>
        /// keyname
        /// </summary>
        public string name { get; set; }
    }

    /// <summary>
    /// 根据class的全路径(namespace.classname)如果找到对应的class就加载
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class ConditionOnClass : Conditional
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="classPath"></param>
        public ConditionOnClass(string classPath)
        {
            this.name = classPath;
            this.Type = typeof(OnClass);
        }

        /// <summary>
        /// keyname
        /// </summary>
        public string name { get; set; }
    }

    /// <summary>
    /// 根据属性来配置
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class ConditionalOnProperty : Conditional
    {
        /// <summary>
        /// ctor
        /// </summary>
        public ConditionalOnProperty()
        {
            this.Type = typeof(OnProperty);
        }

        /// <summary>
        /// ctor
        /// </summary>
        public ConditionalOnProperty(string name) : this()
        {
            this.name = name;
        }

        /// <summary>
        /// ctor
        /// </summary>
        public ConditionalOnProperty(string name, string havingValue) : this(name)
        {
            this.havingValue = havingValue;
        }

        /// <summary>
        /// keyname
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 是否有值
        /// </summary>
        public string havingValue { get; set; }

        /// <summary>
        /// 表示如果没有在appsettings.json设置该属性，则默认为条件符合
        /// </summary>
        public bool matchIfMissing { get; set; }
    }

    /// <summary>
    /// 根据属性来配置
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class ConditionalOnProperties : Conditional
    {
        /// <summary>
        /// ctor
        /// </summary>
        public ConditionalOnProperties()
        {
            this.Type = typeof(OnProperty);
        }

        /// <summary>
        /// ctor
        /// </summary>
        public ConditionalOnProperties(string[] names) : this()
        {
            this.names = names;
        }

        /// <summary>
        /// ctor
        /// </summary>
        public ConditionalOnProperties(string[] names, string havingValue) : this(names)
        {
            this.havingValue = havingValue;
        }

        /// <summary>
        /// keyname
        /// </summary>
        public string[] names { get; set; }

        /// <summary>
        /// 是否有值
        /// </summary>
        public string havingValue { get; set; }

        /// <summary>
        /// 前缀
        /// </summary>
        public string prefix { get; set; }

        /// <summary>
        /// 表示如果没有在appsettings.json设置该属性，则默认为条件符合
        /// </summary>
        public bool matchIfMissing { get; set; }
    }
}