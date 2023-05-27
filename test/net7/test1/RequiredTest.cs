using Autofac.Annotation.Test.issue31;
using Xunit;

namespace Autofac.Annotation.Test.net7.test1;

public class RequiredTest
{
    [Fact]
    public void Test_01()
    {
        var builder = new ContainerBuilder();
        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(RequiredTest).Assembly));
        var container = builder.Build();
        var test = container.Resolve<MyComponent>();
        Assert.NotNull(test.GetReqTest1());
        Assert.Null(test.GetReqTest2());
    }

    [Fact]
    public void Test_02()
    {
    }
}

[Component]
public class MyComponent
{
    public required ReqTest1 ReqTest1 { protected get; init; }

    public ReqTest2 ReqTest2 { get; set; }

    public ReqTest1 GetReqTest1()
    {
        return ReqTest1;
    }

    public ReqTest2 GetReqTest2()
    {
        return ReqTest2;
    }
}

[Component]
public class ReqTest1
{
}

[Component]
public class ReqTest2
{
}