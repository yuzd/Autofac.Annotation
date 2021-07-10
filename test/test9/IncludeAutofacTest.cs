//-----------------------------------------------------------------------
// <copyright file="IncludeAutofacTest .cs" company="Company">
// Copyright (C) Company. All Rights Reserved.
// </copyright>
// <author>nainaigu</author>
// <create>$Date$</create>
// <summary></summary>
//-----------------------------------------------------------------------

using Xunit;

namespace Autofac.Annotation.Test.test9
{

    /// <summary>
    ///  autofac api注册的也能被扫描到
    /// </summary>
    public class IncludeAutofacTest
    {
        
        [Fact]
        public void Test()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<InclueModel1>().InstancePerDependency();
            
            
            builder.RegisterModule(new AutofacAnnotationModule(typeof(IncludeAutofacTest).Assembly));
                  
            var container = builder.Build();
            var inclueModel1 = container.Resolve<InclueModel1>();
            Assert.NotNull(inclueModel1.model2);
            Assert.NotNull(inclueModel1.a9);
        }
        
    }

    //[PropertySource("/file/appsettings1.json")]
    public class InclueModel1
    {
        [Autowired]
        public InclueModel2 model2;

        [Value("${a9}")]
        public string a9;
        
        public string Hello { get; set; } = "world";
    }

    [Component]
    public class InclueModel2
    {
        public string Hello { get; set; } = "world2";
    }
}