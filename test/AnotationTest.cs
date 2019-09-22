using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Core;
using Autofac.Extras.DynamicProxy;
using Autofac.Features.AttributeFilters;
using Autofac.Features.Metadata;
using Castle.DynamicProxy;
using Xunit;

namespace Autofac.Annotation.Test
{
    public class AnotationTest
    {
        [Fact]
        public void Test_Type_01()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly));

            var container = builder.Build();

            var a1 = container.Resolve<A>();

            Assert.NotNull(a1);
        }

        [Fact]
        public void Test_Type_02()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly));

            var container = builder.Build();

            var a1 = container.Resolve<B>();

            Assert.NotNull(a1);

            Assert.Equal("A14", a1.GetSchool());
        }

        [Fact]
        public void Test_Type_03()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly));

            var container = builder.Build();

            var a1 = container.ResolveKeyed<B>("B1");
            var a2 = container.ResolveKeyed<B>("B2");

            Assert.NotNull(a1);
            Assert.NotNull(a2);

            Assert.Equal("测试1", a1.GetSchool());
            Assert.Equal("测试2", a2.GetSchool());
        }

        [Fact]
        public void Test_Type_04()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly));

            var container = builder.Build();

            var a1 = container.ResolveKeyed<A4>("a4");
            var ab = container.ResolveKeyed<B>("a5");
            var a6 = container.Resolve<B>();

            Assert.NotNull(a1);
            Assert.NotNull(ab);
            Assert.NotNull(a6);
            Assert.Equal("测试a5", ab.GetSchool());
        }


        [Fact]
        public void Test_Type_05()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly));

            var container = builder.Build();


            var a1 = container.Resolve<A8>();
            var a2 = container.Resolve<A9>();
            Assert.Equal(4, a2.list.Count);
            Assert.Equal(2, a2.dic.Keys.Count);
            Assert.Equal("a8", a1.GetSchool());
            Assert.Equal("aaaaaaaaa", a2.GetSchool());
        }

        [Fact]
        public void Test_Type_06()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly));

            var container = builder.Build();


            var a2 = container.Resolve<A10>();
            Assert.Equal(3, a2.list.Count);
            Assert.Single(a2.dic.Keys);
            Assert.Equal("aaaaaaaaa1", a2.GetSchool());
        }

        [Fact]
        public void Test_Type_07()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly));

            var container = builder.Build();


            var a2 = container.Resolve<A11>();
            Assert.Equal(3, a2.list.Count);
            Assert.Single(a2.dic.Keys);
            Assert.Equal("aaaaaaaaa1", a2.GetSchool());
        }

        [Fact]
        public void Test_Type_08()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly));

            var container = builder.Build();


            var a2 = container.Resolve<A12>();
            var a3 = container.ResolveKeyed<B>("A12");
            Assert.Equal("A12", a2.GetSchool());
            Assert.Equal("A12", a3.GetSchool());
        }

        [Fact]
        public void Test_Type_09()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly));

            var container = builder.Build();


            var a2 = container.ResolveKeyed<A13>("aa12");
            var a3 = container.ResolveKeyed<B>("A13");
            Assert.Equal("A13", a2.GetSchool());
            Assert.Equal("A13", a3.GetSchool());
        }


        [Fact]
        public void Test_Type_10()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly));

            var container = builder.Build();


            var a2 = container.ResolveKeyed<A14>("aa14");
            var a3 = container.Resolve<B>();
            container.TryResolveKeyed("aa14", typeof(B), out object aa);
            Assert.Null(aa);
            Assert.Equal("A14", a2.GetSchool());
            Assert.Equal("A14", a3.GetSchool());
        }

        [Fact]
        public void Test_Type_11()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly));

            var container = builder.Build();


            var a3 = container.Resolve<A15>();
            Assert.Equal("aaaaaaaaa", a3.School);
            Assert.Equal("aaaaaaaaa", a3.test);
        }

        [Fact]
        public void Test_Type_12()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly));

            var container = builder.Build();


            var a3 = container.Resolve<A16>();
            Assert.NotNull(a3.B);
            Assert.NotNull(a3.b1);
            Assert.Equal("A13", a3.b1.GetSchool());
        }

        [Fact]
        public void Test_Type_13()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly));

            var container = builder.Build();

            Assert.Throws<DependencyResolutionException>(() => container.Resolve<A17>());
        }


        [Fact]
        public void Test_Type_14()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly));

            var container = builder.Build();

            var a1 = container.Resolve<A18>();
            Assert.Null(a1.b1);
        }

        [Fact]
        public void Test_Type_15()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly));

            var container = builder.Build();

            var a1 = container.Resolve<A20>();
            Assert.NotNull(a1.b1);
            Assert.NotNull(a1.b2);
        }

        [Fact]
        public void Test_Type_16()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly));

            var container = builder.Build();

            var a1 = container.Resolve<A21>();
            Assert.NotNull(a1.b1);
            Assert.NotNull(a1.b2);
            Assert.Equal("aaaa", a1.Name);
        }

        [Fact]
        public void Test_Type_17()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly));

            var container = builder.Build();

            var a1 = container.Resolve<A22>();
            Assert.NotNull(a1.A21);
            Assert.Equal("name", a1.Name);
        }

        [Fact]
        public void Test_Type_18()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly));

            var container = builder.Build();

            var a1 = container.Resolve<IA23>();
            var aaa = a1.GetSchool();
            Assert.Equal("a", aaa);
        }


        [Fact]
        public void Test_Type_19()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly));

            var container = builder.Build();

            var a1 = container.Resolve<A24>();
            var aaa = a1.GetSchool();
            Assert.Equal("a", aaa);
        }

        [Fact]
        public void Test_Type_20()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly));

            var container = builder.Build();

            var a1 = container.Resolve<A25>();
            Assert.NotNull(a1);
            Assert.Equal("name", a1.Name);
            Assert.Equal("ddd", a1.Test);
            var aaa = a1.GetSchool();
            Assert.NotNull(a1.A23);
            var a2 = a1.A23.GetSchool();
            Assert.Equal("b", aaa);
        }


        [Fact]
        public void Test_Type_21()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly));

            var container = builder.Build();

            var a1 = container.Resolve<A263>();
            a1.say();
        }

        [Fact]
        public void Test_Type_22()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly));

            var container = builder.Build();

            var a1 = container.Resolve<A272>();
            Assert.NotNull(a1);
            Assert.NotNull(a1.a27);
            Assert.Equal("aaaaa", a1.a27.Test);
            a1.a27.Test = "bbbbb";

            //单例模式 属性都变成了单例的了
            var a2 = container.Resolve<A272>();
            Assert.True(a1 == a2);
            Assert.Equal("bbbbb", a2.a27.Test);
        }

        [Fact]
        public void Test_Type_23()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly));

            var container = builder.Build();

            var a1 = container.Resolve<A28>();
            Assert.NotNull(a1);
            Assert.NotNull(a1.A282);
            Assert.Equal("aaaaa", a1.A282.Test);
            Assert.Equal("aaaaa", a1.Test);
            a1.A282.Test = "bbbbb";
            a1.Test = "bbbbb";

            var a2 = container.Resolve<A28>();
            Assert.False(a1 == a2);
            Assert.Equal("bbbbb", a2.A282.Test);
            Assert.Equal("aaaaa", a2.Test);
        }

        [Fact]
        public void Test_Type_24()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly).SetDefaultAutofacScopeToInstancePerLifetimeScope());

            var container = builder.Build();

            var a1 = container.Resolve<A28>();
            Assert.NotNull(a1);
            Assert.NotNull(a1.A282);
            Assert.Equal("aaaaa", a1.A282.Test);
            Assert.Equal("aaaaa", a1.Test);
            a1.A282.Test = "bbbbb";
            a1.Test = "bbbbb";

            var a2 = container.Resolve<A28>();
            Assert.True(a1 == a2);
            Assert.Equal("bbbbb", a2.A282.Test);
            Assert.Equal("bbbbb", a2.Test);
        }

        [Fact]
        public void Test_Type_25()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly).SetDefaultAutofacScopeToInstancePerLifetimeScope());

            var container = builder.Build();

            A29 a29 = null;
            using (var scope = container.BeginLifetimeScope())
            {
                a29 = scope.Resolve<A29>();
                Assert.Equal("bbbb", a29.Test);
            }

            Assert.NotNull(a29);
            Assert.Null(a29.Test);
        }

        [Fact]
        public void Test_Type_26()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly).SetDefaultAutofacScopeToInstancePerLifetimeScope());

            var container = builder.Build();

            A30 a30 = null;
            using (var scope = container.BeginLifetimeScope())
            {
                a30 = scope.Resolve<A30>();
                Assert.Equal("bbbb", a30.Test);
                Assert.Equal("bbbb", a30.a29.Test);
            }

            Assert.NotNull(a30);
            Assert.NotNull(a30.a29);
            Assert.Null(a30.Test);
            Assert.Null(a30.a29.Test);
        }

        [Fact]
        public void Test_Type_27()
        {
            var builder = new ContainerBuilder();
            
            builder.RegisterType<A13>().As<B>().WithAttributeFiltering();
            builder.RegisterType<Log>().As<AsyncInterceptor>();
            builder.RegisterType<Log2>().Keyed<AsyncInterceptor>("log2");
            builder.RegisterType<A21>().WithAttributeFiltering().PropertiesAutowired();
            builder.RegisterType<A23>().As<IA23>().WithAttributeFiltering().PropertiesAutowired().EnableInterfaceInterceptors().InterceptedBy(typeof(AsyncInterceptor));
            builder.RegisterType<A25>().WithAttributeFiltering().PropertiesAutowired().EnableClassInterceptors().InterceptedBy(new KeyedService("log2",typeof(AsyncInterceptor)));
            var _container = builder.Build();
            var a1 = _container.Resolve<A25>();
            var a2 = a1.A23.GetSchool();
        }
        
        [Fact]
        public void Test_Type_28()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly).SetDefaultAutofacScopeToInstancePerLifetimeScope());

            var container = builder.Build();

            var exception = Assert.Throws<DependencyResolutionException>(() =>
            {
                container.Resolve<A31>();
            });
            
            Assert.Contains("property name:A311",exception.Message);
        }
        
        [Fact]
        public void Test_Type_29()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly));

