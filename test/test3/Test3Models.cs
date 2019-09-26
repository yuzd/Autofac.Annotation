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
}
