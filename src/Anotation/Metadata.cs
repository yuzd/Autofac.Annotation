using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autofac.Annotation.Anotation
{
    /// <summary>
    /// 打在class上面
    /// 可以存在多个 如果有key重复会覆盖
    /// An array of metadata values to associate with the component. Each item specifies the name, type, and value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class Metadata : System.Attribute
    {
        /// <summary>
        /// 字段名称
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 对应的值
        /// </summary>
        public string Value { get; set; }
    }
}