//            builder.RegisterType<A321>().As<A3122>().WithMetadata("tt", "A3211");
//            builder.RegisterType<A322>().As<A3122>().WithMetadata("tt", "A3212");
//            builder.RegisterType<A323>().As<A3122>().WithMetadata("tt", "A3213");
            
             // builder.RegisterType(typeof(A321)).Keyed("A3211",typeof(A3122)).As(typeof(A321)).WithMetadata("tt", "A3211");
             // builder.RegisterType(typeof(A322)).Keyed("A3212", typeof(A3122)).As(typeof(A322)).WithMetadata("tt", "A3211");
             // builder.RegisterType(typeof(A323)).Keyed("A3213", typeof(A3122)).As(typeof(A323)).WithMetadata("tt", "A3211");



              //builder.RegisterType(typeof(A321)).As(new KeyedService("A3211",typeof(A3122))).Named(typeof(A3122).FullName,typeof(A3122));
              //builder.RegisterType(typeof(A322)).As(new KeyedService("A3212", typeof(A3122))).Named(typeof(A3122).FullName, typeof(A3122));
              //builder.RegisterType(typeof(A323)).As(new KeyedService("A3213", typeof(A3122))).Named(typeof(A3122).FullName, typeof(A3122));


            //            builder.RegisterType(typeof(A321)).As(new TypedService(typeof(A3122))).WithMetadata("tt", "A3211");

            //            builder.RegisterType(typeof(A322)).As(new TypedService(typeof(A3122))).WithMetadata("tt", "A3212");

            //            builder.RegisterType(typeof(A323)).As(new TypedService(typeof(A3122))).WithMetadata("tt", "A3213");

            var container = builder.Build();
            
