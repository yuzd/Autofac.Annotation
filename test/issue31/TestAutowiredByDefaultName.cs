//-----------------------------------------------------------------------
// <copyright file="TestAutowiredByDefaultName .cs" company="Company">
// Copyright (C) Company. All Rights Reserved.
// </copyright>
// <author>nainaigu</author>
// <create>$Date$</create>
// <summary></summary>
//-----------------------------------------------------------------------

using Xunit;

namespace Autofac.Annotation.Test.issue31;

public class TestAutowiredByDefaultName
{
    [Fact]
    public void Test_Type_01()
    {
        var builder = new ContainerBuilder();

        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestAutowiredByDefaultName).Assembly));

        var container = builder.Build();

        var test = container.Resolve<TestEntryClass>();

        Assert.IsType<AutowiredByDefaultName1>(test.test1);
        Assert.IsType<AutowiredByDefaultName2>(test.test2);
    }

    [Fact]
    public void Test_Type_02()
    {
        var builder = new ContainerBuilder();

        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestAutowiredByDefaultName).Assembly));

        var container = builder.Build();

        var test = container.Resolve<TestEntryClass1>();

        Assert.IsType<AutowiredByDefaultName4>(test.test1);
        Assert.IsType<AutowiredByDefaultName5>(test.test2);
    }

    [Fact]
    public void Test_Type_03()
    {
        var builder = new ContainerBuilder();

        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestAutowiredByDefaultName).Assembly));

        var container = builder.Build();

        var test = container.Resolve<TestEntryClass2>();

        Assert.IsType<AutowiredByDefaultName7>(test.test1);
    }

    [Fact]
    public void Test_Type_04()
    {
        var builder = new ContainerBuilder();

        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestAutowiredByDefaultName).Assembly));

        var container = builder.Build();

        var test = container.Resolve<TestEntryClass3>();

        Assert.IsType<AutowiredByDefaultName9>(test.test1.GetObject());
    }
}

public interface IAutowiredByDefaultName1
{
}

/// <summary>
/// 没有指定key 默认用className作为key
/// </summary>
[Component]
public class AutowiredByDefaultName1 : IAutowiredByDefaultName1
{
}

/// <summary>
/// 没有指定key 默认用className作为key
/// </summary>
[Component]
public class AutowiredByDefaultName2 : IAutowiredByDefaultName1
{
}

[Component]
public class TestEntryClass
{
    [Autowired("AutowiredByDefaultName1")] public IAutowiredByDefaultName1 test1 { get; set; }

    [Autowired("AutowiredByDefaultName2")] public IAutowiredByDefaultName1 test2 { get; set; }
}

public interface IAutowiredByDefaultName3
{
}

[Component("test1")]
public class AutowiredByDefaultName4 : IAutowiredByDefaultName3
{
}

[Component("test2")]
public class AutowiredByDefaultName5 : IAutowiredByDefaultName3
{
}

[Component]
public class TestEntryClass1
{
    [Autowired("test1")] public IAutowiredByDefaultName3 test1 { get; set; }

    [Autowired("test2")] public IAutowiredByDefaultName3 test2 { get; set; }
}

public interface IAutowiredByDefaultName4
{
}

[Component]
public class AutowiredByDefaultName6 : IAutowiredByDefaultName4
{
}

[Component]
public class AutowiredByDefaultName7 : IAutowiredByDefaultName4
{
}

[Component]
public class TestEntryClass2
{
    // 从appsettings.json里面动态获取
    [Autowired("${Issue31Type}")] public IAutowiredByDefaultName4 test1 { get; set; }
}

public interface IAutowiredByDefaultName5
{
}

[Component]
public class AutowiredByDefaultName8 : IAutowiredByDefaultName5
{
}

[Component]
public class AutowiredByDefaultName9 : IAutowiredByDefaultName5
{
}

[Component]
public class TestEntryClass3
{
    // 从appsettings.json里面动态获取
    [Autowired("${Issue31Type2}")] public ObjectFactory<IAutowiredByDefaultName5> test1 { get; set; }
}