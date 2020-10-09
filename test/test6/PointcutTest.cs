using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac.Aspect;

namespace Autofac.Annotation.Test.test6
{
    /// <summary>
    /// 第一组切面
    /// </summary>
    [Pointcut(Class = "Pointcut*",OrderIndex = 1)]
    public class PointcutTest1
    {
        [Around]
        public async Task Around(AspectContext context,AspectDelegate next)
        {
            Pointcut1Controller.testResult.Add("PointcutTest1.Around-start");
            Pointcut2Controller.testResult.Add("PointcutTest1.Around-start");
            await next(context);
            Pointcut1Controller.testResult.Add("PointcutTest1.Around-end");
            Pointcut2Controller.testResult.Add("PointcutTest1.Around-end");
        }

        [Before]
        public void Before()
        {
            Pointcut1Controller.testResult.Add("PointcutTest1.Before");
            Pointcut2Controller.testResult.Add("PointcutTest1.Before");
        }
        
        [After(Returing = "value")]
        public void After(object value)
        {
            Pointcut1Controller.testResult.Add("PointcutTest1.After");
            Pointcut2Controller.testResult.Add("PointcutTest1.After");
        }
        
        [Throwing(Throwing = "ex")]
        public void Throwing(Exception ex)
        {
            Pointcut1Controller.testResult.Add("PointcutTest1.Throwing");
            Pointcut2Controller.testResult.Add("PointcutTest1.Throwing");
        }
    }
    
    /// <summary>
    /// 第二组切面
    /// </summary>
    [Pointcut(Class = "Pointcut*",OrderIndex = 0)]
    public class PointcutTest2
    {
        [Around]
        public async Task Around(AspectContext context,AspectDelegate next)
        {
            Pointcut1Controller.testResult.Add("PointcutTest2.Around-start");
            Pointcut2Controller.testResult.Add("PointcutTest2.Around-start");
            await next(context);
            Pointcut1Controller.testResult.Add("PointcutTest2.Around-end");
            Pointcut2Controller.testResult.Add("PointcutTest2.Around-end");
        }

        [Before]
        public void Before()
        {
            Pointcut1Controller.testResult.Add("PointcutTest2.Before");
            Pointcut2Controller.testResult.Add("PointcutTest2.Before");
        }
        
        [After(Returing = "value")]
        public void After(object value)
        {
            Pointcut1Controller.testResult.Add("PointcutTest2.After");
            Pointcut2Controller.testResult.Add("PointcutTest2.After");
        }
        
        [Throwing(Throwing = "ex")]
        public void Throwing(Exception ex)
        {
            Pointcut1Controller.testResult.Add("PointcutTest2.Throwing");
            Pointcut2Controller.testResult.Add("PointcutTest2.Throwing");
        }
    }
    
    [Component]
    public class Pointcut1Controller
    {
        public static List<string> testResult = new List<string>();
        /// <summary>
        /// 
        /// </summary>
        public void TestSuccess()
        {
            Pointcut1Controller.testResult.Add("Pointcut1Controller.TestSuccess");
        }
        
        public void TestThrow()
        {
            Pointcut1Controller.testResult.Add("Pointcut1Controller.TestThrow");
            throw new ArgumentException("ddd");
        }
    }
    
    [Component]
    public class Pointcut2Controller
    {
        public static List<string> testResult = new List<string>();
        public void TestSuccess()
        {
            Pointcut2Controller.testResult.Add("Pointcut2Controller.TestSuccess");
        }
        
        public void TestThrow()
        {
            Pointcut2Controller.testResult.Add("Pointcut2Controller.TestThrow");
        }
    }
}