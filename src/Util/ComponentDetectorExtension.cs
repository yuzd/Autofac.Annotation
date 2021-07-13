using System;
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
            var component = type.GetCustomAttribute<Component>();
            if (component != null)
            {
                component.RegisterType = RegisterType.Compoment;
                return component;
            }

            return componentDetector?.Detect(type);
        }
    }
}