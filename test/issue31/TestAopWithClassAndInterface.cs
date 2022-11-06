//-----------------------------------------------------------------------
// <copyright file="TestAopWithClassAndInterface .cs" company="Company">
// Copyright (C) Company. All Rights Reserved.
// </copyright>
// <author>nainaigu</author>
// <create>$Date$</create>
// <summary></summary>
//-----------------------------------------------------------------------

using Castle.DynamicProxy;
using Xunit;

namespace Autofac.Annotation.Test.issue31;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class TestAopWithClassAndInterface
{
    [Fact]
    public void Test_Type_01()
    {
        var builder = new ContainerBuilder();

        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestAopWithClassAndInterface).Assembly));

        var container = builder.Build();

        var rt = container.Resolve<TestInterAndClass1>();
        
          
        // 接口实现 不会走aop   如何走aop请看下面一个单测
        rt.Send("da");
        
        Assert.Single(container.Resolve<TestPointByAttResult2>().Rt2);
        
        
        // class + 虚拟方法会走aop
        rt.Say("hi");
      
        Assert.Equal(2,container.Resolve<TestPointByAttResult2>().Rt1.Count);
    }
    
    [Fact]
    public void Test_Type_02()
    {
        var builder = new ContainerBuilder();

        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestAopWithClassAndInterface).Assembly));

        var container = builder.Build();

        var rt = container.Resolve<TestInterAndClass3>();
        
          
        // 接口显示调用 会走aop 
        (rt as INetBase2).Send("da");
        Assert.Equal(2,container.Resolve<TestPointByAttResult2>().Rt3.Count);
        
        // class + 虚拟方法会走aop
        rt.Say("hi");
      
        Assert.Equal(4,container.Resolve<TestPointByAttResult2>().Rt3.Count);
    }
    
    [Fact]
    public void Test_Type_03()
    {
        var builder = new ContainerBuilder();

        // autofac打标签模式
        builder.RegisterModule(new AutofacAnnotationModule(typeof(TestAopWithClassAndInterface).Assembly));

        var container = builder.Build();

        var rt = container.Resolve<SeriaPort>();
        
          
        // 接口显示调用 会走aop 
        (rt as ICommunication32).Send();
        Assert.Equal(1,container.Resolve<TestPointByAttResult2>().Rt4.Count);
        
        // class + 虚拟方法会走aop
        rt.CheckCommand();
      
        Assert.Equal(2,container.Resolve<TestPointByAttResult2>().Rt4.Count);
    }

    [Fact]
    public void Test_Castle_PROXY()
    {
        ProxyGenerator ProxyGenerator = new ProxyGenerator();
        var proxy = (CastleClass1)ProxyGenerator.CreateClassProxyWithTarget(typeof(CastleClass1), new Type[] { typeof(ICastleInterface1) }, new CastleClass1(),
            new CastleProxy1());

        // 接口的方法 不走aop
        proxy.Send();
        
        // 类的虚拟方法走aop
        proxy.Send1();
        
        // 转成接口就走aop
        (proxy as ICastleInterface1).Send();
    }
}

[Component(AutofacScope = AutofacScope.SingleInstance)]
public class TestPointByAttResult2
{
    public List<string> Rt1 { get; set; } = new();
    public List<string> Rt2 { get; set; } = new();
    public List<string> Rt3 { get; set; } = new();
    public List<string> Rt4 { get; set; } = new();
}

public class NetBase
{
    [Autowired]
    public TestPointByAttResult2 TestPointByAttResult2 { get; set; }
    
    [NetBase1Attribute]
    public virtual void Say(string hello)
    {
        TestPointByAttResult2.Rt1.Add("1");
    }
}

public interface INetBase
{
    [NetBase1Attribute]
    void Send(string hello);
}

[Component]
public class TestInterAndClass1 : NetBase, INetBase
{
  
    public  void Send(string hello)
    {
        TestPointByAttResult2.Rt2.Add("1");
    }
}

public class TestInterAndClass12 : TestInterAndClass1
{
    public override void Say(string hello)
    {
        base.Say(hello);
    }
    
    // 在castle创建的代理类是无法在去重写Send了 
    // public override void Send(string hello)
    // {
    //     base.Send(hello);
    // }
    
    // 所以在Castle是这么做的 所以也就解释了 为什么要强转成接口调用才会走aop
    // public void INetBase.Send(string hello)
    // {
    //     
    // }   
}



public class NetBase2
{
    [Autowired]
    public TestPointByAttResult2 TestPointByAttResult2 { get; set; }
    
    [NetBase2Attribute]
    public virtual void Say(string hello)
    {
        TestPointByAttResult2.Rt3.Add("Say");
    }
}

public interface INetBase2
{
    [NetBase2Attribute]
    void Send(string hello);
}

[Component]
public class TestInterAndClass3 : NetBase2, INetBase2
{
  
    public  void Send(string hello)
    {
        TestPointByAttResult2.Rt3.Add("Send");
    }
}

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public class NetBase1Attribute : Attribute
{
}

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public class NetBase2Attribute : Attribute
{
}

[Pointcut(NameSpace = "Autofac.Annotation.Test.issue31", AttributeType = typeof(NetBase1Attribute))]
public class NetBaseAop
{
    [Autowired] public TestPointByAttResult2 TestPointByAttResult2 { get; set; }

    [Before]
    public void befor()
    {
        TestPointByAttResult2.Rt1.Add("before");
    }
}

public class CastleClass1 : ICastleInterface1
{
    public void Send()
    {
        Console.WriteLine("Send");
    }

    public virtual void Send1()
    {
        Console.WriteLine("Send1");
    }
}

public interface ICastleInterface1
{
    void Send();
}

public class CastleProxy1 : IInterceptor
{
    public void Intercept(IInvocation invocation)
    {
        invocation.Proceed();
    }
}


[Pointcut(NameSpace = "Autofac.Annotation.Test.issue31", AttributeType = typeof(NetBase2Attribute))]
public class NetBaseAop2
{
    [Autowired] public TestPointByAttResult2 TestPointByAttResult2 { get; set; }

    [Before]
    public void befor()
    {
        TestPointByAttResult2.Rt3.Add("before");
    }
}


[NetBase3Attribute]
public class NetBase3
{
    public virtual void CheckCommand()
    {
        
    }
}


[NetBase3Attribute]
public interface ICommunication32
{
    void Send();
}

[Component]
public class SeriaPort : NetBase3 ,ICommunication32
{
    public void Send()
    {
        
    }
}


[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public class NetBase3Attribute : Attribute
{
}


[Pointcut(NameSpace = "Autofac.Annotation.Test.issue31", AttributeType = typeof(NetBase3Attribute))]
public class NetBaseAop3
{
    [Autowired] public TestPointByAttResult2 TestPointByAttResult2 { get; set; }

    [Before]
    public void befor()
    {
        TestPointByAttResult2.Rt4.Add("before");
    }
}