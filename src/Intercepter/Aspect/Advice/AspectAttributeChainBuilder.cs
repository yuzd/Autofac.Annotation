using System.Threading.Tasks;
using Autofac.Aspect.Impl;
using Castle.DynamicProxy;

namespace Autofac.Aspect.Advice
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;


    internal class AdviceMethod
    {
        /// <summary>
        /// 排序
        /// </summary>
        public int OrderIndex { get; set; }

        /// <summary>
        /// 分组名称
        /// </summary>
        public string GroupName { get; set; }


        public AspectBefore AspectBefore { get; set; }
        public AspectAfter AspectAfter { get; set; }
        public AspectThrows AspectThrowing { get; set; }
        public AspectArround AspectArround { get; set; }
    }

    /// <summary>
    /// 增强方法调用链
    /// </summary>
    internal class AspectAttributeChainBuilder
    {
        public List<AdviceMethod> AdviceMethod { get; set; }

        public readonly Lazy<AspectDelegate> AspectFunc;

        public AspectAttributeChainBuilder()
        {
            AspectFunc = new Lazy<AspectDelegate>(this.BuilderMethodChain);
        }

        private AspectDelegate BuilderMethodChain()
        {
            AspectMiddlewareBuilder builder = new AspectMiddlewareBuilder();

            var aroundIndex = 0;
            var throwsIndex = 0;
            var throwingMethodCount = AdviceMethod.Count(r => r.AspectThrowing!=null);
            foreach (var chain in AdviceMethod)
            {
                var isLast = aroundIndex == AdviceMethod.Count - 1;

                if (chain.AspectAfter != null)
                {
                    var after = new AspectAfterInterceptor(chain.AspectAfter);
                    //After 后加进去先执行
                    builder.Use(next => async ctx => await after.OnInvocation(ctx, next));
                }

                if (chain.AspectArround != null)
                {
                    var around = new AspectAroundInterceptor(chain.AspectArround);
                    //Arround 先加进去先执行 后续执行权脚在了Arround的实际运行方法
                    builder.Use(next => async ctx => await around.OnInvocation(ctx, next));
                }
                
                if (chain.AspectThrowing != null)
                {
                    throwsIndex++;
                    var aspectThrowingInterceptor = new AspectThrowingInterceptor(chain.AspectThrowing, throwsIndex ==throwingMethodCount );
                    builder.Use(next => async ctx => { await aspectThrowingInterceptor.OnInvocation(ctx, next); });
                }

                if (chain.AspectBefore != null)
                {
                    //Before先加进去先执行
                    var before = new AspectBeforeInterceptor(chain.AspectBefore);
                    builder.Use(next => async ctx => await before.OnInvocation(ctx, next));
                }

                aroundIndex++;
                if (!isLast) continue;

                if (throwingMethodCount < 1)
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