//-----------------------------------------------------------------------
// <copyright file="TestPointCutUseAutofacRegister .cs" company="Company">
// Copyright (C) Company. All Rights Reserved.
// </copyright>
// <author>nainaigu</author>
// <create>$Date$</create>
// <summary></summary>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Autofac.AspectIntercepter.Advice;
using Xunit;
using Xunit.Abstractions;

namespace Autofac.Annotation.Test.test13;

/// <summary>
///     当用autofac自带的api注册的时候 pointcut功能也能被起作用
/// </summary>
public class TestPointCutUseAutofacRegister
{
    private readonly ITestOutputHelper _testOutputHelper;

    public TestPointCutUseAutofacRegister(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void Test0()
    {
        var builder = new ContainerBuilder();
        builder.RegisterSpring(r => r.RegisterAssembly(typeof(TestPointCutUseAutofacRegister).Assembly));
        builder.RegisterType(typeof(TestModel13_1)).InstancePerDependency();
        var container = builder.Build();
        var a1 = container.Resolve<TestModel13_1>();
        var PointCutTestResult = container.Resolve<PointCutTestResult>();
        a1.TestModel132.Say();
        _testOutputHelper.WriteLine(string.Join(",", PointCutTestResult.result2));
        Assert.Equal(4, PointCutTestResult.result2.Count);
    }

    [Fact]
    public void Test1()
    {
        var builder = new ContainerBuilder();
        builder.RegisterSpring(r => r.RegisterAssembly(typeof(TestPointCutUseAutofacRegister).Assembly));
        builder.RegisterType(typeof(TestModel13_1)).InstancePerDependency();
        var container = builder.Build();
        var a1 = container.Resolve<TestModel13_1>();
        var PointCutTestResult = container.Resolve<PointCutTestResult>();
        a1.Say();
        Assert.Equal(4, PointCutTestResult.result.Count);
    }

    [Fact]
    public void Test2()
    {
        var builder = new ContainerBuilder();
        builder.RegisterSpring(r => r.RegisterAssembly(typeof(TestPointCutUseAutofacRegister).Assembly));
        var container = builder.Build();
        var PointCutTestResult = container.Resolve<PointCutTestResult>();
        var a1 = container.Resolve<AbstractTest2>();
        a1.Test1();
        Assert.Equal(2, PointCutTestResult.result6.Count);
    }

    [Fact]
    public async Task Test3()
    {
        var builder = new ContainerBuilder();
        builder.RegisterSpring(r => r.RegisterAssembly(typeof(TestPointCutUseAutofacRegister).Assembly));
        var container = builder.Build();
        var a1 = container.Resolve<TestModel13_2>();
        await a1.SayAsync();
        var PointCutTestResult = container.Resolve<PointCutTestResult>();
        Assert.Equal(3, PointCutTestResult.result3.Count);
    }

    [Fact]
    public async Task Test4()
    {
        var builder = new ContainerBuilder();
        builder.RegisterSpring(r => r.RegisterAssembly(typeof(TestPointCutUseAutofacRegister).Assembly));
        var container = builder.Build();
        var a1 = container.Resolve<TestModel13_2>();
        await a1.SayAsync2();
        var PointCutTestResult = container.Resolve<PointCutTestResult>();
        Assert.Equal(4, PointCutTestResult.result4.Count);
    }

    [Fact]
    public async Task Test5()
    {
        var builder = new ContainerBuilder();
        builder.RegisterSpring(r => r.RegisterAssembly(typeof(TestPointCutUseAutofacRegister).Assembly));
        var container = builder.Build();
        var a1 = container.Resolve<AbstractTest2>();
        await a1.TestAsync();
        var PointCutTestResult = container.Resolve<PointCutTestResult>();
        Assert.Equal(2, PointCutTestResult.result5.Count);
    }

    [Fact]
    public void Test6()
    {
        var builder = new ContainerBuilder();
        builder.RegisterSpring(r => r.RegisterAssembly(typeof(TestPointCutUseAutofacRegister).Assembly));
        builder.RegisterType(typeof(TestModel13_3)).InstancePerDependency();
        var container = builder.Build();
        var a1 = container.Resolve<TestModel13_3>();
        a1.Say();
        var PointCutTestResult = container.Resolve<PointCutTestResult>();
        Assert.Equal(3, PointCutTestResult.result1.Count);
    }

    [Fact]
    public void Test7()
    {
        var builder = new ContainerBuilder();
        builder.RegisterSpring(r => r.RegisterAssembly(typeof(TestPointCutUseAutofacRegister).Assembly));
        builder.RegisterType(typeof(TestModel8)).InstancePerDependency();
        var container = builder.Build();
        var a1 = container.Resolve<TestModel8>();
        a1.Say();
        var PointCutTestResult = container.Resolve<PointCutTestResult>();
        Assert.Equal(6, PointCutTestResult.result7.Count);
    }
}

/// <summary>
/// 虽然没有打标签注册，
/// 同时被aspect和pointcut作用
/// </summary>
public class TestModel13_1
{
    [Autowired] private PointCutTestResult _pointCutTestResult;
    [Autowired] public TestModel13_2 TestModel132;

