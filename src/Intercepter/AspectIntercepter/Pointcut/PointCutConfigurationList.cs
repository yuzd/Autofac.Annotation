using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Autofac.Annotation;

namespace Autofac.AspectIntercepter.Pointcut
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
        public ConcurrentDictionary<ObjectKey, List<RunTimePointCutConfiguration>> PointcutTargetInfoList { get; set; }

        /// <summary>
        /// 针对动态泛型类的method目标集合
        /// </summary>
        public ConcurrentDictionary<string, List<RunTimePointCutConfiguration>> DynamicPointcutTargetInfoList { get; set; }

        /// <summary>
        /// 对应的class目标集合
        /// </summary>
        public ConcurrentDictionary<Type, bool> PointcutTypeInfoList { get; set; }
    }

    internal class RunTimePointCutConfiguration
    {
        public RunTimePointCutConfiguration(PointcutConfigurationInfo configurationInfo, Tuple<Attribute, int> methodInjectPointcutAttribute)
        {
            PointcutConfigurationInfo = configurationInfo;
            MethodInjectPointcutAttribute = methodInjectPointcutAttribute?.Item1;
            OrderIndex = methodInjectPointcutAttribute?.Item2 ?? 0;
        }

        /// <summary>
        /// 方法 对应的 切面集合
        /// </summary>
        public PointcutConfigurationInfo PointcutConfigurationInfo { get; set; }

        /// <summary>
        /// 方法运行时 有识别到 需要注入 指定注解
        /// </summary>
        public Attribute MethodInjectPointcutAttribute { get; set; }

        /// <summary>
        /// 在继承关系下 方法上的注解继承顺序为：
        /// 方法上 -> 父类方法 -> 接口方法 -> class -> 父类class -> 接口上
        /// </summary>
        public int OrderIndex { get; set; }
    }
}