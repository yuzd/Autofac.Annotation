//-----------------------------------------------------------------------
// <copyright file="TestAutowiredWithValue .cs" company="Company">
// Copyright (C) Company. All Rights Reserved.
// </copyright>
// <author>nainaigu</author>
// <create>$Date$</create>
// <summary></summary>
//-----------------------------------------------------------------------

using System;
using Autofac.Annotation.Condition;
using Autofac.Annotation.Test.test10;
using Xunit;

namespace Autofac.Annotation.Test.issue29;

public class TestAutowiredWithValue
{
   
    [Fact]
    public void Test_Type_01()
    {
        var builder = new ContainerBuilder();

        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestAutowiredWithValue).Assembly));

        var container = builder.Build();

        var testAutowiredWithValueModel3 = container.Resolve<TestAutowiredWithValueModel3>();
        
        Assert.NotNull(testAutowiredWithValueModel3.TestAutowiredWithValueModel1);

        Assert.IsType<TestAutowiredWithValueModel2>(testAutowiredWithValueModel3.TestAutowiredWithValueModel1);
    }

    
    [Fact]
    public void Test_Type_011()
    {
        var builder = new ContainerBuilder();

        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestAutowiredWithValue).Assembly));

        var container = builder.Build();

        var testAutowiredWithValueModel3 = container.Resolve<TestAutowiredWithValueModel31>();
        
        Assert.NotNull(testAutowiredWithValueModel3.TestAutowiredWithValueModel1);

        Assert.IsType<TestAutowiredWithValueModel1>(testAutowiredWithValueModel3.TestAutowiredWithValueModel1);
    }
    
    
    [Fact]
    public void Test_Type_0111()
    {
        var builder = new ContainerBuilder();

        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestAutowiredWithValue).Assembly));

        var container = builder.Build();

        var testAutowiredWithValueModel3 = container.Resolve<TestAutowiredWithValueModel311>();

        var testAutowiredWithValueModel = testAutowiredWithValueModel3.TestAutowiredWithValueModel1.Value;
        Assert.NotNull(testAutowiredWithValueModel);

        Assert.IsType<TestAutowiredWithValueModel2>(testAutowiredWithValueModel);
    }
    
     
    [Fact]
    public void Test_Type_01111()
    {
        var builder = new ContainerBuilder();

        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestAutowiredWithValue).Assembly));

        var container = builder.Build();

        var testAutowiredWithValueModel3 = container.Resolve<TestAutowiredWithValueModel5>();

        var testAutowiredWithValueModel = testAutowiredWithValueModel3._testAutowiredWithValueModel;
        Assert.NotNull(testAutowiredWithValueModel);

        Assert.IsType<TestAutowiredWithValueModel2>(testAutowiredWithValueModel);
    }
    [Fact]
    public void Test_Type_02()
    {
        var builder = new ContainerBuilder();

        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestAutowiredWithValue).Assembly));

        var container = builder.Build();

        var testAutowiredWithValueModel3 = container.Resolve<TestAutowiredWithValueModel4>();
        
        Assert.NotNull(testAutowiredWithValueModel3._testAutowiredWithValueModel1);

        Assert.IsType<TestAutowiredWithValueModel2>(testAutowiredWithValueModel3._testAutowiredWithValueModel1);
    }
    [Fact]
    public void Test_Type_021()
    {
        var builder = new ContainerBuilder();

        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestAutowiredWithValue).Assembly));

        var container = builder.Build();

        var testAutowiredWithValueModel3 = container.Resolve<TestAutowiredWithValueModel41>();
        
        Assert.NotNull(testAutowiredWithValueModel3._testAutowiredWithValueModel1);

        Assert.IsType<TestAutowiredWithValueModel1>(testAutowiredWithValueModel3._testAutowiredWithValueModel1);
    }  
}

public interface ITestAutowiredWithValueModel
{
    string say();
}
[Component("test1")]
public class TestAutowiredWithValueModel1 :ITestAutowiredWithValueModel
{
    public string say()
    {
        return "test1";
    }
}
[Component("test2")]
public class TestAutowiredWithValueModel2 :ITestAutowiredWithValueModel
{

    public string say()
    {
        return "test2";
    }
}

[Component]
public class TestAutowiredWithValueModel3
{
    [Autowired("${ITestAutowiredWithValueModel}")]
    public ITestAutowiredWithValueModel TestAutowiredWithValueModel1 { get; set; }

}

[Component]
[PropertySource("/file/appsettings1.json")]
public class TestAutowiredWithValueModel31
{
    [Autowired("${ITestAutowiredWithValueModel}")]
    public ITestAutowiredWithValueModel TestAutowiredWithValueModel1 { get; set; }

}

[Component]
public class TestAutowiredWithValueModel311
{
    [Autowired("${ITestAutowiredWithValueModel}")]
    public Lazy<ITestAutowiredWithValueModel> TestAutowiredWithValueModel1 { get; set; }

}


public class TestAutowiredWithValueModel5
{
    public ITestAutowiredWithValueModel _testAutowiredWithValueModel;

    public TestAutowiredWithValueModel5(ITestAutowiredWithValueModel testAutowiredWithValueModel)
    {
        _testAutowiredWithValueModel = testAutowiredWithValueModel;
    }
}

[AutoConfiguration]
public class TestAutowiredWithValueAutoConfiguration
{
    [Bean]
    public virtual TestAutowiredWithValueModel5 getTest10Model12([Autowired("${ITestAutowiredWithValueModel}")] ITestAutowiredWithValueModel testAutowiredWithValueModel)
    {
        return new TestAutowiredWithValueModel5(testAutowiredWithValueModel);
    }
}

[Component]
public class TestAutowiredWithValueModel4
{
    public ITestAutowiredWithValueModel _testAutowiredWithValueModel1;

    public TestAutowiredWithValueModel4([Autowired("${ITestAutowiredWithValueModel}")] ITestAutowiredWithValueModel TestAutowiredWithValueModel1)
    {
        _testAutowiredWithValueModel1 = TestAutowiredWithValueModel1;
    }
}

[Component]
[PropertySource("/file/appsettings1.json")]
public class TestAutowiredWithValueModel41
{
    public ITestAutowiredWithValueModel _testAutowiredWithValueModel1;

    public TestAutowiredWithValueModel41([Autowired("${ITestAutowiredWithValueModel}")] ITestAutowiredWithValueModel TestAutowiredWithValueModel1)
    {
        _testAutowiredWithValueModel1 = TestAutowiredWithValueModel1;
    }
}