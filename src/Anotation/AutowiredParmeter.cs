using System;
using System.Collections.Concurrent;
using System.Reflection;
using Autofac.Core;

namespace Autofac.Annotation
{
    /// <summary>
    /// 
    /// </summary>
    public class AutowiredParmeter:Parameter
    {
        /// <summary>
        /// 
        /// </summary>
        public ConcurrentDictionary<string,object> AutowiredChains { get; set; } = new ConcurrentDictionary<string, object>();

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="service"></param>
        /// <param name="instance"></param>
        public void Add(string service,object instance)
        {
            this.AutowiredChains.TryAdd(service, instance);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pi"></param>
        /// <param name="context"></param>
        /// <param name="valueProvider"></param>
        /// <returns></returns>
        public override bool CanSupplyValue(ParameterInfo pi, IComponentContext context, out Func<object> valueProvider)
        {
            valueProvider = null;
            return false;
        }
    }
}