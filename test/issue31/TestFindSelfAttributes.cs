using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Autofac.Annotation.Test.issue31;

public class TestFindSelfAttributes
{
    [Fact]
    public void Test_Type_01()
    {
        var builder = new ContainerBuilder();
        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestFindSelfAttributes).Assembly));
        var container = builder.Build();
        var test = container.Resolve<FindSelfTest1>();
        var dd = container.Resolve<FindSelfTestRt>();
        container.Dispose();
        Assert.Equal("init2,destory2", string.Join(",", dd.result1));
    }

    [Fact]
    public void Test_Type_02()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<FindSelfTestRt>().AsSelf().SingleInstance();
        builder.RegisterType<FindSelfTest2>().AsSelf().SingleInstance().PropertiesAutowired().AutoActivate().OnActivated(r => r.Instance.init2()).OnRelease(r => r.destory2());
        var container = builder.Build();

        var test = container.Resolve<FindSelfTest2>();
        var dd = container.Resolve<FindSelfTestRt>();
        container.Dispose();
        Assert.Equal(2, dd.result2.Count);
    }

    // [Fact]
    // public void Test_Type_02()
    // {
    //     var builder = new ContainerBuilder();
    //     // autofac打标签模式
    //     builder.RegisterModule(new AutofacAnnotationModule(typeof(TestFindSelfAttributes).Assembly));
    //     Assert.Throws<InvalidOperationException>(() => builder.Build());
    // }
}

[Component(AutofacScope = AutofacScope.SingleInstance)]
public class FindSelfTestRt
{
    public List<string> result1 = new();
    public List<string> result2 = new();
    public List<string> result3 = new();
}

[Component]
public class MyComponent : Attribute
{
    [AliasFor(typeof(Component), "AutofacScope")]
    public AutofacScope AutofacScope { get; set; }

    [AliasFor(typeof(Component), "AutoActivate")]
    public bool AutoActivate { get; set; }

    [AliasFor(typeof(Component), "InitMethod")]

    public string InitMethod { get; set; }

    [AliasFor(typeof(Component), "DestroyMethod")]

    public string DestroyMethod { get; set; }

    public string Remark { get; set; }
}

[MyComponent(AutofacScope = AutofacScope.SingleInstance, AutoActivate = true, InitMethod = nameof(init2), DestroyMethod = nameof(destory2), Remark = "test2")]
public class FindSelfTest1
{
    [Autowired] public FindSelfTestRt FindSelfTestRt { get; set; }

    public void init2()
    {
        FindSelfTestRt.result1.Add("init2");
    }

    public void destory2()
    {
        FindSelfTestRt.result1.Add("destory2");
    }
}

public class FindSelfTest2
{
    [Autowired] public FindSelfTestRt FindSelfTestRt { get; set; }

    public void init2()
    {
        FindSelfTestRt.result2.Add("init2");
    }

    public void destory2()
    {
        FindSelfTestRt.result2.Add("destory2");
    }
}

[Component]
public class MyComponent2 : Attribute
{
    [AliasFor(typeof(Component), "AutoActivate")]
    public string AutoActivate1 { get; set; }
}

// [MyComponent2(AutoActivate1 = "true")]
// public class FindSelfTest2
// {
// }