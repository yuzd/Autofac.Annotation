using System;
using System.Collections.Generic;
using Xunit;

namespace Autofac.Annotation.Test.issue31;

/// <summary>
/// 
/// </summary>
public class TestPointcutInject
{
    [Fact]
    public void Test_Type_01()
    {
        var builder = new ContainerBuilder();

        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestPointcutInject).Assembly));
        var container = builder.Build();
        var rt = container.Resolve<PointcutInject1>();
        rt.say();
        Assert.Equal(2, PointcutInjectRt.result1.Count);
        Assert.Equal("PointcutInject1Attribute", PointcutInjectRt.result1[0]);
        Assert.Equal("PointcutInject2Attribute", PointcutInjectRt.result1[1]);
    }

    [Fact]
    public void Test_Type_02()
    {
        var builder = new ContainerBuilder();

        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestPointcutInject).Assembly));
        var container = builder.Build();
        var rt = container.Resolve<PointcutInject2>();
        rt.say();
        Assert.Equal(2, PointcutInjectRt.result2.Count);
        Assert.Equal("PointcutInjectParentAttribute", PointcutInjectRt.result2[0]);
        Assert.Equal("PointcutInjectChildAttribute", PointcutInjectRt.result2[1]);
    }
}

public class PointcutInjectRt
{
    public static List<string> result1 = new();
    public static List<string> result2 = new();
}

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public class PointcutInject1Attribute : Attribute
{
}

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public class PointcutInject2Attribute : Attribute
{
}

[Pointcut(NameSpace = "Autofac.Annotation.Test.issue31", AttributeType = typeof(PointcutInject1Attribute))]
[Pointcut(NameSpace = "Autofac.Annotation.Test.issue31", AttributeType = typeof(PointcutInject2Attribute))]
public class IPointcutInjectAopClass1
{
    // 支持注入 当前Pointcut注解信息进来
    [Before]
    public void befor(Pointcut pointcut)
    {
        PointcutInjectRt.result1.Add(pointcut.AttributeType.Name);
    }
}

[Component]
public class PointcutInject1
{
    [PointcutInject1Attribute]
    [PointcutInject2Attribute]
    public virtual void say()
    {
    }
}

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public class PointcutInjectParentAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public class PointcutInjectChildAttribute : PointcutInjectParentAttribute
{
}

[Pointcut(NameSpace = "Autofac.Annotation.Test.issue31", AttributeType = typeof(PointcutInjectChildAttribute))]
[Pointcut(NameSpace = "Autofac.Annotation.Test.issue31", AttributeType = typeof(PointcutInjectParentAttribute))]
public class IPointcutInjectAopClass2
{
    // 支持注入 当前Attribute注解信息进来
    [Before]
    public void befor(Attribute attribute)
    {
        PointcutInjectRt.result2.Add(attribute.GetType().Name);
    }
}

[Component]
public class PointcutInject2
{
    [PointcutInjectParentAttribute]
    [PointcutInjectChildAttribute]
    public virtual void say()
    {
    }
}