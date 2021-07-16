namespace Autofac.Annotation
{
    /// <summary>
    /// 实例的生命周期
    /// </summary>
    public enum AutofacScope
    {
        /// <summary>
        /// 瞬时
        /// </summary>
        InstancePerDependency,

        /// <summary>
        /// 单例
        /// </summary>
        SingleInstance,

        /// <summary>
        /// 每个scope获取新的实例
        /// </summary>
        InstancePerLifetimeScope,

        /// <summary>
        /// 根据每个请求一个实例
        /// </summary>
        InstancePerRequest,

        /// <summary>
        /// 默认是瞬时
        /// </summary>
        Default
    }
}