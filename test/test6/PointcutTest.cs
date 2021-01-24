using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Autofac.Annotation.Test.test6
{
    /// <summary>
    /// 第一组切面
    /// </summary>
    [Pointcut(NameSpace = "Autofac.Annotation.Test.test6",Class = "Pointcut*",OrderIndex = 1)]
    public class PointcutTest1
    {
        [Around]
        public async Task Around(AspectContext context,AspectDelegate next)
        {
            Pointcut1Controller.testResult.Add("PointcutTest1.Around-start");
            Pointcut2Controller.testResult.Add("PointcutTest1.Around-start");
            PointcutAnotationTest1.testResult.Add("PointcutTest1.Around-start");
            await next(context);
            PointcutAnotationTest1.testResult.Add("PointcutTest1.Around-end");
            Pointcut1Controller.testResult.Add("PointcutTest1.Around-end");
            Pointcut2Controller.testResult.Add("PointcutTest1.Around-end");
        }

        [Before]
        public void Before()
        {
            Pointcut1Controller.testResult.Add("PointcutTest1.Before");
            Pointcut2Controller.testResult.Add("PointcutTest1.Before");
            PointcutAnotationTest1.testResult.Add("PointcutTest1.Before");
            
        }
        
        [After]
        public void After()
        {
            Pointcut1Controller.testResult.Add("PointcutTest1.After");
            Pointcut2Controller.testResult.Add("PointcutTest1.After");
            PointcutAnotationTest1.testResult.Add("PointcutTest1.After");
            
        }
        
        [AfterReturn(Returing = "value1")]
        public void AfterReturn(object value1)
        {
            Pointcut1Controller.testResult.Add("PointcutTest1.AfterReturn");
            Pointcut2Controller.testResult.Add("PointcutTest1.AfterReturn");
            PointcutAnotationTest1.testResult.Add("PointcutTest1.AfterReturn");
        }
        
        [AfterThrows(Throwing = "ex1")]
        public void Throwing(Exception ex1)
        {
            Pointcut1Controller.testResult.Add("PointcutTest1.Throwing");
            Pointcut2Controller.testResult.Add("PointcutTest1.Throwing");
            PointcutAnotationTest1.testResult.Add("PointcutTest1.Throwing");
        }
    }
    
    /// <summary>
    /// 第二组切面
    /// </summary>
    [Pointcut(NameSpace = "Autofac.Annotation.Test.test6",Class = "Pointcut*",OrderIndex = 0)]
    public class PointcutTest2
    {
        [Around]
        public async Task Around(AspectContext context,AspectDelegate next)
        {
            Pointcut1Controller.testResult.Add("PointcutTest2.Around-start");
            Pointcut2Controller.testResult.Add("PointcutTest2.Around-start");
            PointcutAnotationTest1.testResult.Add("PointcutTest2.Around-start");
            await next(context);
            Pointcut1Controller.testResult.Add("PointcutTest2.Around-end");
            Pointcut2Controller.testResult.Add("PointcutTest2.Around-end");
            PointcutAnotationTest1.testResult.Add("PointcutTest2.Around-end");
        }

        [Before]
        public void Before()
        {
            Pointcut1Controller.testResult.Add("PointcutTest2.Before");
            Pointcut2Controller.testResult.Add("PointcutTest2.Before");
            PointcutAnotationTest1.testResult.Add("PointcutTest2.Before");
        }
        
        [After]
        public void After()
        {
            Pointcut1Controller.testResult.Add("PointcutTest2.After");
            Pointcut2Controller.testResult.Add("PointcutTest2.After");
            PointcutAnotationTest1.testResult.Add("PointcutTest2.After");
        }
        
        [AfterReturn(Returing = "value")]
        public void AfterReturn(object value)
        {
            Pointcut1Controller.testResult.Add("PointcutTest2.AfterReturn");
            Pointcut2Controller.testResult.Add("PointcutTest2.AfterReturn");
            PointcutAnotationTest1.testResult.Add("PointcutTest2.AfterReturn");
        }
        
        [AfterThrows(Throwing = "ex")]
        public void Throwing(Exception ex)
        {
            Pointcut1Controller.testResult.Add("PointcutTest2.Throwing");
            Pointcut2Controller.testResult.Add("PointcutTest2.Throwing");
            PointcutAnotationTest1.testResult.Add("PointcutTest2.Throwing");
        }
    }
    
    [Component]
    public class Pointcut1Controller
    {
        public static List<string> testResult = new List<string>();
        /// <summary>
        /// 
        /// </summary>
        public virtual void TestSuccess()
        {
            Pointcut1Controller.testResult.Add("Pointcut1Controller.TestSuccess");
        }
        
        public virtual void TestThrow()
        {
            Pointcut1Controller.testResult.Add("Pointcut1Controller.TestThrow");
            throw new ArgumentException("ddd");
        }
    }
    
    [Component]
    public class Pointcut2Controller
    {
        public static List<string> testResult = new List<string>();
        public virtual string TestSuccess()
        {
            Pointcut2Controller.testResult.Add("Pointcut2Controller.TestSuccess");
            return "abc";
        }
        
        public virtual void TestThrow()
        {
            Pointcut2Controller.testResult.Add("Pointcut2Controller.TestThrow");
            throw new ArgumentException("ddd");
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequestWatch : Attribute
    {
        public int Timeout { get; }

        public RequestWatch(int timeout)
        {
            Timeout = timeout;
        }
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequestWatch2 : Attribute
    {
        public int Timeout { get; }

        public RequestWatch2(int timeout)
        {
            Timeout = timeout;
        }
    }

    /// <summary>
    /// 对于打了RequestWatch的进行切面，如果在class上打了 那么这个class下所有的方法都会被切面 如果只在method上打了 那就这个method会被切面
    /// </summary>
    [Pointcut(NameSpace = "Autofac.Annotation.Test.test6",AttributeType = typeof(RequestWatch),OrderIndex = 1)]
    public class PointcutTest3
    {
        
        [Around]
        public async Task Around(AspectContext context,AspectDelegate next,RequestWatch requestWatch)
        {
            PointcutAnotationTest1.testResult.Add("PointcutTest3.Around-start");
            await next(context);
            PointcutAnotationTest1.testResult.Add("PointcutTest3.Around-end");
        }
    
    }
    
    [Pointcut(NameSpace = "Autofac.Annotation.Test.test6",AttributeType = typeof(RequestWatch2))]
    public class PointcutTest4
    {
        
        [Around]
        public async Task Around(AspectContext context,AspectDelegate next,RequestWatch2 requestWatch)
        {
            PointcutAnotationTest1.testResult.Add("PointcutTest4.Around-start");
            await next(context);
            PointcutAnotationTest1.testResult.Add("PointcutTest4.Around-end");
        }
    }
    
    [Component]
    public class PointcutAnotationTest1
    {
        public static List<string> testResult = new List<string>();
        
        [RequestWatch(2000)]
        [RequestWatch2(1000)]
        public virtual string TestSuccess()
        {
            PointcutAnotationTest1.testResult.Add("PointcutAnotationTest1.TestSuccess");
            return "abc";
        }
        
        [RequestWatch(3000)]
        [RequestWatch2(2000)]
        public virtual void TestThrow()
        {
            PointcutAnotationTest1.testResult.Add("PointcutAnotationTest1.TestThrow");
            throw new ArgumentException("ddd");
        }
    }
    
    [Component]
    public class PointcutAnotationTest3
    {
        [RequestWatch(3000)]
        [RequestWatch2(2000)]
        public virtual async ValueTask<string> Test(string dd)
        {
            if(dd == "hello") return "ddd";

            await Task.Delay(1000);
            PointcutAnotationTest1.testResult.Add("Test");
            return "aaaa";
        }
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequestWatch3 : Attribute
    {
        public int Timeout { get; }

        public RequestWatch3(int timeout)
        {
            Timeout = timeout;
        }
    }
    [Pointcut(NameSpace = "Autofac.Annotation.Test.test6",AttributeType = typeof(RequestWatch3))]
    public class PointcutTest5
    {
        
        [Around]
        public async Task Around(AspectContext context,AspectDelegate next,RequestWatch3 requestWatch)
        {
            PointcutAnotationTest1.testResult.Add("PointcutTest4.Around-start");
            await next(context);
            PointcutAnotationTest1.testResult.Add("PointcutTest4.Around-end");
        }
    }
    
    [Component]
    public class ValueTaskAnotationTest4
    {
        [RequestWatch3(2000)]
        public virtual async ValueTask<string> Test(string dd)
        {
            if(dd == "hello") return "ddd";

            await Task.Delay(1000);
            PointcutAnotationTest1.testResult.Add("Test");
            return "aaaa";
        }
        
        [RequestWatch3(2000)]
        public virtual async ValueTask Test2(string dd)
        {
            if(dd == "hello") return ;

            await Task.Delay(1000);
            PointcutAnotationTest1.testResult.Add("Test");
        }
    }
}