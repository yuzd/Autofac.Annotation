using System.Threading.Tasks;
using Autofac.Aspect.Advice.Impl;
using Castle.DynamicProxy;

namespace Autofac.Aspect.Advice
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

  
    /// <summary>
    /// 增强方法调用链
    /// </summary>
    internal class AspectAttributeChainBuilder
    {
        public AspectAttributeChainBuilder()
        {
            AspectBeforeAttributeList = new List<AspectBeforeAttribute>();
            AspectAfterAttributeList = new List<AspectAfterAttribute>();
            AspectAfterThrowingAttributeList = new List<AspectThrowingAttribute>();
            AspectArroundAttributeList = new List<AspectArroundAttribute>();
        }
        public List<AspectBeforeAttribute> AspectBeforeAttributeList { get; set; }
        public List<AspectAfterAttribute> AspectAfterAttributeList { get; set; }
        public List<AspectThrowingAttribute> AspectAfterThrowingAttributeList { get; set; }
        public List<AspectArroundAttribute> AspectArroundAttributeList { get; set; }


        public AspectDelegate BuilderMethodChain<TResult>(AspectContext aspectContext, Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed)
        {
            AspectMiddlewareBuilder builder = new AspectMiddlewareBuilder();

            //Arround 先加进去先执行
            foreach (var arroundAttribute in this.AspectArroundAttributeList)
            {
                builder.Use(next => async ctx =>
                {
                    await arroundAttribute.OnInvocation(ctx, next);
                    //如果有拦截器设置 ReturnValue 那么就直接拿这个作为整个拦截器的方法返回值
                    if (ctx.InvocationContext.ReturnValue != null)
                    {
                        ctx.Result = ctx.InvocationContext.ReturnValue;
                    }
                });
            }
            
            //After 后加进去先执行
            foreach (var after in AspectAfterAttributeList.OrderBy(r=>r.OrderIndex).Select(r=> new AspectAfterInterceptor(r)))
            {
                builder.Use(next => async ctx =>
                {
                    await after.OnInvocation(ctx, next);
                });
            }
            
            //Before先加进去先执行
            foreach (var before in AspectBeforeAttributeList.Select(r=> new AspectBeforeInterceptor(r)))
            {
                builder.Use(next => async ctx =>
                {
                    await before.OnInvocation(ctx, next);
                });
            }
            
            //异常放在真正的方法执行的上一层
            if (this.AspectAfterThrowingAttributeList.Any())
            {
                var aspectThrowingInterceptor = new AspectThrowingInterceptor(this.AspectAfterThrowingAttributeList);
                builder.Use(next => async ctx =>
                {
                    await aspectThrowingInterceptor.OnInvocation(ctx, next);
                });
            }
            
            //真正的方法
            builder.Use(next => async ctx =>
            {
                ctx.Result = await proceed(aspectContext.InvocationContext, aspectContext.CaptureProceedInfo);
                aspectContext.InvocationContext.ReturnValue = ctx.Result; //原方法的执行返回值
            });
            
            var aspectfunc = builder.Build();
            return aspectfunc;
        }
        
        public AspectDelegate BuilderMethodChain(AspectContext aspectContext, Func<IInvocation, IInvocationProceedInfo, Task> proceed)
        {
             AspectMiddlewareBuilder builder = new AspectMiddlewareBuilder();

            //Arround 先加进去先执行
            foreach (var arroundAttribute in this.AspectArroundAttributeList)
            {
                builder.Use(next => async ctx =>
                {
                    await arroundAttribute.OnInvocation(ctx, next);
                });
            }
            
            //After 后加进去先执行
            foreach (var after in AspectAfterAttributeList.OrderBy(r=>r.OrderIndex).Select(r=> new AspectAfterInterceptor(r)))
            {
                builder.Use(next => async ctx =>
                {
                    await after.OnInvocation(ctx, next);
                });
            }
            
            //Before先加进去先执行
            foreach (var before in AspectBeforeAttributeList.Select(r=> new AspectBeforeInterceptor(r)))
            {
                builder.Use(next => async ctx =>
                {
                    await before.OnInvocation(ctx, next);
                });
            }
            
            //异常放在真正的方法执行的上一层
            if (this.AspectAfterThrowingAttributeList.Any())
            {
                var aspectThrowingInterceptor = new AspectThrowingInterceptor(this.AspectAfterThrowingAttributeList);
                builder.Use(next => async ctx =>
                {
                    await aspectThrowingInterceptor.OnInvocation(ctx, next);
                });
            }
            
            //真正的方法
            builder.Use(next => async ctx =>
            {
                 await proceed(aspectContext.InvocationContext, aspectContext.CaptureProceedInfo);
            });
            
            var aspectfunc = builder.Build();
            return aspectfunc;
        }
    }
}