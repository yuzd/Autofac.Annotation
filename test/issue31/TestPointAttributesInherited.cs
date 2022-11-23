using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Autofac.Annotation.Test.issue31;

public class TestPointAttributesInherited
{
    [Fact]
    public void Test_Type_01()
    {
        var builder = new ContainerBuilder();
        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestPointAttributesInherited).Assembly));
        var container = builder.Build();
        var testPointAttriCls1 = container.Resolve<TestPointAttriCls1>();
        testPointAttriCls1.say();
        Assert.Equal(3, TestPointAttributesInheritedRt.result1.Count);
    }

    [Fact]
    public void Test_Type_02()
    {
        var builder = new ContainerBuilder();
        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestPointAttributesInherited).Assembly));
        var container = builder.Build();
        var testPointAttriCls2 = container.Resolve<TestPointAttriCls2>();
        testPointAttriCls2.say();
        Assert.Equal(2, TestPointAttributesInheritedRt.result2.Count);
    }
    
    [Fact]
    public void Test_Type_03()
    {
        var builder = new ContainerBuilder();
        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestPointAttributesInherited).Assembly));
        var container = builder.Build();
        var testPointAttriCls3 = container.Resolve<TestPointAttriCls3>();
        testPointAttriCls3.say();
        Assert.Equal(3, TestPointAttributesInheritedRt.result3.Count);
    }
}

public class TestPointAttributesInheritedRt
{
    public static List<string> result1 = new();
    public static List<string> result2 = new();
    public static List<string> result3 = new();
}

// 因为设置了AssignableFrom
// 所以打了TestPointAttributes1和它的父类TestPointAttributes1Parent都会被识别
[Pointcut(NameSpace = "Autofac.Annotation.Test.issue31", AttributeType = typeof(TestPointAttributes1), AttributeFlag = AssignableFlag.AssignableFrom)]
public class TestPointAttributesAop1
{
    [Before]
    public void befor()
    {
        TestPointAttributesInheritedRt.result1.Add("TestPointAttributesAop1.Before");
    }
}

//只会识别打了TestPointAttributes1Parent的类
[Pointcut(NameSpace = "Autofac.Annotation.Test.issue31", AttributeType = typeof(TestPointAttributes1Parent))]
public class TestPointAttributesAop2
{
    [After]
    public void after()
    {
        TestPointAttributesInheritedRt.result1.Add("TestPointAttributesAop2.after");
    }
}

[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
public class TestPointAttributes1Parent : Attribute
{
}

[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
public class TestPointAttributes1 : TestPointAttributes1Parent
{
}

//会走2个pointcut
[TestPointAttributes1Parent]
[Component]
public class TestPointAttriCls1
{
    public virtual void say()
    {
        TestPointAttributesInheritedRt.result1.Add("say");
    }
}

// 因为设置了AssignableFrom
// 所以打了TestPointAttributes3和它的父类TestPointAttributes3Parent都会被识别
[Pointcut(NameSpace = "Autofac.Annotation.Test.issue31", AttributeType = typeof(TestPointAttributes3),AttributeFlag = AssignableFlag.AssignableFrom)]
public class TestPointAttributesAop3
{
    [Before]
    public void befor()
    {
        TestPointAttributesInheritedRt.result2.Add("TestPointAttributesAop3.Before");
    }
}

//只会识别打了TestPointAttributes1Parent的类
[Pointcut(NameSpace = "Autofac.Annotation.Test.issue31", AttributeType = typeof(TestPointAttributes3Parent))]
public class TestPointAttributesAop4
{
    [After]
    public void after()
    {
        TestPointAttributesInheritedRt.result2.Add("TestPointAttributesAop4.after");
    }
}

[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
public class TestPointAttributes3Parent : Attribute
{
}

[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
public class TestPointAttributes3 : TestPointAttributes3Parent
{
}

//只会走1个pointcut
[TestPointAttributes3]
[Component]
public class TestPointAttriCls2
{
    public virtual void say()
    {
        TestPointAttributesInheritedRt.result2.Add("say");
    }
}



// 只会扫描TestPointAttributes4
[Pointcut(NameSpace = "Autofac.Annotation.Test.issue31", AttributeType = typeof(TestPointAttributes4))]
public class TestPointAttributesAop5
{
    [Before]
    public void befor()
    {
        TestPointAttributesInheritedRt.result3.Add("TestPointAttributesAop5.Before");
    }
}

//因为设置了AssignableTo
//TestPointAttributes4Parent 和 它的实现类：TestPointAttributes4 都会被识别
[Pointcut(NameSpace = "Autofac.Annotation.Test.issue31", AttributeType = typeof(TestPointAttributes4Parent),AttributeFlag = AssignableFlag.AssignableTo)]
public class TestPointAttributesAop6
{
    [After]
    public void after()
    {
        TestPointAttributesInheritedRt.result3.Add("TestPointAttributesAop6.after");
    }
}

[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
public class TestPointAttributes4Parent : Attribute
{
}

[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
public class TestPointAttributes4: TestPointAttributes4Parent
{
}

[TestPointAttributes4]
[Component]
public class TestPointAttriCls3
{
    public virtual void say()
    {
        TestPointAttributesInheritedRt.result3.Add("say");
    }
}