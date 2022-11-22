using System;

namespace Autofac.Annotation
{
    /// <summary>
    ///  类似Spring的AliasFor注解
    /// 是为了让自定义的Attribute也具有别的注解的定义的对应属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public sealed class AliasFor : Attribute
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public AliasFor(Type forType, string field)
        {
            ForType = forType;
            ForField = field;
        }

        /// <summary>
        /// 对应的类型
        /// </summary>
        public Type ForType { get; set; }

        /// <summary>
        /// 属性
        /// </summary>

        public string ForField { get; set; }
    }
}