using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Annotation.Util;

namespace Autofac.Annotation
{
    /// <summary>
    /// DependsOn标签 配合Bean和Component标签使用
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class DependsOn : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public DependsOn(params Type[] types) : this()
        {
            this.dependsOn = types;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public DependsOn(params string[] types) : this()
        {
            this.dependsTypes = types;
        }

        /// <summary>
        /// ctor
        /// </summary>
        public DependsOn()
        {
            DependsOnLazy = new Lazy<Type[]>(getDependsOn);
        }

        /// <summary>
        /// 依赖的 是用来表示一个bean A的实例化依赖另一个bean B的实例化， 但是A并不需要持有一个B的对象
        /// </summary>
        public Type[] dependsOn { get; set; }

        /// <summary>
        /// 依赖的 是用来表示一个bean A的实例化依赖另一个bean B的实例化
        /// 类的完全路径数组
        /// </summary>
        public String[] dependsTypes { get; set; }

        /// <summary>
        ///     依赖的 是用来表示一个bean A的实例化依赖另一个bean B的实例化， 但是A并不需要持有一个B的对象
        /// </summary>
        internal readonly Lazy<Type[]> DependsOnLazy;

        /// <summary>
        /// 解析配置的depends
        /// </summary>
        /// <returns></returns>
        private Type[] getDependsOn()
        {
            var list = new List<Type>();
            if (dependsOn != null && dependsOn.Any())
            {
                list.AddRange(dependsOn);
            }

            if (dependsTypes != null && dependsTypes.Any())
            {
                list.AddRange(dependsTypes.Select(type => type.FindClassIgnoreErr()).Where(temp => temp != null));
            }

            return list.Distinct().ToArray();
        }
    }
}