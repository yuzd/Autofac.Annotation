using System;
using System.Runtime.InteropServices;
using Autofac.Annotation.Condition;
using Autofac.Core.Registration;

namespace Autofac.Annotation.Test.test10
{

    [AutoConfiguration]
    public class Test10Config
    {
        [Bean]
        [Conditional(typeof(Test10Condition))]//在mac下系统不被注册
        public virtual Test10Model1 getTest10Model1()
        {
            Console.WriteLine("registered Test10Model1");
            return new Test10Model1();
        }
        
        [Bean]
        [Conditional(typeof(Test10Condition2))]//在mac系统下才被注册
        public virtual Test10Model2 getTest10Model2()
        {
            Console.WriteLine("registered Test10Model2");
            return new Test10Model2();
        }
        
        [Bean]
        [ConditionOnMissingBean(typeof(Test10Model1))]
        public virtual Test10Model3 getTest10Model3()
        {
            //mac下不会注册Test10Model1 所以可以注册Test10Model3
            Console.WriteLine("registered Test10Model3");
            return new Test10Model3();
        }
        
        [Bean]
        [ConditionOnMissingBean(typeof(Test10Model2))]
        public virtual Test10Model4 getTest10Model4()
        {
            //mac下会注册Test10Model2 所以不注册Test10Model4
            Console.WriteLine("registered Test10Model4");
            return new Test10Model4();
        }
        
        [Bean]
        [ConditionOnBean(typeof(Test10Model3))]
        public virtual Test10Model5 getTest10Model5()
        {
            //mac下会注册Test10Model3 所以可以注册Test10Model5
            Console.WriteLine("registered Test10Model5");
            return new Test10Model5();
        }
        
        [Bean]
        [ConditionOnClass("Autofac.Annotation.Test.test10.Test10Model2,Autofac.Configuration.Test")]
        public virtual Test10Model6 getTest10Model6()
        {
            //找的到class 所以可以注册Test10Model6
            Console.WriteLine("registered Test10Model6");
            return new Test10Model6();
        }
        
        [Bean]
        [ConditionOnMissingClass("Autofac.Annotation.Test.test10.Test10Model2,xxxx")]
        public virtual Test10Model7 getTest10Model7()
        {
            //找不到class 所以注册Test10Model7
            Console.WriteLine("registered Test10Model7");
            return new Test10Model7();
        }
    }

    public class Test10Condition : ICondition
    {
        /// <summary>
        /// 只有当 windows 系统下才被注册
        /// </summary>
        /// <param name="context"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public bool match(IComponentRegistryBuilder context, object metadata)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return true;
            }
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return true;
            }
            
            return false;
        }
    }
    
    public class Test10Condition2 : ICondition
    {
        /// <summary>
        /// 只有当 osx 系统下才被注册
        /// </summary>
        /// <param name="context"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public bool match(IComponentRegistryBuilder context, object metadata)
        {
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return false;
            }
            
            return true;
        }
    }
    public class Test10Model1
    {
        public string Hello { get; set; } = "Test10Model1";
    }
    public class Test10Model2
    {
        public string Hello { get; set; } = "Test10Model2";
    }

    public class Test10Model3
    {
        public string Hello { get; set; } = "Test10Model3";

    }
    
    public class Test10Model4
    {
        public string Hello { get; set; } = "Test10Model4";

    }
    public class Test10Model5
    {
        public string Hello { get; set; } = "Test10Model5";

    }
    
    public class Test10Model6
    {
        public string Hello { get; set; } = "Test10Model6";

    }
    
    public class Test10Model7
    {
        public string Hello { get; set; } = "Test10Model7";

    }
}