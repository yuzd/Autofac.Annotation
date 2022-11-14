using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Autofac.Annotation.Test.issue31;

/// <summary>
///  接口上和方法上都打AOP 会崩溃？
/// </summary>
public class TestInterfaceAop
{
    [Fact]
    public void Test_Type_01()
    {
        var builder = new ContainerBuilder();

        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestInterfaceAop).Assembly));

        var container = builder.Build();

        var test = container.Resolve<InterfaceAopRt>();
        test.say();

        Assert.Equal(2, test.result1.Count);
    }

    [Fact]
    public async Task Test_Type_02()
    {
        var builder = new ContainerBuilder();

        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestInterfaceAop).Assembly));

        var container = builder.Build();

        var test = container.Resolve<InterfaceAopRt>();
        await test.sayAsync();

        Assert.Equal(2, test.result2.Count);
    }
}

[Component(AutofacScope = AutofacScope.SingleInstance)]
public class InterfaceAopRt
{
    public List<string> result1 = new();
    public List<string> result2 = new();

    /// <summary>
    /// 接口方式的aop
    /// </summary>
    [Autowired]
    public InterfaceAop1 InterfaceAopCls1 { get; set; }

    [Autowired]
    public InterfaceAop2 InterfaceAopCls2 { get; set; }

    public void say()
    {
        InterfaceAopCls1.say("hello");
    }

    public async Task sayAsync()
    {
        await InterfaceAopCls2.sayAsync("hello");
    }
}

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public class InterfaceAop1Attribute : Attribute
{
}

[Pointcut(NameSpace = "Autofac.Annotation.Test.issue31", AttributeType = typeof(InterfaceAop1Attribute))]
public class InterfaceAop1Class1
{
    [Autowired] public InterfaceAopRt InterfaceAopRt { get; set; }

    [Before]
    public void befor()
    {
        InterfaceAopRt.result1.Add("before");
    }
}

public interface InterfaceAop1
{
    [InterfaceAop1Attribute]
    void say(string str);
}

[Component]
public class InterfaceAopCls1 : InterfaceAop1
{
    [Autowired] public InterfaceAopRt InterfaceAopRt { get; set; }

    public void say(string str)
    {
        InterfaceAopRt.result1.Add("say");
    }
}

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public class InterfaceAop2Attribute : Attribute
{
}
[Pointcut(NameSpace = "Autofac.Annotation.Test.issue31", AttributeType = typeof(InterfaceAop2Attribute))]
public class InterfaceAop1Class2
{
    [Autowired] public InterfaceAopRt InterfaceAopRt { get; set; }
    [Around]
    public async Task around(AspectContext context, AspectDelegate next)
    {
        InterfaceAopRt.result2.Add("around");
        await next(context);
    }
}

public interface InterfaceAop2
{
    [InterfaceAop2Attribute]
    Task sayAsync(string str);
}

[Component]
public class InterfaceAopCls2 : InterfaceAop2
{
    [Autowired] public InterfaceAopRt InterfaceAopRt { get; set; }
    public async Task sayAsync(string str)
    {
        await Task.Delay(3000);
        InterfaceAopRt.result2.Add("sayAsync");
    }
}