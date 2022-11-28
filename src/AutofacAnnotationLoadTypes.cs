using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac.Annotation.Condition;
using Autofac.Annotation.Config;
using Autofac.Annotation.Util;
using Autofac.AspectIntercepter.Advice;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Annotation
{
    /// <summary>
    ///     Condition 打在类上
    ///     CoditionOnMissingClass ConditionOnClass 打在类和方法
    ///     ConditionOnMissingBean ConditionOnBean 只打在方法
    ///     针对 Compoment Bean AutoConfiguration Import PointCut
    ///     ConditionOnMissingBean ConditionOnBean 只设计在AutoConfiguration下才有效
    ///     由于默认的加载顺序问题 https://zenn.dev/kawakawaryuryu/articles/d97361bcde98ed
    /// </summary>
    public partial class AutofacAnnotationModule
    {
        /// <summary>
        ///     註冊BeanPostProcessor處理器
        /// </summary>
        private void RegisterBeforeBeanPostProcessor<TReflectionActivatorData>(ComponentModel component,
            IRegistrationBuilder<object, TReflectionActivatorData, object> registrar)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            //过滤掉框架类
            if (component.CurrentType.Assembly == GetType().Assembly ||
                component.CurrentType.Assembly == typeof(LifetimeScope).Assembly)
                return;

            if (component.IsBenPostProcessor) return;

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
            if (component.CurrentType.Assembly == GetType().Assembly ||
                component.CurrentType.Assembly == typeof(LifetimeScope).Assembly)
                return;

            if (component.IsBenPostProcessor) return;

            registrar.ConfigurePipeline(p =>
                p.Use(PipelinePhase.Activation, MiddlewareInsertionMode.StartOfPhase, (ctxt, next) =>
                {
                    next(ctxt);
                    DoAfterBeanPostProcessor(ctxt);
                }));
        }

        /// <summary>
        ///     註冊BeanPostProcessor處理器
        /// </summary>
        private void RegisterBeforeBeanPostProcessor(ComponentModel component,
            IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> registrar)
        {
            if (component.IsBenPostProcessor) return;

            registrar.ConfigurePipeline(p =>
                p.Use(PipelinePhase.Activation, MiddlewareInsertionMode.StartOfPhase, (ctxt, next) =>
                {
                    next(ctxt);
                    DoBeforeBeanPostProcessor(ctxt);
                }));
        }

        private void RegisterBeforeBeanPostProcessor(ComponentModel component, IComponentRegistration registrar)
        {
            if (component.IsBenPostProcessor) return;

            registrar.ConfigurePipeline(p =>
                p.Use(PipelinePhase.Activation, MiddlewareInsertionMode.StartOfPhase, (ctxt, next) =>
                {
                    next(ctxt);
                    DoBeforeBeanPostProcessor(ctxt);
                }));
        }

        private void RegisterAfterBeanPostProcessor(ComponentModel component,
            IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> registrar)
        {
            if (component.IsBenPostProcessor) return;

            registrar.ConfigurePipeline(p =>
                p.Use(PipelinePhase.Activation, MiddlewareInsertionMode.EndOfPhase, (ctxt, next) =>
                {
                    next(ctxt);
                    DoAfterBeanPostProcessor(ctxt);
                }));
        }

        private void RegisterAfterBeanPostProcessor(ComponentModel component, IComponentRegistration registrar)
        {
            if (component.IsBenPostProcessor) return;

            registrar.ConfigurePipeline(p =>
                p.Use(PipelinePhase.Activation, MiddlewareInsertionMode.EndOfPhase, (ctxt, next) =>
                {
                    next(ctxt);
                    DoAfterBeanPostProcessor(ctxt);
                }));
        }

        /// <summary>
        ///     BeanPostProcessor處理器
        ///     该方法在bean实例化完毕（且已经注入完毕），在afterPropertiesSet或自定义init方法执行之前
        /// </summary>
        /// <param name="context"></param>
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        private void DoBeforeBeanPostProcessor(ResolveRequestContext context)
        {
            if (!context.ComponentRegistry.Properties.TryGetValue(nameof(List<BeanPostProcessor>), out var temp))
                return;

            if (!(temp is List<BeanPostProcessor> beanPostProcessors) || !beanPostProcessors.Any()) return;

            foreach (var beanPostProcessor in beanPostProcessors)
                context.Instance = beanPostProcessor.PostProcessBeforeInitialization(context.Instance);
        }

        /// <summary>
        ///     BeanPostProcessor處理器
        ///     在afterPropertiesSet或自定义init方法执行之后
        /// </summary>
        /// <param name="context"></param>
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        private void DoAfterBeanPostProcessor(ResolveRequestContext context)
        {
            if (!context.ComponentRegistry.Properties.TryGetValue(nameof(List<BeanPostProcessor>), out var temp))
                return;

            if (!(temp is List<BeanPostProcessor> beanPostProcessors) || !beanPostProcessors.Any()) return;

            foreach (var beanPostProcessor in beanPostProcessors)
                context.Instance = beanPostProcessor.PostProcessAfterInitialization(context.Instance);
        }

        /// <summary>
        ///     针对Compoment注解
        /// </summary>
        /// <returns></returns>
        internal static bool shouldSkip(IComponentRegistryBuilder context, Type currentType)
        {
            //拿到打在method上的Conditianl标签
            var conditionList = currentType.GetCustomAttributes<Conditional>().ToList();
            if (!conditionList.Any()) return false;

            var cache = new Dictionary<Type, ICondition>();

            foreach (var conditional in conditionList)
            {
                if (conditional.Type == null || typeof(Conditional).IsAssignableFrom(conditional.Type))
                    throw new InvalidCastException(
                        $"`{currentType.Namespace}.{currentType.Name}.` [conditional] load fail,`{conditional.Type?.FullName}` must be implements of `Condition`");

                if (!cache.TryGetValue(conditional.Type, out var condition))
                {
                    condition = Activator.CreateInstance(conditional.Type) as ICondition;
                    if (condition == null) continue;

                    cache.Add(conditional.Type, condition);
                }

                if (condition.ShouldSkip(context, conditional)) return true;
            }

            return false;
        }

        /// <summary>
        ///     注册AutoConfiguration注解标识的类里面的Bean时候的过滤逻辑
        /// </summary>
        /// <returns></returns>
        internal static bool shouldSkip(IComponentRegistryBuilder context, Type currentType, MethodInfo beanMethod)
        {
            //拿到打在method上的Conditianl标签
            var conditionList = beanMethod.GetCustomAttributes<Conditional>().ToList();
            if (!conditionList.Any()) return false;

            var cache = new Dictionary<Type, ICondition>();

            foreach (var conditional in conditionList)
            {
                if (conditional.Type == null || typeof(Conditional).IsAssignableFrom(conditional.Type))
                    throw new InvalidCastException(
                        $"`{currentType.Namespace}.{currentType.Name}.{beanMethod.Name}` [conditional] load fail,`{conditional.Type?.FullName}` must be implements of `Condition`");

                if (!cache.TryGetValue(conditional.Type, out var condition))
                {
                    condition = Activator.CreateInstance(conditional.Type) as ICondition;
                    if (condition == null) continue;

                    cache.Add(conditional.Type, condition);
                }

                if (condition.ShouldSkip(context, conditional)) return true;
            }

            return false;
        }

        /// <summary>
        ///     注册DependsOn
        /// </summary>
        /// <param name="compoment"></param>
        /// <param name="registrar"></param>
        /// <typeparam name="TReflectionActivatorData"></typeparam>
        private void RegisterDependsOn<TReflectionActivatorData>(ComponentModel compoment,
            IRegistrationBuilder<object, TReflectionActivatorData, object> registrar)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            if (compoment.DependsOn == null) return;

            registrar.ConfigurePipeline(p =>
            {
                //DepondsOn注入
                p.Use(PipelinePhase.RegistrationPipelineStart, (context, next) =>
                {
                    foreach (var dependsType in compoment.DependsOn.DependsOnLazy.Value)
                        new Autowired(false).Resolve(context, compoment.CurrentType, dependsType, dependsType.Name,
                            context.Parameters.ToList());
                    next(context);
                });
            });
        }


        /// <summary>
        ///     自动发现Aspect标签
        /// </summary>
        /// <param name="aspectClass"></param>
        private static bool NeedWarpForAspect(ComponentModel aspectClass)
        {
            if (aspectClass.CurrentType.IsInterface || aspectClass.CurrentType.IsAbstract) return false;
            if (aspectClass.AspectAttributeCache.Any()) return true;

            //class上的标签（包括父类，接口上的）也是包含继承关系
            var allAttributesinClass = from item in aspectClass.CurrentClassTypeAttributes
                let att = item as AspectInvokeAttribute
                where att != null
                select new { IsClass = true, Attribute = att, Index = att.OrderIndex };

            //class下的方法包含继承关系
            var myArrayMethodInfo = aspectClass.CurrentType.GetAllInstanceMethod();
            Parallel.ForEach(myArrayMethodInfo, method =>
            {
                var allAttributes = allAttributesinClass.Concat(method
                    .GetCustomAttributes(typeof(AspectInvokeAttribute), true).OfType<AspectInvokeAttribute>()
                    .Select(r => new { IsClass = false, Attribute = r, Index = r.OrderIndex }));

                var allAttributesInculdeFromInterface = allAttributes.Concat(
                    from a in method.GetCustomAttributesByImplementedInterfaces<AspectInvokeAttribute>()
                    select new { IsClass = false, Attribute = a, Index = a.OrderIndex });

                //如果class上也打了 method上也打了 优先用method上的
                //如果method上打了 父类的method或者接口方法上打了 优先用本类的method上的
                var attributes = allAttributesInculdeFromInterface
                    .OrderBy(r => r.IsClass).ThenByDescending(r => r.Index)
                    .GroupBy(r => r.Attribute.GetType().FullName)
                    .Select(r => r.First().Attribute).ToList();

                // 看是否有设置了要Ignore的
                var ignore = method.GetCustomAttribute<IgnoreAop>();
                if (ignore != null && (IgnoreFlags.Advice & ignore.IgnoreFlags) != 0)
                {
                    if (ignore.Target == null || !ignore.Target.Any())
                    {
                        attributes = new List<AspectInvokeAttribute>(1);
                    }
                    else if (attributes.Any())
                    {
                        attributes = attributes.Where(r => !ignore.Target.Contains(r.GetType())).ToList();
                    }
                }

                if (attributes.Any()) aspectClass.AspectAttributeCache.TryAdd(method, attributes);
            });

            if (!aspectClass.AspectAttributeCache.Any()) return false;
            aspectClass.EnableAspect = true;
            return true;
        }
    }
}