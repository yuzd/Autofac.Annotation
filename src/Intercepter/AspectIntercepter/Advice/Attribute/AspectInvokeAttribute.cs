using System;

namespace Autofac.AspectIntercepter.Advice
{
    /// <summary>
    ///     AOP拦截器 默认包含继承关系
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Interface)]
    public class AspectInvokeAttribute : Attribute
    {
        /// <summary>
        ///     排序 值越低，优先级越高
        /// </summary>
        public int OrderIndex { get; set; }

        /// <summary>
        ///     分组名称
        /// </summary>
        public string GroupName { get; set; }
    }
}