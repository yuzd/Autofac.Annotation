using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac.Annotation;
using Autofac.AspectIntercepter.Impl;

namespace Autofac.AspectIntercepter.Pointcut
{
    /// <summary>
    ///     构建拦截器链的方法
    /// </summary>
    internal class PointcutMethod
    {
        /// <summary>
        ///     排序
        /// </summary>
        public int OrderIndex { get; set; }

        /// <summary>
        ///     切面配置对应的前置方法
        /// </summary>
        public RunTimePointcutMethod<Before> BeforeMethod { get; set; }

        /// <summary>
        ///     切面配置对应的后置方法(只是正常)
        /// </summary>
        public RunTimePointcutMethod<AfterReturn> AfterReturnMethod { get; set; }

        /// <summary>
        ///     切面配置对应的后置方法(不管正常还是异常)
        /// </summary>
        public RunTimePointcutMethod<After> AfterMethod { get; set; }


        /// <summary>
        ///     切面配置对应的环绕方法
        /// </summary>
        public RunTimePointcutMethod<Around> AroundMethod { get; set; }

        /// <summary>
        ///     切面配置对应的错误拦截方法(只是异常)
        /// </summary>
        public RunTimePointcutMethod<AfterThrows> AfterThrowsMethod { get; set; }
    }

    internal class RunTimePointcutMethod<T>
    {
        /// <summary>
        ///     对应的是哪种pointcut方法标签 有 After Before Around Throws
        /// </summary>
        public T PointcutBasicAttribute { get; set; }

        /// <summary>
        ///     被拦截实例
        /// </summary>
        public dynamic Instance { get; set; }

        /// <summary>
        ///     被拦截方法
        /// </summary>
        public MethodInfo MethodInfo { get; set; }

        /// <summary>
        ///     方法返回类型
        /// </summary>
        public Type MethodReturnType { get; set; }

        /// <summary>
        ///     方法参数
        /// </summary>
        public ParameterInfo[] MethodParameters { get; set; }

        /// <summary>
        ///     被拦截方法上的指定的切面注解
        /// </summary>
        public Attribute PointcutInjectAnotation { get; set; }

        /// <summary>
        ///    PointCut注解
        /// </summary>
        public Annotation.Pointcut Pointcut { get; set; }
    }


    /// <summary>
    /// </summary>
    internal class PointcutMethodChainBuilder
    {
        public readonly Lazy<AspectDelegate> AspectFunc;

        public PointcutMethodChainBuilder()
        {
            AspectFunc = new Lazy<AspectDelegate>(() => BuilderMethodChain());
        }

        /// <summary>
        ///     支持把Aspect传进来
        /// </summary>
        /// <param name="mimin"></param>
        public PointcutMethodChainBuilder(Lazy<AspectDelegate> mimin)
        {
            AspectFunc = new Lazy<AspectDelegate>(() => BuilderMethodChain(mimin));
        }

        public List<PointcutMethod> PointcutMethodChainList { get; set; }

        private AspectDelegate BuilderMethodChain(Lazy<AspectDelegate> mimin = null)
        {
            var builder = new AspectMiddlewareBuilder();

            var aroundIndex = 0;
            foreach (var chain in PointcutMethodChainList)
            {
                var isLast = aroundIndex == PointcutMethodChainList.Count - 1;

                if (chain.AfterReturnMethod != null)
                {
                    var after = new AspectAfterReturnInterceptor(chain.AfterReturnMethod);
                    //After 后加进去先执行
                    builder.Use(next => async ctx => await after.OnInvocation(ctx, next));
                }


                if (chain.AroundMethod != null)
                {
                    var around = new AspectAroundInterceptor(chain.AroundMethod,
                        chain.AfterMethod != null ? new AspectAfterInterceptor(chain.AfterMethod, true) : null,
                        chain.AfterThrowsMethod != null ? new AspectAfterThrowsInterceptor(chain.AfterThrowsMethod, true) : null);
                    //Arround 先加进去先执行 后续执行权脚在了Arround的实际运行方法
                    builder.Use(next => async ctx => await around.OnInvocation(ctx, next));
                }


                if (chain.AroundMethod == null && chain.AfterThrowsMethod != null)
                {
                    var aspectThrowingInterceptor = new AspectAfterThrowsInterceptor(chain.AfterThrowsMethod);
                    builder.Use(next => async ctx => { await aspectThrowingInterceptor.OnInvocation(ctx, next); });
                }

                if (chain.AroundMethod == null && chain.AfterMethod != null)
                {
                    var after = new AspectAfterInterceptor(chain.AfterMethod);
                    builder.Use(next => async ctx => await after.OnInvocation(ctx, next));
                }


                if (chain.BeforeMethod != null)
                {
                    //Before先加进去先执行
                    var before = new AspectBeforeInterceptor(chain.BeforeMethod);
                    builder.Use(next => async ctx => await before.OnInvocation(ctx, next));
                }

                aroundIndex++;

                if (!isLast) continue;

                //真正的方法
                builder.Use(next => async ctx =>
                {
                    try
                    {
                        if (mimin != null)
                            await mimin.Value.Invoke(ctx);
                        else
                            await ctx.Proceed();
                    }
                    catch (Exception ex)
                    {
                        ctx.Exception = ex; // 只有这里会设置值 到错误增强器里面去处理并 在最后一个错误处理器里面throw
                        throw;
                    }

                    await next(ctx);
                });
            }

            var aspectfunc = builder.Build();
            return aspectfunc;
        }
    }
}