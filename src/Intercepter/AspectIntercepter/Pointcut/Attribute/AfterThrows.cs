using System;

namespace Autofac.Annotation
{
    /// <summary>
    /// 配合pointCut的错误拦截器
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class AfterThrows : PointcutBasicAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        public AfterThrows()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public AfterThrows(string groupName)
        {
            this.GroupName = groupName;
        }

        /// <summary>
        /// 
        /// </summary>
        public AfterThrows(Type exceptionType)
        {
            this.ExceptionType = exceptionType;
        }

        /// <summary>
        /// 
        /// </summary>
        public AfterThrows(string groupName, Type exceptionType)
        {
            this.GroupName = groupName;
            this.ExceptionType = exceptionType;
        }

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