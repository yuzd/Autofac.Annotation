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
using System.Threading.Tasks;
using Autofac.AspectIntercepter.Advice;
using Xunit;

namespace Autofac.Annotation.Test.test13;

/// <summary>
///     当用autofac自带的api注册的时候 pointcut功能也能被起作用
/// </summary>
public class TestPointCutUseAutofacRegister
{
    [Fact]
    public void Test1()
    {
        var builder = new ContainerBuilder();
        builder.RegisterSpring(r => r.RegisterAssembly(typeof(TestPointCutUseAutofacRegister).Assembly));
        builder.RegisterType(typeof(TestModel13_1)).InstancePerDependency();
        var container = builder.Build();
        var a1 = container.Resolve<TestModel13_1>();
        a1.TestModel132.Say();
        Assert.Equal(2,TestModel13_1.result.Count);
        a1.Say();
        Assert.Equal(5,TestModel13_1.result.Count);
    }
}

public class TestModel13_1
{
    public static List<string> result = new();

    [Autowired] public TestModel13_2 TestModel132;
    
    public virtual void Say()
    {
        TestModel13_1.result.Add("2");
        Console.WriteLine("hello");
    }
}

/// <summary>
/// 因为有EnableAspect导致了pointcut无法生效，因为他们是互斥的
/// </summary>
[Component(EnableAspect = true)]
public class TestModel13_2
{
    public string hello { get; set; }

    [BeforeIntecepor]
    public virtual void Say()
    {
        TestModel13_1.result.Add("2");
        Console.WriteLine("hello");
    }
}

public class BeforeIntecepor : AspectBefore
{
    public override Task Before(AspectContext aspectContext)
    {
        TestModel13_1.result.Add("1");
        return Task.CompletedTask;
    }
}

[Pointcut(NameSpace = "Autofac.Annotation.Test.test13", Class = "TestModel13*")]
public class Pointcut13
{
    [Around]
    public async Task Around(AspectContext context, AspectDelegate next)
    {
        TestModel13_1.result.Add("1");
        await next(context);
        TestModel13_1.result.Add("3");
    }
}