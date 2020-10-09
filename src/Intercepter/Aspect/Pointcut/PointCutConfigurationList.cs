using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Autofac.Annotation;

namespace Autofac.Aspect.Pointcut
{
    /// <summary>
    /// PointCut装配集合数据源
    /// </summary>
    public class PointCutConfigurationList
    {

        /// <summary>
        /// PointCut装配集合数据源
        /// </summary>
        public List<PointcutConfigurationInfo> PointcutConfigurationInfoList { get; set; }
        
        /// <summary>
        /// 对应的method目标集合
        /// </summary>
        public ConcurrentDictionary<MethodInfo,List<PointcutConfigurationInfo>> PointcutTargetInfoList { get; set; }
        
        /// <summary>
        /// 针对动态泛型类的method目标集合
        /// </summary>
        public ConcurrentDictionary<string,List<PointcutConfigurationInfo>> DynamicPointcutTargetInfoList { get; set; }
        
        /// <summary>
        /// 对应的class目标集合
        /// </summary>
        public ConcurrentDictionary<Type,bool> PointcutTypeInfoList { get; set; }

    }
}