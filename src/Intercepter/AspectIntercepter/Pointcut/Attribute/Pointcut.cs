using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac.Annotation.Condition;
using Autofac.Annotation.Util;
using Autofac.AspectIntercepter.Advice;
using Autofac.AspectIntercepter.Pointcut;
using Type = System.Type;

namespace Autofac.Annotation
{
    /// <summary>
    /// 在打了AOP配置类的方法上面 可以配置切入点 这样就不用一个个class上去配置了
    /// sql like的匹配模式 % 代表通配符 _代表匹配任意一个字符
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class Pointcut : Attribute
    {
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="groupName">名称 唯一</param>
        public Pointcut(string groupName) : this()
        {
            GroupName = groupName;
        }

        /// <summary>
        /// 空构造
        /// </summary>
        public Pointcut()
        {
            AttributeTypeArrLazy = new Lazy<Type[]>(getAttributeTypeArrLazy);
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
        /// 值越低，优先级越高,越先被调用
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
        /// 是否设置过
        /// </summary>
        private bool _nameSpaceSetted = false;

        /// <summary>
        /// 用于匹配包类型 支持sql的like表达式
        /// </summary>
        public string NameSpace
        {
            get => _nameSpace;
            set
            {
                _nameSpace = value.Replace("*", "%").Replace("?", "_");
                _nameSpaceSetted = true;
            }
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
        /// 指定拦截的Attribute类指定继承还是实现
        /// 默认就是自己 不指定需要扫描继承它 还是实现它
        /// </summary>
        public AssignableFlag AttributeFlag { get; set; } = AssignableFlag.NONE;

        /// <summary>
        ///  <seealso cref="AttributeType"/> 如果AttributeInherited=true 那么字段AttributeType的继承父类也同样纳入扫描
        /// </summary>
        private readonly Lazy<Type[]> AttributeTypeArrLazy;

        private Type[] getAttributeTypeArrLazy()
        {
            if (AttributeType == null)
            {
                return null;
            }

            if (AttributeFlag == AssignableFlag.NONE)
            {
                return new[] { AttributeType };
            }

            if (AttributeFlag == AssignableFlag.AssignableFrom)
            {
                var rt = new List<Type> { AttributeType };
                rt.AddRange(AttributeType.GetParentTypes(false).Where(r => !r.IsAbstract).ToList());
                return rt.Distinct().ToArray();
            }

            // 也包括是我的子类的话 需要特殊判断
            return new[] { AttributeType };
        }

        /// <summary>
        /// 不能注入尽量的切面注解
        /// </summary>
        private static readonly Dictionary<Type, bool> IgnoreAttributeType = new Dictionary<Type, bool>
        {
            { typeof(AutoConfiguration), false },
            { typeof(Autowired), false },
            { typeof(Bean), false },
            { typeof(Component), false },
            { typeof(Conditional), false },
            { typeof(ConditionOnBean), false },
            { typeof(ConditionOnMissingBean), false },
            { typeof(ConditionOnClass), false },
            { typeof(ConditionOnMissingClass), false },
            { typeof(Order), false },
            { typeof(Import), false },
            { typeof(Value), false },
            { typeof(PostConstruct), false },
            { typeof(InterfaceInterceptor), false },
            { typeof(ClassInterceptor), false },
            { typeof(AfterReturn), false },
            { typeof(Around), false },
            { typeof(Before), false },
            { typeof(Pointcut), false },
            { typeof(AfterThrows), false },
            { typeof(DependsOn), false },
            { typeof(IgnoreAop), false },
            { typeof(AliasFor), false }
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
        /// pointCutClassInjectAnotation.Item2 = true代表是class的注解
        /// </summary>
        /// <returns></returns>
        internal bool IsVaildClass(ComponentModel component, out Tuple<Attribute, int> pointCutClassInjectAnotation)
        {
            int orderIndex = 10000;
            pointCutClassInjectAnotation = null;
            var classType = component.CurrentType;

            //如果没有设定clasname的匹配  也没有设置标签拦截指定 就不继续往下了
            if (string.IsNullOrEmpty(Class) && AttributeType == null)
                throw new InvalidOperationException($"PointCut:`{GetType().FullName}` -> `Class` or `AttributeType` One of them must be set ! ");

            if (_nameSpaceSetted && !SqlLikeStringUtilities.SqlLike(NameSpace, classType.Namespace)) return false;

            //配置了class
            if (!string.IsNullOrEmpty(Class) && !SqlLikeStringUtilities.SqlLike(Class, classType.Name)) return false;

            if (AttributeType != null)
            {
                foreach (var type in AttributeTypeArrLazy.Value)
                {
                    //框架内部的不可
                    if (IgnoreAttributeType.ContainsKey(type))
                        throw new InvalidOperationException(
                            $"PointCut:`{GetType().FullName}` -> `AttributeType` can not be set to special type: `${type.Name}` ! ");

                    //继承了AspectInvokeAttribute的不可
                    else if (typeof(AspectInvokeAttribute).IsAssignableFrom(type))
                        throw new InvalidOperationException(
                            $"PointCut:`{GetType().FullName}` -> `AttributeType`:{type.Name} can not be set to  instance of `AspectInvokeAttribute` ! ");
                }


                if (component.CurrentClassTypeAttributes != null && component.CurrentClassTypeAttributes.Any())
                {
                    foreach (var classAttribute in component.CurrentClassTypeAttributes)
                    {
                        orderIndex++;
                        if (AttributeTypeArrLazy.Value.Contains(classAttribute.GetType()))
                        {
                            pointCutClassInjectAnotation = Tuple.Create(classAttribute, orderIndex);
                            break;
                        }
                        else if (AttributeFlag == AssignableFlag.AssignableTo && AttributeType.IsInstanceOfType(classAttribute))
                        {
                            // classAttribute 是 AttributeType 的实现类
                            pointCutClassInjectAnotation = Tuple.Create(classAttribute, orderIndex);
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
        internal bool IsVaild(PointcutConfigurationInfo pointcut, MethodInfo methodInfoCache,
            Tuple<Attribute, int> parentClassinjectPointcutAnnotationCache, out Tuple<Attribute, int> injectPointcutAnnotationCache)
        {
            int orderIndex = 0;
            var methodInfo = methodInfoCache;
            injectPointcutAnnotationCache = null;
            //如果本身带了_的话
            //test_a  
            if (!SqlLikeStringUtilities.SqlLike(RetType, methodInfo.ReturnType.Name)) return false;

            if (!SqlLikeStringUtilities.SqlLike(Method, methodInfo.Name)) return false;

            if (AttributeType == null) return true;
            Tuple<Attribute, int> annotation = null;
            var ignore = methodInfoCache.GetCustomAttribute<IgnoreAop>();
            Type[] ignoreTarget = null;
            if (ignore != null && (IgnoreFlags.PointCut & ignore.IgnoreFlags) != 0)
            {
                ignoreTarget = ignore.Target;
                if ((ignoreTarget == null || !ignoreTarget.Any()))
                {
                    // 没有配置Target认为是过滤所有Pointcut类别的aop
                    injectPointcutAnnotationCache = null;
                    return false;
                }
                else if (ignoreTarget.Contains(pointcut.PointClass))
                {
                    // 指定要过滤了某个PointCut的类
                    injectPointcutAnnotationCache = null;
                    return false;
                }
            }

            //先优先自身
            foreach (var attr in methodInfoCache.GetCustomAttributes())
            {
                orderIndex++;
                var isIgnore = ignoreTarget?.Contains(attr.GetType()) ?? false;
                if (isIgnore) continue;
                if (AttributeTypeArrLazy.Value.Contains(attr.GetType()))
                {
                    annotation = Tuple.Create(attr, orderIndex);
                    break;
                }
                else if (AttributeFlag == AssignableFlag.AssignableTo && AttributeType.IsInstanceOfType(attr))
                {
                    annotation = Tuple.Create(attr, orderIndex);
                    break;
                }
            }

            if (annotation == null)
            {
                // 自身没有的检查接口上有没有
                foreach (var attr in methodInfoCache.GetCustomAttributesByImplementedInterfaces<Attribute>())
                {
                    orderIndex++;
                    var isIgnore = ignoreTarget?.Contains(attr.GetType()) ?? false;
                    if (isIgnore) continue;
                    if (AttributeTypeArrLazy.Value.Contains(attr.GetType()))
                    {
                        annotation = Tuple.Create(attr, orderIndex);
                        break;
                    }
                    else if (AttributeFlag == AssignableFlag.AssignableTo && AttributeType.IsInstanceOfType(attr))
                    {
                        annotation = Tuple.Create(attr, orderIndex);
                        break;
                    }
                }
            }

            // 用打在class上的
            if (annotation == null && parentClassinjectPointcutAnnotationCache != null)
            {
                if (ignoreTarget != null
                    && ignoreTarget.Contains(parentClassinjectPointcutAnnotationCache.Item1.GetType()))
                {
                    return false;
                }

                annotation = parentClassinjectPointcutAnnotationCache;
            }

            if (annotation == null) return false;
            injectPointcutAnnotationCache = annotation;
            return true;
        }
    }

    /// <summary>
    /// 继承还是实现
    /// </summary>
    public enum AssignableFlag
    {
        /// <summary>
        /// 没有
        /// </summary>
        NONE,

        /// <summary>
        /// 也包括是我的父类
        /// </summary>
        AssignableFrom,

        /// <summary>
        /// 也包括是我的实现类
        /// </summary>
        AssignableTo
    }
}