using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Autofac.Annotation;
using Autofac.Annotation.Test;
using Autofac.Aspect;
using Castle.DynamicProxy;

namespace Autofac.Configuration.Test.test3
{
    public class TestModel3
    {
        public string Name { get; set; } = "test3";
    }

    public interface ITestModel4
    {
         string Name { get; set; }
    }

    public class TestModel4Parent: ITestModel4
    {
        public string Name { get; set; } = "TestModel4Parent";
    }
    public class TestModel4: TestModel4Parent
    {
    }


    public interface ITestModel5
    {
        string Name { get; set; }
    }

    public class TestModel5 : ITestModel5
    {
        public string Name { get; set; } = "TestModel4Parent";
    }

    public interface ITestModel6
    {
        void Hello(string aa);
    }

    [Component(Interceptor = typeof(Log))]
    public class TestModel6 : ITestModel6
    {
        public void Hello(string aa)
        {
            Console.WriteLine(nameof(TestModel6.Hello));
        }
    }

    [Component(Interceptor = typeof(Log),InterceptorType = InterceptorType.Class)]
    public class TestModel61 : ITestModel6
    {
        public virtual void Hello(string aa)
        {
            Console.WriteLine(nameof(TestModel61.Hello));
        }
    }

    [Component]
    public class TestModel7
    {
        [Autowired]
        public TestModel71 TestModel71 { get; set; }
    }

    [Component]
    public class TestModel71
    {
        [Autowired]
        public TestModel7 TestModel7 { get; set; }
    }


    [Component]
    public class TestModel8
    {

        public TestModel8([Autowired]ITestModel6 _TestModel81)
        {
            TestModel6 = _TestModel81;
        }

        public ITestModel6 TestModel6 { get; set; }

        [Autowired]
        public TestModel81 TestModel81 { get; set; }
    }

    [Component]
    public class TestModel81
    {
        public TestModel81([Autowired]ITestModel6 _TestModel8)
        {
            TestModel6 = _TestModel8;
        }

        public ITestModel6 TestModel6 { get; set; }

        [Autowired]
        public TestModel8 TestModel8 { get; set; }
    }

    [Component]
    [Aspect]
    public class TestModel9
    {

        [TestHelloBefor]
        public virtual void Say()
        {
            Console.WriteLine("say");
        }

        [TestHelloAfter]
        public virtual void SayAfter()
        {
            Console.WriteLine("SayAfter");
        }

        [TestHelloArround]
        public virtual void SayArround()
        {
            Console.WriteLine("SayArround");
        }
    }

    [Component]
    [Aspect]
    public class TestModel911
    {

        [TestHelloBefor]
        public virtual void Say()
        {
            Console.WriteLine("say");
            throw new Exception("ddd");
        }

        [TestHelloAfter]
        public virtual void SayAfter()
        {
            Console.WriteLine("SayAfter");
            throw new Exception("ddd");
        }

        [TestHelloArround]
        public virtual void SayArround()
        {
            Console.WriteLine("SayArround");
            throw new Exception("ddd");
        }
    }
    public class TestHelloBefor : AspectBeforeAttribute
    {
        public override Task Before(IComponentContext context, IInvocation invocation)
        {
            var aa1 = context.Resolve<TestModel81>();
            Console.WriteLine("TestHelloBefor");
            return Task.CompletedTask;
        }
    }

    public class TestHelloAfter : AspectAfterAttribute
    {

        public override Task After(IComponentContext context, IInvocation invocation, Exception exp)
        {
            if(exp!=null) Console.WriteLine(exp.Message);
            Console.WriteLine("TestHelloAfter");
            return Task.CompletedTask;
        }
    }


    public class TestHelloArround : AspectAroundAttribute
    {

        public override Task After(IComponentContext context, IInvocation invocation, Exception exp)
        {
            if (exp != null) Console.WriteLine(exp.Message);
            Console.WriteLine("TestHelloArround");
            return Task.CompletedTask;
        }

        public override Task Before(IComponentContext context, IInvocation invocation)
        {
            Console.WriteLine("TestHelloArround.Before");
            return Task.CompletedTask;
        }
    }
    [Component]
    [Aspect]
    public class TestModel91
    {

        [TestHelloBefor]
        public virtual async Task Say()
        {
            Console.WriteLine("say");
            await Task.Delay(1000);
        }

        [TestHelloAfter]
        public virtual async Task<string> SayAfter()
        {
            Console.WriteLine("SayAfter");
            await Task.Delay(1000);
            return "SayAfter";
        }

        [TestHelloArround]
        public virtual async Task<string> SayArround()
        {
            Console.WriteLine("SayArround");
            await Task.Delay(1000);
            return "SayArround";
        }
    }

    [Component]
    [Aspect]
    public class TestModel912
    {

        [TestHelloBefor]
        public virtual async Task Say()
        {
            Console.WriteLine("say");
            await Task.Delay(1000);
            throw new Exception("ddd");
        }

        [TestHelloAfter]
        public virtual async Task<string> SayAfter()
        {
            Console.WriteLine("SayAfter");
            await Task.Delay(1000);
            throw new Exception("ddd");
            return "SayAfter";
        }

        [TestHelloArround]
        public virtual async Task<string> SayArround()
        {
            Console.WriteLine("SayArround");
            await Task.Delay(1000);
            throw new Exception("ddd");
            return "SayArround";
        }
    }

    //[Component(Interceptor = typeof(Log))]
    [Aspect]
    public class TestModel10
    {

        [TestHelloBefor]
        public virtual void Say()
        {
            Console.WriteLine("say");
        }

        [TestHelloAfter]
        public virtual void SayAfter()
        {
            Console.WriteLine("SayAfter");
        }

        [TestHelloArround]
        public virtual void SayArround()
        {
            Console.WriteLine("SayArround");
        }
    }
}
