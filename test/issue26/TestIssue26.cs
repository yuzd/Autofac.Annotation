//-----------------------------------------------------------------------
// <copyright file="TestIssue26 .cs" company="Company">
// Copyright (C) Company. All Rights Reserved.
// </copyright>
// <author>nainaigu</author>
// <create>$Date$</create>
// <summary></summary>
//-----------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Autofac.Annotation.Test.issue26.A.B.Command.Controller;
using Autofac.Annotation.Test.test13;
using Xunit;

namespace Autofac.Annotation.Test.issue26;

public class TestIssue26
{
    
    
    [Fact]
    public void Test_Type_01()
    {
        var builder = new ContainerBuilder();

        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestIssue26).Assembly));

        var container = builder.Build();

        var a1 = container.Resolve<TestAController>();
        
        a1.Test();
        var a12 = container.Resolve<PointCutTestResult>();
        Assert.Contains("A.B.Command.Controller.TestAController.Test", a12.result12);
        Assert.True(a12.result12.Count == 3);

        
        var a31 = container.Resolve<Autofac.Annotation.Test.issue26.A.B.Config.Controller.TestAController>();
        
        a31.Test();
        
        var a1233 = container.Resolve<PointCutTestResult>();
        Assert.Contains("A.B.Config.Controller.TestAController.Test", a1233.result12);
        Assert.True(a1233.result12.Count == 4);
    }
    
}

[Pointcut(NameSpace = "Autofac.Annotation.Test.issue26.A.B.Co[^n]%", Class = "*Controller")]
public class PointcutForIgnoreNamespce
{
    [Around]
    public async Task Around(AspectContext context, AspectDelegate next)
    {
        var _pointCutTestResult = context.ComponentContext.Resolve<PointCutTestResult>();

        _pointCutTestResult.result12.Add("PointcutForIgnoreNamespce.Around.start");
        await next(context);
        _pointCutTestResult.result12.Add("PointcutForIgnoreNamespce.Around.end");
    }


    [Before]
    public void Before()
    {
        Console.WriteLine("PointcutForIgnoreNamespce.Before");
    }

    [After]
    public void After()
    {
        Console.WriteLine("PointcutForIgnoreNamespce.After");
    }

    [AfterReturn(Returing = "value1")]
    public void AfterReturn(object value1)
    {
        Console.WriteLine("PointcutForIgnoreNamespce.AfterReturn");
    }
}