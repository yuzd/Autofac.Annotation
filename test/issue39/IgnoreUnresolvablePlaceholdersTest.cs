using Xunit;

namespace Autofac.Annotation.Test.issue39;

public class IgnoreUnresolvablePlaceholdersTest
{
    [Fact]
    public void Test()
    {
        var builder = new ContainerBuilder();

        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(IgnoreUnresolvablePlaceholdersTest).Assembly));

        var _container = builder.Build();

        var h = _container.Resolve<MyConfig>();
    }
}

[Component]
[PropertySource("/file/testissue39.json")]
public class MyConfig
{
    [Value("${abc}")] public string Abc { get; set; }

    [Value("${test}", IgnoreUnresolvablePlaceholders = true)]
    public string Test { get; set; }
}