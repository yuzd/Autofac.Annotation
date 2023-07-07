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
        [Conditional(typeof(Test10Condition))] //在mac下系统不被注册
        public virtual Test10Model1 getTest10Model1()
        {
            Console.WriteLine("registered Test10Model1");
            return new Test10Model1();
        }

        [Bean]
        [Conditional(typeof(Test10Condition2))] //在mac系统下才被注册
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

        [Bean]
        [ConditionalOnProperty("onproperty", "on")]
        public virtual Test10Model8 getTest10Model8()
        {
            Console.WriteLine("registered Test10Model8");
            return new Test10Model8();
        }

        [Bean]
        [ConditionalOnProperty("onproperty", "off")]
        public virtual Test10Model9 getTest10Model9()
        {
            Console.WriteLine("registered Test10Model9");
            return new Test10Model9();
        }

        [Bean]
        [ConditionalOnProperty("onproperty1", matchIfMissing = true)]
        public virtual Test10Model10 getTest10Model10()
        {
            Console.WriteLine("registered Test10Model10");
            return new Test10Model10();
        }

        [Bean]
        [ConditionalOnProperty("onproperty", matchIfMissing = true)]
        public virtual Test10Model11 getTest10Model11()
        {
            Console.WriteLine("registered Test10Model11");
            return new Test10Model11();
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
        public bool ShouldSkip(IComponentRegistryBuilder context, object metadata)
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
        public bool ShouldSkip(IComponentRegistryBuilder context, object metadata)
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

    public class Test10Model8
    {
        public string Hello { get; set; } = "Test10Model8";
    }

    public class Test10Model9
    {
        public string Hello { get; set; } = "Test10Model9";
    }

    public class Test10Model10
    {
        public string Hello { get; set; } = "Test10Model10";
    }

    public class Test10Model11
    {
        public string Hello { get; set; } = "Test10Model11";
    }

    public class Test10Model12
    {
        public string Hello { get; set; } = "Test10Model12";
    }

    [AutoConfiguration]
    [ConditionalOnProperty("onproperty", "fff")]
    public class AutoConfTest1
    {
        [Bean]
        public virtual Test10Model12 getTest10Model12()
        {
            Console.WriteLine("registered Test10Model12");
            return new Test10Model12();
        }
    }

    [Component]
    [ConditionalOnProperty("onproperty", "fff")]
    public class AutoConfTest12
    {
    }

    [Component]
    [ConditionalOnProperty("onproperty1")] // 存在onproperty1就注册
    public class AutoConfTest122
    {
    }

    [Component]
    [ConditionalOnProperty("onproperty2")] // 存在onproperty2就注册
    public class AutoConfTest1222
    {
    }

    [Component]
    [ConditionalOnProperties(new[] { "onproperty1" })] // 存在onproperty1就注册
    public class AutoConfTestMulti1
    {
    }

    [Component]
    [ConditionalOnProperties(new[] { "onproperty2" })] // 存在onproperty2就注册
    public class AutoConfTestMulti2
    {
    }

    [Component]
    [ConditionalOnProperties(new[] { "onproperty", "onproperty2" })] // 存在onproperty2就注册
    public class AutoConfTestMulti3
    {
    }

    [Component]
    [ConditionalOnProperties(new[] { "onproperty", "onproperty1" })] // 存在onproperty2就注册
    public class AutoConfTestMulti4
    {
    }

    [Component]
    [ConditionOnClass("Autofac.Annotation.Test.test10.Test10Model2,Autofac.Configuration.Test")]
    public class AutoConfTest13
    {
    }

    [Component]
    [ConditionOnMissingClass("Autofac.Annotation.Test.test10.Test10Model2,Autofac.Configuration.Test")]
    public class AutoConfTest14
    {
    }
}