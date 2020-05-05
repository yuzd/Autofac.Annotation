using System;
using BenchmarkDotNet.Running;

namespace Autofac.Annotation.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            //var autowiredTest = new AutowiredResolveBenchmark();
            //autowiredTest.Setup();
            //autowiredTest.AutofacAnnotation();


            //var autowiredTest2 = new AutofacAutowiredResolveBenchmark();
            //autowiredTest2.Setup();
            //autowiredTest2.Autofac();

            //BenchmarkRunner.Run<AutofacAutowiredResolveBenchmark>();
            BenchmarkRunner.Run<AutowiredResolveBenchmark>();
           
        }
    }
}