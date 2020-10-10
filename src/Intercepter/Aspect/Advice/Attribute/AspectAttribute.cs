using System;

namespace Autofac.Annotation
{
    /// <summary>
    /// 给某一个class 开启AOP拦截器
    /// 配合 AspectAroundAttribute  AspectBeforeAttribute AspectAfterAttribute 使用
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class Aspect : Attribute
    {
        /// <summary>
        /// ctor
        /// </summary>
        public Aspect()
        {
            
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="type"></param>
        public Aspect(InterceptorType type)
        {
            AspectType = type;
        }

        /// <summary>
        /// 拦截器类型 默认是当前class的拦截器
        /// </summary>
        public InterceptorType AspectType { get; set; } = InterceptorType.Class;
    }

}