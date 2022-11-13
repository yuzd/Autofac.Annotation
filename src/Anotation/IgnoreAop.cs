//-----------------------------------------------------------------------
// <copyright file="IgnoreAdvice .cs" company="Company">
// Copyright (C) Company. All Rights Reserved.
// </copyright>
// <author>nainaigu</author>
// <create>$Date$</create>
// <summary></summary>
//-----------------------------------------------------------------------

namespace Autofac.Annotation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    ///  打了此注解代表方法不需要增强，适用于以下场景
    ///  1. 接口上打了Advice类或者PointCut类，但是子类实现的方法并不想要被增强
    ///  2. 可以指定不需要增强Advice类别还是PointCut类别
    ///  3. 可以指定过滤具体增强的类
    /// </summary>
    [AttributeUsage(AttributeTargets.Method,Inherited = false,AllowMultiple = false)]
    public sealed class IgnoreAop : Attribute
    {
        /// <summary>
        /// Ctor 忽悠不管是Advice类别还是Pointcut类别实现的aop
        /// </summary>
        public IgnoreAop()
        {
            IgnoreFlags = IgnoreFlags.ALL;
        }
        
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="ignoreFlags">过滤Advice类别</param>
        public IgnoreAop(IgnoreFlags ignoreFlags)
        {
            IgnoreFlags = ignoreFlags;
        }
        /// <summary>
        ///  过滤类别
        /// </summary>
        internal IgnoreFlags IgnoreFlags { get; set; }

        /// <summary>
        /// 过滤具体的类别 可以不指定
        /// </summary>
        public Type[] Target { get; set; }
    }

    /// <summary>
    ///  过滤Advice类别
    /// </summary>
    [Flags]
    public enum IgnoreFlags
    {
        /// <summary>
        /// 过滤Advice打标签实现的AOP
        /// </summary>
        Advice = 1,

        /// <summary>
        /// 过滤Pointcut实现的AOP
        /// </summary>
        PointCut = 2,

        /// <summary>
        ///  过滤所有的AOP
        /// </summary>
        ALL = Advice | PointCut
    }
}