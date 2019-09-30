using System;
using System.Collections.Generic;
using System.Text;
using Autofac.Annotation;
using Autofac.Annotation.Anotation;

namespace Autofac.Configuration.Test.test3
{

    [Annotation.Anotation.AutoConfiguration]
    public class TestConfiguration
    {
        [Bean]
        public TestModel3 getTest3()
        {
            return new TestModel3
            {
                Name = "aa"
            };
        }

        [Bean("getTest31")]
        public TestModel3 getTest31()
        {
            return new TestModel3
            {
                Name = "aa1"
            };
        }

        [Bean]
        public TestModel4Parent getTest4()
        {
            return new TestModel4
            {
                Name = "getTest4"
            };
        }

        [Bean]
        private ITestModel4 getTest5()
        {
            return new TestModel4
            {
                Name = "getTest5"
            };
        }
        [Bean(nameof(getTest61))]
        private ITestModel5 getTest61()
        {
            return new TestModel5
            {
                Name = "getTest61"
            };
        }
        [Bean(nameof(getTest62))]
        private ITestModel5 getTest62()
        {
            return new TestModel5
            {
                Name = "getTest62"
            };
        }


        [Bean]
        private TestModel88 getTest7(TestModel99 testModel99,[Value("#{a9}")] string test)
        {
            return new TestModel88
            {
                Name = testModel99.Name + test
            };
        }
    }
}
