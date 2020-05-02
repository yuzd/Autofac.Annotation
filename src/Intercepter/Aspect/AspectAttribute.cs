using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac.Annotation;
using Autofac.Annotation.Util;
using Castle.DynamicProxy;

namespace Autofac.Aspect
{

    /// <summary>
    /// AOP拦截器
    /// </summary>
    [AttributeUsage(AttributeTargets.Method|AttributeTargets.Class)]
    public class AspectInvokeAttribute : Attribute
    {

        /// <summary>
        /// 排序 越大的先先调用
        /// </summary>
        public int OrderIndex { get; set; }

    }

    /// <summary>
    /// 给某一个class 开启AOP拦截器
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
    /// 配合pointCut的前置拦截器
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class BeforeAttribute : Attribute
    {
        /// <summary>
        /// 唯一名称
        /// </summary>
        public string Name { get; set; }
    }
    
    /// <summary>
    /// 配合pointCut的后置拦截器
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class AfterAttribute : Attribute
    {
        /// <summary>
        /// 唯一名称
        /// </summary>
        public string Name { get; set; }
    }
    
    /// <summary>
    /// 配合pointCut的环绕拦截器
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class AroundAttribute : Attribute
    {
        /// <summary>
        /// 唯一名称
        /// </summary>
        public string Name { get; set; }
    }
    
    
    /// <summary>
    /// 配置pointCut 指定拦截类型为class
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ClassInterceptorAttribute : Attribute
    {
    }
    
    /// <summary>
    /// 配置pointCut 指定拦截类型为接口
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class InterfaceInterceptorAttribute : Attribute
    {
    }
    
    /// <summary>
    /// 在打了AOP配置类的方法上面 可以配置切入点 这样就不用一个个class上去配置了
    /// sql like的匹配模式 % 代表通配符 _代表匹配任意一个字符
    /// </summary>
    [AttributeUsage(AttributeTargets.Class,AllowMultiple = true)]
    public class PointcutAttribute : Attribute
    {
        
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="name">名称 唯一</param>
        public PointcutAttribute(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// 空构造
        /// </summary>
        public PointcutAttribute()
        {
            
        }

        /// <summary>
        /// 唯一的名称
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// 用于匹配返回类型
        /// </summary>
        private string _retType = "%";
        /// <summary>
        /// 用于匹配返回类型 支持sql的like表达式
        /// </summary>
        public string RetType {
            get => _retType;
            set => _retType = value.Replace("*", "%").Replace("?","_");
        } 
        
        /// <summary>
        /// 用于匹配包类型
        /// </summary>
        private string _nameSpace = "%";
        /// <summary>
        /// 用于匹配包类型 支持sql的like表达式
        /// </summary>
        public string NameSpace {
            get => _nameSpace;
            set => _nameSpace = value.Replace("*", "%").Replace("?","_");
        }

        
        /// <summary>
        /// class的名称
        /// </summary>
        private string _clasName;
        /// <summary>
        /// class的名称  支持sql的like表达式
        /// </summary>
        public string ClassName
        {
            get => _clasName;
            set => _clasName = value.Replace("*", "%").Replace("?","_");
        } 

        /// <summary>
        /// class的名称
        /// </summary>
        private string _methodName = "%";
        /// <summary>
        /// 方法名称 支持sql的like表达式
        /// </summary>
        public string MethodName 
        {
            get => _methodName;
            set => _methodName = value.Replace("*", "%").Replace("?","_");
        }


        /// <summary>
        /// 是否当前class满足
        /// </summary>
        /// <param name="classType"></param>
        /// <returns></returns>
        public bool IsVaildClass(Type classType)
        {
            if (!SqlLikeStringUtilities.SqlLike(this.NameSpace, classType.Namespace))
            {
                return false;
            }
            
            if (!SqlLikeStringUtilities.SqlLike(this.ClassName, classType.Name))
            {
                return false;
            }

            return true;
        }
        

        /// <summary>
        /// 是否可用
        /// </summary>
        /// <returns></returns>
        public bool IsVaild(MethodInfo methodInfo)
        {
            var classType = methodInfo.DeclaringType;
            
            //如果没有设定clasname的匹配 就不继续往下了
            if (string.IsNullOrEmpty(ClassName) || classType == null) return false;
            
            //如果本身带了_的话
            //test_a  
            if (!SqlLikeStringUtilities.SqlLike(this.RetType, methodInfo.ReturnType.Name))
            {
                return false;
            }
            
            if (!SqlLikeStringUtilities.SqlLike(this.NameSpace, classType.Namespace))
            {
                return false;
            }
            
            if (!SqlLikeStringUtilities.SqlLike(this.ClassName, classType.Name))
            {
                return false;
            }

            if (!SqlLikeStringUtilities.SqlLike(this.MethodName, methodInfo.Name))
            {
                return false;
            }

            return true;
        }

    }
    
    
    /// <summary>
    /// AOP环绕拦截器
    /// </summary>
    public abstract class AspectAroundAttribute : AspectInvokeAttribute
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
    public abstract class AspectBeforeAttribute : AspectInvokeAttribute
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
    public abstract class AspectAfterAttribute : AspectInvokeAttribute
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
    public abstract class AspectPointAttribute : AspectInvokeAttribute
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
        /// 防止异步出问题
        /// </summary>
        public IInvocationProceedInfo InvocationProceedInfo { get; set; }
        
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
