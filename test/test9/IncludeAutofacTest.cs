//-----------------------------------------------------------------------
// <copyright file="IncludeAutofacTest .cs" company="Company">
// Copyright (C) Company. All Rights Reserved.
// </copyright>
// <author>nainaigu</author>
// <create>$Date$</create>
// <summary></summary>
//-----------------------------------------------------------------------

using System;
using System.Linq;
using Castle.DynamicProxy;
using Xunit;

namespace Autofac.Annotation.Test.test9
{

    /// <summary>
    ///  autofac api注册的也能被扫描到
    /// </summary>
    public class IncludeAutofacTest
    {
        
        [Fact]
        public void Test()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<InclueModel1>().InstancePerDependency();

            builder.RegisterSpring(r => r.RegisterAssembly(typeof(IncludeAutofacTest).Assembly));
            
            var container = builder.Build();
            var inclueModel1 = container.Resolve<InclueModel1>();
            Assert.NotNull(inclueModel1.model2);
            Assert.NotNull(inclueModel1.a9);
        }
        
        [Fact]
        public void Test1()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<InclueModel1>().InstancePerDependency();
            builder.RegisterGeneric(typeof(InclueModel3<>)).InstancePerDependency();
            
            
            builder.RegisterSpring(new AutofacAnnotationModule(typeof(IncludeAutofacTest).Assembly));
                  
            var container = builder.Build();
            var inclueModel4 = container.Resolve<InclueModel4<string>>();
            var inclueModel1 = container.Resolve<InclueModel3<string>>();
            var inclueModel11 = container.Resolve<InclueModel3<string>>();
            Assert.NotNull(inclueModel4.model1);
            Assert.NotNull(inclueModel1.model1);
            Assert.NotNull(inclueModel11.model1);
        }
        
        [Fact]
        public void Test2()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<InclueModel1>().InstancePerDependency().ExclueFromSpring();

            builder.RegisterSpring(r => r.RegisterAssembly(typeof(IncludeAutofacTest).Assembly));
            
            var container = builder.Build();
            var inclueModel1 = container.Resolve<InclueModel1>();
            Assert.Null(inclueModel1.model2);
            Assert.Null(inclueModel1.a9);
        }
        
        [Fact]
        public void Test3()
        {
            var builder = new ContainerBuilder();

            builder.Register(c => new CallLogger());
            builder.RegisterType<IncludeModel5>().InstancePerDependency().EnableClassInterceptors().InterceptedBy(typeof(CallLogger));

            builder.RegisterSpring(r => r.RegisterAssembly(typeof(IncludeAutofacTest).Assembly));
            
            var container = builder.Build();
            var inclueModel1 = container.Resolve<IncludeModel5>();
            Assert.NotNull(inclueModel1.model2);
            Assert.NotNull(inclueModel1.a9);
            inclueModel1.Test("dddd");
        }
    }

    //[PropertySource("/file/appsettings1.json")]
    public class InclueModel1
    {
        [Autowired]
        public InclueModel2 model2;

        [Value("${a9}")]
        public string a9;
        
        public string Hello { get; set; } = "world";
    }

    [Component]
    public class InclueModel2
    {
        public string Hello { get; set; } = "world2";
    }

    public class InclueModel3<T> where T: class
    {
        [Autowired]
        public InclueModel1 model1;
        public string Hello { get; set; } = "world4";
    }
    
    [Component]
    public class InclueModel4<T> where T: class
    {
        [Autowired]
        public InclueModel1 model1;
        public string Hello { get; set; } = "world4";
    }
    
    public class CallLogger : IInterceptor
    {
        [Autowired]
        public InclueModel2 model2;
        
        [Value("${a9}")]
        public string a9;
        public void Intercept(IInvocation invocation)
        {
            Console.WriteLine("Calling method {0} with parameters {1}... ",
                invocation.Method.Name,
                string.Join(", ", invocation.Arguments.Select(a => (a ?? "").ToString()).ToArray()));

            invocation.Proceed();

            Console.WriteLine("Done: result was {0}.", invocation.ReturnValue);
        }
    }

    public class IncludeModel5
    {
        [Autowired]
        public InclueModel2 model2;
        
        [Value("${a9}")]
        public string a9;
        
        public string Hello { get; set; } = "world5";

        public virtual void Test(string ag)
        {
            Console.WriteLine("call in class->" + ag);
        }
    }
}