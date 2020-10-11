using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac.Annotation;
using Autofac.Aspect.Impl;
using Castle.DynamicProxy;

namespace Autofac.Aspect.Pointcut
{
    /// <summary>
    ///  构建拦截器链的方法
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
        public RunTimePointcutMethod<Before> BeforeMethod { get; set; }

        /// <summary>
        /// 切面配置对应的后置方法
        /// </summary>
        public RunTimePointcutMethod<After> AfterMethod { get; set; }

        /// <summary>
        /// 切面配置对应的环绕方法
        /// </summary>
        public RunTimePointcutMethod<Around> AroundMethod { get; set; }

        /// <summary>
        /// 切面配置对应的错误拦截方法
        /// </summary>
        public RunTimePointcutMethod<Throws> ThrowingMethod { get; set; }
    }

    internal class RunTimePointcutMethod<T>
    {
        /// <summary>
        /// 对应的是哪种pointcut方法标签 有 After Before Around Throws
        /// </summary>
        public T PointcutBasicAttribute { get; set; }
        
        /// <summary>
        /// 被拦截实例
        /// </summary>
        public object Instance { get; set; }
        
        /// <summary>
        /// 被拦截方法
        /// </summary>
        public MethodInfo MethodInfo { get; set; }
        
        /// <summary>
        /// 被拦截方法上的指定的切面注解
        /// </summary>
        public Attribute PointcutInjectAnotation { get; set; }
    }
    
    

    /// <summary>
    /// 构建拦截器链
    /// </summary>
    internal class PointcutMethodChainBuilder
    {
        public List<PointcutMethod> PointcutMethodChainList { get; set; }

        public readonly Lazy<AspectDelegate> AspectFunc;

        public PointcutMethodChainBuilder()
        {
            AspectFunc = new Lazy<AspectDelegate>(this.BuilderMethodChain);
        }

        private AspectDelegate BuilderMethodChain()
        {
            AspectMiddlewareBuilder builder = new AspectMiddlewareBuilder();

            var aroundIndex = 0;
            var throwsIndex = 0;
            var throwingMethodCount = PointcutMethodChainList.Count(r => r.ThrowingMethod!=null);
            foreach (var chain in PointcutMethodChainList)
            {
                var isLast = aroundIndex == PointcutMethodChainList.Count - 1;

                if (chain.AfterMethod != null)
                {
                    var after = new AspectAfterInterceptor(chain.AfterMethod);
                    //After 后加进去先执行
                    builder.Use(next => async ctx => await after.OnInvocation(ctx, next));
                }

                if (chain.AroundMethod != null)
                {
                    var around = new AspectAroundInterceptor(chain.AroundMethod);
                    //Arround 先加进去先执行 后续执行权脚在了Arround的实际运行方法
                    builder.Use(next => async ctx => await around.OnInvocation(ctx, next));
                }
                
                if (chain.ThrowingMethod != null)
                {
                    throwsIndex++;
                    var aspectThrowingInterceptor = new AspectThrowingInterceptor(chain.ThrowingMethod, throwsIndex == throwingMethodCount);
                    builder.Use(next => async ctx => { await aspectThrowingInterceptor.OnInvocation(ctx, next); });
                }

                if (chain.BeforeMethod != null)
                {
                    //Before先加进去先执行
                    var before = new AspectBeforeInterceptor(chain.BeforeMethod);
                    builder.Use(next => async ctx => await before.OnInvocation(ctx, next));
                }

                aroundIndex++;

                if (!isLast) continue;

                if (throwingMethodCount<1)
                {
                    //真正的方法
                    builder.Use(next => async ctx =>
                    {
                        await ctx.Proceed();
                        await next(ctx);
                    });
                    continue;
                }
                
                //真正的方法
                builder.Use(next => async ctx =>
                {
                    try
                    {
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