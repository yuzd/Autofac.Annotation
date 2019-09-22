using System;
using System.Collections.Generic;
using System.Text;
using Autofac.Annotation.Anotation;

namespace Autofac.Configuration.Test.test3
{

    [AutoConfiguration]
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
    }
}
