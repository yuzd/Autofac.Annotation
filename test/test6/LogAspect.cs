using System;
using Autofac.Aspect;

namespace Autofac.Annotation.Test.test6
{
    [Pointcut(ClassName = "LogAspectTest?",MethodName = "Test1")]
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

}