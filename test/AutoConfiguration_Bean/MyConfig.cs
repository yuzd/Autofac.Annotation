using System;
using System.Collections.Generic;
using System.Text;
using Autofac.Annotation;
using Autofac.Annotation.Anotation;

namespace Autofac.Configuration.Test.AutoConfiguration_Bean
{
    [Annotation.Anotation.AutoConfiguration("Test")]
    public class MyConfig
    {
        [Bean]
        public MyModel GetMyModel()
        {
            return new MyModel
            {
                Name = "yuzd"
            };
        }
        [Bean]
        public MyModel2 GetMyModel2()
        {
            return new MyModel2
            {
                Name = "yuzd21"
            };
        }
    }

    [Annotation.Anotation.AutoConfiguration("Test2")]
    public class MyConfig2
    {
        [Bean]
        public MyModel GetMyModel()
        {
            return new MyModel
            {
                Name = "yuzd2"
            };
        }
        [Bean]
        public MyModel2 GetMyModel2()
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
        [Autowired]
        public IEnumerable<MyModel2> MyModel2 { get; set; }
    }
}
