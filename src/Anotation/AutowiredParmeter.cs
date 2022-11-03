using System;
using System.Linq;
using System.Reflection;
using Autofac.Core;
using Autofac.Core.Resolving;

namespace Autofac.Annotation
{
    /// <summary>
    /// 
    /// </summary>
    internal class AutowiredParmeterStack : Parameter
    {
        /// <summary>
        /// 集合
        /// </summary>
        private readonly SegmentedStack<Tuple<Service, object>> chainList = new SegmentedStack<Tuple<Service, object>>();

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="service"></param>
        /// <param name="instance"></param>
        public void Push(Service service, object instance)
        {
            chainList.Push(new Tuple<Service, object>(service, instance));
        }

        /// <summary>
        /// 删除
        /// </summary>
        public void Dispose()
        {
            chainList.Pop();
        }

        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="service"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public bool CircularDetected(Service service, out object instance)
        {
            foreach (var requestEntry in chainList)
                if (requestEntry.Item1 == service)
                {
                    instance = requestEntry.Item2;
                    return true;
                }

            instance = null;
            return false;
        }


        /// <summary>
        /// Circular component dependency detected
        /// </summary>
        /// <returns></returns>
        public string GetCircualrChains(Service service)
        {
            var err = "Circular component dependency detected:" + string.Join("->", (from obj in chainList
                    let ob = obj.Item2.GetType()
                    select ob.Namespace + "." + ob.Name
                ).Reverse()) + "->" + service?.Description;
            return err;
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