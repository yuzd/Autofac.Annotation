using System;

namespace Autofac.Annotation
{
    /// <summary>
    /// DependsOn标签 配合Bean和Component标签使用
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class DependsOn : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public DependsOn(params Type[] types)
        {
            this.dependsOn = types;
        }

        

        /// <summary>
        /// 依赖的 是用来表示一个bean A的实例化依赖另一个bean B的实例化， 但是A并不需要持有一个B的对象
        /// </summary>
        public Type[] dependsOn { get; set; }
    }
}