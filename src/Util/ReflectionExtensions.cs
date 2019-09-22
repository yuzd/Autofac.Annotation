using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Autofac.Annotation.Util
{
    /// <summary>
    /// 
    /// </summary>
    internal static class ReflectionExtensions
    {
        /// <summary>
        /// 获取IEnumerable泛型的类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type GetElementType(this Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return type.GetGenericArguments()[0];

            return type;
        }
        
        /// <summary>
        /// Maps from a property-set-value parameter to the declaring property.
        /// </summary>
        /// <param name="pi">Parameter to the property setter.</param>
        /// <param name="prop">The property info on which the setter is specified.</param>
        /// <returns>True if the parameter is a property setter.</returns>
        public static bool TryGetDeclaringProperty(this ParameterInfo pi, out PropertyInfo prop)
        {
            var mi = pi.Member as MethodInfo;
            if (mi != null && mi.DeclaringType!=null && mi.IsSpecialName && mi.Name.StartsWith("set_", System.StringComparison.Ordinal))
            {
                prop = mi.DeclaringType.GetProperty(mi.Name.Substring(4));
                return true;
            }

            prop = null;
            return false;
        }

        /// <summary>
        /// Get all the method of a class
        /// </summary>
        /// <param name="type">Type object of that class</param>
        /// <returns></returns>
        public static IEnumerable<MethodInfo> GetAllMethod(this Type type)
        {
            if (type == null)
            {
                return Enumerable.Empty<MethodInfo>();
            }

            BindingFlags flags = BindingFlags.Public |
                                 BindingFlags.NonPublic |
                                 BindingFlags.Static |
                                 BindingFlags.Instance |
                                 BindingFlags.DeclaredOnly;

            return type.GetMethods(flags).Union(GetAllMethod(type.BaseType));
        }
        
        /// <summary>
        /// 先获取当前类的method，没有的话在找父类的
        /// </summary>
        /// <param name="type"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public static MethodInfo GetMethod(this Type type,string methodName)
        {
            if (type == null)
            {
                return null;
            }

            BindingFlags flags = BindingFlags.Public |
                                 BindingFlags.NonPublic |
                                 BindingFlags.Static |
                                 BindingFlags.Instance |
                                 BindingFlags.DeclaredOnly;

            var method = type.GetMethod(methodName, flags);
            if (method != null)
            {
                return method;
            }
            
            return GetMethod(type.BaseType,methodName);
        }
        
        /// <summary>
        /// Get all the fields of a class
        /// </summary>
        /// <param name="type">Type object of that class</param>
        /// <returns></returns>
        public static IEnumerable<FieldInfo> GetAllFields(this Type type)
        {
            if (type == null)
            {
                return Enumerable.Empty<FieldInfo>();
            }

            BindingFlags flags = BindingFlags.Public |
                                 BindingFlags.NonPublic |
                                 BindingFlags.Static |
                                 BindingFlags.Instance |
                                 BindingFlags.DeclaredOnly;

            return type.GetFields(flags).Union(GetAllFields(type.BaseType));
        }

        /// <summary>
        /// Get all properties of a class
        /// </summary>
        /// <param name="type">Type object of that class</param>
        /// <returns></returns>
        public static IEnumerable<PropertyInfo> GetAllProperties(this Type type)
        {
            if (type == null)
            {
                return Enumerable.Empty<PropertyInfo>();
            }

            BindingFlags flags = BindingFlags.Public |
                                 BindingFlags.NonPublic |
                                 BindingFlags.Static |
                                 BindingFlags.Instance |
                                 BindingFlags.DeclaredOnly;

            return type.GetProperties(flags).Union(GetAllProperties(type.BaseType));
        }
        
        public static bool IsGenericEnumerableInterfaceType(this Type type)
        {
            return type.IsGenericTypeDefinedBy(typeof(IEnumerable<>))
                   || type.IsGenericListOrCollectionInterfaceType();
        }

        public static bool IsGenericListOrCollectionInterfaceType(this Type t)
        {
            return t.IsGenericTypeDefinedBy(typeof(IList<>))
                   || t.IsGenericTypeDefinedBy(typeof(ICollection<>))
                   || t.IsGenericTypeDefinedBy(typeof(IReadOnlyCollection<>))
                   || t.IsGenericTypeDefinedBy(typeof(IReadOnlyList<>));
        }

        public static bool IsGenericTypeDefinedBy(this Type @this, Type openGeneric)
        {
            return !@this.GetTypeInfo().ContainsGenericParameters
                   && @this.GetTypeInfo().IsGenericType
                   && @this.GetGenericTypeDefinition() == openGeneric;
        }

        public static IEnumerable<Type> GetParentTypes(this Type type)
        {
            // is there any base type?
            if (type == null)
            {
                yield break;
            }

            // return all implemented or inherited interfaces
            foreach (var i in type.GetInterfaces())
            {
                yield return i;
            }

            // return all inherited types
            var currentBaseType = type.BaseType;
            while (currentBaseType != null)
            {
                yield return currentBaseType;
                currentBaseType = currentBaseType.BaseType;
            }
        }
    }
}
