using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac.Aspect;

namespace Autofac.Annotation
{
    /// <summary>
    /// 和Spring的Configuration功能类似
    /// Bean标签在这个类里面才有作用
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class AutoConfiguration : System.Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public AutoConfiguration()
        {
            this.Key = "";
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="key"></param>
        public AutoConfiguration(string key)
        {
            this.Key = key;
        }

        /// <summary>
        /// 注册单个的key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 值越大越优先处理
        /// </summary>
        public int OrderIndex { get; set; }

    }

    internal class AutofacConfigurationInfo
    {
        public AutoConfiguration AutofacConfiguration { get; set; }
        public Type Type { get; set; }
        public string Key { get; set; }
        public int OrderIndex { get; set; }
    }

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
        /// 切面配置对应的前置方法
        /// </summary>
        public MethodInfo BeforeMethod { get; set; }
        
        /// <summary>
        /// 切面配置对应的后置方法
        /// </summary>
        public MethodInfo AfterMethod { get; set; }
        
        /// <summary>
        /// 切面配置对应的环绕方法
        /// </summary>
        public MethodInfo AroundMethod { get; set; }

    }

}
