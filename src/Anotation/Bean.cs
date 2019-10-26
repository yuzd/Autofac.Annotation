using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autofac.Annotation
{
    /// <summary>
    /// Bean标签
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class Bean : System.Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public Bean()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="key"></param>
        public Bean(string key)
        {
            this.Key = key;
        }

        /// <summary>
        /// 注册单个的key
        /// </summary>
        public string Key { get; private set; }



    }
}
