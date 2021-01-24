using System;

namespace Autofac.Annotation
{
    /// <summary>
    /// 配置pointCut 指定拦截类型为接口
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class InterfaceInterceptor : Attribute
    {
    }
}