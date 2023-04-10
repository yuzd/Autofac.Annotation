using System;
using System.Threading.Tasks;
using Autofac.Annotation.Test.issue31;
using Xunit;

namespace Autofac.Annotation.Test.issue35;

public class TestInternerClass
{
    [Fact]
    public async Task Test_Type_01()
    {
        var builder = new ContainerBuilder();
        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestInternerClass).Assembly));
        var container = builder.Build();
        var intReader = container.Resolve<IntReader>();
        Assert.NotNull(intReader);

    }
    
}

[Component]
internal class IntReader
{
    public int ReadInt()
    {
        return new Random().Next();
    }
}