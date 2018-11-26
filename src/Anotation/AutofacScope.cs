using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autofac.Annotation.Anotation
{
    public enum AutofacScope
    {
        InstancePerDependency,
        SingleInstance,
        InstancePerLifetimeScope,
        InstancePerMatchingLifetimeScope,
        InstancePerRequest,
        InstancePerOwned
    }
}
