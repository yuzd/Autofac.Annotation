using System;
using System.Linq;
using System.Reflection;

namespace Autofac.Annotation.Util
{
    internal static class ComponentDetectorExtension
    {
        /// <summary>
        /// 根据 <see cref="Component"/> 特性和 <see cref="IComponentDetector"/> 接口来判断类型是否为 Component。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="componentDetector"></param>
        /// <returns></returns>
        public static Component GetComponent(this Type type, IComponentDetector componentDetector)
        {
            // Component 特性的优先级高于 IComponentDetector 接口。
            var attributes = type.GetCustomAttributes();
            foreach (var attribute in attributes)
            {
                if (attribute is Component component)
                {
                    component.RegisterType = RegisterType.Compoment;
                    return component;
                }

                var alaisForComponet = attribute.GetAlaisForComponet(type);
                if (alaisForComponet != null)
                {
                    alaisForComponet.RegisterType = RegisterType.Compoment;
                    return alaisForComponet;
                }
            }

            return componentDetector?.Detect(type);
        }

        /// <summary>
        /// 找有没有自定义类型的Attribute有打Component注解
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static Component GetAlaisForComponet(this Attribute attribute, Type type)
        {
            var comp = attribute.GetType().GetCustomAttribute<Component>();
            if (comp == null) return null;

            try
            {
                Component target = new Component(type);
                var alaisForProperty = from item in attribute.GetType().GetAllProperties()
                    from atts in item.GetCustomAttributes<AliasFor>()
                    where atts.ForType == typeof(Component)
                    select new
                    {
                        AliasFor = atts,
                        Property = item
                    };
                foreach (var p1 in alaisForProperty)
                {
                    if (string.IsNullOrEmpty(p1.AliasFor.ForField))
                    {
                        continue;
                    }

                    target.GetType().GetProperty(p1.AliasFor.ForField)?.SetValue(target, p1.Property.GetValue(attribute));
                }

                var alaisForField = from item in attribute.GetType().GetAllFields()
                    from atts in item.GetCustomAttributes<AliasFor>()
                    where atts.ForType == typeof(Component)
                    select new
                    {
                        AliasFor = atts,
                        Property = item
                    };
                foreach (var p1 in alaisForField)
                {
                    if (string.IsNullOrEmpty(p1.AliasFor.ForField))
                    {
                        continue;
                    }

                    target.GetType().GetField(p1.AliasFor.ForField)?.SetValue(target, p1.Property.GetValue(attribute));
                }

                return target;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(
                    $"The class `{type.FullName}` found Attribute: `{attribute.GetType().Name}` with [Component] , init error {e.Message}");
            }
        }
    }
}