using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac.Annotation;

namespace Autofac.Aspect.Pointcut
{
    /// <summary>
    /// 一个method 可能对应多个切面 需要重新排序
    /// </summary>
    [Component(AutofacScope = AutofacScope.SingleInstance, AutoActivate = true,NotUseProxy = true)]
    public class PointcutMethodInvokeCache
    {

        /// <summary>
        /// 初始化
        /// </summary>
        public PointcutMethodInvokeCache(IComponentContext context)
        {
            var pointCutConfigurationList = context.Resolve<PointCutConfigurationList>();

            CacheList = new ConcurrentDictionary<MethodInfo, PointcutMethodChainBuilder>();

            DynamicCacheList = new ConcurrentDictionary<string, PointcutMethodChainBuilder>();

            //每个方法维度的 每个方法对应的是一组pointcut 每个pointcut 就是一个切面 一个切面里面会有多组 拦截器方法
            foreach (var methodCache in pointCutConfigurationList.PointcutTargetInfoList)
            {
                var pointCutMethodChain = new PointcutMethodChainBuilder
                {
                    PointcutMethodChainList = new List<PointcutMethod>()
                };

                //一个方法会有多个pointcut
                foreach (var pointcut in methodCache.Value)
                {
                    var pointCutMethod = new PointcutMethod()
                    {
                        OrderIndex = pointcut.Pointcut.OrderIndex
                    };

                    pointCutMethodChain.PointcutMethodChainList.Add(pointCutMethod);

                    //每个切换先拿到对应的实例 重复拿也没关系 因为是单例的
                    var instance = context.Resolve(pointcut.PointClass);

                    if (pointcut.BeforeMethod != null)
                    {
                        pointCutMethod.BeforeMethod = new Tuple<object, MethodInfo>(instance, pointcut.BeforeMethod);
                    }

                    if (pointcut.AfterMethod != null)
                    {
                        pointCutMethod.AfterMethod =
                            new Tuple<object, AfterAttribute, MethodInfo>(instance, pointcut.AfterMethod.Item1, pointcut.AfterMethod.Item2);
                    }

                    if (pointcut.AroundMethod != null)
                    {
                        pointCutMethod.AroundMethod = new Tuple<object, MethodInfo>(instance, pointcut.AroundMethod);
                    }

                    if (pointcut.ThrowingMethod != null)
                    {
                        pointCutMethod.ThrowingMethod =
                            new Tuple<object, ThrowingAttribute, MethodInfo>(instance, pointcut.ThrowingMethod.Item1, pointcut.ThrowingMethod.Item2);
                    }
                }

                pointCutMethodChain.PointcutMethodChainList = pointCutMethodChain.PointcutMethodChainList.OrderBy(r => r.OrderIndex).ToList();
                CacheList.TryAdd(methodCache.Key, pointCutMethodChain);
            }


            foreach (var methodCache in pointCutConfigurationList.DynamicPointcutTargetInfoList)
            {
                var pointCutMethodChain = new PointcutMethodChainBuilder
                {
                    PointcutMethodChainList = new List<PointcutMethod>()
                };

                //一个方法会有多个pointcut
                foreach (var pointcut in methodCache.Value)
                {
                    var pointCutMethod = new PointcutMethod()
                    {
                        OrderIndex = pointcut.Pointcut.OrderIndex
                    };

                    pointCutMethodChain.PointcutMethodChainList.Add(pointCutMethod);

                    //每个切换先拿到对应的实例 重复拿也没关系 因为是单例的
                    var instance = context.Resolve(pointcut.PointClass);

                    if (pointcut.BeforeMethod != null)
                    {
                        pointCutMethod.BeforeMethod = new Tuple<object, MethodInfo>(instance, pointcut.BeforeMethod);
                    }

                    if (pointcut.AfterMethod != null)
                    {
                        pointCutMethod.AfterMethod =
                            new Tuple<object, AfterAttribute, MethodInfo>(instance, pointcut.AfterMethod.Item1, pointcut.AfterMethod.Item2);
                    }

                    if (pointcut.AroundMethod != null)
                    {
                        pointCutMethod.AroundMethod = new Tuple<object, MethodInfo>(instance, pointcut.AroundMethod);
                    }

                    if (pointcut.ThrowingMethod != null)
                    {
                        pointCutMethod.ThrowingMethod =
                            new Tuple<object, ThrowingAttribute, MethodInfo>(instance, pointcut.ThrowingMethod.Item1, pointcut.ThrowingMethod.Item2);
                    }
                }

                pointCutMethodChain.PointcutMethodChainList = pointCutMethodChain.PointcutMethodChainList.OrderBy(r => r.OrderIndex).ToList();
                DynamicCacheList.TryAdd(methodCache.Key, pointCutMethodChain);
            }
        }


        /// <summary>
        /// 缓存
        /// </summary>
        internal ConcurrentDictionary<MethodInfo, PointcutMethodChainBuilder> CacheList { get; set; }

        /// <summary>
        /// 由于动态泛型的method是跟着泛型T变化的  所以需要单独缓存
        /// </summary>
        internal ConcurrentDictionary<string, PointcutMethodChainBuilder> DynamicCacheList { get; set; }
    }
}