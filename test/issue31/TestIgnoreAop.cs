//-----------------------------------------------------------------------
// <copyright file="TestIgnoreAop .cs" company="Company">
// Copyright (C) Company. All Rights Reserved.
// </copyright>
// <author>nainaigu</author>
// <create>$Date$</create>
// <summary></summary>
//-----------------------------------------------------------------------

using System.Threading.Tasks;
using Autofac.AspectIntercepter.Advice;
using Xunit;
using System;
using System.Collections.Generic;

namespace Autofac.Annotation.Test.issue31;

/// <summary>
///  测试忽略指定的aop
/// </summary>
public class TestIgnoreAop
{
    [Fact]
    public void Test_Type_01()
    {
        var builder = new ContainerBuilder();

        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestIgnoreAop).Assembly));

        var container = builder.Build();

        var test = container.Resolve<IgnoreAop1>();
        test.sayIgnoreExceptionAop2();
        test.say();
        test.sayIgnore();

        Assert.Equal(2, TestIgnoreAopRt.result.Count);
        Assert.Equal(1, TestIgnoreAopRt.result1.Count);
        Assert.Equal(2, TestIgnoreAopRt.result2.Count);
    }

    [Fact]
    public void Test_Type_02()
    {
        var builder = new ContainerBuilder();

        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestIgnoreAop).Assembly));

        var container = builder.Build();

        var test = container.Resolve<IgnoreAop2>();
        test.say();
        test.sayIgnore();
        test.sayIgnore2();
        test.sayIgnore3();

        Assert.Equal(2, TestIgnoreAopRt.result3.Count);
        Assert.Equal(1, TestIgnoreAopRt.result4.Count);
        Assert.Equal(2, TestIgnoreAopRt.result5.Count);
        Assert.Equal(1, TestIgnoreAopRt.result6.Count);
    }
}

public class TestIgnoreAopRt
{
    public static List<string> result = new();
    public static List<string> result1 = new();
    public static List<string> result2 = new();
    public static List<string> result3 = new();
    public static List<string> result4 = new();
    public static List<string> result5 = new();
    public static List<string> result6 = new();
}

public class ExceptionAopAttribute : AspectAfter
{
    public override async Task After(AspectContext aspectContext, object result)
    {
        TestIgnoreAopRt.result.Add("ExceptionAopAttribute.After");
        await Task.Delay(100);
    }
}

public class ExceptionAop2Attribute : AspectAfter
{
    public override async Task After(AspectContext aspectContext, object result)
    {
        TestIgnoreAopRt.result1.Add("ExceptionAop2Attribute.After");
        await Task.Delay(100);
    }
}

public class ExceptionAop3Attribute : AspectAfter
{
    public override async Task After(AspectContext aspectContext, object result)
    {
        TestIgnoreAopRt.result2.Add("ExceptionAop3Attribute.After");
        await Task.Delay(100);
    }
}

public interface InterIgnoreAop
{
    [ExceptionAopAttribute]
    void say();

    [ExceptionAop2Attribute]
    void sayIgnore();

    [ExceptionAop3Attribute]
    [ExceptionAop2Attribute]
    void sayIgnoreExceptionAop2();
}

[Component]
public class IgnoreAop1 : InterIgnoreAop
{
    public virtual void say()
    {
        TestIgnoreAopRt.result.Add("say");
    }

    [IgnoreAop]
    public virtual void sayIgnore()
    {
        TestIgnoreAopRt.result1.Add("say");
    }

    [IgnoreAop(Target = new[] { typeof(ExceptionAop2Attribute) })]
    public virtual void sayIgnoreExceptionAop2()
    {
        TestIgnoreAopRt.result2.Add("sayIgnoreExceptionAop2");
    }
}

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public class ExceptionAttIgnore1Attribute : Attribute
{
}

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public class ExceptionAttIgnore2Attribute : Attribute
{
}

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public class ExceptionAttIgnore3Attribute : Attribute
{
}

[Pointcut(NameSpace = "Autofac.Annotation.Test.issue31", AttributeType = typeof(ExceptionAttIgnore1Attribute))]
public class TestIgnoreAopClass1
{
    [Before]
    public void befor()
    {
        TestIgnoreAopRt.result3.Add("before");
    }
}

[Pointcut(NameSpace = "Autofac.Annotation.Test.issue31", AttributeType = typeof(ExceptionAttIgnore2Attribute))]
public class TestIgnoreAopClass2
{
    [Before]
    public void befor()
    {
        TestIgnoreAopRt.result4.Add("before");
        TestIgnoreAopRt.result5.Add("before");
    }
}

[Pointcut(NameSpace = "Autofac.Annotation.Test.issue31", AttributeType = typeof(ExceptionAttIgnore3Attribute))]
public class TestIgnoreAopClass3
{
    [Before]
    public void befor()
    {
        TestIgnoreAopRt.result5.Add("before");
    }
}

public interface InterIgnoreAop2
{
    [ExceptionAttIgnore1Attribute]
    void say();

    [ExceptionAttIgnore2Attribute]
    [ExceptionAttIgnore1Attribute]
    void sayIgnore();

    [ExceptionAttIgnore2Attribute]
    [ExceptionAttIgnore3Attribute]
    void sayIgnore2();

    [ExceptionAttIgnore1Attribute]
    [ExceptionAttIgnore2Attribute]
    [ExceptionAttIgnore3Attribute]
    void sayIgnore3();
}

[Component]
public class IgnoreAop2 : InterIgnoreAop2
{
    public virtual void say()
    {
        TestIgnoreAopRt.result3.Add("say");
    }

    [IgnoreAop]
    public virtual void sayIgnore()
    {
        TestIgnoreAopRt.result4.Add("sayIgnore");
    }

    [IgnoreAop(Target = new[] { typeof(ExceptionAttIgnore2Attribute) })]
    public virtual void sayIgnore2()
    {
        TestIgnoreAopRt.result5.Add("sayIgnore2");
    }

    [IgnoreAop(Target = new[] { typeof(TestIgnoreAopClass3), typeof(TestIgnoreAopClass2), typeof(TestIgnoreAopClass1) })]
    public void sayIgnore3()
    {
        TestIgnoreAopRt.result6.Add("sayIgnore3");
    }
}