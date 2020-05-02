using Xunit;

namespace Autofac.Annotation.Test.test6
{
    public class UnitTest6
    {
        [Fact]
        public void Test_Type_01()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(UnitTest6).Assembly));

            var container = builder.Build();
                
            var a1 = container.Resolve<LogAspectTest1>();
            var a2 = container.Resolve<LogAspectTest1>();
            var a3 = container.Resolve<LogAspectT1est>();
           
            //被拦截
            a1.Test1();
            //不会被拦截
            a1.Test2();
            
            //被拦截
            a2.Test1();
            //不会被拦截
            a2.Test2();

            //不会拦截
            a3.Test1();
            a3.Test2();


        }
        
        [Fact]
        public void Test_Type_02()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(UnitTest6).Assembly));

            var container = builder.Build();
                
            var a1 = container.Resolve<IAspectA>();
            var a2 = container.Resolve<IAspecB>();
           
            a1.Hello("ddd");
            var a111 = a1.Hello2("ssss");
            
            a2.Hello("ddd2");
            var a1112 = a2.Hello2("ssss2");

        }
        
        [Fact]
        public void Test_Type_03()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(UnitTest6).Assembly));

            var container = builder.Build();
                
            var a1 = container.Resolve<LogAroundTest>();
           
            a1.Hello("ddd");
            var a111 = a1.Hello2("ssss");
            

        }
    }
}