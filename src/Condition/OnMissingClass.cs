using System;
using Autofac.Annotation.Util;
using Autofac.Core.Registration;

namespace Autofac.Annotation.Condition
{
    /// <summary>
    /// 条件里面配置的 根据class的全路径(namespace.classname)如果没有找到对应的class就加载
    /// </summary>
    internal class OnMissingClass : ICondition
    {
        /// <summary>
        /// true代表要过滤
        /// </summary>
        /// <param name="context"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public bool match(IComponentRegistryBuilder context, object config)
        {
            if (!(config is ConditionOnMissingClass metaConfig))
            {
                return false;
            }
            //匹配name
            return metaConfig.name.FindClass();
        }
    }
}