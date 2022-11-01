//-----------------------------------------------------------------------
// <copyright file="ApiPationInfoController .cs" company="Company">
// Copyright (C) Company. All Rights Reserved.
// </copyright>
// <author>nainaigu</author>
// <create>$Date$</create>
// <summary></summary>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac.AspectIntercepter.Advice;
using Xunit;

namespace Autofac.Annotation.Test.issue31;

using System;

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
}

[Component]
public class ApiPationInfoController
{
    public static List<string> result = new List<string>();
    public static List<string> result2 = new List<string>();
    public static List<string> result3 = new List<string>();
    public static List<string> result4 = new List<string>();
    public static List<string> result5 = new List<string>();
    public static List<string> result6 = new List<string>();
    public static List<string> result7 = new List<string>();

    [Autowired("test1")] public ICommunication test3 { get; set; }
    [Autowired("test2")] public AbTest1 AbTest1 { get; set; }

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