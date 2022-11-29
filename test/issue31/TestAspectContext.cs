using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac.AspectIntercepter.Advice;
using Xunit;

namespace Autofac.Annotation.Test.issue31;

public class TestAspectContext
{
    [Fact]
    public void Test_Type_01()
    {
        var builder = new ContainerBuilder();
        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestAspectContext).Assembly));
        var container = builder.Build();
        var test1 = container.Resolve<TestAspectContextCls1>();
        test1.say();
        Assert.Equal(5,TestAspectContextRt.result1.Count);
        Assert.Equal("TestAspectContextAroundAttribute",TestAspectContextRt.result1.Last());
    }

}
public class TestAspectContextRt
{
    public static List<string> result1 = new();
}

[Component]
public class TestAspectContextCls1
{
    [TestAspectContextAroundAttribute]
    [TestAspectContextBeforeAttribute]
    [TestAspectContextAfterAttribute]
    public virtual void say()
    {
        TestAspectContextRt.result1.Add("say");
    }
}

public class TestAspectContextAroundAttribute : AspectArround
{
    public override Task OnInvocation(AspectContext aspectContext, AspectDelegate _next)
    {
        TestAspectContextRt.result1.Add("AspectArround");
        // 通过AdditionalData字典来传递
        aspectContext.AdditionalData["test"] = "TestAspectContextAroundAttribute";
        return _next(aspectContext);
    }
}

public class TestAspectContextBeforeAttribute : AspectBefore
{
    public override Task Before(AspectContext aspectContext)
    {
        TestAspectContextRt.result1.Add("AspectBefore");
        return Task.CompletedTask;
    }
}

public class TestAspectContextAfterAttribute : AspectAfter
{

    public override Task After(AspectContext aspectContext, object result)
    {
        TestAspectContextRt.result1.Add("AspectAfter");
        TestAspectContextRt.result1.Add(aspectContext.AdditionalData["test"].ToString()); 
        return Task.CompletedTask;
    }
}