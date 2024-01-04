using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac.AspectIntercepter.Advice;
using Xunit;

namespace Autofac.Annotation.Test.issue38;

/// <summary>
/// <author>nainaigu</author>
/// <create>2023-11-20</create>
/// </summary>
public class GenericMethodTest
{
    public static List<string> Rt1 = new List<string>();
    public static List<string> Rt2 = new List<string>();


    public static List<string> Rt3 = new List<string>();
    public static List<string> Rt4 = new List<string>();
    public static List<string> Rt5 = new List<string>();
    public static List<string> Rt6 = new List<string>();
    public static List<string> Rt7 = new List<string>();
    public static List<string> Rt8 = new List<string>();

    [Fact]
    public void Test()
    {
        var builder = new ContainerBuilder();

        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(GenericMethodTest).Assembly));

        var _container = builder.Build();

        var h = _container.Resolve<GenericHost<int>>();

        h.Say();

        Assert.Equal(3, Rt1.Count);
        Assert.Equal("Say", Rt1[1]);

        h.Say2();
        Assert.Equal(3, Rt2.Count);
        Assert.Equal("SayT", Rt2[1]);


        Rt1.Clear();
        Rt2.Clear();
        var h2 = _container.Resolve<GenericHost<string>>();

        h2.Say();

        Assert.Equal(3, Rt1.Count);
        Assert.Equal("Say", Rt1[1]);

        h2.Say2();
        Assert.Equal(3, Rt2.Count);
        Assert.Equal("SayT", Rt2[1]);
    }


    [Fact]
    public void Test11()
    {
        var builder = new ContainerBuilder();

        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(GenericMethodTest).Assembly));

        var _container = builder.Build();

        var h = _container.Resolve<GenericHost2>();


        h.Say2<int>();
        Assert.Equal(3, Rt3.Count);
        Assert.Equal("SayT", Rt3[1]);
    }

    [Fact]
    public void Test12()
    {
        var builder = new ContainerBuilder();

        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(GenericMethodTest).Assembly));

        var _container = builder.Build();

        var h = _container.Resolve<GenericHost3<long>>();


        long a = h.Say2<int>(1L);
        Assert.Equal(1L, a);
        Assert.Equal(3, Rt4.Count);
        Assert.Equal("SayT", Rt4[1]);

        var b = h.Say3<string>("1");

        Assert.Equal(3, Rt5.Count);
        Assert.Equal("SayT", Rt5[1]);
    }


    [Fact]
    public void Test13()
    {
        var builder = new ContainerBuilder();

        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(GenericMethodTest).Assembly));

        var _container = builder.Build();

        var h = _container.Resolve<PointCutHost1<int>>();

        h.Say();

        Assert.Equal(3, Rt6.Count);
        Assert.Equal("Say", Rt6[1]);

        h.Say2();
        Assert.Equal(3, Rt7.Count);
        Assert.Equal("SayT", Rt7[1]);


        Rt6.Clear();
        Rt7.Clear();

        var h2 = _container.Resolve<PointCutHost1<string>>();

        h2.Say();

        Assert.Equal(3, Rt6.Count);
        Assert.Equal("Say", Rt6[1]);

        h2.Say2();
        Assert.Equal(3, Rt7.Count);
        Assert.Equal("SayT", Rt7[1]);
    }

    [Fact]
    public void Test14()
    {
        var builder = new ContainerBuilder();

        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(GenericMethodTest).Assembly));

        var _container = builder.Build();

        var h = _container.Resolve<PointCutHost2>();


        h.Say2<int>();
        Assert.Equal(3, Rt8.Count);
        Assert.Equal("SayT", Rt8[1]);
    }
}

[Component]
// 泛型类里面的Method.MetaToken 不会因为泛型变化而改变
public class GenericHost<K>
{
    public GenericHost()
    {
    }

    [TracingArround]
    // ok
    public virtual void Say()
    {
        GenericMethodTest.Rt1.Add("Say");
    }

