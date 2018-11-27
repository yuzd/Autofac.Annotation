using System.Collections.Generic;
using Autofac.Features.AttributeFilters;

namespace Autofac.Annotation.Test
{
    [Bean]
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

    [Bean(Services = new []{typeof(B)})]
    public class A1:B
    {
        public string School { get; set; } = "测试";

        public override string GetSchool()
        {
            return this.School;
        }
    }

    [Bean(Services = new []{typeof(B)},Keys = new []{"B1"})]
    public class A2:B
    {
        public string School { get; set; } = "测试1";

        public override string GetSchool()
        {
            return this.School;
        }
    }
    [Bean(Services = new []{typeof(B)},Keys = new []{"B2"})]
    public class A3:B
    {
        public string School { get; set; } = "测试2";

        public override string GetSchool()
        {
            return this.School;
        }
    }

    [Bean("a4")]
    public class A4
    {
        public string School { get; set; } = "测试2";
    }

    [Bean(typeof(B),"a5")]
    public class A5:B
    {
        public string School { get; set; } = "测试a5";
        public override string GetSchool()
        {
            return this.School;
        }
    }

    [Bean(typeof(B))]
    public class A6:B
    {
        public string School { get; set; } = "测试a6";
        public override string GetSchool()
        {
            return this.School;
        }
    }

    [Bean(typeof(B))]
    public class A7:B
    {
        public string School { get; set; } = "测试a7";
        public override string GetSchool()
        {
            return this.School;
        }
    }

    [Bean]
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
    [Bean]
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

    [Bean]
    [PropertySource("/file/appsettings1.json")]
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

    [Bean]
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

    [Bean(typeof(A12),Services = new[] { typeof(B) }, Keys = new[] { "A12" })]
    public class A12 : B
    {
        public string School { get; set; } = "A12";

        public override string GetSchool()
        {
            return this.School;
        }
    }
    [Bean(typeof(A13),"aa12", Services = new[] { typeof(B) }, Keys = new[] { "A13" })]
    public class A13 : B
    {
        public string School { get; set; } = "A13";

        public override string GetSchool()
        {
            return this.School;
        }
    }

    [Bean(typeof(A14), "aa14", Services = new[] { typeof(B) })]
    public class A14 : B
    {
        public string School { get; set; } = "A14";

        public override string GetSchool()
        {
            return this.School;
        }
    }

    [Bean]
    public class A15
    {
        [Value("#{a9}")]
        public string test = "t";

        [Value("#{a9}")]
        public string School { get; set; } = "A14";

    }

    [Bean]
    public class A16
    {
        [Autowired("A13")]
        public B b1;
        public string School { get; set; } = "A16";

        [Autowired]
        public B B { get; set; }
    }

    [Bean]
    public class A17
    {
        [Autowired("A131111")]
        public B b1;
        public string School { get; set; } = "A17";

        [Autowired]
        public B B { get; set; }
    }

    [Bean]
    public class A18
    {
        [Autowired("adadada",Required = false)]
        public B b1;
        public string School { get; set; } = "A18";

        [Autowired]
        public B B { get; set; }
    }

    public class A19
    {
        [Autowired("A13",Required = false)]
        public B b1;
       
        [Autowired("A13",Required = false)]
        public B b2;

        [Autowired("A13",Required = false)]
        private B b3;
    }

    [Bean]
    public class A20:A19
    {
        [Value("aaaa")]
        public string Name { get; set; }
    }

    [Bean]
    public class A21:A20
    {
       
    }

    [Bean]
    public class A22
    {
        public A22([Value("name")]string name,[Autowired]A21 a21)
        {
            Name = name;
            A21 = a21;
        }

        public string Name { get; set; }
        public A21 A21 { get; set; }
    }
}