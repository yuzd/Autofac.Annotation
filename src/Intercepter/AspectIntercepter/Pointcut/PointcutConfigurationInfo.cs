using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac.Annotation;

namespace Autofac.AspectIntercepter.Pointcut
{
    /// <summary>
    /// 切面配置类
    /// </summary>
    public class PointcutConfigurationInfo
    {
        /// <summary>
        /// 切面配置class
        /// </summary>
        public Type PointClass { get; set; }

        /// <summary>
        /// 切面配置信息
        /// </summary>
        public Annotation.Pointcut Pointcut { get; set; }

        /// <summary>
        /// 分组name
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 切面配置对应的前置方法
        /// </summary>
        public Tuple<Before, MethodInfo> BeforeMethod { get; set; }

        /// <summary>
        /// 切面配置对应的后置方法 (正常)
        /// </summary>
        public Tuple<AfterReturn, MethodInfo> AfterReturnMethod { get; set; }

        /// <summary>
        ///  切面配置对应的后置方法(不管正常还是异常)
        /// </summary>
        public Tuple<After, MethodInfo> AfterMethod { get; set; }

        /// <summary>
        /// 切面配置对应的环绕方法
        /// </summary>
        public Tuple<Around, MethodInfo> AroundMethod { get; set; }

        /// <summary>
        /// 切面配置对应的错误拦截方法 (异常)
        /// </summary>
        public Tuple<AfterThrows, MethodInfo> AfterThrows { get; set; }
    }
}