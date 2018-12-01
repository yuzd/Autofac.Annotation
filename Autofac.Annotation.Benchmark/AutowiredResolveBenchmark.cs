using System.ComponentModel;
using Autofac.Annotation.Test;
using BenchmarkDotNet.Attributes;

namespace Autofac.Annotation.Benchmark
{
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
        public void Test()
        {
            var a1 = _container.Resolve<A25>();
            var a2= a1.A23.GetSchool();
        }
    }
}