//-----------------------------------------------------------------------
// <copyright file="TestPlugin1 .cs" company="Company">
// Copyright (C) Company. All Rights Reserved.
// </copyright>
// <author>nainaigu</author>
// <create>$Date$</create>
// <summary></summary>
//-----------------------------------------------------------------------

using System.Runtime.Loader;
using Plugin;

namespace Autofac.Annotation.Test.net7.testasmload;

public class TestPlugin1
{
    // [Fact]
    public void Test_01()
    {
        var builder = new ContainerBuilder();
        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestPlugin1).Assembly));
        var container = builder.Build();

        var loadContext = new AssemblyLoadContext("PluginContext", isCollectible: true);

        var scope = container.BeginLoadContextLifetimeScope(loadContext, builder =>
        {
            var pluginAssembly =
                loadContext.LoadFromAssemblyPath(
                    @"E:\workspace\Autofac.Annotation\test\bin\Debug\net7.0\MyPluginTest.dll");
            builder.RegisterAssemblyTypes(pluginAssembly).AsImplementedInterfaces();
            // builder.RegisterModule(new AutofacAnnotationModule(pluginAssembly));
        });
        var resolve = scope.Resolve<IPlugin>();
        scope.Dispose();
        // 在dll中打的注解 如何主scope中的联动？ 比如dll中注册了一个Compoment 且需要被aop的话
        loadContext.Unloading += context => { };
        loadContext.Unload();
    }
}