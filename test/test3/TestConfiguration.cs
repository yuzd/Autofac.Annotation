using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Autofac.Annotation;

namespace Autofac.Configuration.Test.test3
{

//    [PropertySource("/file/appsettings1.json")]
    [AutoConfiguration]
    [Import(typeof(TestImportSelector))]
    public class TestConfiguration
    {
        [Bean]
        public virtual TestModel3 getTest3()
        {
            return new TestModel3
            {
                Name = "aa"
            };
        }

        [Bean("getTest31")]
        public virtual TestModel3 getTest31()
        {
            return new TestModel3
            {
                Name = "aa1"
            };
        }

        [Bean]
        public virtual TestModel4Parent getTest4()
        {
            return new TestModel4
            {
                Name = "getTest4"
            };
        }

        [Bean]
        public virtual ITestModel4 getTest5()
        {
            return new TestModel4
            {
                Name = "getTest5"
            };
        }
        [Bean(nameof(getTest61))]
        public virtual ITestModel5 getTest61()
        {
            return new TestModel5
            {
                Name = "getTest61"
            };
        }
        
        [Bean(nameof(getTest62))]
        public virtual ITestModel5 getTest62()
        {
            return new TestModel5
            {
                Name = "getTest62"
            };
        }


        [Bean]
        public virtual TestModel88 getTest7(TestModel99 testModel99,[Value("${a9}")] string test)
        {
            return new TestModel88
            {
                Name = testModel99.Name + test
            };
        }
        
        [Bean(nameof(getTest63))]
        public virtual async Task<ITestModel5> getTest63()
        {
            return await Task.FromResult<ITestModel5>(new TestModel5
            {
                Name = "getTest63"
            });
        }
        
        [Bean]
        public virtual TestModel1000 getTestModel1000()
        {
            return new TestModel1000()
            {
                Name = "getTestModel1000"
            };
        }
        
        [Bean]
        public virtual TestModel1001 getTestModel10001()
        {
            var model = getTestModel1000();//不会实例化2次的！
            return new TestModel1001()
            {
                Name = "getTestModel10001",
                TestModel1000 = model
            };
        }
    }


    [AutoConfiguration]
    public class TestConfiguration2
    {
        [Bean]
        public virtual TestModel1002 getTest3()
        {
            return new TestModel1002();
        }
    }
}
