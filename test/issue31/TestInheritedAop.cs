//-----------------------------------------------------------------------
// <copyright file="ApiPationInfoController .cs" company="Company">
// Copyright (C) Company. All Rights Reserved.
// </copyright>
// <author>nainaigu</author>
// <create>$Date$</create>
// <summary></summary>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac.AspectIntercepter.Advice;
using Xunit;

namespace Autofac.Annotation.Test.issue31;

public class TestInheritedAop
{
    [Fact]
    public void Test_Type_01()
    {
        var builder = new ContainerBuilder();

        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestInheritedAop).Assembly));

        var container = builder.Build();

        var test = container.Resolve<ApiPationInfoController>();

        Assert.NotNull(test.test3);

        test.test();

        Assert.Equal(2, ApiPationInfoController.result.Count);
        Assert.Equal(3, ApiPationInfoController.result3.Count);
    }

    [Fact]
    public async Task Test_Type_02()
    {
        var builder = new ContainerBuilder();

        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestInheritedAop).Assembly));

        var container = builder.Build();

        var test = container.Resolve<ApiPationInfoController>();

        Assert.NotNull(test.test3);

        await test.testAsync();

        Assert.Equal(2, ApiPationInfoController.result2.Count);
    }

    [Fact]
    public async Task Test_Type_03()
    {
        var builder = new ContainerBuilder();

        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestInheritedAop).Assembly));

        var container = builder.Build();

        var test = container.Resolve<ApiPationInfoController>();

        Assert.NotNull(test.test3);

        await test.testValueAsync();

        Assert.Equal(2, ApiPationInfoController.result4.Count);
        Assert.Equal(3, ApiPationInfoController.result5.Count);
    }

    [Fact]
    public void Test_Type_04()
    {
        var builder = new ContainerBuilder();

        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestInheritedAop).Assembly));

        var container = builder.Build();

        var test = container.Resolve<ApiPationInfoController>();

        Assert.NotNull(test.AbTest1);

        test.test2();

        Assert.Equal(2, ApiPationInfoController.result6.Count);
    }

    [Fact]
    public void Test_Type_05()
    {
        var builder = new ContainerBuilder();

        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestInheritedAop).Assembly));

        var container = builder.Build();

        var test = container.Resolve<ApiPationInfoController>();

        Assert.NotNull(test.AbTest1);

        test.test2Virtual();

        Assert.Equal(2, ApiPationInfoController.result7.Count);
    }

    [Fact]
    public void Test_Type_06()
    {
        var builder = new ContainerBuilder();

        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestInheritedAop).Assembly));

        var container = builder.Build();

        var test = container.Resolve<ApiPationInfoController>();

        Assert.NotNull(test.Test33);

        test.Test3Send();

        Assert.Equal(2, ApiPationInfoController.result8.Count);
    }

    [Fact]
    public void Test_Type_07()
    {
        var builder = new ContainerBuilder();

        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestInheritedAop).Assembly));

        var container = builder.Build();

        var test = container.Resolve<ApiPationInfoController>();

        Assert.NotNull(test.Test4);

        test.Test4Send();

        Assert.Equal(2, ApiPationInfoController.result9.Count);
    }

    [Fact]
    public void Test_Type_08()
    {
        var builder = new ContainerBuilder();

        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestInheritedAop).Assembly));

        var container = builder.Build();

        var test = container.Resolve<ApiPationInfoController>();

        Assert.NotNull(test.Test5);

        test.Test5Send();

        Assert.Equal(2, ApiPationInfoController.result10.Count);
    }
}

[Component]
public class ApiPationInfoController
{
    public static List<string> result = new();
    public static List<string> result2 = new();
    public static List<string> result3 = new();
    public static List<string> result4 = new();
    public static List<string> result5 = new();
    public static List<string> result6 = new();
    public static List<string> result7 = new();
    public static List<string> result8 = new();
    public static List<string> result9 = new();
    public static List<string> result10 = new();

    [Autowired("test1")] public ICommunication test3 { get; set; }
    [Autowired("test2")] public AbTest1 AbTest1 { get; set; }
    [Autowired("test3")] public ICommunication2 Test33 { get; set; }
    [Autowired("test4")] public ICommunication3 Test4 { get; set; }
    [Autowired("test5")] public ICommunication4 Test5 { get; set; }

    public void test()
    {
        test3.Send("hello");
    }

    public async Task testAsync()
    {
        await test3.SendAsync("hello");
    }

    public async ValueTask testValueAsync()
    {
        await test3.SendValueAsync("hello");
    }

    public void test2()
    {
        AbTest1.Send("hello");
    }

    public void test2Virtual()
    {
        AbTest1.SendVirtual("hello");
    }

    public void Test3Send()
    {
        Test33.Send("hello");
    }

    public void Test4Send()
    {
        Test4.Send("hello");
    }

    public void Test5Send()
    {
        Test5.Send("hello");
    }
}

public interface ICommunication2
{
    [Exception3Attribute]
    void Send(string data);
}

