using System;

namespace Autofac.Annotation
{
    /// <summary>
    /// Bean标签
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class Bean : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public Bean()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="key"></param>
        public Bean(string key)
        {
            this.Key = key;
        }

        /// <summary>
        /// 注册单个的key
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// 作用域
        /// </summary>
        public AutofacScope AutofacScope { get; set; } = AutofacScope.SingleInstance;

        /// <summary>
        /// 被创建后执行的方法
        /// </summary>
        public string InitMethod { get; set; }

        /// <summary>
        /// 被Release时执行的方法
        /// </summary>
        public string DestroyMethod { get; set; }
    }
}