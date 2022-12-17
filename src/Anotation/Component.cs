using System;

namespace Autofac.Annotation
{
    /// <summary>
    ///     会扫描有该注解的类 自动装配到autofac容器内
    ///     只能打在class上面 打在abstract不支持会被忽略
    ///     允许打多个 如果打多个有重复的话会覆盖
    ///     打在父类上子类没打的话子类就获取不到
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class Component : Attribute
    {
        /// <summary>
        ///     A Boolean indicating if the component should auto-activate.
        ///     true的话 autofac容器在构建后自动触发该类的初始化
        /// </summary>
        public bool AutoActivate { get; set; }

        /// <summary>
        ///     A Boolean indicating whether property (setter) injection for the component should be enabled.
        /// </summary>
        public bool InjectProperties { get; set; } = true;

        /// <summary>
        ///     属性自动装配的类型
        /// </summary>
        public InjectPropertyType InjectPropertyType { get; set; } = InjectPropertyType.Autowired;

        /// <summary>
        ///     作用域
        /// </summary>
        public AutofacScope AutofacScope { get; set; } = AutofacScope.Default;

        /// <summary>
        ///     如果设置值为external代表需要自己管理dispose
        /// </summary>
        public Ownership Ownership { get; set; }

        /// <summary>
        ///     指定拦截器类型，默认为class类型拦截器，被拦截的方法必须指定为virtual
        ///     如果指定了的话就无法被PointCut或者Aspect拦截
        /// </summary>
        public Type Interceptor { get; set; }

        /// <summary>
        ///     拦截器类型
        /// </summary>
        public InterceptorType InterceptorType { get; set; } = InterceptorType.Class;

        /// <summary>
        ///     开启拦截器代理 从4.0.7版本开始不需要显示设定
        /// </summary>
        [Obsolete("auto detect this from version 4.0.7")]
        public bool EnableAspect { get; set; }

        /// <summary>
        /// 是否Pointcut要拦截继承(比如抽象父类)的方法 默认为true
        /// </summary>
        public bool EnablePointcutInherited { get; set; } = true;

        /// <summary>
        ///     如果同一个类型的拦截器有多个 可以指定Key
        /// </summary>
        public string InterceptorKey { get; set; }

        /// <summary>
        ///     被创建后执行的方法
        /// </summary>
        public string InitMethod { get; set; }

        /// <summary>
        ///     被Release时执行的方法
        /// </summary>
        public string DestroyMethod { get; set; }

        /// <summary>
        ///     注册类型
        /// </summary>
        internal RegisterType RegisterType { get; set; } = RegisterType.Compoment;

        #region Constructor

        /// <summary>
        ///     构造函数
        /// </summary>
        public Component()
        {
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="_service"></param>
        public Component(Type _service)
        {
            Service = _service;
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="_service"></param>
        /// <param name="key"></param>
        public Component(Type _service, string key) : this(_service)
        {
            Key = key;
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="key"></param>
        public Component(string key)
        {
            Key = key;
        }

        #endregion

        #region Services

        /// <summary>
        ///     注册的类型
        ///     如果为null则注册为本身类型
        /// </summary>
        public Type[] Services { get; set; }

        /// <summary>
        ///     注册单个的类型
        /// </summary>
        public Type Service { get; }

        /// <summary>
        ///     注册单个的key
        /// </summary>
        public string Key { get; }

        /// <summary>
        ///     是否不允许被代理 比如Pointcut的类就不能被代理
        /// </summary>
        public bool NotUseProxy { get; set; }


        /// <summary>
        ///     自定义注册顺序 越小越先注册 但是注意 相同的类型谁最后注册就会拿resolve谁
        ///     因为autofac是允许重复注册的  以最后一次注册为准
        ///     注意： 在设置AutoActivate=true的场景，orderIndex越大的越先被初始化
        /// </summary>
        public int OrderIndex { get; set; }

        /// <summary>
        ///     注册key 在同一个类型注册多个的时候就需要用到key来做区分
        /// </summary>
        public string[] Keys { get; set; }

        #endregion
    }

    /// <summary>
    ///     拦截器类型
    /// </summary>
    public enum InterceptorType
    {
        /// <summary>
        ///     使用接口模式 自己指定拦截器
        /// </summary>
        Interface,

        /// <summary>
        ///     使用class的虚方法模式 自己指定拦截器
        /// </summary>
        Class
    }

    /// <summary>
    ///     自动注册属性类型
    /// </summary>
    public enum InjectPropertyType
    {
        /// <summary>
        ///     代表打了Autowired标签的才会装配
        /// </summary>
        Autowired,

        /// <summary>
        ///     代表全部自动装配
        /// </summary>
        ALL
    }
}