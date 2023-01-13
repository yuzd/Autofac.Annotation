//-----------------------------------------------------------------------
// <copyright file="TestValueInject .cs" company="Company">
// Copyright (C) Company. All Rights Reserved.
// </copyright>
// <author>nainaigu</author>
// <create>$Date$</create>
// <summary></summary>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using Xunit;

namespace Autofac.Annotation.Test.issue31;

/// <summary>
/// 
/// </summary>
public class TestValueInject
{
    [Fact]
    public void Test_Type_01()
    {
        var builder = new ContainerBuilder();
        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestValueInject).Assembly));
        var container = builder.Build();
        var valueInject1 = container.Resolve<ValueInject1>();
        var list2 = valueInject1.list2;
        Assert.Equal(2, list2.Count);
        Assert.NotNull(valueInject1.ValueInjectModel1);
        Assert.Equal(20, valueInject1.ValueInjectModel1.Age);
        Assert.Equal(2, valueInject1.list22.Value.Count);
    }
}

[Component]
public class ValueInject1
{
    [Value("list2", UseSpel = false)] public List<string> list2 { get; set; }

    [Value("ValueInjectModel1", UseSpel = false)]
    public ValueInjectModel1 ValueInjectModel1 { get; set; }
    
    [Value("list2", UseSpel = false)]
    public IValue<List<string>> list22 { get; set; }
}

public class ValueInjectModel1
{
    public string Name { get; set; }
    public int Age { get; set; }
}