using System.ComponentModel;
using Autofac.Annotation.Test;
using Autofac.Core;
using Autofac.Features.AttributeFilters;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Castle.DynamicProxy;

namespace Autofac.Annotation.Benchmark
{
    [SimpleJob(RunStrategy.ColdStart, warmupCount: 0)]
    [MinColumn, MaxColumn, MeanColumn, MedianColumn]
    public class AutofacAutowiredResolveBenchmark
    {
        private IContainer _container;

        [GlobalSetup]
        public void Setup()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<A13>().As<B>().WithAttributeFiltering();
            builder.RegisterType<Log>().As<AsyncInterceptor>();
            builder.RegisterType<Log2>().Keyed<AsyncInterceptor>("log2");
            builder.RegisterType<A21>().WithAttributeFiltering().PropertiesAutowired();
            builder.RegisterType<A23>().As<IA23>().WithAttributeFiltering().PropertiesAutowired().EnableInterfaceInterceptors()
                .InterceptedBy(typeof(AsyncInterceptor));
            builder.RegisterType<A25>().WithAttributeFiltering().PropertiesAutowired().EnableClassInterceptors()
                .InterceptedBy(new KeyedService("log2", typeof(AsyncInterceptor)));
            _container = builder.Build();
        }

        [Benchmark]
        public void Autofac()
        {
            var a1 = _container.Resolve<A25>();
            var a2= a1.A23.GetSchool();
        }
    }
}