using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Autofac.Annotation;
namespace Autofac.Configuration.Test.test4
{
    [Component]
    public class ValueModel1
    {
        [Value("${a9}")]
        public string A9 { get; set; }
    }


    [Component]
    public class ValueModel2
    {
        [Autowired]
        public ValueModel1 ValueModel1 { get; set; }

        [Value("#{@(Autofac.Configuration.Test.test4.ValueModel1,Autofac.Configuration.Test).A9}")]
        public string CallA9 { get; set; }
    }

    public class ParentValueModel
    {
        public virtual string GetName()
        {
            return nameof(ParentValueModel);
        }
    }

    [Component(typeof(ParentValueModel),"key32")]
    public class ValueModel32: ParentValueModel
    {
        [Value("${a9}_ValueModel32")]
        public string A9 { get; set; }

        public override string GetName()
        {
            return A9 + "_override";
        }
    }

    [Component(typeof(ParentValueModel), "key33")]
    public class ValueModel33 : ParentValueModel
    {
        [Value("${a9}_ValueModel33")]
        public string A9 { get; set; }

        public override string GetName()
        {
            return A9 + "_override"; ;
        }
    }

    [Component]
    public class ValueModel4
    {

        [Value("#{@(Autofac.Configuration.Test.test4.ParentValueModel,Autofac.Configuration.Test@key32).GetName()}")]
        public string CallA9 { get; set; }
    }

    [Component]
    public class ValueModel5
    {

        [Value("${parent:name}")]
        public string ParentName { get; set; }
    }
}
