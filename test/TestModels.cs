using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac.AspectIntercepter.Advice;
using Autofac.Features.AttributeFilters;
using Castle.DynamicProxy;

namespace Autofac.Annotation.Test
{
    public interface IAA
    {
        void Test1();
    }
    public interface IA:IAA
    {
        void Test();
    }

    public class ABA: IA
    {
        public void Test1()
        {}

        public void Test()
        {
        }
    }

    [Component]
    public class AImpl : ABA
    {
        public void Test()
        {
            Console.WriteLine("1");
        }

        public void Test1()
        {
        }
    }
    [Component("AImpl2")]
    public class AImpl2 : ABA
    {
        public void Test()
        {
            Console.WriteLine("1");
        }

        public void Test1()
        {
        }
    }
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
        public A9([Value("${a9}")]string school,[Value("${list}")]int[] list,[Value("#{${dic}}")]Dictionary<string,string> dic)
        {
            this.School = school;
            this.list = list;
            this.dic = dic;

        }
        public string School { get; set; } = "测试a9";
        public int[] list { get; set; } 
        public Dictionary<string,string> dic { get; set; } 
        public string GetSchool()
        {
            return this.School;
        }
    }

    [Component]
    [PropertySource("/file/appsettings1.json")]
    public class A10
    {
        public A10([Value("${a10}")]string school,[Value("${list}")]int[] list,[Value("#{${dic}}")]Dictionary<string,string> dic,[Value("")]int dddd)
        {
            this.School = school;
            this.list = list;
            this.dic = dic;

        }
        public string School { get; set; }
        public int[] list { get; set; } 
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
        public A11([Value("${a11}")]string school,[Value("${list}")]List<int> list,[Value("#{${dic}}")]Dictionary<string,string> dic,[Value("")]int dddd)
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

    [Component(typeof(A12),Services = new[] { typeof(B) }, Keys = new[] { "A12" })]
    public class A12 : B
    {
        public string School { get; set; } = "A12";

        public override string GetSchool()
        {
            return this.School;
        }
    }
    [Component(typeof(A13),"aa12", Services = new[] { typeof(B) }, Keys = new[] { "A13" })]
    public class A13 : B
    {
        public string School { get; set; } = "A13";

        public override string GetSchool()
        {
            return this.School;
        }
    }

    [Component(typeof(A14), "aa14", Services = new[] { typeof(B) })]
    public class A14 : B
    {
        public string School { get; set; } = "A14";

        public override string GetSchool()
        {
            return this.School;
        }
    }

    [Component]
    public class A15
    {
        [Value("${a9}")]
        public string test = "t";

        [Value("${a9}")]
        public string School { get; set; } = "A14";

    }

    [Component]
    public class A16
    {
        [Autowired("A13")]
        public B b1;
        public string School { get; set; } = "A16";

        [Autowired]
        public B B { get; set; }
    }

    [Component]
    public class A17
    {
        [Autowired("A131111")]
        public B b1;
        public string School { get; set; } = "A17";

        [Autowired]
        public B B { get; set; }
    }

    [Component]
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
        [Autowired("A13", Required = false)]
        public B b1 { get; set; }

        [Autowired("A13",Required = false)]
        public B b2 { get; set; }

        //[Autowired("A13",Required = false)]
        //private B b3 { get; set; }
    }

    [Component]
    public class A20:A19
    {
        //[Value("aaaa")]
        public string Name { get; set; }
    }

    [Component]
    public class A21:A20
    {
       
    }

    [Component]
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

    [Component(Interceptor = typeof(AsyncInterceptor),InterceptorType = InterceptorType.Interface)]
    public class A23:IA23
    {
        //public A23([Value("name")]string name)
        //{
        //    Name = name;
        //}


        public  string GetSchool()
        {
            return "cc";
        }


        public string Name { get; set; }

        [Autowired]
        public A21 A21 { get; set; }

        //[Autowired]
        //private A21 A221;

        //[Value("test")]
        private string ttt;
    }

    [Component(Interceptor = typeof(AsyncInterceptor) )]
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

    [Component(Interceptor = typeof(AsyncInterceptor),InterceptorKey = "log2")]
    public class A25
    {
        //public A25([Value("name")]string name)
        //{
        //    Name = name;
        //}

        public A25()
        {
        }


        public virtual  string GetSchool()
        {
            return "cc";
        }


        public string Name { get; set; }
        
        //[Value("ddd")]
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
    [Component]
    public class A263:A262
    {
        public void say()
        {

        }
    }
    
    [Component()]
    public class A27
    {
        [Value("aaaaa")]
        public string Test { get; set; }
    }
    
    [Component(AutofacScope = AutofacScope.SingleInstance )]
    public class A272
    {
        [Autowired]
        public A27 a27;
    }
    
     
    [Component(AutofacScope= AutofacScope.SingleInstance)]
    public class A282
    {
        [Value("aaaaa")]
        public string Test { get; set; }
    }
    
    [Component(AutofacScope = AutofacScope.InstancePerDependency)]
    public class A28
    {
        [Value("aaaaa")]
        public string Test { get; set; }
        
        [Autowired]
        public A282 A282;    
    }
    
    [Component(InitMethod = "start",DestroyMethod = "destroy")]
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
    
    [Component(InitMethod = "start",DestroyMethod = "destroy")]
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

    [Component]
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
    
    [Component(typeof(A3122),"A3211")]
    public class A321:A3122
    {
        public new string Name { get; set; } = "A321";
    }
    
    [Component(typeof(A3122),"A3212")]
    public class A322:A3122
    {
        public new string Name { get; set; } = "A322";
    }
    
    [Component(typeof(A3122),"A3213")]
    public class A323:A3122
    {
        public new string Name { get; set; } = "A323";
    }
    [Component(typeof(A3122), "A3213")]
    public class A324:A3122
    {
        public new string Name { get; set; } = "A324";
    }
    
    [Component]
    public class A32
    {
        [Autowired()]
        public IList<A3122> A31List { get; set; }
    }
    
    
    [Component]
    public class A33
    {
        [Autowired("A3213")]
        public IEnumerable<A3122> A31List { get; set; }
    }
    
    [Component]
    public class A34
    {
        [Autowired]
        public IEnumerable<Lazy<A3122>> A31List { get; set; }
    }
    
    [Component]
    public class A35
    {
        [Autowired("A3213")]
        public IEnumerable<Lazy<A3122>> A31List { get; set; }
    }
    
    public class A36
    {
        
    }
    
    [Component(typeof(A36),"A3611")]
    public class A3611:A36
    {
      
        
        [Autowired("A3612")]
        public A36 A3612 { get; set; }
        
        
        public override string ToString()
        {
            return "A3611";
        }
    }
    
    [Component(typeof(A36),"A3612")]
    public class A3612:A36
    {
        [Autowired]
        public A38 A38 { get; set; }

        public override string ToString()
        {
            return "A3612";
        }
    }
    [Component]
    public class A37
    {
        [Autowired("A3611")]
        public A36 A36 { get; set; }
        
    }
    [Component]
    public class A38
    {
        [Autowired("A3612")]
        public A36 A36 { get; set; }
        
        [Autowired("A3611")]
        public A36 A3611 { get; set; }
        
        [Autowired]
        public A37 A37 { get; set; }

        public DateTime Now = DateTime.Now;
    }

    [Component(AutofacScope = AutofacScope.SingleInstance)]
    public class A39
    {
        public DateTime Now  = DateTime.Now;
        
        [Autowired]
        public ObjectFactory<A38> A38 { get; set; }
    }

    [Component]
    public class Model1
    {
        
        public override string ToString()
        {
            return nameof(Model1);
        }
    }
    
    //泛型接口
    public interface IMongodbHelp<T> where T : new()
    {
        string GetName();
    }
    
    //泛型方法实现 https://github.com/yuzd/Autofac.Annotation/issues/13
    [Component(typeof(IMongodbHelp<>),InitMethod = "InitMethod")]
    public class MongodbHelp<T> : IMongodbHelp<T> where T : new()
    {
        [Autowired("A3612")]
        public A36 A36 { get; set; }
        
        [Value("aaaaa")]
        public string Test { get; set; }

        public string Now { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        public string GetName()
        {
            return (new T()).ToString();
        }

        public void InitMethod()
        {
            var aa = 1;
            
        }
        
    }
    [Component(typeof(TestMongodb2<,>))]
    public class TestMongodb2<T1, T2> 
    {
        [Autowired("A3612")]
        public A36 A36 { get; set; }
        
        [Value("aaaaa")]
        public string Test { get; set; }

        public string Now { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
    }

    public interface ITestAutowiredModal
    {
        
    }
    
    [Component("abc")]
    public class TestAutowiredModal1:ITestAutowiredModal
    {
        [Autowired("A3612")]
        public A36 A36 { get; set; }
        
        [Value("aaaaa")]
        public string Test { get; set; }

    }
    [Component("def")]
    public class TestAutowiredModal2:ITestAutowiredModal
    {
        [Autowired("A3612")]
        public A36 A36 { get; set; }
        
        [Value("aaaaa")]
        public string Test { get; set; }
       
    }

    [Component]
    public class TestAutowiredModal3
    {
        [Autowired]
        public ITestAutowiredModal abc { get; set; }
        
        [Autowired]
        public ITestAutowiredModal def { get; set; }
    }
    
    [Component(AutofacScope = AutofacScope.SingleInstance)]
    public class TestAutowiredModal4
    {
        [Autowired]
        public ObjectFactory<ITestAutowiredModal> abc { get; set; }
        
        [Autowired]
        public ObjectFactory<ITestAutowiredModal> def { get; set; }
    }

    [Component]
    public class TestLazyModel1
    {
        [Autowired]
        public Lazy<TestAutowiredModal4> TestAutowiredModal4 { get; set; }
        
        [Autowired]
        public TestAutowiredModal3 TestAutowiredModal3 { get; set; }
    }
    
    [Component]
    public class TestCircular1
    {
        [Autowired]
        public TestCircular2 TestCircular2 { get; set; }
    }
    
    [Component]
    public class TestCircular2
    {
        [Autowired(CircularDependencies = true)]
        public TestCircular1 TestCircular1 { get; set; }
    }
    
    
    [Component]
    public class TestCircular3
    {
        [Autowired]
        public TestCircular4 TestCircular4 { get; set; }
    }
    
    [Component]
    public class TestCircular4
    {
        [Autowired(CircularDependencies = false)]
        public TestCircular3 TestCircular3 { get; set; }
    }

    [Component]
    public class LazyModel1
    {
        [Autowired]
        public Lazy<LazyModel2> LazyModel2 { get; set; }
    }
    
    [Component]
    public class LazyModel2
    {
        public string Name { get; set; } = "LazyModel2";
    }
    
    
    public class TestHelloBefore1:AspectBefore
    {
        public override Task Before(AspectContext aspectContext)
        {
            Console.WriteLine("TestHelloBefore1");
            return Task.CompletedTask;
        }
    }
    
    public class TestHelloAfter1:AspectAfter
    {
        //这个 returnValue 如果目标方法正常返回的话 那就是目标方法的返回值
        // 如果目标方法抛异常的话 那就是异常本身
        public override Task After(AspectContext aspectContext,object returnValue)
        {
            Console.WriteLine("TestHelloAfter1");
            return Task.CompletedTask;
        }
    }
    
    public class TestHelloAfterReturn1:AspectAfterReturn
    {
        //result 是目标方法的返回 (如果目标方法是void 则为null)
        public override Task AfterReturn(AspectContext aspectContext, object result)
        {
            Console.WriteLine("TestHelloAfterReturn1");
            return Task.CompletedTask;
        }
    }
    
    public class TestHelloAround1:AspectArround
    {
        public override async Task OnInvocation(AspectContext aspectContext, AspectDelegate _next)
        {
            Console.WriteLine("TestHelloAround1 start");
            await _next(aspectContext);
            Console.WriteLine("TestHelloAround1 end");
        }
    }
    
    public class TestHelloAfterThrows1:AspectAfterThrows
    {
       
        public override Task AfterThrows(AspectContext aspectContext, Exception exception)
        {
            Console.WriteLine("TestHelloAfterThrows1");
            return Task.CompletedTask;
        }
    }
    
    

    //////////////////////////////////////////////
    public class TestHelloBefore2:AspectBefore
    {
        public override Task Before(AspectContext aspectContext)
        {
            Console.WriteLine("TestHelloBefore2");
            return Task.CompletedTask;
        }
    }
    
    public class TestHelloAfter2:AspectAfter
    {
        //这个 returnValue 如果目标方法正常返回的话 那就是目标方法的返回值
        // 如果目标方法抛异常的话 那就是异常本身
        public override Task After(AspectContext aspectContext,object returnValue)
        {
            Console.WriteLine("TestHelloAfter2");
            return Task.CompletedTask;
        }
    }
    
    public class TestHelloAfterReturn2:AspectAfterReturn
    {
        //result 是目标方法的返回 (如果目标方法是void 则为null)
        public override Task AfterReturn(AspectContext aspectContext, object result)
        {
            Console.WriteLine("TestHelloAfterReturn2");
            return Task.CompletedTask;
        }
    }
    
    public class TestHelloAround2:AspectArround
    {
        public override async Task OnInvocation(AspectContext aspectContext, AspectDelegate _next)
        {
            Console.WriteLine("TestHelloAround2 start");
            await _next(aspectContext);
            Console.WriteLine("TestHelloAround2 end");
        }
    }
    
    public class TestHelloAfterThrows2:AspectAfterThrows
    {
       
        public override Task AfterThrows(AspectContext aspectContext, Exception exception)
        {
            Console.WriteLine("TestHelloAfterThrows2");
            return Task.CompletedTask;
        }
    }
    
    [Component(EnableAspect = true)]
    public class TestHello
    {

        [
            TestHelloAround1(GroupName = "Aspect1",OrderIndex = 10),
            TestHelloBefore1(GroupName = "Aspect1",OrderIndex = 10),
            TestHelloAfter1(GroupName = "Aspect1",OrderIndex = 10),
            TestHelloAfterReturn1(GroupName = "Aspect1",OrderIndex = 10),
            TestHelloAfterThrows1(GroupName = "Aspect1",OrderIndex = 10)
        ]
        [
            TestHelloAround2(GroupName = "Aspect2",OrderIndex = 1),
            TestHelloBefore2(GroupName = "Aspect2",OrderIndex = 1),
            TestHelloAfter2(GroupName = "Aspect2",OrderIndex = 1),
            TestHelloAfterReturn2(GroupName = "Aspect2",OrderIndex = 1),
            TestHelloAfterThrows2(GroupName = "Aspect2",OrderIndex = 1)
        ]
        public virtual void SayGroup()
        {
            Console.WriteLine("Say");
            throw new ArgumentException("exception");
        }
    }
    
    
    
    
}
