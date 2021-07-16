using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Autofac.Annotation.Condition;
using Autofac.Annotation.Config;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Core.Resolving.Pipeline;

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
        /// 註冊BeanPostProcessor處理器
        /// </summary>
        private void RegisterBeforeBeanPostProcessor<TReflectionActivatorData>(ComponentModel component,
            IRegistrationBuilder<object, TReflectionActivatorData, object> registrar)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            //过滤掉框架类
            if (component.CurrentType.Assembly == this.GetType().Assembly ||
                component.CurrentType.Assembly == typeof(Autofac.Core.Lifetime.LifetimeScope).Assembly)
            {
                return;
            }

            if (component.IsBenPostProcessor)
            {
                return;
            }

            registrar.ConfigurePipeline(p =>
                p.Use(PipelinePhase.Activation, MiddlewareInsertionMode.StartOfPhase, (ctxt, next) =>
                {
                    next(ctxt);
                    DoBeforeBeanPostProcessor(ctxt);
                }));
        }

        private void RegisterAfterBeanPostProcessor<TReflectionActivatorData>(ComponentModel component,
            IRegistrationBuilder<object, TReflectionActivatorData, object> registrar)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            //过滤掉框架类
            if (component.CurrentType.Assembly == this.GetType().Assembly ||
                component.CurrentType.Assembly == typeof(Autofac.Core.Lifetime.LifetimeScope).Assembly)
            {
                return;
            }

            if (component.IsBenPostProcessor)
            {
                return;
            }

            registrar.ConfigurePipeline(p =>
                p.Use(PipelinePhase.Activation, MiddlewareInsertionMode.StartOfPhase, (ctxt, next) =>
                {
                    next(ctxt);
                    DoAfterBeanPostProcessor(ctxt);
                }));
        }

        /// <summary>
        /// 註冊BeanPostProcessor處理器
        /// </summary>
        private void RegisterBeforeBeanPostProcessor(ComponentModel component, IComponentRegistration registrar)
        {
            if (component.IsBenPostProcessor)
            {
                return;
            }

            registrar.ConfigurePipeline(p =>
                p.Use(PipelinePhase.Activation, MiddlewareInsertionMode.StartOfPhase, (ctxt, next) =>
                {
                    next(ctxt);
                    DoBeforeBeanPostProcessor(ctxt);
                }));
        }

        private void RegisterAfterBeanPostProcessor(ComponentModel component, IComponentRegistration registrar)
        {
            if (component.IsBenPostProcessor)
            {
                return;
            }

            registrar.ConfigurePipeline(p =>
                p.Use(PipelinePhase.Activation, MiddlewareInsertionMode.EndOfPhase, (ctxt, next) =>
                {
                    next(ctxt);
                    DoAfterBeanPostProcessor(ctxt);
                }));
        }

        /// <summary>
        /// BeanPostProcessor處理器
        /// 该方法在bean实例化完毕（且已经注入完毕），在afterPropertiesSet或自定义init方法执行之前
        /// </summary>
        /// <param name="context"></param>
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        private void DoBeforeBeanPostProcessor(ResolveRequestContext context)
        {
            context.TryResolve<IEnumerable<BeanPostProcessor>>(out var beanPostProcessors);
            if (beanPostProcessors == null || !beanPostProcessors.Any())
            {
                return;
            }

            foreach (var beanPostProcessor in beanPostProcessors.ToList())
            {
                context.Instance = beanPostProcessor.PostProcessBeforeInitialization(context.Instance);
            }
        }

        /// <summary>
        /// BeanPostProcessor處理器
        /// 在afterPropertiesSet或自定义init方法执行之后
        /// </summary>
        /// <param name="context"></param>
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        private void DoAfterBeanPostProcessor(ResolveRequestContext context)
        {
            context.TryResolve<IEnumerable<BeanPostProcessor>>(out var beanPostProcessors);
            if (beanPostProcessors == null || !beanPostProcessors.Any())
            {
                return;
            }

            foreach (var beanPostProcessor in beanPostProcessors.ToList())
            {
                context.Instance = beanPostProcessor.PostProcessAfterInitialization(context.Instance);
            }
        }

        /// <summary>
        /// 针对Compoment注解
        /// </summary>
        /// <returns></returns>
        internal static bool shouldSkip(IComponentRegistryBuilder context, Type currentType)
        {
            //拿到打在method上的Conditianl标签
            var conditionList = currentType.GetCustomAttributes<Conditional>().ToList();
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
                        $"`{currentType.Namespace}.{currentType.Name}.` [conditional] load fail,`{conditional.Type?.FullName}` must be implements of `Condition`");
                }

                if (!cache.TryGetValue(conditional.Type, out var condition))
                {
                    condition = Activator.CreateInstance(conditional.Type) as ICondition;
                    if (condition == null)
                    {
                        continue;
                    }

                    cache.Add(conditional.Type, condition);
                }

                if (condition.ShouldSkip(context, conditional))
                {
                    return true;
                }
            }

            return false;
        }

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

                    cache.Add(conditional.Type, condition);
                }

                if (condition.ShouldSkip(context, conditional))
                {
                    return true;
                }
            }

            return false;
        }
    }
}