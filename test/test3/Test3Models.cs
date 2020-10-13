using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Autofac.Annotation;
using Autofac.Annotation.Test;
using Autofac.Aspect;
using Autofac.Aspect.Advice;
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
    [Annotation.Aspect]
    public class TestModel9
    {

        public static List<string> testResult = new List<string>();
        
        [TestHelloBefor]
        public virtual void Say()
        {
            testResult.Add("Say");
            Console.WriteLine("say");
        }

        [TestHelloAfter]
        public virtual void SayAfter()
        {
            testResult.Add("SayAfter");
            Console.WriteLine("SayAfter");
        }

       
    }

    [Component]
    [Annotation.Aspect]
    public class TestModel911
    {
        public static List<string> testResult = new List<string>();
        [TestHelloBefor]
        [TestHelloAfterThrowing(typeof(ArgumentException))]//这个进不去 因为 指定的异常 和 抛出去的异常类型不一致
        public virtual void Say()
        {   
            testResult.Add("Say");
            Console.WriteLine("say");
            throw new Exception("ddd");
        }

        [TestHelloAfter]
        [TestHelloAfterThrowing]
        public virtual void SayAfter()
        {
           
            testResult.Add("SayAfter");
            Console.WriteLine("SayAfter");
            throw new ArgumentException("ddd");
        }

    }
    public class TestHelloBefor : AspectBefore
    {
        public override Task Before(AspectContext aspectContext)
        {
            TestModel91.testResult.Add("TestHelloBefor");
            TestModel911.testResult.Add("TestHelloBefor");
            TestModel9.testResult.Add("TestHelloBefor");
            var aa1 = aspectContext.ComponentContext.Resolve<TestModel81>();
            Console.WriteLine("TestHelloBefor");
            return Task.CompletedTask;
        }
    }

    public class TestHelloAfter : AspectAfterReturn
    {
        public override Task AfterReturn(AspectContext aspectContext, object result)
        {
            TestModel91.testResult.Add("TestHelloAfter");
            TestModel911.testResult.Add("TestHelloAfter");
            TestModel9.testResult.Add("TestHelloAfter");
            Console.WriteLine("TestHelloAfter");
            return Task.CompletedTask;
        }
    }

    public class TestHelloAfterThrowing : AspectAfterThrows
    {
        /// <summary>
        /// 
        /// </summary>
        public TestHelloAfterThrowing()
        {
            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        public TestHelloAfterThrowing(Type e)
        {
            ExceptionType = e;
        }
        
        
        public override Type ExceptionType { get; } 

        public override Task AfterThrows(AspectContext aspectContext, Exception exception)
        {
            TestModel911.testResult.Add("TestHelloAfterThrowing");
            var ex = exception as ArgumentException;
            Console.WriteLine(ex.Message);
            return Task.CompletedTask;
        }
    }

    [Component]
    [Annotation.Aspect]
    public class TestModel91
    {
        public static List<string> testResult = new List<string>();
        
        [TestHelloBefor]
        public virtual async Task Say()
        {
            Console.WriteLine("say");
            await Task.Delay(1000);
            testResult.Add("Say");
        }

        [TestHelloAfter]
        public virtual async Task<string> SayAfter()
        {
            Console.WriteLine("SayAfter");
            await Task.Delay(1000);
            testResult.Add("SayAfter");
            return "SayAfter";
        }

       
    }

    [Component]
    [Annotation.Aspect]
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

    }

    //[Component(Interceptor = typeof(Log))]
    [Annotation.Aspect]
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
    [Annotation.Aspect]
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

    public class StopWatchInterceptor : AspectArround
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
    
    
    public class TransactionInterceptor : AspectArround
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
    [Annotation.Aspect(InterceptorType.Interface)]
    [StopWatchInterceptor(GroupName = "a2",OrderIndex = 100)]
    public class AopModel1 : BaseRepository<AopClass>, IAopModel
    {
        [StopWatchInterceptor(GroupName = "a3",OrderIndex = 101)]
        [TransactionInterceptor(GroupName = "a1")]
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
    public class AdviceArroundTest1:AspectArround
    {
        public override async Task OnInvocation(AspectContext aspectContext, AspectDelegate _next)
        {
            AdviseModel1.testModel.Add("Arround1-start");
            await _next(aspectContext);
            AdviseModel1.testModel.Add("Arround1-end");
        }
    }

    public class AdviceArroundTest2:AspectArround
    {
        public override async Task OnInvocation(AspectContext aspectContext, AspectDelegate _next)
        {
            AdviseModel1.testModel.Add("Arround2-start");
            await _next(aspectContext);
            AdviseModel1.testModel.Add("Arround2-end");
        }
    }
    public class AdviceBeforeTest1:AspectBefore
    {
        public override Task Before(AspectContext aspectContext)
        {
            AdviseModel1.testModel.Add("Before1");
            Console.WriteLine("AdviceBeforeTest1");
            return Task.CompletedTask;
        }
    }
    public class AdviceAfterTest1:AspectAfter
    {
        public override Task After(AspectContext aspectContext,object returnValue)
        {
            AdviseModel1.testModel.Add("After1");
            Console.WriteLine("After1");
            return Task.CompletedTask;
        }
    }
    
    public class AdviceAfterTest2:AspectAfter
    {
        public override Task After(AspectContext aspectContext,object returnValue)
        {
            AdviseModel1.testModel.Add("After2");
            Console.WriteLine("After2");
            return Task.CompletedTask;
        }
    }
    public class AdviceAfterReturnTest1:AspectAfterReturn
    {

        public override Task AfterReturn(AspectContext aspectContext, object result)
        {
            AdviseModel1.testModel.Add("AfterReturn1");
            Console.WriteLine("AdviceAfterTest1");
            return Task.CompletedTask;
        }
    }

    public class AdviceBeforeTest2:AspectBefore
    {
        public override Task Before(AspectContext aspectContext)
        {
            AdviseModel1.testModel.Add("Before2");
            Console.WriteLine("AdviceBeforeTest2");
            return Task.CompletedTask;
        }
    }
    public class AdviceAfterReturnTest2:AspectAfterReturn
    {

        public override Task AfterReturn(AspectContext aspectContext, object result)
        {
            AdviseModel1.testModel.Add("AfterReturn2");
            Console.WriteLine("AdviceAfterTest2");
            return Task.CompletedTask;
        }
    }
    public class AdviceAfterThrowsTest1:AspectAfterThrows
    {
        public override Task AfterThrows(AspectContext aspectContext, Exception exception)
        {
            AdviseModel1.testModel.Add("throw1");
            Console.WriteLine(exception.Message);
            return Task.CompletedTask;
        }
    }
    public class AdviceAfterThrowsTest2:AspectAfterThrows
    {
        public override Task AfterThrows(AspectContext aspectContext, Exception exception)
        {
            AdviseModel1.testModel.Add("throw2");
            Console.WriteLine(exception.Message);
            return Task.CompletedTask;
        }
    }
    
    [Component]
    [Annotation.Aspect]
    public class AdviseModel1
    {
        public static List<string> testModel = new List<string>();
        
        [AdviceArroundTest1,AdviceBeforeTest1,AdviceAfterReturnTest1,AdviceAfterThrowsTest1]
        public virtual void TestArroundBeforeAfter()
        {
            AdviseModel1.testModel.Add("TestArroundBeforeAfter");
        }
        
        [AdviceArroundTest1]
        [AdviceBeforeTest1]
        [AdviceAfterTest1]
        [AdviceAfterReturnTest1]
        [AdviceAfterThrowsTest1]
        public virtual void TestArroundBeforeThrows()
        {
            AdviseModel1.testModel.Add("TestArroundBeforeThrows");
            throw new Exception("dddddddddd");
        }
        
        
        [AdviceBeforeTest1(GroupName = "a1")]
        [AdviceBeforeTest2(GroupName = "a2")]
        public virtual void TestMuiltBefore()
        {
            AdviseModel1.testModel.Add("TestMuiltBefore");
        }
        
        [AdviceAfterReturnTest1(GroupName = "a1")]
        [AdviceAfterReturnTest2(GroupName = "a2")]
        public virtual void TestMuiltAfter()
        {
            AdviseModel1.testModel.Add("TestMuiltAfter");
        }
        
        [AdviceAfterThrowsTest1(GroupName = "a1")]
        [AdviceAfterThrowsTest2(GroupName = "a2")]
        public virtual void TestMuiltThrows()
        {
            AdviseModel1.testModel.Add("TestMuiltThrows");
            throw new Exception("dddddddddd");
        }
        
        [AdviceArroundTest1(GroupName = "a1")]
        [AdviceAfterReturnTest1(GroupName = "a1")]
        [AdviceAfterTest1(GroupName = "a1")]
        [AdviceBeforeTest1(GroupName = "a1")]
        [AdviceArroundTest2(GroupName = "a2")]
        [AdviceBeforeTest2(GroupName = "a2")]
        [AdviceAfterReturnTest2(GroupName = "a2")]
        [AdviceAfterTest2(GroupName = "a2")]
        public virtual void TestMuiltBeforeAfter()
        {
            AdviseModel1.testModel.Add("TestMuiltBeforeAfter");
        }
    }
    
}
