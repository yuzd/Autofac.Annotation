using System.ComponentModel;
using Autofac.Annotation.Test;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;

namespace Autofac.Annotation.Benchmark
{
    [SimpleJob(RunStrategy.ColdStart, warmupCount: 0)]
    [MinColumn, MaxColumn, MeanColumn, MedianColumn]
    public class AutowiredResolveBenchmark
    {
        private IContainer _container;


        [GlobalSetup]
        public void Setup()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new AutofacAnnotationModule(typeof(A13).Assembly));
            _container = builder.Build();
        }

        [Benchmark]
        public void AutofacAnnotation()
        {
            var a1 = _container.Resolve<A25>();
            var a2 = a1.A23.GetSchool();
        }
    }
}