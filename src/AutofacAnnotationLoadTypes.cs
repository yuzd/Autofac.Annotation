using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac.Annotation.Condition;
using Autofac.Core.Registration;

namespace Autofac.Annotation
{
    /// <summary>
    /// Condition 打在类上
    /// CoditionOnMissingClass ConditionOnClass 打在类和方法
    /// ConditionOnMissingBean ConditionOnBean 只打在方法
    /// 针对 Compoment Bean AutoConfiguration Import PointCut
    /// ConditionOnMissingBean ConditionOnBean 只设计在AutoConfiguration下才有效
    /// 由于默认的加载顺序问题 https://zenn.dev/kawakawaryuryu/articles/d97361bcde98ed
    /// </summary>
    public partial class AutofacAnnotationModule
    {
        /// <summary>
        /// 注册AutoConfiguration注解标识的类里面的Bean时候的过滤逻辑
        /// </summary>
        /// <returns></returns>
        internal static bool shouldSkip(IComponentRegistryBuilder context, Type currentType, MethodInfo beanMethod)
        {
            //拿到打在method上的Conditianl标签
            var conditionList = beanMethod.GetCustomAttributes<Conditional>().ToList();
            if (!conditionList.Any())
            {
                return false;
            }

            Dictionary<Type, ICondition> cache = new Dictionary<Type, ICondition>();

            foreach (var conditional in conditionList)
            {
                if (conditional.Type == null || typeof(Conditional).IsAssignableFrom(conditional.Type))
                {
                    throw new InvalidCastException(
                        $"`{currentType.Namespace}.{currentType.Name}.{beanMethod.Name}` [conditional] load fail,`{conditional.Type?.FullName}` must be implements of `Condition`");
                }

                if (!cache.TryGetValue(conditional.Type, out var condition))
                {
                    condition = Activator.CreateInstance(conditional.Type) as ICondition;
                    if (condition == null)
                    {
                        continue;
                    }
                    cache.Add(conditional.Type,condition);
                }

                if (condition.match(context, conditional))
                {
                    return true;
                }
            }

            return false;
        }
    }
}