using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
        private readonly ApsectAdviceMethodInvokeCache _cache;
        private readonly IComponentContext _context;

        /// <summary>
        ///     初始化
        /// </summary>
        public PointcutMethodInvokeCache(IComponentContext context)
        {
            _context = context;
            var pointCutConfigurationList = context.Resolve<PointCutConfigurationList>();
            _cache = context.Resolve<ApsectAdviceMethodInvokeCache>();

            CacheList = new ConcurrentDictionary<ObjectKey, PointcutMethodChainBuilder>();

            DynamicCacheList = new ConcurrentDictionary<string, PointcutMethodChainBuilder>();

            //每个方法维度的 每个方法对应的是一组pointcut 每个pointcut 就是一个切面 一个切面里面会有多组 拦截器方法
            foreach (var methodCache in pointCutConfigurationList.PointcutTargetInfoList)
            {
                AddCacheInter(methodCache.Key, methodCache.Value);
            }


            foreach (var methodCache in pointCutConfigurationList.DynamicPointcutTargetInfoList)
            {
                AddDynamicCacheInter(methodCache.Key, methodCache.Value);
            }
        }

        /// <summary>
        /// 针对泛型
        /// </summary>
        /// <param name="componentModel"></param>
        internal void AddCache(ComponentModel componentModel)
        {
            lock (_cache)
            {
                var pointCutConfigurationList = _context.Resolve<PointCutConfigurationList>();
                foreach (var key in componentModel.DynamicGenricMethodsNeedPointcuts.Distinct())
                {
                    if (pointCutConfigurationList.DynamicPointcutTargetInfoList.TryGetValue(key, out var value))
                    {
                        AddDynamicCacheInter(key, value);
                    }
                }

                foreach (var key in componentModel.MethodsNeedPointcuts.Distinct())
                {
                    if (pointCutConfigurationList.PointcutTargetInfoList.TryGetValue(key, out var value))
                    {
                        AddCacheInter(key, value);
                    }
                }

                componentModel.MethodsNeedPointcuts.Clear();
                componentModel.DynamicGenricMethodsNeedPointcuts.Clear();
            }
        }

        /// <summary>
        /// 针对泛型
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private void AddDynamicCacheInter(string key, List<RunTimePointCutConfiguration> value)
        {
            var pointCutMethodChain = _cache != null && _cache.DynamicCacheList.TryGetValue(key, out var attribute)
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
            foreach (var pointcutRunTime in value)
            {
                var pointcut = pointcutRunTime.PointcutConfigurationInfo;
                var pointCutMethod = new PointcutMethod
                {
                    OrderIndex = pointcut.Pointcut.OrderIndex
                };

                pointCutMethodChain.PointcutMethodChainList.Add(pointCutMethod);

                //每个切换先拿到对应的实例 重复拿也没关系 因为是单例的
                var instance = _context.Resolve(pointcut.PointClass);

                if (pointcut.BeforeMethod != null)
                    pointCutMethod.BeforeMethod = new RunTimePointcutMethod<Before>
                    {
                        Instance = instance,
                        MethodInfo = pointcut.BeforeMethod.Item2,
                        MethodReturnType = pointcut.BeforeMethod.Item2.ReturnType,
                        MethodParameters = pointcut.BeforeMethod.Item2.GetParameters(),
                        PointcutBasicAttribute = pointcut.BeforeMethod.Item1,
                        PointcutInjectAnotation = pointcutRunTime.MethodInjectPointcutAttribute
                    };

                if (pointcut.AfterMethod != null)
                    pointCutMethod.AfterMethod = new RunTimePointcutMethod<After>
                    {
                        Instance = instance,
                        MethodInfo = pointcut.AfterMethod.Item2,
                        MethodReturnType = pointcut.AfterMethod.Item2.ReturnType,
                        MethodParameters = pointcut.AfterMethod.Item2.GetParameters(),
                        PointcutBasicAttribute = pointcut.AfterMethod.Item1,
                        PointcutInjectAnotation = pointcutRunTime.MethodInjectPointcutAttribute
                    };

                if (pointcut.AfterReturnMethod != null)
                    pointCutMethod.AfterReturnMethod = new RunTimePointcutMethod<AfterReturn>
                    {
                        Instance = instance,
                        MethodInfo = pointcut.AfterReturnMethod.Item2,
                        MethodReturnType = pointcut.AfterReturnMethod.Item2.ReturnType,
                        MethodParameters = pointcut.AfterReturnMethod.Item2.GetParameters(),
                        PointcutBasicAttribute = pointcut.AfterReturnMethod.Item1,
                        PointcutInjectAnotation = pointcutRunTime.MethodInjectPointcutAttribute
                    };

                if (pointcut.AroundMethod != null)
                    pointCutMethod.AroundMethod = new RunTimePointcutMethod<Around>
                    {
                        Instance = instance,
                        MethodInfo = pointcut.AroundMethod.Item2,
                        MethodReturnType = pointcut.AroundMethod.Item2.ReturnType,
                        MethodParameters = pointcut.AroundMethod.Item2.GetParameters(),
                        PointcutBasicAttribute = pointcut.AroundMethod.Item1,
                        PointcutInjectAnotation = pointcutRunTime.MethodInjectPointcutAttribute
                    };

                if (pointcut.AfterThrows != null)
                    pointCutMethod.AfterThrowsMethod = new RunTimePointcutMethod<AfterThrows>
                    {
                        Instance = instance,
                        MethodInfo = pointcut.AfterThrows.Item2,
                        MethodReturnType = pointcut.AfterThrows.Item2.ReturnType,
                        MethodParameters = pointcut.AfterThrows.Item2.GetParameters(),
                        PointcutBasicAttribute = pointcut.AfterThrows.Item1,
                        PointcutInjectAnotation = pointcutRunTime.MethodInjectPointcutAttribute
                    };
            }

            pointCutMethodChain.PointcutMethodChainList = pointCutMethodChain.PointcutMethodChainList.OrderBy(r => r.OrderIndex).ToList();
            DynamicCacheList.TryAdd(key, pointCutMethodChain);
        }

        /// <summary>
        /// 针对非泛型
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private void AddCacheInter(ObjectKey key, List<RunTimePointCutConfiguration> value)
        {
            var pointCutMethodChain = _cache != null && _cache.CacheList.TryGetValue(key.Method, out var attribute)
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
            foreach (var pointcutRunTime in value)
            {
                var pointcut = pointcutRunTime.PointcutConfigurationInfo;
                var pointCutMethod = new PointcutMethod
                {
                    OrderIndex = pointcut.Pointcut.OrderIndex
                };

                pointCutMethodChain.PointcutMethodChainList.Add(pointCutMethod);

                //每个切换先拿到对应的实例 重复拿也没关系 因为是单例的
                var instance = _context.Resolve(pointcut.PointClass);

                if (pointcut.BeforeMethod != null)
                    pointCutMethod.BeforeMethod = new RunTimePointcutMethod<Before>
                    {
                        Instance = instance,
                        MethodInfo = pointcut.BeforeMethod.Item2,
                        MethodReturnType = pointcut.BeforeMethod.Item2.ReturnType,
                        MethodParameters = pointcut.BeforeMethod.Item2.GetParameters(),
                        PointcutBasicAttribute = pointcut.BeforeMethod.Item1,
                        PointcutInjectAnotation = pointcutRunTime.MethodInjectPointcutAttribute
                    };

                if (pointcut.AfterMethod != null)
                    pointCutMethod.AfterMethod = new RunTimePointcutMethod<After>
                    {
                        Instance = instance,
                        MethodInfo = pointcut.AfterMethod.Item2,
                        MethodReturnType = pointcut.AfterMethod.Item2.ReturnType,
                        MethodParameters = pointcut.AfterMethod.Item2.GetParameters(),
                        PointcutBasicAttribute = pointcut.AfterMethod.Item1,
                        PointcutInjectAnotation = pointcutRunTime.MethodInjectPointcutAttribute
                    };

                if (pointcut.AfterReturnMethod != null)
                    pointCutMethod.AfterReturnMethod = new RunTimePointcutMethod<AfterReturn>
                    {
                        Instance = instance,
                        MethodInfo = pointcut.AfterReturnMethod.Item2,
                        MethodReturnType = pointcut.AfterReturnMethod.Item2.ReturnType,
                        MethodParameters = pointcut.AfterReturnMethod.Item2.GetParameters(),
                        PointcutBasicAttribute = pointcut.AfterReturnMethod.Item1,
                        PointcutInjectAnotation = pointcutRunTime.MethodInjectPointcutAttribute
                    };

                if (pointcut.AroundMethod != null)
                    pointCutMethod.AroundMethod = new RunTimePointcutMethod<Around>
                    {
                        Instance = instance,
                        MethodInfo = pointcut.AroundMethod.Item2,
                        MethodReturnType = pointcut.AroundMethod.Item2.ReturnType,
                        MethodParameters = pointcut.AroundMethod.Item2.GetParameters(),
                        PointcutBasicAttribute = pointcut.AroundMethod.Item1,
                        PointcutInjectAnotation = pointcutRunTime.MethodInjectPointcutAttribute
                    };

                if (pointcut.AfterThrows != null)
                    pointCutMethod.AfterThrowsMethod = new RunTimePointcutMethod<AfterThrows>
                    {
                        Instance = instance,
                        MethodInfo = pointcut.AfterThrows.Item2,
                        MethodReturnType = pointcut.AfterThrows.Item2.ReturnType,
                        MethodParameters = pointcut.AfterThrows.Item2.GetParameters(),
                        PointcutBasicAttribute = pointcut.AfterThrows.Item1,
                        PointcutInjectAnotation = pointcutRunTime.MethodInjectPointcutAttribute
                    };
            }

            pointCutMethodChain.PointcutMethodChainList = pointCutMethodChain.PointcutMethodChainList.OrderBy(r => r.OrderIndex).ToList();
            CacheList.TryAdd(key, pointCutMethodChain);
        }

        /// <summary>
        ///     缓存
        /// </summary>
        internal ConcurrentDictionary<ObjectKey, PointcutMethodChainBuilder> CacheList { get; set; }

        /// <summary>
        ///     由于动态泛型的method是跟着泛型T变化的  所以需要单独缓存
        /// </summary>
        internal ConcurrentDictionary<string, PointcutMethodChainBuilder> DynamicCacheList { get; set; }
    }
}