using System;

namespace Autofac.Annotation
{
    /// <summary>
    /// 当bean创建后执行
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class PostConstruct : Attribute
    {
    }

    /// <summary>
    /// 当bean准备结束执行
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class PreDestory : Attribute
    {
    }
}