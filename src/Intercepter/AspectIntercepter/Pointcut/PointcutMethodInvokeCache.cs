using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Extensions.Reflection;
using Autofac.Annotation;
using Autofac.AspectIntercepter.Advice;

namespace Autofac.AspectIntercepter.Pointcut
{
    /// <summary>
    ///     一个method 可能对应多个切面 需要重新排序
    /// </summary>
    [Component(AutofacScope = AutofacScope.SingleInstance, AutoActivate = true, NotUseProxy = true)]
    public class PointcutMethodInvokeCache
    {
        /// <summary>
        ///     初始化
        /// </summary>
        public PointcutMethodInvokeCache(IComponentContext context)
        {
            var pointCutConfigurationList = context.Resolve<PointCutConfigurationList>();
            var _cache = context.Resolve<ApsectAdviceMethodInvokeCache>();

            CacheList = new ConcurrentDictionary<MethodInfo, PointcutMethodChainBuilder>();

            DynamicCacheList = new ConcurrentDictionary<string, PointcutMethodChainBuilder>();

            //每个方法维度的 每个方法对应的是一组pointcut 每个pointcut 就是一个切面 一个切面里面会有多组 拦截器方法
            foreach (var methodCache in pointCutConfigurationList.PointcutTargetInfoList)
            {
                var pointCutMethodChain = _cache != null && _cache.CacheList.TryGetValue(methodCache.Key, out var attribute)
                        ? new PointcutMethodChainBuilder
                            (attribute.AspectFunc)
                            {
                                PointcutMethodChainList = new List<PointcutMethod>()
                            }
                        : new PointcutMethodChainBuilder
                        {
                            PointcutMethodChainList = new List<PointcutMethod>()
                        }
                    ;

                //一个方法会有多个pointcut
                foreach (var pointcutRunTime in methodCache.Value)
                {
                    var pointcut = pointcutRunTime.PointcutConfigurationInfo;
                    var pointCutMethod = new PointcutMethod
                    {
                        OrderIndex = pointcut.Pointcut.OrderIndex
                    };

                    pointCutMethodChain.PointcutMethodChainList.Add(pointCutMethod);

                    //每个切换先拿到对应的实例 重复拿也没关系 因为是单例的
                    var instance = context.Resolve(pointcut.PointClass);

                    if (pointcut.BeforeMethod != null)
                        pointCutMethod.BeforeMethod = new RunTimePointcutMethod<Before>
                        {
                            Instance = instance,
                            MethodInfo = pointcut.BeforeMethod.Item2.GetReflector(),
                            MethodReturnType = pointcut.BeforeMethod.Item2.ReturnType,
                            MethodParameters = pointcut.BeforeMethod.Item2.GetParameters(),
                            PointcutBasicAttribute = pointcut.BeforeMethod.Item1,
                            PointcutInjectAnotation = pointcutRunTime.MethodInjectPointcutAttribute
                        };

                    if (pointcut.AfterMethod != null)
                        pointCutMethod.AfterMethod = new RunTimePointcutMethod<After>
                        {
                            Instance = instance,
                            MethodInfo = pointcut.AfterMethod.Item2.GetReflector(),
                            MethodReturnType = pointcut.AfterMethod.Item2.ReturnType,
                            MethodParameters = pointcut.AfterMethod.Item2.GetParameters(),
                            PointcutBasicAttribute = pointcut.AfterMethod.Item1,
                            PointcutInjectAnotation = pointcutRunTime.MethodInjectPointcutAttribute
                        };

                    if (pointcut.AfterReturnMethod != null)
                        pointCutMethod.AfterReturnMethod = new RunTimePointcutMethod<AfterReturn>
                        {
                            Instance = instance,
                            MethodInfo = pointcut.AfterReturnMethod.Item2.GetReflector(),
                            MethodReturnType = pointcut.AfterReturnMethod.Item2.ReturnType,
                            MethodParameters = pointcut.AfterReturnMethod.Item2.GetParameters(),
                            PointcutBasicAttribute = pointcut.AfterReturnMethod.Item1,
                            PointcutInjectAnotation = pointcutRunTime.MethodInjectPointcutAttribute
                        };

                    if (pointcut.AroundMethod != null)
                        pointCutMethod.AroundMethod = new RunTimePointcutMethod<Around>
                        {
                            Instance = instance,
                            MethodInfo = pointcut.AroundMethod.Item2.GetReflector(),
                            MethodReturnType = pointcut.AroundMethod.Item2.ReturnType,
                            MethodParameters = pointcut.AroundMethod.Item2.GetParameters(),
                            PointcutBasicAttribute = pointcut.AroundMethod.Item1,
                            PointcutInjectAnotation = pointcutRunTime.MethodInjectPointcutAttribute
                        };

                    if (pointcut.AfterThrows != null)
                        pointCutMethod.AfterThrowsMethod = new RunTimePointcutMethod<AfterThrows>
                        {
                            Instance = instance,
                            MethodInfo = pointcut.AfterThrows.Item2.GetReflector(),
                            MethodReturnType = pointcut.AfterThrows.Item2.ReturnType,
                            MethodParameters = pointcut.AfterThrows.Item2.GetParameters(),
                            PointcutBasicAttribute = pointcut.AfterThrows.Item1,
                            PointcutInjectAnotation = pointcutRunTime.MethodInjectPointcutAttribute
                        };
                }

                pointCutMethodChain.PointcutMethodChainList = pointCutMethodChain.PointcutMethodChainList.OrderBy(r => r.OrderIndex).ToList();
                CacheList.TryAdd(methodCache.Key, pointCutMethodChain);
            }


            foreach (var methodCache in pointCutConfigurationList.DynamicPointcutTargetInfoList)
            {
                var pointCutMethodChain = _cache != null && _cache.DynamicCacheList.TryGetValue(methodCache.Key, out var attribute)
                    ? new PointcutMethodChainBuilder
                        (attribute.AspectFunc)
                        {
                            PointcutMethodChainList = new List<PointcutMethod>()
                        }
                    : new PointcutMethodChainBuilder
                    {
                        PointcutMethodChainList = new List<PointcutMethod>()
                    };

                //一个方法会有多个pointcut
                foreach (var pointcutRunTime in methodCache.Value)
                {
                    var pointcut = pointcutRunTime.PointcutConfigurationInfo;
                    var pointCutMethod = new PointcutMethod
                    {
                        OrderIndex = pointcut.Pointcut.OrderIndex
                    };

                    pointCutMethodChain.PointcutMethodChainList.Add(pointCutMethod);

                    //每个切换先拿到对应的实例 重复拿也没关系 因为是单例的
                    var instance = context.Resolve(pointcut.PointClass);

                    if (pointcut.BeforeMethod != null)
                        pointCutMethod.BeforeMethod = new RunTimePointcutMethod<Before>
                        {
                            Instance = instance,
                            MethodInfo = pointcut.BeforeMethod.Item2.GetReflector(),
                            MethodReturnType = pointcut.BeforeMethod.Item2.ReturnType,
                            MethodParameters = pointcut.BeforeMethod.Item2.GetParameters(),
                            PointcutBasicAttribute = pointcut.BeforeMethod.Item1,
                            PointcutInjectAnotation = pointcutRunTime.MethodInjectPointcutAttribute
                        };

                    if (pointcut.AfterMethod != null)
                        pointCutMethod.AfterMethod = new RunTimePointcutMethod<After>
                        {
                            Instance = instance,
                            MethodInfo = pointcut.AfterMethod.Item2.GetReflector(),
                            MethodReturnType = pointcut.AfterMethod.Item2.ReturnType,
                            MethodParameters = pointcut.AfterMethod.Item2.GetParameters(),
                            PointcutBasicAttribute = pointcut.AfterMethod.Item1,
                            PointcutInjectAnotation = pointcutRunTime.MethodInjectPointcutAttribute
                        };

                    if (pointcut.AfterReturnMethod != null)
                        pointCutMethod.AfterReturnMethod = new RunTimePointcutMethod<AfterReturn>
                        {
                            Instance = instance,
                            MethodInfo = pointcut.AfterReturnMethod.Item2.GetReflector(),
                            MethodReturnType = pointcut.AfterReturnMethod.Item2.ReturnType,
                            MethodParameters = pointcut.AfterReturnMethod.Item2.GetParameters(),
                            PointcutBasicAttribute = pointcut.AfterReturnMethod.Item1,
                            PointcutInjectAnotation = pointcutRunTime.MethodInjectPointcutAttribute
                        };

                    if (pointcut.AroundMethod != null)
                        pointCutMethod.AroundMethod = new RunTimePointcutMethod<Around>
                        {
                            Instance = instance,
                            MethodInfo = pointcut.AroundMethod.Item2.GetReflector(),
                            MethodReturnType = pointcut.AroundMethod.Item2.ReturnType,
                            MethodParameters = pointcut.AroundMethod.Item2.GetParameters(),
                            PointcutBasicAttribute = pointcut.AroundMethod.Item1,
                            PointcutInjectAnotation = pointcutRunTime.MethodInjectPointcutAttribute
                        };

                    if (pointcut.AfterThrows != null)
                        pointCutMethod.AfterThrowsMethod = new RunTimePointcutMethod<AfterThrows>
                        {
                            Instance = instance,
                            MethodInfo = pointcut.AfterThrows.Item2.GetReflector(),
                            MethodReturnType = pointcut.AfterThrows.Item2.ReturnType,
                            MethodParameters = pointcut.AfterThrows.Item2.GetParameters(),
                            PointcutBasicAttribute = pointcut.AfterThrows.Item1,
                            PointcutInjectAnotation = pointcutRunTime.MethodInjectPointcutAttribute
                        };
                }

                pointCutMethodChain.PointcutMethodChainList = pointCutMethodChain.PointcutMethodChainList.OrderBy(r => r.OrderIndex).ToList();
                DynamicCacheList.TryAdd(methodCache.Key, pointCutMethodChain);
            }
        }


        /// <summary>
        ///     缓存
        /// </summary>
        internal ConcurrentDictionary<MethodInfo, PointcutMethodChainBuilder> CacheList { get; set; }

        /// <summary>
        ///     由于动态泛型的method是跟着泛型T变化的  所以需要单独缓存
        /// </summary>
        internal ConcurrentDictionary<string, PointcutMethodChainBuilder> DynamicCacheList { get; set; }
    }
}