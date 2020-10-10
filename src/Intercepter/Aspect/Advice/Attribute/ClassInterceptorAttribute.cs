using System;

namespace Autofac.Annotation
{
    /// <summary>
    /// 配置pointCut 指定拦截类型为class
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ClassInterceptor : Attribute
    {
    }
}