[Component("test3")]
[InterfaceInterceptor]
public class Test3 : ICommunication2
{
    public void Send(string data)
    {
        ApiPationInfoController.result8.Add("data");
    }
}

public interface ICommunication3
{
    [Exception4Attribute]
    void Send(string data);
}

[Component("test4")]
public class Test4 : ICommunication3
{
    public virtual void Send(string data)
    {
        ApiPationInfoController.result9.Add("data");
    }
}

public interface ICommunication4
{
    [Exception5Attribute]
    void Send(string data);
}

public abstract class Abclass4 : ICommunication4
{
    public abstract void Send(string data);
}

[Component("test5")]
[InterfaceInterceptor]
public class Test5 : Abclass4
{
    public override void Send(string data)
    {
        ApiPationInfoController.result10.Add("data");
    }
}

public interface ICommunication
{
    void Send(string data);

    Task SendAsync(string data);
    ValueTask SendValueAsync(string data);
}

[Component("test1", AutofacScope = AutofacScope.SingleInstance, AutoActivate = true)]
public class Test1 : ICommunication
{
    public void Send(string data)
    {
        ApiPationInfoController.result.Add("data");
        ApiPationInfoController.result3.Add("data");
        Console.Write(data);
    }

    public async Task SendAsync(string data)
    {
        ApiPationInfoController.result2.Add("data");
        await Task.Delay(100);
        Console.Write(data);
    }

    public async ValueTask SendValueAsync(string data)
    {
        ApiPationInfoController.result4.Add("data");
        ApiPationInfoController.result5.Add("data");
        await Task.Delay(100);
        Console.Write(data);
    }
}

[Pointcut(NameSpace = "Autofac.Annotation.Test.issue31", Class = "Test1", Method = "Send")]
public class Issue31LogInterceptor
{
    [Before]
    public void befor()
    {
        ApiPationInfoController.result.Add("Issue31LogInterceptor.Before");
        Console.WriteLine("Issue31LogInterceptor.Before");
    }

    [Around]
    public async Task Around(AspectContext context, AspectDelegate next)
    {
        ApiPationInfoController.result3.Add("Issue31LogInterceptor.Around1");
        await next(context);
        ApiPationInfoController.result3.Add("Issue31LogInterceptor.Around2");
    }
}

[Pointcut(NameSpace = "Autofac.Annotation.Test.issue31", Class = "Test1", Method = "SendAsync")]
public class Issue31LogInterceptor2
{
    [Before]
    public void befor()
    {
        ApiPationInfoController.result2.Add("Issue31LogInterceptor2.Before");
        Console.WriteLine("Issue31LogInterceptor2.Before");
    }
}

[Pointcut(NameSpace = "Autofac.Annotation.Test.issue31", Class = "Test1", Method = "SendValueAsync")]
public class Issue31LogInterceptor3
{
    [Before]
    public void befor()
    {
        ApiPationInfoController.result4.Add("Issue31LogInterceptor3.Before");
        Console.WriteLine("Issue31LogInterceptor3.Before");
    }

    [Around]
    public async Task Around(AspectContext context, AspectDelegate next)
    {
        ApiPationInfoController.result5.Add("Issue31LogInterceptor3.Around1");
        await next(context);
        ApiPationInfoController.result5.Add("Issue31LogInterceptor3.Around2");
    }
}

////////////////////////////////////下面是针对Attribute的AOP测试/////////////////////////////////////////////////////////////////////////////////////////////
public abstract class Test2
{
    [ExceptionAttribute]
    public abstract void Send(string data);

    [Exception2Attribute]
    public virtual void SendVirtual(string data)
    {
    }
}

[Component("test2", AutofacScope = AutofacScope.SingleInstance, AutoActivate = true)]
public class AbTest1 : Test2
{
    public override void Send(string data)
    {
        ApiPationInfoController.result6.Add(data);
    }

    public override void SendVirtual(string data)
    {
        ApiPationInfoController.result7.Add(data);
    }
}

public class ExceptionAttribute : AspectAfter
{
    public override async Task After(AspectContext aspectContext, object result)
    {
        ApiPationInfoController.result6.Add("ExceptionAttribute.After");
        await Task.Delay(100);
    }
}

public class Exception2Attribute : AspectAfter
{
    public override async Task After(AspectContext aspectContext, object result)
    {
        ApiPationInfoController.result7.Add("Exception2Attribute.After");
        await Task.Delay(100);
    }
}

public class Exception3Attribute : AspectAfter
{
    public override async Task After(AspectContext aspectContext, object result)
    {
        ApiPationInfoController.result8.Add("Exception3Attribute.After");
        await Task.Delay(100);
    }
}

public class Exception4Attribute : AspectAfter
{
    public override async Task After(AspectContext aspectContext, object result)
    {
        ApiPationInfoController.result9.Add("Exception4Attribute.After");
        await Task.Delay(100);
    }
}

public class Exception5Attribute : AspectAfter
{
    public override async Task After(AspectContext aspectContext, object result)
    {
        ApiPationInfoController.result10.Add("Exception4Attribute.After");
        await Task.Delay(100);
    }
}