    [BeforeIntecepor0]
    public virtual void Say()
    {
        _pointCutTestResult.result.Add("TestModel13_1.Say");
        Console.WriteLine("hello");
    }
}

/// <summary>
/// EnableAspect 和 pointcut共同起作用
/// </summary>
[Component]
public class TestModel13_2
{
    [Autowired] private PointCutTestResult _pointCutTestResult;
    public string hello { get; set; }

    [BeforeIntecepor]
    public virtual void Say()
    {
        _pointCutTestResult.result2.Add("TestModel13_2.say");
        Console.WriteLine("hello");
    }

    public virtual async Task<string> SayAsync()
    {
        _pointCutTestResult.result3.Add("TestModel13_2.SayAsync");
        return await Task.FromResult("SayAsync");
    }

    [BeforeIntecepor3]
    public virtual async Task<string> SayAsync2()
    {
        _pointCutTestResult.result4.Add("TestModel13_2.SayAsync2");
        return await Task.FromResult("SayAsync2");
    }
}

public class BeforeIntecepor0 : AspectBefore
{
    public override Task Before(AspectContext aspectContext)
    {
        var _pointCutTestResult = aspectContext.ComponentContext.Resolve<PointCutTestResult>();
        _pointCutTestResult.result.Add("BeforeIntecepor.Before");
        return Task.CompletedTask;
    }
}

public class BeforeIntecepor : AspectBefore
{
    public override Task Before(AspectContext aspectContext)
    {
        var _pointCutTestResult = aspectContext.ComponentContext.Resolve<PointCutTestResult>();
        _pointCutTestResult.result2.Add("BeforeIntecepor.Before");
        return Task.CompletedTask;
    }
}

public class BeforeIntecepor3 : AspectBefore
{
    public override Task Before(AspectContext aspectContext)
    {
        var _pointCutTestResult = aspectContext.ComponentContext.Resolve<PointCutTestResult>();
        _pointCutTestResult.result4.Add("BeforeIntecepor3.Before");
        return Task.CompletedTask;
    }
}

[Pointcut(NameSpace = "Autofac.Annotation.Test.test13", Class = "TestModel13*")]
public class Pointcut13
{
    [Around]
    public async Task Around(AspectContext context, AspectDelegate next)
    {
        var _pointCutTestResult = context.ComponentContext.Resolve<PointCutTestResult>();

        _pointCutTestResult.result.Add("Pointcut13.Around.start");
        _pointCutTestResult.result1.Add("Pointcut13.Around.start");
        _pointCutTestResult.result2.Add("Pointcut13.Around.start");
        _pointCutTestResult.result3.Add("Pointcut13.Around.start");
        _pointCutTestResult.result4.Add("Pointcut13.Around.start");
        await next(context);
        _pointCutTestResult.result.Add("Pointcut13.Around.end");
        _pointCutTestResult.result2.Add("Pointcut13.Around.end");
        _pointCutTestResult.result3.Add("Pointcut13.Around.end");
        _pointCutTestResult.result4.Add("Pointcut13.Around.end");
        _pointCutTestResult.result1.Add("Pointcut13.Around.end");
    }
}

public class BeforeIntecepor2 : AspectBefore
{
    public override Task Before(AspectContext aspectContext)
    {
        var _pointCutTestResult = aspectContext.ComponentContext.Resolve<PointCutTestResult>();
        _pointCutTestResult.result6.Add("BeforeIntecepor.Before");
        return Task.CompletedTask;
    }
}

public class BeforeIntecepor4 : AspectBefore
{
    public override Task Before(AspectContext aspectContext)
    {
        var _pointCutTestResult = aspectContext.ComponentContext.Resolve<PointCutTestResult>();
        _pointCutTestResult.result5.Add("BeforeIntecepor4.Before");
        return Task.CompletedTask;
    }
}

public abstract class AbstractTest1
{
    [Autowired] private PointCutTestResult _pointCutTestResult;

