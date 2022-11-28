using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Autofac.Annotation.Test.issue31;

public class TestBeanScope
{
    [Fact]
    public void Test_Type_01()
    {
        var builder = new ContainerBuilder();
        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestBeanScope).Assembly));
        var container = builder.Build();
        var test1 = container.Resolve<TestBeanScopeCls1>();
        var test2 = container.Resolve<TestBeanScopeCls1>();
        Assert.True(test1.Date != test2.Date);
    }

    [Fact]
    public void Test_Type_02()
    {
        var builder = new ContainerBuilder();
        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestBeanScope).Assembly));
        var container = builder.Build();
        var test1 = container.Resolve<TestBeanScopeCls2>();
        var test2 = container.Resolve<TestBeanScopeCls2>();
        Assert.True(test1.Date == test2.Date);
    }
    [Fact]
    public void Test_Type_03()
    {
        var builder = new ContainerBuilder();
        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestBeanScope).Assembly).SetDefaultAutofacScopeToSingleInstance());
        var container = builder.Build();
        var test1 = container.Resolve<TestBeanScopeCls3>();
        var test2 = container.Resolve<TestBeanScopeCls3>();
        Assert.True(test1.Date == test2.Date);
    }
}

// 默认是瞬时的
[AutoConfiguration]
public class TestBeanScopeCfg1
{
    // Bean的注册默认是瞬时的
    [Bean]
    public TestBeanScopeCls1 initTestBeanScopeCls1()
    {
        return new TestBeanScopeCls1();
    }

    // Bean的注册默认是单例的,可以指定为单例的
    [Bean(AutofacScope = AutofacScope.SingleInstance)]
    public TestBeanScopeCls2 initTestBeanScopeCls2()
    {
        return new TestBeanScopeCls2();
    }
}

public class TestBeanScopeCls1
{
    public DateTime Date { get; set; }

    public TestBeanScopeCls1()
    {
        Date = DateTime.Now;
    }
}

public class TestBeanScopeCls2
{
    public DateTime Date { get; set; }

    public TestBeanScopeCls2()
    {
        Date = DateTime.Now;
    }
}

[AutoConfiguration]
public class TestBeanScopeCfg2
{
    // Bean的注册scope没有特别指定就用全局
    [Bean]
    public TestBeanScopeCls3 initTestBeanScopeCls3()
    {
        return new TestBeanScopeCls3();
    }

}

public class TestBeanScopeCls3
{
    public DateTime Date { get; set; }

    public TestBeanScopeCls3()
    {
        Date = DateTime.Now;
    }
}