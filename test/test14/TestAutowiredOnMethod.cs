//-----------------------------------------------------------------------
// <copyright file="TestAutowiredOnMethod .cs" company="Company">
// Copyright (C) Company. All Rights Reserved.
// </copyright>
// <author>nainaigu</author>
// <create>$Date$</create>
// <summary></summary>
//-----------------------------------------------------------------------

using Xunit;

namespace Autofac.Annotation.Test.test14;

/// <summary>
/// 
/// </summary>
public class TestAutowiredOnMethod
{
    [Fact]
    public void Test1()
    {
        var builder = new ContainerBuilder();
        builder.RegisterSpring(r => r.RegisterAssembly(typeof(AutowiredOn1).Assembly));
        var container = builder.Build();
        var a1 = container.Resolve<AutowiredOn1>();
        Assert.NotNull(a1._autowiredOn2);
    }

    [Fact]
    public void Test2()
    {
        var builder = new ContainerBuilder();
        builder.RegisterSpring(r => r.RegisterAssembly(typeof(AutowiredOn1).Assembly));
        var container = builder.Build();
        var a1 = container.Resolve<AutowiredOn3>();
        Assert.NotNull(a1._autowiredOn2);
        Assert.NotNull(a1._school);
    }

    [Fact]
    public void Test3()
    {
        var builder = new ContainerBuilder();
        builder.RegisterSpring(r => r.RegisterAssembly(typeof(AutowiredOn1).Assembly));
        var container = builder.Build();
        var a1 = container.Resolve<AutowiredOn6>();
        Assert.NotNull(a1._autiwriedBase);
        Assert.Equal("AutowiredOn5", a1._autiwriedBase.Hello);
        Assert.NotNull(a1._school);
    }
}

[Component]
public class AutowiredOn1
{
    public AutowiredOn2 _autowiredOn2;

    [Autowired]
    public void SetAutowiredOn2(AutowiredOn2 autowiredOn2)
    {
        _autowiredOn2 = autowiredOn2;
    }
}

[Component]
public class AutowiredOn2
{
    public string Hello { get; set; } = "world";
}

[Component]
public class AutowiredOn3
{
    public AutowiredOn2 _autowiredOn2;
    public string _school;

    [Autowired]
    public void SetAutowiredOn2(AutowiredOn2 autowiredOn2, [Value("${a9}")] string school)
    {
        _autowiredOn2 = autowiredOn2;
        _school = school;
    }
}

public class AutiwriedBase
{
    public string Hello { get; set; }
}

[Component("AutowiredOn4")]
public class AutowiredOn4 : AutiwriedBase
{
    public AutowiredOn4()
    {
        Hello = "AutowiredOn4";
    }
}

[Component("AutowiredOn5")]
public class AutowiredOn5 : AutiwriedBase
{
    public AutowiredOn5()
    {
        Hello = "AutowiredOn5";
    }
}

[Component]
public class AutowiredOn6
{
    public AutiwriedBase _autiwriedBase;
    public string _school;

    [Autowired]
    public void SetAutowiredOn2([Autowired("AutowiredOn5")] AutiwriedBase autiwriedBase, [Value("${a9}")] string school)
    {
        _autiwriedBase = autiwriedBase;
        _school = school;
    }
}