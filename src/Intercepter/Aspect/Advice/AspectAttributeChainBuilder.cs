using System.Threading.Tasks;
using Autofac.Aspect.Advice.Impl;
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
            foreach (var chain in AdviceMethod)
            {
                var isLast = aroundIndex == AdviceMethod.Count - 1;

                // var reverseAfter = AdviceMethod.Count == 1? chain.AspectAfter:  AdviceMethod[AdviceMethod.Count - 1 - aroundIndex].AspectAfter;
                if (chain.AspectAfter != null)
                {
                    var after = new AspectAfterInterceptor(chain.AspectAfter);
                    //After 后加进去先执行
                    builder.Use(next => async ctx => await after.OnInvocation(ctx, next));
                }

                if (chain.AspectArround != null)
                {
                    //Arround 先加进去先执行 后续执行权脚在了Arround的实际运行方法
                    builder.Use(next => async ctx =>
                    {
                        await chain.AspectArround.OnInvocation(ctx, next);
                        //如果有拦截器设置 ReturnValue 那么就直接拿这个作为整个拦截器的方法返回值
                        if (ctx.InvocationContext.ReturnValue != null)
                        {
                            ctx.Result = ctx.InvocationContext.ReturnValue;
                        }
                    });
                }
                
                var haveThrowingMethod = chain.AspectThrowing != null;

                if (haveThrowingMethod)
                {
                    var aspectThrowingInterceptor = new AspectThrowingInterceptor(chain.AspectThrowing, isLast);
                    builder.Use(next => async ctx => { await aspectThrowingInterceptor.OnInvocation(ctx, next); });
                }

                if (chain.AspectBefore != null)
                {
                    //Before先加进去先执行
                    var before = new AspectBeforeInterceptor(chain.AspectBefore);
                    builder.Use(next => async ctx => await before.OnInvocation(ctx, next));
                }

                if (isLast)
                {
                    //真正的方法
                    builder.Use(next => async ctx =>
                    {
                        try
                        {
                            await ctx.Proceed();
                        }
                        catch (Exception ex)
                        {
                            if (!haveThrowingMethod) throw; //如果没有错误增强器直接throw
                            ctx.Exception = ex; // 只有这里会设置值 到错误增强器里面去处理并 在最后一个错误处理器里面throw
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