//            var warrior = container.Resolve<IEnumerable<Meta<A3122>>>();

            //var b = container.Resolve<IEnumerable<Meta<A3122>>>();
            
            var a = container.ResolveKeyed<IEnumerable<A3122>>(typeof(A3122).FullName);
            var a1 = container.ResolveKeyed<IEnumerable<A3122>>("A3213");
            var a11 = container.ResolveKeyed<A3122>("A3213");
            var a111 = container.Resolve<A32>();

            Assert.Equal(4,a111.A31List.Count());

        }

        [Fact]
        public void Test_Type_30()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly));

            
            var container = builder.Build();
            
            
            var a = container.Resolve<A33>();
            
            Assert.Equal(2,a.A31List.Count());

        }
        
        
        [Fact]
        public void Test_Type_31()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
           // builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly));

//            builder.RegisterType<A321>().As<A3122>().WithMetadata("tt", "A3211");
//            builder.RegisterType<A322>().As<A3122>().WithMetadata("tt", "A3212");
//            builder.RegisterType<A323>().As<A3122>().WithMetadata("tt", "A3213");
            
            builder.RegisterType<A321>().As<A3122>().Keyed<A3122>("A3211");
            builder.RegisterType<A322>().As<A3122>().Keyed<A3122>("A3212");
            builder.RegisterType<A323>().As<A3122>().Keyed<A3122>("A3213");
            
