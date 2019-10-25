using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autofac.Annotation
{
    /// <summary>
    /// Specifies how instances of the
    /// class must apply environment variables when replacing values.
    /// </summary>
    /// <author>Mark Pollack</author>
    [Serializable]
    public enum EnvironmentVariableMode
    {
        /// <summary>
        /// Never replace environment variables.
        /// </summary>
        Never = 1,

        /// <summary>
        /// If properties are not specified via a resource, 
        /// then resolve using environment variables.
        /// </summary>
        Fallback = 2,

        /// <summary>
        /// Apply environment variables first before applying properties from a
        /// resource.
        /// </summary>
        Override = 3
    }
}
