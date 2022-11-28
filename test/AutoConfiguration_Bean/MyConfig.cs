using System;
using System.Collections.Generic;
using Autofac.Annotation;

namespace Autofac.Configuration.Test.AutoConfiguration_Bean
{
    [AutoConfiguration("Test")]
    public class MyConfig
    {
        [Bean(AutofacScope = AutofacScope.SingleInstance)]
        public virtual MyModel GetMyModel()
        {
            return new MyModel
            {
                Name = "yuzd"
            };
        }

        [Bean(AutofacScope = AutofacScope.SingleInstance)]
        public virtual MyModel2 GetMyModel2()
        {
            return new MyModel2
            {
                Name = "yuzd21"
            };
        }
    }

    [AutoConfiguration("Test2")]
    public class MyConfig2
    {
        [Bean(AutofacScope = AutofacScope.SingleInstance)]
        public virtual MyModel GetMyModel()
        {
            return new MyModel
            {
                Name = "yuzd2"
            };
        }

        [Bean(AutofacScope = AutofacScope.SingleInstance)]

        public virtual MyModel2 GetMyModel2()
        {
            return new MyModel2
            {
                Name = "yuzd22"
            };
        }
    }

    public class MyModel
    {
        public string Name { get; set; }
    }

    public class MyModel2
    {
        public string Name { get; set; }
    }

    [Component]
    public class MyModel3
    {
        [Autowired] public IEnumerable<MyModel2> MyModel2 { get; set; }
    }

    [AutoConfiguration]
    public class MyConfig3
    {
        [Bean(AutofacScope = AutofacScope.InstancePerDependency)]
        public virtual MyModel4 GetMyModel()
        {
            return new MyModel4
            {
                Name = DateTime.Now.ToString("HH:mm:ss")
            };
        }

        [Bean(AutofacScope = AutofacScope.InstancePerDependency, InitMethod = nameof(MyModel5.Test), DestroyMethod = nameof(MyModel5.End))]
        public virtual MyModel5 GetMyModel2()
        {
            return new MyModel5
            {
                Name = DateTime.Now.ToString("HH:mm:ss")
            };
        }
    }

    public class MyModel4
    {
        public string Name { get; set; }
    }

    public class MyModel5
    {
        public void Test()
        {
            Name = "test";
        }

        public void End()
        {
            Name = "end";
        }

        public string Name { get; set; }
    }


    [Component]
    public class TestPostConstruct1
    {
        public string Name { get; set; }

        [PostConstruct]
        public void init()
        {
            Name = "test";
        }

        [PreDestory]
        public void end()
        {
            Name = "end";
        }
    }
}