using System;

namespace Autofac.Annotation
{
    /// <summary>
    /// 配合pointCut的前置拦截器
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class Before : PointcutBasicAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        public Before()
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        public Before(string groupName)
        {
            this.GroupName = groupName;
        }
        
     
    }

}