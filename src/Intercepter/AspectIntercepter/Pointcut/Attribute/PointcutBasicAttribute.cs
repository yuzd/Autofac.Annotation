using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Annotation
{
    /// <summary>
    /// 基础类
    /// </summary>
    public class PointcutBasicAttribute : Attribute
    {
        /// <summary>
        /// 唯一名称
        /// </summary>
        public string GroupName { get; set; }
        
    }
}