using System;
using Autofac.Annotation;

namespace Autofac.Configuration.Test.test3
{
    public class TestImportSelector:ImportSelector
    {
        public BeanDefination[] SelectImports()
        {
            return new[]
            {
                new BeanDefination(typeof(TestImport1))
            };
        }
    }


    public interface ITestImport
    {
        void Test();
    }
    public class TestImport1:ITestImport
    {
        public string Name { get; set; } = nameof(TestImport1);
        public void Test()
        {
            Console.WriteLine("test");
        }
    }
}