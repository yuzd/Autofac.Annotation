using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac.Annotation;
using Autofac.Annotation.Util;
using Autofac.Aspect.Advice;
using Autofac.Aspect.Advice.Impl;
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
        /// 
        /// </summary>
        public BeforeAttribute()
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        public BeforeAttribute(string name)
        {
            this.Name = name;
        }
        
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
        /// 
        /// </summary>
        public AfterAttribute()
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        public AfterAttribute(string name)
        {
            this.Name = name;
        }

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
        /// 
        /// </summary>
        public AroundAttribute()
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        public AroundAttribute(string name)
        {
            this.Name = name;
        }
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
        /// 被创建后执行的方法
        /// </summary>
        public string InitMethod { get; set; }
        /// <summary>
        /// 被Release时执行的方法
        /// </summary>
        public string DestroyMethod { get; set; }

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
        private string _clas;
        /// <summary>
        /// class的名称  支持sql的like表达式
        /// </summary>
        public string Class
        {
            get => _clas;
            set => _clas = value.Replace("*", "%").Replace("?","_");
        } 

        /// <summary>
        /// class的名称
        /// </summary>
        private string _method = "%";
        /// <summary>
        /// 方法名称 支持sql的like表达式
        /// </summary>
        public string Method 
        {
            get => _method;
            set => _method = value.Replace("*", "%").Replace("?","_");
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
            
            if (!SqlLikeStringUtilities.SqlLike(this.Class, classType.Name))
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
            if (string.IsNullOrEmpty(Class) || classType == null) return false;
            
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
            
            if (!SqlLikeStringUtilities.SqlLike(this.Class, classType.Name))
            {
                return false;
            }

            if (!SqlLikeStringUtilities.SqlLike(this.Method, methodInfo.Name))
            {
                return false;
            }

            return true;
        }

    }



    
    
  
    
    /// <summary>
    /// 拦截器上下文
    /// </summary>
    public class PointcutContext
    {
       
        /// <summary>
        /// autofac容器
        /// </summary>
        public IComponentContext ComponentContext { get; set; }
        
        /// <summary>
        /// 被代理的原执行方法
        /// </summary>

        public MethodInfo InvocationMethod { get; set; }
        
        
        /// <summary>
        /// 错误
        /// </summary>
        public Exception Exception { get; set; }
        
        /// <summary>
        /// 回调 只有Arround的时候才有值
        /// </summary>
        public Func<Task> Proceed { get; set; }
        
    }

    /// <summary>
    /// 拦截器
    /// </summary>
    /// <param name="context"></param>
    public delegate System.Threading.Tasks.Task AspectDelegate(AspectContext context);
    
    

}
