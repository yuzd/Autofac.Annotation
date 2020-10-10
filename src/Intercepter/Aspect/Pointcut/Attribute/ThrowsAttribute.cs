using System;

namespace Autofac.Annotation
{
    /// <summary>
    /// 配合pointCut的错误拦截器
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class Throws : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public Throws()
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        public Throws(string groupName)
        {
            this.GroupName = groupName;
        }

        /// <summary>
        /// 
        /// </summary>
        public Throws(Type exceptionType)
        {
            this.ExceptionType = exceptionType;
        }
        
        /// <summary>
        /// 
        /// </summary>
        public Throws(string groupName,Type exceptionType)
        {
            this.GroupName = groupName;
            this.ExceptionType = exceptionType;
        }

        /// <summary>
        /// 唯一名称
        /// </summary>
        public string GroupName { get; set; }
        
        /// <summary>
        /// 指定拦截的错误类型
        /// </summary>

        public Type ExceptionType { get; set; }
        
        /// <summary>
        /// 返回异常参数
        /// </summary>
        public string Throwing { get; set; }
    }

}