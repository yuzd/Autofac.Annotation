using System;
using BenchmarkDotNet.Running;

namespace Autofac.Annotation.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<AutowiredResolveBenchmark>();
        }
    }
}