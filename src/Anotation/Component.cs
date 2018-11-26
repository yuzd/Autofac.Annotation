using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac.Annotation.Anotation;

namespace Autofac.Configuration.Anotation
{
    /// <summary>
    /// 会扫描有该注解的类 自动装配到autofac容器内
    /// 只能打在class上面 打在abstract不支持会被忽略
    /// 允许打多个 如果打多个有重复的话会覆盖
    /// 打在父类上子类没打的话子类就获取不到
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class Component : System.Attribute
    {
        #region Services
        /// <summary>
        /// 注册的类型
        /// 如果为null则注册为本身类型
        /// </summary>
        public Type[] Services { get; set; }

        /// <summary>
        /// 注册key 在同一个类型注册多个的时候就需要用到key来做区分
        /// </summary>
        public string[] Key { get; set; }


        #endregion

        /// <summary>
        /// A Boolean indicating if the component should auto-activate.
        /// </summary>
        public bool AutoActivate { get; set; }

        /// <summary>
        /// A Boolean indicating whether property (setter) injection for the component should be enabled.
        /// </summary>
        public bool InjectProperties { get; set; }

        /// <summary>
        /// 作用域
        /// </summary>
        public AutofacScope AutofacScope { get; set; } = AutofacScope.InstancePerLifetimeScope;

        /// <summary>
        /// 如果设置值为external代表需要自己管理dispose
        /// </summary>
        public string Ownership { get; set; }

    }
}
