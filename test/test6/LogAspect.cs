using System;
using System.Reflection;
using Autofac.Aspect;

namespace Autofac.Annotation.Test.test6
{
    [Pointcut(ClassName = "LogAspectTest?",MethodName = "Test1")]
    [Pointcut("name1",ClassName = "LogAspectT1est",MethodName = "Test1")]
    [Pointcut("name2",ClassName = "LogAspect[ABC]")]
    [Pointcut("name3",ClassName = "LogAroundTest",MethodName="Hello*")]
    public class LogAspect
    {
        [Before]
        public void Before()
        {
            Console.WriteLine("Before");
        }
        
        [After]
        public void After()
        {
            Console.WriteLine("After");
        }
        
        [Before("name1")]
        public void Before1()
        {
            Console.WriteLine("Before1");
        }
        
        [After("name1")]
        public void After1()
        {
            Console.WriteLine("After1");
        }
        
        
        [Before("name2")]
        public void Before2()
        {
            Console.WriteLine("Before2");
        }
        
        [After("name2")]
        public void After2()
        {
            Console.WriteLine("After2");
        }

        [Around("name3")]
        public void Around(AspectContext context)
        {
            Console.WriteLine(context.InvocationContext.MethodInvocationTarget.Name + "-->Start");
            context.InvocationProceedInfo.Invoke();
            context.InvocationContext.ReturnValue = "around";
            Console.WriteLine(context.InvocationContext.MethodInvocationTarget.Name + "-->End");
        }
    }

    [Component]
    public class LogAspectTest1
    {
        public virtual void Test1()
        {
            Console.WriteLine("Test1");
        }
        
        public virtual void Test2()
        {
            Console.WriteLine("Test2");
        }
    }
    
    [Component]
    public class LogAspectTest2
    {
        public virtual void Test1()
        {
            Console.WriteLine("Test1");
        }
        
        public virtual void Test2()
        {
            Console.WriteLine("Test2");
        }
    }
    
    
    [Component]
    public class LogAspectT1est
    {
        public virtual void Test1()
        {
            Console.WriteLine("Test1");
        }
        
        public virtual void Test2()
        {
            Console.WriteLine("Test2");
        }
    }
    
    public interface IAspectA
    {
        void Hello(string msg);
        string Hello2(string msg);
    }

    [Component]
    public class LogAspectA : IAspectA
    {
        public void Hello(string msg)
        {
            Console.WriteLine(msg);
        }

        public string Hello2(string msg)
        {
            return msg;
        }
    }
    public interface IAspecB
    {
        void Hello(string msg);
        string Hello2(string msg);
    }
    [Component]
    public class LogAspectB : IAspecB
    {
        public void Hello(string msg)
        {
            Console.WriteLine(msg);
        }
        
        public string Hello2(string msg)
        {
            return msg;
        }
    }
    
    [Component]
    public class LogAroundTest
    {
        public virtual void Hello(string msg)
        {
            Console.WriteLine(msg);
        }
        
        public virtual string Hello2(string msg)
        {
            return msg;
        }
    }

}