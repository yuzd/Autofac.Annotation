using System;

namespace Autofac.Annotation
{
    /// <summary>
    /// 配合pointCut的后置拦截器
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class AfterReturn : PointcutBasicAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        public AfterReturn()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public AfterReturn(string groupName)
        {
            this.GroupName = groupName;
        }

        /// <summary>
        /// 返回的参数
        /// </summary>
        public string Returing { get; set; }
    }
}