using System;

namespace Autofac.Annotation
{
    /// <summary>
    /// 配合pointCut的后置拦截器
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class After : PointcutBasicAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        public After()
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        public After(string groupName)
        {
            this.GroupName = groupName;
        }
        
        /// <summary>
        /// 如果目标方法成功返回 那么就是返回的值 如果目标方法异常 那么就是异常本身
        /// </summary>
        public string Returing { get; set; }

    }

}