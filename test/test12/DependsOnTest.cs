//-----------------------------------------------------------------------
// <copyright file="DependsOnTest .cs" company="Company">
// Copyright (C) Company. All Rights Reserved.
// </copyright>
// <author>nainaigu</author>
// <create>$Date$</create>
// <summary></summary>
//-----------------------------------------------------------------------

using Autofac.Annotation.Test.test10;
using Xunit;

namespace Autofac.Annotation.Test.test12;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// 
/// </summary>
public class DependsOnTest
{
    [Fact]
    public void Test1()
    {
        var builder = new ContainerBuilder();
        builder.RegisterSpring(r => r.RegisterAssembly(typeof(ConditionTest).Assembly));
        var container = builder.Build();
        var a1 = container.Resolve<Test12Bean1>();
        var a2 = container.Resolve<Test12Bean1>();
        Assert.NotEqual(a1,a2);
    }
    [Fact]
    public void Test2()
    {
        var builder = new ContainerBuilder();
        builder.RegisterSpring(r => r.RegisterAssembly(typeof(ConditionTest).Assembly));
        var container = builder.Build();
        var a1 = container.Resolve<Test12Bean2>();
        Assert.NotNull(a1);
        Assert.NotNull(a1.Bean1);
    }
    
    [Fact]
    public void Test3()
    {
        var builder = new ContainerBuilder();
        builder.RegisterSpring(r => r.RegisterAssembly(typeof(ConditionTest).Assembly));
        var container = builder.Build();
        var a1 = container.Resolve<Test12Bean3>();
        Assert.Equal(6,Test12Models.result.Count);
        Assert.Equal("get14",Test12Models.result[0]);
        Assert.Equal("get18",Test12Models.result[1]);
        Assert.Equal("get17",Test12Models.result[2]);
        Assert.Equal("get16",Test12Models.result[3]);
        Assert.Equal("get15",Test12Models.result[4]);
        Assert.Equal("get13",Test12Models.result[5]);
    }
    
    [Fact]
    public void Test4()
    {
        var builder = new ContainerBuilder();
        builder.RegisterSpring(r => r.RegisterAssembly(typeof(ConditionTest).Assembly));
        var container = builder.Build();
        var a1 = container.Resolve<Test12Bean9>();
        Assert.Equal(2,Test12Models.result2.Count);
    }
}