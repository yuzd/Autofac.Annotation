using System;
using System.Collections.Generic;
using System.Reflection;

namespace Autofac.Aspect.Pointcut
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
        public PointcutAttribute Pointcut { get; set; }
        
        /// <summary>
        /// 分组name
        /// </summary>
        public string GroupName { get; set; }
        
        /// <summary>
        /// 切面配置对应的前置方法
        /// </summary>
        public MethodInfo BeforeMethod { get; set; }
        
        /// <summary>
        /// 切面配置对应的后置方法
        /// </summary>
        public Tuple<AfterAttribute,MethodInfo> AfterMethod { get; set; }
        
        /// <summary>
        /// 切面配置对应的环绕方法
        /// </summary>
        public MethodInfo AroundMethod { get; set; }
        
        /// <summary>
        /// 切面配置对应的错误拦截方法
        /// </summary>
        public Tuple<ThrowingAttribute,MethodInfo> ThrowingMethod { get; set; }

    }
}