using System;

namespace Autofac.Annotation
{
    /// <summary>
    /// 配合pointCut的前置拦截器
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class Before : Attribute
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
        
        /// <summary>
        /// 唯一名称
        /// </summary>
        public string GroupName { get; set; }
    }

}