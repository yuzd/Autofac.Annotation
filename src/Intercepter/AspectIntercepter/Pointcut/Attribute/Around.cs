using System;

namespace Autofac.Annotation
{
    /// <summary>
    /// 配合pointCut的环绕拦截器
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class Around : PointcutBasicAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        public Around()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public Around(string groupName)
        {
            this.GroupName = groupName;
        }
    }
}