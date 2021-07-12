using System;

namespace Autofac.Annotation.Condition
{
    /// <summary>
    /// attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class Conditional: System.Attribute
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
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
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
        public ConditionOnMissingBean(Type type):this()
        {
            this.type = type;
        }
        
        
        /// <summary>
        /// ctor
        /// </summary>
        public ConditionOnMissingBean(Type type,string name):this(type)
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
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
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
        public ConditionOnBean(Type type):this()
        {
            this.type = type;
        }
        
        
        /// <summary>
        /// ctor
        /// </summary>
        public ConditionOnBean(Type type,string name):this(type)
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
    
}