//-----------------------------------------------------------------------
// <copyright file="IncludeAutofacTest .cs" company="Company">
// Copyright (C) Company. All Rights Reserved.
// </copyright>
// <author>nainaigu</author>
// <create>$Date$</create>
// <summary></summary>
//-----------------------------------------------------------------------

using System;
using Autofac.Annotation;
using Xunit;

namespace Autofac.Configuration.Test.fix
{

    /// <summary>
    ///  autofac api注册的也能被扫描到
    /// </summary>
    public class FixIssueTest
    {
        
        [Fact]
        public void Test()
        {
            var builder = new ContainerBuilder();

            builder.RegisterSpring(r => r.RegisterAssembly(typeof(BaseManager_1).Assembly));
            var container = builder.Build();
            var inclueModel1 = container.Resolve<BaseManager_1>();
            Assert.Equal("test", BaseManager_1.Hello);
        }

        [Fact]
        public void Test2()
        {
            var builder = new ContainerBuilder();
            builder.RegisterSpring(r => r.RegisterAssembly(typeof(BaseManager_1).Assembly));
            var container = builder.Build();
            var inclueModel1 = container.Resolve<BaseManager_2>();
            Assert.Equal("test2", BaseManager_2.Hello);
        }

        [Fact]
        public void Test3()
        {
            var builder = new ContainerBuilder();
            builder.RegisterSpring(r => r.RegisterAssembly(typeof(BaseManager_1).Assembly));
            var container = builder.Build();
            var inclueModel1 = container.Resolve<BaseManager_3>();
            Assert.Equal("test3", BaseManager_3.Hello);
        }

        [Fact]
        public void Test4()
        {
            var builder = new ContainerBuilder();
            builder.RegisterSpring(r => r.RegisterAssembly(typeof(BaseManager_1).Assembly));
            var container = builder.Build();
            var inclueModel1 = container.Resolve<BaseManager_4>();
            Assert.Equal("test4", BaseManager_4.Hello);
        }

        [Fact]
        public void Test5()
        {
            var builder = new ContainerBuilder();
            builder.RegisterSpring(r => r.RegisterAssembly(typeof(BaseManager_1).Assembly));
            var container = builder.Build();
            var inclueModel1 = container.Resolve<BaseManager_5>();
            Assert.Equal("test5", BaseManager_5.Hello);
        }
    }


    public abstract class BaseManager<T, PK>
    {
        public void Init()
        {
            // 父类的初始化
            DoInit();
        }
        // 子类扩展的初始化方法
        public abstract void DoInit();

        public virtual void Test()
        {
            throw new Exception("should call from child class");
        }

        public  void Test3()
        {
            throw new Exception("should call from child class");
        }

        public void Test4()
        {
            BaseManager_4.Hello = "test4";
        }
    }

    [Component(InitMethod = "Init", AutoActivate = true)]
    public class BaseManager_1 : BaseManager<string, int>
    {
        public static string Hello;

        public override void DoInit()
        {
            Hello = "test";
            Console.WriteLine("test");
        }
    }

    [Component(InitMethod = "Test", AutoActivate = true)]
    public class BaseManager_2 : BaseManager<string, int>
    {
        public static string Hello;

        public override void DoInit()
        {
            Hello = "test";
            Console.WriteLine("test");
        }

        public override void Test()
        {
            Hello = "test2";
        }
    }

    [Component(InitMethod = "Test3", AutoActivate = true)]
    public class BaseManager_3 : BaseManager<string, int>
    {
        public static string Hello;

        public override void DoInit()
        {
            Hello = "test";
            Console.WriteLine("test");
        }

        public override void Test()
        {
            Hello = "test2";
        }

        public void Test3()
        {
            Hello = "test3";
        }
    }

    [Component(InitMethod = "Test4", AutoActivate = true)]
    public class BaseManager_4: BaseManager<string, int>
    {
        public static string Hello;

        public override void DoInit()
        {
            Hello = "test";
            Console.WriteLine("test");
        }

        public override void Test()
        {
            Hello = "test2";
        }

    }


    public abstract class BaseManager2
    {
        [PostConstruct]
        public void Init()
        {
            // 父类的初始化
            DoInit();
        }
        // 子类扩展的初始化方法
        public abstract void DoInit();
    }


    [Component(AutoActivate = true)]
    public class BaseManager_5 : BaseManager2
    {
        public static string Hello;

        public override void DoInit()
        {
            Hello = "test5";
        }
    }
}