using System;

namespace Autofac.Annotation
{
    /// <summary>
    /// 类似Spring的@Order
    /// 1. 值越小越排在前面加载
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class Order : System.Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public Order()
        {
            Index = 0;
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        public Order(int value)
        {
            Index = value;
        }


        /// <summary>
        /// 注册的类型
        /// </summary>
        internal int Index { get; set; }
    }
}