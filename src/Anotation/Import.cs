using System;

namespace Autofac.Annotation
{
    /// <summary>
    /// 会扫描这个特性的类
    /// 1. 如果采用的是无参数构造方法 会检查必须实现了特定的接口->ImportSelector 才行 否则会报错
    /// 2. 如果采用的有参数方法的话 会先把参数注册到容器 然后在检查是否实现了特定的接口 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class Import: System.Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public Import()
        {

        }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public Import(params Type[] importTypes)
        {
            this.ImportTypes = importTypes;
        }
        
        /// <summary>
        /// 注册的类型
        /// </summary>
        internal Type[] ImportTypes { get; set; }
    }

    /// <summary>
    /// 配合Import特性 导入自定义的BeanDefinition到容器中
    /// </summary>
    public interface ImportSelector
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
         BeanDefination[] SelectImports();
    }
}