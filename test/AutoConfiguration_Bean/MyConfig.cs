using System;
using System.Collections.Generic;
using System.Text;
using Autofac.Annotation;

namespace Autofac.Configuration.Test.AutoConfiguration_Bean
{
    [AutoConfiguration("Test")]
    public class MyConfig
    {
        [Bean]
        public virtual MyModel GetMyModel()
        {
            return new MyModel
            {
                Name = "yuzd"
            };
        }
        [Bean]
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
        [Bean]
        public virtual MyModel GetMyModel()
        {
            return new MyModel
            {
                Name = "yuzd2"
            };
        }
        [Bean]
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
        [Autowired]
        public IEnumerable<MyModel2> MyModel2 { get; set; }
    }
}
