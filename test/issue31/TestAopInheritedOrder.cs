using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac.AspectIntercepter.Advice;
using Xunit;

namespace Autofac.Annotation.Test.issue31;

/// <summary>
/// 
/// </summary>
public class TestAopInheritedOrder
{
    [Fact]
    public void Test_Type_01()
    {
        var builder = new ContainerBuilder();

        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestAopInheritedOrder).Assembly));

        var container = builder.Build();

        var test = container.Resolve<InheritedOrder1>();
        test.say();
        var rt = Inherited1TestRt.result1; // 顺序是 2 4 6 1 3 5 本方法上， 父类方法 ，接口方法， 类， 父类， 接口
        Assert.Equal(6, rt.Count);
        Assert.Equal("Inherited2Attribute.Before", rt[0]);
        Assert.Equal("Inherited4Attribute.Before", rt[1]);
        Assert.Equal("Inherited6Attribute.Before", rt[2]);
        Assert.Equal("Inherited1Attribute.Before", rt[3]);
        Assert.Equal("Inherited3Attribute.Before", rt[4]);
        Assert.Equal("Inherited5Attribute.Before", rt[5]);
    }

    [Fact]
    public void Test_Type_02()
    {
        var builder = new ContainerBuilder();

        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestAopInheritedOrder).Assembly));

        var container = builder.Build();

        var test = container.Resolve<InheritedPoint1>();
        test.say();
        // 顺序是 2 4 6 1 3 5 本方法上， 父类方法 ，接口方法， 类， 父类， 接口
        var rt = Inherited1TestRt.result2;
        Assert.Equal(6, rt.Count);
        Assert.Equal("PointCutAtt2Attribute.before", rt[0]);
        Assert.Equal("PointCutAtt4Attribute.before", rt[1]);
        Assert.Equal("PointCutAtt6Attribute.before", rt[2]);
        Assert.Equal("PointCutAtt1Attribute.before", rt[3]);
        Assert.Equal("PointCutAtt3Attribute.before", rt[4]);
        Assert.Equal("PointCutAtt5Attribute.before", rt[5]);
    }
}

public class Inherited1TestRt
{
    public static List<string> result1 = new();
    public static List<string> result2 = new();
}

////////////////////////////测试PointCut标签拦截器的在继承的执行顺序/////////////////////////////////////////////////////////////////////////////////

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public class PointCutAtt1Attribute : Attribute
{
}

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public class PointCutAtt2Attribute : Attribute
{
}

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public class PointCutAtt3Attribute : Attribute
{
}

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public class PointCutAtt4Attribute : Attribute
{
}

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public class PointCutAtt5Attribute : Attribute
{
}

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public class PointCutAtt6Attribute : Attribute
{
}

[Pointcut(NameSpace = "Autofac.Annotation.Test.issue31", AttributeType = typeof(PointCutAtt1Attribute))]
public class PointCutAttAopClass1
{
    [Before]
    public void befor()
    {
        Inherited1TestRt.result2.Add("PointCutAtt1Attribute.before");
    }
}

[Pointcut(NameSpace = "Autofac.Annotation.Test.issue31", AttributeType = typeof(PointCutAtt2Attribute))]
public class PointCutAttAopClass2
{
    [Before]
    public void befor()
    {
        Inherited1TestRt.result2.Add("PointCutAtt2Attribute.before");
    }
}

[Pointcut(NameSpace = "Autofac.Annotation.Test.issue31", AttributeType = typeof(PointCutAtt3Attribute))]
public class PointCutAttAopClass3
{
    [Before]
    public void befor()
    {
        Inherited1TestRt.result2.Add("PointCutAtt3Attribute.before");
    }
}

[Pointcut(NameSpace = "Autofac.Annotation.Test.issue31", AttributeType = typeof(PointCutAtt4Attribute))]
public class PointCutAttAopClass4
{
    [Before]
    public void befor()
    {
        Inherited1TestRt.result2.Add("PointCutAtt4Attribute.before");
    }
}

[Pointcut(NameSpace = "Autofac.Annotation.Test.issue31", AttributeType = typeof(PointCutAtt5Attribute))]
public class PointCutAttAopClass5
{
    [Before]
    public void befor()
    {
        Inherited1TestRt.result2.Add("PointCutAtt5Attribute.before");
    }
}

[Pointcut(NameSpace = "Autofac.Annotation.Test.issue31", AttributeType = typeof(PointCutAtt6Attribute))]
public class PointCutAttAopClass6
{
    [Before]
    public void befor()
    {
        Inherited1TestRt.result2.Add("PointCutAtt6Attribute.before");
    }
}

[Component]
[PointCutAtt1Attribute]
public class InheritedPoint1 : InheritedPoint2
{
    [PointCutAtt2Attribute]
    public override void say()
    {
    }
}

[PointCutAtt3Attribute]
public abstract class InheritedPoint2 : InheritedPointTop1
{
    [PointCutAtt4Attribute]
    public abstract void say();
}

[PointCutAtt5Attribute]
public interface InheritedPointTop1
{
    [PointCutAtt6Attribute]
    void say();
}
////////////////////////////测试advice标签拦截器的在继承的执行顺序/////////////////////////////////////////////////////////////////////////////////

public class Inherited1Attribute : AspectBefore
{
    public override Task Before(AspectContext aspectContext)
    {
        Inherited1TestRt.result1.Add("Inherited1Attribute.Before");
        return Task.CompletedTask;
    }
}

public class Inherited2Attribute : AspectBefore
{
    public override Task Before(AspectContext aspectContext)
    {
        Inherited1TestRt.result1.Add("Inherited2Attribute.Before");
        return Task.CompletedTask;
    }
}

public class Inherited3Attribute : AspectBefore
{
    public override Task Before(AspectContext aspectContext)
    {
        Inherited1TestRt.result1.Add("Inherited3Attribute.Before");
        return Task.CompletedTask;
    }
}

public class Inherited4Attribute : AspectBefore
{
    public override Task Before(AspectContext aspectContext)
    {
        Inherited1TestRt.result1.Add("Inherited4Attribute.Before");
        return Task.CompletedTask;
    }
}

public class Inherited5Attribute : AspectBefore
{
    public override Task Before(AspectContext aspectContext)
    {
        Inherited1TestRt.result1.Add("Inherited5Attribute.Before");
        return Task.CompletedTask;
    }
}

public class Inherited6Attribute : AspectBefore
{
    public override Task Before(AspectContext aspectContext)
    {
        Inherited1TestRt.result1.Add("Inherited6Attribute.Before");
        return Task.CompletedTask;
    }
}

[Component]
[Inherited1Attribute]
public class InheritedOrder1 : InheritedOrder2
{
    [Inherited2Attribute]
    public override void say()
    {
    }
}

[Inherited3Attribute]
public abstract class InheritedOrder2 : InheritedOrderTop1
{
    [Inherited4Attribute]
    public abstract void say();
}

[Inherited5Attribute]
public interface InheritedOrderTop1
{
    [Inherited6Attribute]
    void say();
}