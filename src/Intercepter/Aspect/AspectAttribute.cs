using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac.Annotation;
using Castle.DynamicProxy;

namespace Autofac.Aspect
{

    /// <summary>
    /// AOP拦截器
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class AspectMethodAttribute : Attribute
    {


    }

    /// <summary>
    /// 开启AOP拦截器
    /// 配合 AspectAroundAttribute  AspectBeforeAttribute AspectAfterAttribute 使用
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AspectAttribute : Attribute
    {
        /// <summary>
        /// ctor
        /// </summary>
        public AspectAttribute()
        {
            
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="type"></param>
        public AspectAttribute(InterceptorType type)
        {
            AspectType = type;
        }

        /// <summary>
        /// 拦截器类型 默认是当前class的拦截器
        /// </summary>
        public InterceptorType AspectType { get; set; } = InterceptorType.Class;
    }
    /// <summary>
    /// AOP环绕拦截器
    /// </summary>
    public abstract class AspectAroundAttribute : AspectMethodAttribute
    {


        /// <summary>
        /// 后置执行
        /// </summary>
        /// <param name="aspectContext"></param>
        public abstract Task After(AspectContext aspectContext);


        /// <summary>
        /// 前置执行
        /// </summary>
        /// <param name="aspectContext"></param>
        public abstract Task Before(AspectContext aspectContext);

    }

   

    /// <summary>
    /// AOP前置拦截器
    /// </summary>
    public abstract class AspectBeforeAttribute : AspectMethodAttribute
    {

        /// <summary>
        /// 前置执行
        /// </summary>
        /// <param name="aspectContext"></param>
        public abstract Task Before(AspectContext aspectContext);

    }

  

    /// <summary>
    /// AOP后置拦截器
    /// </summary>
    public abstract class AspectAfterAttribute : AspectMethodAttribute
    {

        /// <summary>
        /// 后置执行
        /// </summary>
        /// <param name="aspectContext"></param>
        public abstract Task After(AspectContext aspectContext);

    }

    /// <summary>
    /// 切入点拦截器
    /// </summary>
    public abstract class PointcutAttribute : AspectMethodAttribute
    {

        /// <summary>
        /// 拦截器
        /// </summary>
        /// <param name="aspectContext">拦截上下文</param>
        /// <param name="_next">下一个拦截器 最后一个是执行被拦截的方法</param>
        /// <returns></returns>
        public abstract Task OnInvocation(AspectContext aspectContext,AspectDelegate _next);

    }

    
    /// <summary>
    /// 拦截器上下文
    /// </summary>
    public class AspectContext
    {
        /// <summary>
        /// 空的构造方法
        /// </summary>
        public AspectContext()
        {
            
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="context"></param>
        /// <param name="invocation"></param>
        public AspectContext(IComponentContext context,IInvocation invocation)
        {
            this.ComponentContext = context;
            this.InvocationContext = invocation;
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="context"></param>
        /// <param name="invocation"></param>
        /// <param name="exception"></param>
        public AspectContext(IComponentContext context,IInvocation invocation,Exception exception)
        {
            this.ComponentContext = context;
            this.InvocationContext = invocation;
            this.Exception = exception;
        }

        /// <summary>
        /// autofac容器
        /// </summary>
        public IComponentContext ComponentContext { get; set; }
        
        /// <summary>
        /// 执行环节上下文
        /// </summary>

        public IInvocation InvocationContext { get; set; }
        
        /// <summary>
        /// 错误
        /// </summary>
        public Exception Exception { get; set; }
        
        /// <summary>
        /// 有返回结果的
        /// </summary>
        internal object Result { get; set; }
        
        
    }

    /// <summary>
    /// 拦截器
    /// </summary>
    /// <param name="context"></param>
    public delegate System.Threading.Tasks.Task AspectDelegate(AspectContext context);
    
    /// <summary>
    /// 拦截node组装
    /// </summary>
    internal class AspectMiddlewareComponentNode
    {
        /// <summary>
        /// 下一个
        /// </summary>
        public AspectDelegate Next;
        /// <summary>
        /// 执行器
        /// </summary>
        public AspectDelegate Process;
        /// <summary>
        /// 组件
        /// </summary>
        public Func<AspectDelegate, AspectDelegate> Component;
    }

    internal class AspectMiddlewareBuilder
    {
        private readonly LinkedList<AspectMiddlewareComponentNode> Components = new LinkedList<AspectMiddlewareComponentNode>();
       
        /// <summary>
        /// 新增拦截器链
        /// </summary>
        /// <param name="component"></param>
        public void Use(Func<AspectDelegate, AspectDelegate> component)
        {
            var node = new AspectMiddlewareComponentNode
            {
                Component = component
            };
 
            Components.AddLast(node);
        }
        
        /// <summary>
        /// 构建拦截器链
        /// </summary>
        /// <returns></returns>
        public AspectDelegate Build()
        {
            var node = Components.Last;
            while(node != null)
            {
                node.Value.Next = GetNextFunc(node);
                node.Value.Process = node.Value.Component(node.Value.Next);
                node = node.Previous;
            }
            return Components.First.Value.Process;
        }
 
        /// <summary>
        /// 获取下一个
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private AspectDelegate GetNextFunc(LinkedListNode<AspectMiddlewareComponentNode> node)
        {
            return node.Next == null ? ctx => Task.CompletedTask : node.Next.Value.Process;
        }
    }
}
