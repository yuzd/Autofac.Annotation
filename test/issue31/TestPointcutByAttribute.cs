//-----------------------------------------------------------------------
// <copyright file="TestPointcutByAttribute .cs" company="Company">
// Copyright (C) Company. All Rights Reserved.
// </copyright>
// <author>nainaigu</author>
// <create>$Date$</create>
// <summary></summary>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Xunit;

namespace Autofac.Annotation.Test.issue31;

/// <summary>
/// 
/// </summary>
public class TestPointcutByAttribute
{
    [Fact]
    public void Test_Type_01()
    {
        var builder = new ContainerBuilder();

        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestPointcutByAttribute).Assembly));

        var container = builder.Build();

        var test = container.Resolve<TestPointByAttCls1>();
        test.Send("hello");
        var rt = container.Resolve<TestPointByAttResult>();

        Assert.Equal(2, rt.Rt1.Count);
    }

    [Fact]
    public void Test_Type_02()
    {
        var builder = new ContainerBuilder();

        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestPointcutByAttribute).Assembly));

        var container = builder.Build();

        var test = container.Resolve<ITestPointByAttCls2>();
        test.Send2("hello");
        var rt = container.Resolve<TestPointByAttResult>();

        Assert.Single(rt.Rt2);
    }
}

[Component(AutofacScope = AutofacScope.SingleInstance)]
public class TestPointByAttResult
{
    public List<string> Rt1 { get; set; } = new();
    public List<string> Rt2 { get; set; } = new();
}

public interface TestPointByAttCls1
{
    [ExceptionAtt1Attribute]
    void Send(string data);
}

[Component]
[InterfaceInterceptor]
public class TestPointByAttCls2 : TestPointByAttCls1
{
    [Autowired] public TestPointByAttResult TestPointByAttResult { get; set; }

    // 应该要走aop
    public void Send(string data)
    {
        TestPointByAttResult.Rt1.Add(data);
    }
}

public interface ITestPointByAttCls2
{
    [ExceptionAtt2Attribute]
    void Send(string data);

    void Send2(string data);
}

[Component]
[InterfaceInterceptor]
public class TestPointByAttCls3 : ITestPointByAttCls2
{
    [Autowired] public TestPointByAttResult TestPointByAttResult { get; set; }

    // 应该要走aop
    public void Send(string data)
    {
        TestPointByAttResult.Rt2.Add(data);
    }

    // 不应该要走aop
    public void Send2(string data)
    {
        TestPointByAttResult.Rt2.Add(data);
    }
}

[Pointcut(NameSpace = "Autofac.Annotation.Test.issue31", AttributeType = typeof(ExceptionAtt1Attribute))]
public class Issue31TestAopClass
{
    [Autowired] public TestPointByAttResult TestPointByAttResult { get; set; }

    [Before]
    public void befor()
    {
        TestPointByAttResult.Rt1.Add("before");
        Console.WriteLine("Issue31TestAopClass.Before");
    }
}

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public class ExceptionAtt1Attribute : Attribute
{
}

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public class ExceptionAtt2Attribute : Attribute
{
}

[Pointcut(NameSpace = "Autofac.Annotation.Test.issue31", AttributeType = typeof(ExceptionAtt2Attribute))]
public class Issue31TestAopClass2
{
    [Autowired] public TestPointByAttResult TestPointByAttResult { get; set; }

    [Before]
    public void befor()
    {
        TestPointByAttResult.Rt2.Add("before");
        Console.WriteLine("Issue31TestAopClass2.Before");
    }
}