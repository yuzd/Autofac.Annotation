using System;
using System.Collections.Generic;
using System.Text;
using Autofac.Annotation;
using Autofac.Annotation.Test;

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

}
