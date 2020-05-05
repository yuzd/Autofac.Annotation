using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Autofac.Annotation;
using Autofac.Annotation.Test;
using Autofac.Aspect;
using Autofac.Configuration.Test.test2;
using Castle.DynamicProxy;

namespace Autofac.Configuration.Test.test3
{
    public class TestModel1000
    {
        public string Name { get; set; } = "1000"; 
    }

    public class TestModel1001
    {
        public string Name { get; set; } = "1001";

        public TestModel1000 TestModel1000 { get; set; }
    }
    public class TestModel1002
    {
        public string Name { get; set; } = "1002";

    }
    
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
        public override Task Before(AspectContext aspectContext)
        {
            var aa1 = aspectContext.ComponentContext.Resolve<TestModel81>();
            Console.WriteLine("TestHelloBefor");
            return Task.CompletedTask;
        }
    }

    public class TestHelloAfter : AspectAfterAttribute
    {

        public override Task After(AspectContext aspectContext)
        {
            if(aspectContext.Exception!=null) Console.WriteLine(aspectContext.Exception.Message);
            Console.WriteLine("TestHelloAfter");
            return Task.CompletedTask;
        }
    }


    public class TestHelloArround : AspectAroundAttribute
    {

        public override Task After(AspectContext aspectContext)
        {
            if (aspectContext.Exception != null) Console.WriteLine(aspectContext.Exception.Message);
            Console.WriteLine("TestHelloArround");
            return Task.CompletedTask;
        }

        public override Task Before(AspectContext aspectContext)
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

    public class TestModel88 
    {
        public string Name { get; set; } = "TestModel88";
    }

    [Component]
    public class TestModel99
    {
        public string Name { get; set; } = "TestModel99";
    }

    [Component]
    [Aspect]
    public class TestModel101
    {
        public string Name { get; set; } = "TestModel101";

        [StopWatchInterceptor]
        [TransactionInterceptor]
        public virtual void TestInterceptor()
        {
            Console.WriteLine("TestInterceptor");
        }

        public virtual void TestNoInterceptor()
        {
            Console.WriteLine("TestNoInterceptor");
        }
        
        [StopWatchInterceptor]
        [TransactionInterceptor]
        public virtual async Task<string> TestInterceptor2()
        {
            Task.Delay(1000);   
            Console.WriteLine("TestInterceptor2");
            return "TestInterceptor2";
        }
    }

    public class StopWatchInterceptor : AspectPointAttribute
    {
        public override async Task OnInvocation(AspectContext aspectContext, AspectDelegate _next)
        {
            Console.WriteLine("StopWatchInterceptor Start");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                await _next(aspectContext);
            } 
            finally
            {
                stopwatch.Stop();
                Console.WriteLine("StopWatchInterceptor End");
            }
        }
    }
    
    
    public class TransactionInterceptor : AspectPointAttribute
    {
        public override async Task OnInvocation(AspectContext aspectContext, AspectDelegate _next)
        {
            Console.WriteLine("TransactionInterceptor Start");
            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _next(aspectContext);
                transactionScope.Complete();
                Console.WriteLine("TransactionInterceptor End");
                
            }
        }
    }
    public interface IRepository
    {

    }
    public interface IRepository<T> : IRepository where T : class
    {

    }

    public class AopClass
    {

    }
    public class BaseRepository : IRepository
    {

    }
    public class BaseRepository<T> : BaseRepository, IRepository<T> where T : class
    {

    }

    public interface IAopModel : IRepository<AopClass>
    {
        void SayHello();
    }

    [Component]
    [Aspect(InterceptorType.Interface)]
    [StopWatchInterceptor(OrderIndex = 100)]
    public class AopModel1 : BaseRepository<AopClass>, IAopModel
    {
        [StopWatchInterceptor(OrderIndex = 101)]
        [TransactionInterceptor]
        public void SayHello()
        {
            Console.WriteLine("hello");
        }
    }

    [Component]
    public class AopModel2
    {
        [Autowired]
        public IAopModel AopModel1 { get; set; }
    }
    
}
