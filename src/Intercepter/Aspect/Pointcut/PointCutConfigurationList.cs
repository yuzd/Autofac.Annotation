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
    internal class PointCutConfigurationList
    {

        /// <summary>
        /// PointCut装配集合数据源
        /// </summary>
        public List<PointcutConfigurationInfo> PointcutConfigurationInfoList { get; set; }
        
        /// <summary>
        /// 对应的method目标集合
        /// </summary>
        public ConcurrentDictionary<MethodInfo,List<RunTimePointCutConfiguration>> PointcutTargetInfoList { get; set; }
        
        /// <summary>
        /// 针对动态泛型类的method目标集合
        /// </summary>
        public ConcurrentDictionary<string,List<RunTimePointCutConfiguration>> DynamicPointcutTargetInfoList { get; set; }
        
        /// <summary>
        /// 对应的class目标集合
        /// </summary>
        public ConcurrentDictionary<Type,bool> PointcutTypeInfoList { get; set; }

    }

    internal class RunTimePointCutConfiguration
    {
        
        public RunTimePointCutConfiguration(PointcutConfigurationInfo configurationInfo,Attribute methodInjectPointcutAttribute)
        {
            PointcutConfigurationInfo = configurationInfo;
            MethodInjectPointcutAttribute = methodInjectPointcutAttribute;
        }
        /// <summary>
        /// 方法 对应的 切面集合
        /// </summary>
        public PointcutConfigurationInfo  PointcutConfigurationInfo{ get; set; }
        
        /// <summary>
        /// 方法运行时 有识别到 需要注入 指定注解
        /// </summary>
        public Attribute MethodInjectPointcutAttribute { get; set; }
    }
}