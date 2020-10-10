using System;

namespace Autofac.Annotation
{

    /// <summary>
    /// 配合pointCut的环绕拦截器
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class Around : System.Attribute
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
        /// <summary>
        /// 唯一名称
        /// </summary>
        public string GroupName { get; set; }
    }

}