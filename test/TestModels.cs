using System.Collections.Generic;
using Autofac.Annotation;

namespace Autofac.Configuration.Test
{
    [Component]
    public class A
    {
        public string Name { get; set; }
    }


    public class B
    {
        public virtual string GetSchool()
        {
            return "B";
        }
    }

    [Component(Services = new []{typeof(B)})]
    public class A1:B
    {
        public string School { get; set; } = "测试";

        public override string GetSchool()
        {
            return this.School;
        }
    }

    [Component(Services = new []{typeof(B)},Keys = new []{"B1"})]
    public class A2:B
    {
        public string School { get; set; } = "测试1";

        public override string GetSchool()
        {
            return this.School;
        }
    }
    [Component(Services = new []{typeof(B)},Keys = new []{"B2"})]
    public class A3:B
    {
        public string School { get; set; } = "测试2";

        public override string GetSchool()
        {
            return this.School;
        }
    }

    [Component("a4")]
    public class A4
    {
        public string School { get; set; } = "测试2";
    }

    [Component(typeof(B),"a5")]
    public class A5:B
    {
        public string School { get; set; } = "测试a5";
        public override string GetSchool()
        {
            return this.School;
        }
    }

    [Component(typeof(B))]
    public class A6:B
    {
        public string School { get; set; } = "测试a6";
        public override string GetSchool()
        {
            return this.School;
        }
    }

    [Component(typeof(B))]
    public class A7:B
    {
        public string School { get; set; } = "测试a7";
        public override string GetSchool()
        {
            return this.School;
        }
    }

    [Component]
    public class A8
    {
        public A8([Value("a8")]string school)
        {
            this.School = school;
        }
        public string School { get; set; } = "测试a7";
        public string GetSchool()
        {
            return this.School;
        }
    }
    [Component]
    public class A9
    {
        public A9([Value("#{a9}")]string school,[Value("#{list}")]List<int> list,[Value("#{dic}")]Dictionary<string,string> dic)
        {
            this.School = school;
            this.list = list;
            this.dic = dic;

        }
        public string School { get; set; } = "测试a9";
        public List<int> list { get; set; } 
        public Dictionary<string,string> dic { get; set; } 
        public string GetSchool()
        {
            return this.School;
        }
    }

    [Component]
    [PropertySource(@"E:\WorkSpace\github\Autofac\Autofac.Annotation\test\file\appsettings1.json")]
    public class A10
    {
        public A10([Value("#{a10}")]string school,[Value("#{list}")]List<int> list,[Value("#{dic}")]Dictionary<string,string> dic,[Value("")]int dddd)
        {
            this.School = school;
            this.list = list;
            this.dic = dic;

        }
        public string School { get; set; }
        public List<int> list { get; set; } 
        public Dictionary<string,string> dic { get; set; } 
        public string GetSchool()
        {
            return this.School;
        }
    }

    [Component]
    [PropertySource("/file/appsettings1.xml")]
    public class A11
    {
        public A11([Value("#{a11}")]string school,[Value("#{list}")]List<int> list,[Value("#{dic}")]Dictionary<string,string> dic,[Value("")]int dddd)
        {
            this.School = school;
            this.list = list;
            this.dic = dic;

        }
        public string School { get; set; }
        public List<int> list { get; set; } 
        public Dictionary<string,string> dic { get; set; } 
        public string GetSchool()
        {
            return this.School;
        }
    }

}