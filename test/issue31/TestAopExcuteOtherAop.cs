using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Autofac.Annotation.Test.issue31;

public class TestAopExcuteOtherAop
{
    [Fact]
    public async Task Test_Type_01()
    {
        var builder = new ContainerBuilder();
        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestAopExcuteOtherAop).Assembly));
        var container = builder.Build();
        var test = container.Resolve<AopNumberController>();
        await test.delete();
        Assert.Equal(4, TestAopExcuteOtherAopRt.result1.Count);
    }

    [Fact]
    public async Task Test_Type_02()
    {
        var builder = new ContainerBuilder();
        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestAopExcuteOtherAop).Assembly));
        var container = builder.Build();
        var test = container.Resolve<AopNumberController2>();
        await test.delete();
        Assert.Equal(6, TestAopExcuteOtherAopRt.result2.Count);
    }
}

public class TestAopExcuteOtherAopRt
{
    public static List<string> result1 = new();
    public static List<string> result2 = new();
}

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public class AopNumber1Attribute : Attribute
{
}

[Component]
public class Communication2
{
    // [AopNumber1Attribute]
    public virtual async Task send()
    {
        await Task.Delay(1000);
        TestAopExcuteOtherAopRt.result1.Add("Communication2.send");
    }
}

[Pointcut(NameSpace = "Autofac.Annotation.Test.issue31", AttributeType = typeof(AopNumber1Attribute))]
public class AopNumber1AopClass
{
    [Autowired] public Communication2 Communication2 { get; set; }

    [Around]
    public async Task around(AspectContext context, AspectDelegate next)
    {
        TestAopExcuteOtherAopRt.result1.Add("AopNumber1AopClass.around-start");
        await next(context);
        // 调用async方法
        await Communication2.send();
        TestAopExcuteOtherAopRt.result1.Add("AopNumber1AopClass.around-end");
    }
}

[Component]
public class AopNumberController
{
    // 会走aop：AopNumber1AopClass
    [AopNumber1Attribute]
    public virtual Task delete()
    {
        TestAopExcuteOtherAopRt.result1.Add("AopNumberController.delete");
        return Task.CompletedTask;
    }
}

/////////////////////////////////aop里面调用aop/////////////////////////////////////////////////////////////////////////

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public class AopNumber2Attribute : Attribute
{
}

[Pointcut(NameSpace = "Autofac.Annotation.Test.issue31", AttributeType = typeof(AopNumber2Attribute))]
public class AopNumber2AopClass
{
    [Autowired] //这也是个aop
    public AopNumberController3 AopNumberController3 { get; set; }

    [Around]
    public async Task around(AspectContext context, AspectDelegate next)
    {
        TestAopExcuteOtherAopRt.result2.Add("AopNumber2AopClass.around-start");
        await next(context);
        // aop
        await AopNumberController3.delete();
        TestAopExcuteOtherAopRt.result2.Add("AopNumber2AopClass.around-end");
    }
}

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public class AopNumber3Attribute : Attribute
{
}

[Pointcut(NameSpace = "Autofac.Annotation.Test.issue31", AttributeType = typeof(AopNumber3Attribute))]
public class AopNumber3AopClass
{
    [Around]
    public async Task around(AspectContext context, AspectDelegate next)
    {
        TestAopExcuteOtherAopRt.result2.Add("AopNumber3AopClass.around-start");
        await next(context);
        TestAopExcuteOtherAopRt.result2.Add("AopNumber3AopClass.around-start");
    }
}

[Component]
public class AopNumberController2
{
    // 会走aop：AopNumber1AopClass
    [AopNumber2Attribute]
    public virtual Task delete()
    {
        TestAopExcuteOtherAopRt.result2.Add("AopNumberController2.delete");
        return Task.CompletedTask;
    }
}

[Component]
public class AopNumberController3
{
    // 会走aop：AopNumber3AopClass
    [AopNumber3Attribute]
    public virtual async Task delete()
    {
        await Task.Delay(1000);
        TestAopExcuteOtherAopRt.result2.Add("AopNumberController3.delete");
    }
}