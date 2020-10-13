using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac.Annotation.Util;
using Autofac.Aspect.Advice;
using Autofac.Aspect.Pointcut;

namespace Autofac.Annotation
{
    /// <summary>
    /// 在打了AOP配置类的方法上面 可以配置切入点 这样就不用一个个class上去配置了
    /// sql like的匹配模式 % 代表通配符 _代表匹配任意一个字符
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class Pointcut : System.Attribute
    {
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="groupName">名称 唯一</param>
        public Pointcut(string groupName)
        {
            this.GroupName = groupName;
        }

        /// <summary>
        /// 空构造
        /// </summary>
        public Pointcut()
        {
        }

        /// <summary>
        /// 唯一的名称
        /// </summary>
        public string GroupName { get; set; } = "";

        /// <summary>
        /// 被创建后执行的方法
        /// </summary>
        public string InitMethod { get; set; }

        /// <summary>
        /// 被Release时执行的方法
        /// </summary>
        public string DestroyMethod { get; set; }

        /// <summary>
        /// 值越低，优先级越高
        /// </summary>
        public int OrderIndex { get; set; }

        /// <summary>
        /// 用于匹配返回类型
        /// </summary>
        private string _retType = "%";

        /// <summary>
        /// 用于匹配返回类型 支持sql的like表达式
        /// </summary>
        public string RetType
        {
            get => _retType;
            set => _retType = value.Replace("*", "%").Replace("?", "_");
        }

        /// <summary>
        /// 用于匹配包类型
        /// </summary>
        private string _nameSpace = "%";

        /// <summary>
        /// 用于匹配包类型 支持sql的like表达式
        /// </summary>
        public string NameSpace
        {
            get => _nameSpace;
            set => _nameSpace = value.Replace("*", "%").Replace("?", "_");
        }


        /// <summary>
        /// class的名称
        /// </summary>
        private string _class;

        /// <summary>
        /// class的名称  支持sql的like表达式
        /// </summary>
        public string Class
        {
            get => _class;
            set => _class = value.Replace("*", "%").Replace("?", "_");
        }

        /// <summary>
        /// 拦截Attribute
        /// </summary>
        public Type AttributeType { get; set; }


        /// <summary>
        /// 不能注入尽量的切面注解
        /// </summary>
        private static readonly Dictionary<Type, bool> IgnoreAttributeType = new Dictionary<Type, bool>
        {
            {typeof(AutoConfiguration), false},
            {typeof(Autowired), false},
            {typeof(Bean), false},
            {typeof(Component), false},
            {typeof(Import), false},
            {typeof(Value), false},
            {typeof(Aspect), false},
            {typeof(InterfaceInterceptor), false},
            {typeof(ClassInterceptor), false},
            {typeof(AfterReturn), false},
            {typeof(Around), false},
            {typeof(Before), false},
            {typeof(Pointcut), false},
            {typeof(AfterThrows), false},
        };

        /// <summary>
        /// method的名称
        /// </summary>
        private string _method = "%";

        /// <summary>
        /// 方法名称 支持sql的like表达式
        /// </summary>
        public string Method
        {
            get => _method;
            set => _method = value.Replace("*", "%").Replace("?", "_");
        }


        /// <summary>
        /// 是否当前class满足
        /// </summary>
        /// <returns></returns>
        internal bool IsVaildClass(ComponentModel component,out Attribute pointCutClassInjectAnotation)
        {
            pointCutClassInjectAnotation = null;
            var classType = component.CurrentType;
            
            //如果没有设定clasname的匹配  也没有设置标签拦截指定 就不继续往下了
            if ((string.IsNullOrEmpty(Class) && AttributeType == null))
            {
                throw new InvalidOperationException($"PointCut:`{this.GetType().FullName}` -> `Class` or `AttributeType` One of them must be set ! ");
            }

            if (!SqlLikeStringUtilities.SqlLike(this.NameSpace, classType.Namespace))
            {
                return false;
            }

            //配置了class
            if (!string.IsNullOrEmpty(this.Class) && !SqlLikeStringUtilities.SqlLike(this.Class, classType.Name))
            {
                return false;
            }

            if (this.AttributeType != null)
            {
                //框架内部的不可
                if (IgnoreAttributeType.ContainsKey(this.AttributeType))
                {
                    throw new InvalidOperationException($"PointCut:`{this.GetType().FullName}` -> `AttributeType` can not be set to special type: `${AttributeType.Name}` ! ");
                }
                
                //继承了AspectInvokeAttribute的不可
                else if (typeof(AspectInvokeAttribute).IsAssignableFrom(this.AttributeType))
                {
                    throw new InvalidOperationException($"PointCut:`{this.GetType().FullName}` -> `AttributeType` can not be set to  instance of `AspectInvokeAttribute` ! ");
                }
                
                if (component.CurrentClassTypeAttributes != null && component.CurrentClassTypeAttributes.Any())
                {
                    foreach (var classAttribute in component.CurrentClassTypeAttributes)
                    {
                        if (classAttribute.GetType() == this.AttributeType)
                        {
                            pointCutClassInjectAnotation = classAttribute;
                            break;
                        }
                    }
                }
                
            }

            return true;
        }


        /// <summary>
        /// 是否可用
        /// </summary>
        /// <returns></returns>
        internal bool IsVaild(ComponentModel component,KeyValuePair<MethodInfo,List<Attribute>> methodInfoCache,Attribute parentClassinjectPointcutAnnotationCache,out Attribute injectPointcutAnnotationCache)
        {
            var methodInfo = methodInfoCache.Key;
            injectPointcutAnnotationCache = null;
            //如果本身带了_的话
            //test_a  
            if (!SqlLikeStringUtilities.SqlLike(this.RetType, methodInfo.ReturnType.Name))
            {
                return false;
            }

            if (!SqlLikeStringUtilities.SqlLike(this.Method, methodInfo.Name))
            {
                return false;
            }

            if (this.AttributeType != null)
            {
                Attribute annotation = null;
                foreach (var attr in methodInfoCache.Value)
                {
                    if (AttributeType == attr.GetType())
                    {
                        annotation = attr;
                        break;
                    }
                }

                if (annotation==null && parentClassinjectPointcutAnnotationCache != null)
                {
                    annotation = parentClassinjectPointcutAnnotationCache;
                }

                if (annotation != null)
                {
                    injectPointcutAnnotationCache = annotation;
                    return true;
                }
                
                return false;
            }

            return true;
        }
    }
}