using System.Collections.Generic;
using Xunit;

namespace Autofac.Annotation.Test.issue31;

/// <summary>
/// 
/// </summary>
public class TestComponentOrderIndex
{
    [Fact]
    public void Test_Type_01()
    {
        var builder = new ContainerBuilder();

        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestComponentOrderIndex).Assembly));
        var container = builder.Build();
        var rt = container.Resolve<OrderIndexTestRt>();
        Assert.Equal(3, rt.result1.Count);
        Assert.Equal("TestOrderIndex3", rt.result1[0]);
        Assert.Equal("TestOrderIndex1", rt.result1[2]);
    }
}

[Component(AutofacScope = AutofacScope.SingleInstance)]
public class OrderIndexTestRt
{
    public List<string> result1 = new();
}

[Component(AutofacScope = AutofacScope.SingleInstance, AutoActivate = true, OrderIndex = 1)]
public class TestOrderIndex1
{
    public TestOrderIndex1(OrderIndexTestRt OrderIndexTestRt)
    {
        OrderIndexTestRt.result1.Add("TestOrderIndex1");
    }
}

[Component(AutofacScope = AutofacScope.SingleInstance, AutoActivate = true, OrderIndex = 2)]
public class TestOrderIndex2
{
    public TestOrderIndex2(OrderIndexTestRt OrderIndexTestRt)
    {
        OrderIndexTestRt.result1.Add("TestOrderIndex2");
    }
}

[Component(AutofacScope = AutofacScope.SingleInstance, AutoActivate = true, OrderIndex = 3)]
public class TestOrderIndex3
{
    public TestOrderIndex3(OrderIndexTestRt OrderIndexTestRt)
    {
        OrderIndexTestRt.result1.Add("TestOrderIndex3");
    }
}