    [BeforeIntecepor2]
    public virtual void Test1()
    {
        _pointCutTestResult.result6.Add("Test1");
    }

    [BeforeIntecepor4]
    public virtual async Task<string> TestAsync()
    {
        _pointCutTestResult.result5.Add("Test1");
        return await Task.FromResult("TestAsync");
    }
}

[Component]
public class AbstractTest2 : AbstractTest1
{
    public void Test2()
    {
    }
}

[Component(AutofacScope = AutofacScope.SingleInstance, AutoActivate = true)]
public class PointCutTestResult
{
    public List<string> result1 { get; set; } = new List<string>();
    public List<string> result2 { get; set; } = new List<string>();
    public List<string> result3 { get; set; } = new List<string>();
    public List<string> result4 { get; set; } = new List<string>();
    public List<string> result { get; set; } = new List<string>();
    public List<string> result5 { get; set; } = new List<string>();
    public List<string> result6 { get; set; } = new List<string>();
    public List<string> result7 { get; set; } = new List<string>();
}

public abstract class TestModelBase
{
    [Autowired] private PointCutTestResult _pointCutTestResult;

    [BeforeIntecepor0]
    public virtual void Say()
    {
        _pointCutTestResult.result1.Add("TestModel13_3.Say");
        Console.WriteLine("hello");
    }
}

public class TestModel13_3 : TestModelBase
{
    public void he()
    {
    }
}

public abstract class TestModelBase2
{
    [Autowired] public PointCutTestResult _pointCutTestResult;

    public virtual void Say2()
    {
        _pointCutTestResult.result7.Add("TestModel13_4.Say2");
    }
}

public class TestModel8 : TestModelBase2
{
    public virtual void Say()
    {
        _pointCutTestResult.result7.Add("TestModel13_4.Say");
        Say2();
        Console.WriteLine("hello");
    }
}

[Pointcut(NameSpace = "Autofac.Annotation.Test.test13", Class = "TestModel8")]
public class Pointcut14
{
    [Around]
    public async Task Around(AspectContext context, AspectDelegate next)
    {
        var _pointCutTestResult = context.ComponentContext.Resolve<PointCutTestResult>();

         _pointCutTestResult.result7.Add("Pointcut13.Around.start");
        await next(context);
        _pointCutTestResult.result7.Add("Pointcut13.Around.end");
    }
    
    
    [Before]
    public void Before()
    {
        Console.WriteLine("PointcutTest1.Before");

    }

    [After]
    public void After()
    {
        Console.WriteLine("PointcutTest1.After");

    }

    [AfterReturn(Returing = "value1")]
    public void AfterReturn(object value1)
    {
        Console.WriteLine("PointcutTest1.AfterReturn");
    }
}