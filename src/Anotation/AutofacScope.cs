using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autofac.Annotation
{
    /// <summary>
    /// 实例的生命周期
    /// </summary>
    public enum AutofacScope
    {
        /// <summary>
        /// 每次请求获取
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
        /// 这个还没用到过
        /// </summary>
        InstancePerMatchingLifetimeScope,
        /// <summary>
        /// 根据每个请求一个实例
        /// </summary>
        InstancePerRequest,
        /// <summary>
        /// 根据别的实例的生命周期
        /// </summary>
        InstancePerOwned
    }
}
