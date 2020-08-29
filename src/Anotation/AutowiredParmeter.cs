using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac.Core;

namespace Autofac.Annotation
{
    /// <summary>
    /// 
    /// </summary>
    internal class AutowiredParmeter:Parameter
    {
        /// <summary>
        /// 集合
        /// </summary>
        private readonly ConcurrentDictionary<string,object> AutowiredChains = new ConcurrentDictionary<string, object>();
        
        private readonly List<string> chainList = new List<string>();

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="service"></param>
        /// <param name="instance"></param>
        public bool TryAdd(string service,object instance)
        { 
            chainList.Add(service);
           return this.AutowiredChains.TryAdd(service, instance);
        }

        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="service"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public bool TryGet(string service, out object instance)
        {
           return this.AutowiredChains.TryGetValue(service,out instance);
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

        /// <summary>
        /// Circular component dependency detected
        /// </summary>
        /// <returns></returns>
        public string GetCircualrChains()
        {
            return "Circular component dependency detected:"+string.Join("->", chainList);
        }

        /// <summary>
        /// release
        /// </summary>
        public void Dispose()
        {
            this.chainList.Clear();
            this.AutowiredChains.Clear();
        }
    }
}