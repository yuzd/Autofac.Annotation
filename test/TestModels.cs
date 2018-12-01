using System;
using System.Collections.Generic;
using Autofac.Features.AttributeFilters;
using Castle.DynamicProxy;

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
        [Autowired("A13", Required = false)] public B b1 { get; set; }

        [Autowired("A13",Required = false)]
        public B b2 { get; set; }

        [Autowired("A13",Required = false)]
        private B b3 { get; set; }
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

    public interface IA23
    {
        string GetSchool();
    }

    [Bean(typeof(IA23),Interceptor = typeof(AsyncInterceptor))]
    public class A23:IA23
    {
        public A23([Value("name")]string name)
        {
            Name = name;
        }


        public  string GetSchool()
        {
            return "cc";
        }


        public string Name { get; set; }

        [Autowired]
        public A21 A21 { get; set; }

        [Autowired]
        private A21 A221;

        [Value("test")]
        private string ttt;
    }

    [Bean(Interceptor = typeof(AsyncInterceptor),InterceptorType = InterceptorType.Class )]
    public class A24
    {
        public A24([Value("name")]string name,[Autowired]A21 a21)
        {
            Name = name;
            A21 = a21;
        }


        public virtual  string GetSchool()
        {
            return "cc";
        }


        public string Name { get; set; }
        public A21 A21 { get; set; }
    }

    [Bean(Interceptor = typeof(AsyncInterceptor),InterceptorType = InterceptorType.Class,InterceptorKey = "log2")]
    public class A25
    {
        public A25([Value("name")]string name)
        {
            Name = name;
        }


        public virtual  string GetSchool()
        {
            return "cc";
        }


        public string Name { get; set; }
        
        [Value("ddd")]
        public string Test { get; set; }
        [Autowired]
        public A21 A21 { get; set; }

        [Autowired]
        public IA23 A23 { get; set; }
    }

    
    public class A26
    {
        [Autowired]
        private IA23 A23;
    }
    public class A262:A26
    {
        
    }
    [Bean]
    public class A263:A262
    {
        public void say()
        {

        }
    }
    
    [Bean()]
    public class A27
    {
        [Value("aaaaa")]
        public string Test { get; set; }
    }
    
    [Bean(AutofacScope = AutofacScope.SingleInstance )]
    public class A272
    {
        [Autowired]
        public A27 a27;
    }
    
     
    [Bean(AutofacScope= AutofacScope.SingleInstance)]
    public class A282
    {
        [Value("aaaaa")]
        public string Test { get; set; }
    }
    
    [Bean(AutofacScope = AutofacScope.InstancePerDependency)]
    public class A28
    {
        [Value("aaaaa")]
        public string Test { get; set; }
        
        [Autowired]
        public A282 A282;    
    }
    
    [Bean(InitMethod = "start",DestroyMetnod = "destroy")]
    public class A29
    {
        [Value("aaaaa")]
        public string Test { get; set; }

        void start()
        {
            this.Test = "bbbb";
        }

        void destroy()
        {
            this.Test = null;
        }
    }
    
    [Bean(InitMethod = "start",DestroyMetnod = "destroy")]
    public class A30
    {
        [Value("aaaaa")]
        public string Test { get; set; }

        public A29 a29;

        void start(IComponentContext context)
        {
            this.Test = "bbbb";
            a29 = context.Resolve<A29>();
        }

        void destroy()
        {
            this.Test = null;
            a29.Test = null;
        }
    }
    
    
    public class A311
    {
        
    }

    [Bean]
    public class A31
    {
        [Autowired("A311")]
        public A311 A311 { get; set; }
    }
    
//    [Bean]
    public class A3122
    {
        public string Name { get; set; } = "A3122";
    }
    
    [Bean(typeof(A3122),"A3211")]
    public class A321:A3122
    {
        public new string Name { get; set; } = "A321";
    }
    
    [Bean(typeof(A3122),"A3212")]
    public class A322:A3122
    {
        public new string Name { get; set; } = "A322";
    }
    
    [Bean(typeof(A3122),"A3213")]
    public class A323:A3122
    {
        public new string Name { get; set; } = "A323";
    }
    [Bean(typeof(A3122),"A3213")]
    public class A324:A3122
    {
        public new string Name { get; set; } = "A324";
    }
    
    [Bean]
    public class A32
    {
        [Autowired]
        public IEnumerable<A3122> A31List { get; set; }
    }
    
    
    [Bean]
    public class A33
    {
        [Autowired("A3213")]
        public IEnumerable<A3122> A31List { get; set; }
    }
    
    [Bean]
    public class A34
    {
        [Autowired]
        public IEnumerable<Lazy<A3122>> A31List { get; set; }
    }
    
    [Bean]
    public class A35
    {
        [Autowired("A3213")]
        public IEnumerable<Lazy<A3122>> A31List { get; set; }
    }
}
