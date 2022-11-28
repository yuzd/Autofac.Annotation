using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Autofac.Annotation.Test.issue31;

public class TestAutoActivate
{
    [Fact]
    public void Test_Type_01()
    {
        var builder = new ContainerBuilder();
        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestAutoActivate).Assembly));
        var container = builder.Build();
        var test1 = container.Resolve<AutoActivateCls1>();
        var test2 = container.Resolve<AutoActivateCls1>();
        Assert.True(test1.Date!=test2.Date);
    }

 
    [Fact]
    public void Test_Type_02()
    {
        var builder = new ContainerBuilder();
        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestAutoActivate).Assembly));
        var container = builder.Build();
        var test1 = container.Resolve<AutoActivateCls2>();
        var test2 = container.Resolve<AutoActivateCls2>();
        Assert.True(test1.Date==test2.Date);
    }
}
// 默认是瞬时的
[Component(AutoActivate = true)]
public class AutoActivateCls1
{
    public DateTime Date { get; set; }

    public AutoActivateCls1()
    {
        Date = DateTime.Now;
    }
}

// 指定单例模式
[Component(AutofacScope = AutofacScope.SingleInstance,AutoActivate = true)]
public class AutoActivateCls2
{
    public DateTime Date { get; set; }

    public AutoActivateCls2()
    {
        Date = DateTime.Now;
    }
}