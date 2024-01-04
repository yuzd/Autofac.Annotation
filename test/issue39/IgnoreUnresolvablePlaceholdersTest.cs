using Autofac.Core;
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

        Assert.Equal("abc", h.Abc);
        Assert.Null(h.Test);
        Assert.Null(h.Test2);

        Assert.Throws<DependencyResolutionException>(() => _container.Resolve<MyConfig2>());
    }
}

[Component]
[PropertySource("/file/testissue39.json")]
public class MyConfig
{
    [Value("${abc}")] public string Abc { get; set; }

    [Value("${test}", IgnoreUnresolvablePlaceholders = true)]
    public string Test { get; set; }


    // 设置了usespel=false则采用原生的方式去拿， 因为没有test并且设置了Ignore  所以不会报错
    [Value("test", UseSpel = false, IgnoreUnresolvablePlaceholders = true)]
    public string Test2 { get; set; }
}

[Component]
[PropertySource("/file/testissue39.json")]
public class MyConfig2
{
    // 设置了usespel=false则采用原生的方式去拿， 因为没有test 会报错
    [Value("test", UseSpel = false)] public string Test2 { get; set; }
}