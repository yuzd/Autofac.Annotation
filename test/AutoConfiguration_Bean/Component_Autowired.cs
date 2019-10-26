using System;
using System.Collections.Generic;
using System.Text;
using Autofac.Annotation;
using Autofac.Configuration.Test.AutoConfiguration_Bean;
using Xunit;

namespace Autofac.Configuration.Test.Component_Autowired
{
    public class Component_Autowired
    {
        [Fact]
        public void Test_Type_01()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(Student).Assembly));
            var container = builder.Build();
            var a1 = container.Resolve<Student7>();
            container.Dispose();
            
            Assert.NotNull(a1);
        }
    }

    [Component]
    public class Student
    {

    }

    public class Person
    {

    }

    [Component]
    public class Student2: Person
    {

    }

    public interface ISay
    {
        void SayHello();
    }

    [Component(typeof(ISay), "Student3")]
    public class Student3 : ISay
    {
        public void SayHello()
        {
            Console.WriteLine("hello");
        }
    }
    [Component(typeof(ISay), "Student4")]
    public class Student4 : ISay
    {
        public void SayHello()
        {
            Console.WriteLine("hello");
        }
    }

    [Component(AutofacScope = AutofacScope.SingleInstance ,AutoActivate = true)]
    public class Student5
    {
        public string Name { get; set; }
        public Student5()
        {
            this.Name = "yuzd" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Console.WriteLine($"{nameof(Student)}.constructor");
        }
    }

    [Component(InitMethod = nameof(Student6.Start),DestroyMethod = nameof(Student6.Stop))]
    public class Student6
    {
        public void Start()
        {
            Console.WriteLine($"{nameof(Student6.Start)} invoked!");
        }

        public void Stop()
        {
            Console.WriteLine($"{nameof(Student6.Stop)} invoked!");
        }
    }
    
    [Component(InitMethod = nameof(Student7.Start),DestroyMethod = nameof(Student7.Stop))]
    public class Student7
    {
        public void Start([Value("${a9}")] string a9,[Autowired] Student5 student5)
        {
            Console.WriteLine($"{nameof(Student7.Start)} invoked! + " + a9 + "+ " + student5.Name);
        }

        public void Stop()
        {
            Console.WriteLine($"{nameof(Student7.Stop)} invoked!");
        }
    }
    
    [Component]
    public class Student8
    {
        private Student6 _student6;

        [Autowired]
        private Student7 _student7;

        [Autowired]
        public Student5 Student5 { get; set; }
        
        public Student8([Autowired] Student6 student6)
        {
            _student6 = student6;
        }
    }
}