    [TracingArround2]
    public virtual K Say2()
    {
        GenericMethodTest.Rt2.Add("SayT");
        return default(K);
    }
}

[Component]
public class GenericHost2
{
    [TracingArround3]
    public virtual T Say2<T>()
    {
        GenericMethodTest.Rt3.Add("SayT");
        return default(T);
    }
}

[Component]
public class GenericHost3<T2>
{
    [TracingArround4]
    public virtual T2 Say2<T>(T2 tt)
    {
        GenericMethodTest.Rt4.Add("SayT");
        return tt;
    }

    [TracingArround5]
    public virtual List<T2> Say3<K>(K tt)
    {
        GenericMethodTest.Rt5.Add("SayT");
        return new List<T2>();
    }
}

public class TracingArround : AspectArround
{
    public override async Task OnInvocation(AspectContext aspectContext, AspectDelegate _next)
    {
        GenericMethodTest.Rt1.Add("around start");
        await _next(aspectContext);
        GenericMethodTest.Rt1.Add("around end");
    }
}

public class TracingArround2 : AspectArround
{
    public override async Task OnInvocation(AspectContext aspectContext, AspectDelegate _next)
    {
        GenericMethodTest.Rt2.Add("around start");
        await _next(aspectContext);
        GenericMethodTest.Rt2.Add("around end");
    }
}

public class TracingArround3 : AspectArround
{
    public override async Task OnInvocation(AspectContext aspectContext, AspectDelegate _next)
    {
        GenericMethodTest.Rt3.Add("around start");
        await _next(aspectContext);
        GenericMethodTest.Rt3.Add("around end");
    }
}

public class TracingArround4 : AspectArround
{
    public override async Task OnInvocation(AspectContext aspectContext, AspectDelegate _next)
    {
        GenericMethodTest.Rt4.Add("around start");
        await _next(aspectContext);
        GenericMethodTest.Rt4.Add("around end");
    }
}

public class TracingArround5 : AspectArround
{
    public override async Task OnInvocation(AspectContext aspectContext, AspectDelegate _next)
    {
        GenericMethodTest.Rt5.Add("around start");
        await _next(aspectContext);
        GenericMethodTest.Rt5.Add("around end");
    }
}

[Pointcut(AttributeType = typeof(ForGenericPointCutTest1Attribute))]
public class GenericPointCutTest1
{
    [Around]
    public async Task Around(AspectContext context, AspectDelegate next)
    {
        GenericMethodTest.Rt6.Add("before");
        await next(context);
        GenericMethodTest.Rt6.Add("after");
    }
}

[Pointcut(AttributeType = typeof(ForGenericPointCutTest2Attribute))]
public class GenericPointCutTest2
{
    [Around]
    public async Task Around(AspectContext context, AspectDelegate next)
    {
        GenericMethodTest.Rt7.Add("before");
        await next(context);
        GenericMethodTest.Rt7.Add("after");
    }
}

[Pointcut(AttributeType = typeof(ForGenericPointCutTest3Attribute))]
public class GenericPointCutTest3
{
    [Around]
    public async Task Around(AspectContext context, AspectDelegate next)
    {
        GenericMethodTest.Rt8.Add("before");
        await next(context);
        GenericMethodTest.Rt8.Add("after");
    }
}

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public class ForGenericPointCutTest1Attribute : Attribute
{
}

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public class ForGenericPointCutTest2Attribute : Attribute
{
}

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public class ForGenericPointCutTest3Attribute : Attribute
{
}

[Component]
public class PointCutHost1<K>
{
    [ForGenericPointCutTest1]
    public virtual void Say()
    {
        GenericMethodTest.Rt6.Add("Say");
    }


    [ForGenericPointCutTest2]
    public virtual K Say2()
    {
        GenericMethodTest.Rt7.Add("SayT");
        return default(K);
    }
}

[Component]
public class PointCutHost2
{
    [ForGenericPointCutTest3]
    public virtual T Say2<T>()
    {
        GenericMethodTest.Rt8.Add("SayT");
        return default(T);
    }
}