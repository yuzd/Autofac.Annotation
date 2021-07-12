using Autofac.Core;
using Autofac.Core.Registration;

namespace Autofac.Annotation.Condition
{
    /// <summary>
    /// 条件里面配置的 没有被注册过才要添加到容器
    /// </summary>
    internal class OnMissingBean : ICondition
    {
        /// <summary>
        /// true代表要过滤
        /// </summary>
        /// <param name="context"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public bool match(IComponentRegistryBuilder context, object config)
        {
            if (!(config is ConditionOnMissingBean metaConfig))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(metaConfig.name) && metaConfig.type != null)
            {
                //匹配name加类型
                return context.IsRegistered(new KeyedService(metaConfig.name, metaConfig.type));
            }
            else if (metaConfig.type != null)
            {
                //只匹配类型
                return context.IsRegistered(new TypedService(metaConfig.type));
            }

            return false;
        }
    }
}