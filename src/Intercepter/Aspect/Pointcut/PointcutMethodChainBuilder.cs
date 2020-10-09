using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac.Annotation;
using Autofac.Aspect.Advice.Impl;
using Castle.DynamicProxy;

namespace Autofac.Aspect.Pointcut
{
    /// <summary>
    /// 
    /// </summary>
    internal class PointcutMethod
    {
        /// <summary>
        /// 排序
        /// </summary>
        public int OrderIndex { get; set; }

        /// <summary>
        /// 切面配置对应的前置方法
        /// </summary>
        public Tuple<object, MethodInfo> BeforeMethod { get; set; }

        /// <summary>
        /// 切面配置对应的后置方法
        /// </summary>
        public Tuple<object, AfterAttribute, MethodInfo> AfterMethod { get; set; }

        /// <summary>
        /// 切面配置对应的环绕方法
        /// </summary>
        public Tuple<object, MethodInfo> AroundMethod { get; set; }

        /// <summary>
        /// 切面配置对应的错误拦截方法
        /// </summary>
        public Tuple<object, ThrowingAttribute, MethodInfo> ThrowingMethod { get; set; }
    }


    internal class PointcutMethodChainBuilder
    {
        public List<PointcutMethod> PointcutMethodChainList { get; set; }

        public AspectDelegate BuilderMethodChain<TResult>(AspectContext aspectContext, Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed)
        {
            AspectMiddlewareBuilder builder = new AspectMiddlewareBuilder();

            var aroundIndex = 0;

            foreach (var chain in PointcutMethodChainList)
            {
                var isLast = aroundIndex == PointcutMethodChainList.Count - 1;

                if (chain.AfterMethod != null)
                {
                    var after = new AspectAfterInterceptor((chain.AfterMethod.Item1, chain.AfterMethod.Item2, chain.AfterMethod.Item3));
                    //After 后加进去先执行
                    builder.Use(next => async ctx => await after.OnInvocation(ctx, next));
                }

                if (chain.AroundMethod != null)
                {
                    //Arround 先加进去先执行 后续执行权脚在了Arround的实际运行方法
                    builder.Use(next => async ctx =>
                    {
                        var rt = AutoConfigurationHelper.InvokeInstanceMethod(chain.AroundMethod.Item1, chain.AroundMethod.Item2,
                            aspectContext.ComponentContext,
                            aspectContext, next);
                        if (typeof(Task).IsAssignableFrom(chain.AroundMethod.Item2.ReturnType))
                        {
                            await ((Task) rt).ConfigureAwait(false);
                        }

                        //如果有拦截器设置 ReturnValue 那么就直接拿这个作为整个拦截器的方法返回值
                        if (ctx.InvocationContext.ReturnValue != null)
                        {
                            ctx.Result = ctx.InvocationContext.ReturnValue;
                        }
                    });
                }
                
                var haveThrowingMethod = chain.ThrowingMethod != null;
                if (haveThrowingMethod)
                {
                    var aspectThrowingInterceptor = new AspectThrowingInterceptor(chain.ThrowingMethod, isLast);
                    builder.Use(next => async ctx => { await aspectThrowingInterceptor.OnInvocation(ctx, next); });
                }

                if (chain.BeforeMethod != null)
                {
                    //Before先加进去先执行
                    var before = new AspectBeforeInterceptor((chain.BeforeMethod.Item1, chain.BeforeMethod.Item2));
                    builder.Use(next => async ctx => await before.OnInvocation(ctx, next));
                }

                if (isLast)
                {
                    //真正的方法
                    builder.Use(next => async ctx =>
                    {
                        try
                        {
                            ctx.Result = await proceed(aspectContext.InvocationContext, aspectContext.CaptureProceedInfo);
                            aspectContext.InvocationContext.ReturnValue = ctx.Result; //原方法的执行返回值
                        }
                        catch (Exception ex)
                        {
                            if (!haveThrowingMethod) throw; //如果没有错误增强器直接throw
                            aspectContext.Exception = ex; // 只有这里会设置值 到错误增强器里面去处理并 在最后一个错误处理器里面throw
                        }

                        await next(ctx);
                    });
                }

                aroundIndex++;
            }

            var aspectfunc = builder.Build();
            return aspectfunc;
        }

        public AspectDelegate BuilderMethodChain(AspectContext aspectContext, Func<IInvocation, IInvocationProceedInfo, Task> proceed)
        {
            AspectMiddlewareBuilder builder = new AspectMiddlewareBuilder();

            var aroundIndex = 0;

            foreach (var chain in PointcutMethodChainList)
            {
                var isLast = aroundIndex == PointcutMethodChainList.Count - 1;

                if (chain.AfterMethod != null)
                {
                    var after = new AspectAfterInterceptor((chain.AfterMethod.Item1, chain.AfterMethod.Item2, chain.AfterMethod.Item3));
                    //After 后加进去先执行
                    builder.Use(next => async ctx => await after.OnInvocation(ctx, next));
                }

                if (chain.AroundMethod != null)
                {
                    //Arround 先加进去先执行 后续执行权脚在了Arround的实际运行方法
                    builder.Use(next => async ctx =>
                    {
                        var rt = AutoConfigurationHelper.InvokeInstanceMethod(chain.AroundMethod.Item1, chain.AroundMethod.Item2,
                            aspectContext.ComponentContext,
                            aspectContext, next);
                        if (typeof(Task).IsAssignableFrom(chain.AroundMethod.Item2.ReturnType))
                        {
                            await ((Task) rt).ConfigureAwait(false);
                        }

                        //如果有拦截器设置 ReturnValue 那么就直接拿这个作为整个拦截器的方法返回值
                        if (ctx.InvocationContext.ReturnValue != null)
                        {
                            ctx.Result = ctx.InvocationContext.ReturnValue;
                        }
                    });
                }

                var haveThrowingMethod = chain.ThrowingMethod != null;
                if (haveThrowingMethod)
                {
                    var aspectThrowingInterceptor = new AspectThrowingInterceptor(chain.ThrowingMethod, isLast);
                    builder.Use(next => async ctx => { await aspectThrowingInterceptor.OnInvocation(ctx, next); });
                }
                
                if (chain.BeforeMethod != null)
                {
                    //Before先加进去先执行
                    var before = new AspectBeforeInterceptor((chain.BeforeMethod.Item1, chain.BeforeMethod.Item2));
                    builder.Use(next => async ctx => await before.OnInvocation(ctx, next));
                }

                if (isLast)
                {
                    //真正的方法
                    builder.Use(next => async ctx =>
                    {
                        try
                        {
                            await proceed(aspectContext.InvocationContext, aspectContext.CaptureProceedInfo);
                        }
                        catch (Exception ex)
                        {
                            if (!haveThrowingMethod) throw; //如果没有错误增强器直接throw
                            aspectContext.Exception = ex; // 只有这里会设置值 到错误增强器里面去处理并 在最后一个错误处理器里面throw
                        }

                        await next(ctx);
                    });
                }

                aroundIndex++;
            }


            var aspectfunc = builder.Build();
            return aspectfunc;
        }
    }
}