//            builder.RegisterType(typeof(A321)).As(new KeyedService("A3211",typeof(A3122)));
////            builder.RegisterType(typeof(A321)).As(new TypedService(typeof(A3122)));
//            builder.RegisterType(typeof(A322)).As(new KeyedService("A3212",typeof(A3122)));
////            builder.RegisterType(typeof(A322)).As(new TypedService(typeof(A3122)));
//            builder.RegisterType(typeof(A323)).As(new KeyedService("A3213",typeof(A3122)));
////            builder.RegisterType(typeof(A323)).As(new TypedService(typeof(A3122)));
//            
            var container = builder.Build();
            
            var warrior = container.Resolve<IEnumerable<A3122>>();


        }
        
        [Fact]
        public void Test_Type_32()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly));

            
            var container = builder.Build();
            
            
            var a = container.Resolve<A35>();
            var b = container.Resolve<A34>();
            
            Assert.Equal(2,a.A31List.Count());
            Assert.Equal(4,b.A31List.Count());
        
        }
        
        [Fact]
        public void Test_Type_33()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly).SetAllowCircularDependencies(true));

            
            var container = builder.Build();
            
            
            var a = container.Resolve<A37>();
            Assert.NotNull(a);
            Assert.NotNull(a.A36);

        }

        [Fact]
        public void Test_Type_34()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly).SetAllowCircularDependencies(true));


            var container = builder.Build();


            var a = container.Resolve<AImpl>();
            var a1 = container.Resolve<A1>();
            Assert.NotNull(a);
            Assert.NotNull(a1);

        }

        [Fact]
        public void Test_Type_35()
        {
            var builder = new ContainerBuilder();

            // autofac打标签模式
            builder.RegisterModule(new AutofacAnnotationModule(typeof(AnotationTest).Assembly).SetAllowCircularDependencies(true));


            var container = builder.Build();

            var b = container.ResolveKeyed<IA>("AImpl2");
            var a = container.Resolve<IA>();
            var a1 = container.Resolve<AImpl2>();
            Assert.NotNull(a);
            Assert.NotNull(a1);
            Assert.NotNull(b);
            Assert.NotEqual(b,a);
            Assert.Equal(b.GetType(),a1.GetType());

        }
    }
}