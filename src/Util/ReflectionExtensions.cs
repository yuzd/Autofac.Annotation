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
        /// 判断是否存在class
        /// </summary>
        /// <param name="classPath"></param>
        /// <returns></returns>
        internal static bool FindClass(this string classPath)
        {
            try
            {
                return Type.GetType(classPath) != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 获取方法的唯一string
        /// </summary>
        /// <param name="mi"></param>
        /// <returns></returns>
        public static String GetMethodInfoUniqueName(this MethodInfo mi)
        {
            String signatureString = String.Join(",", mi.GetParameters().Select(p => p.ParameterType.Name).ToArray());
            String returnTypeName = mi.ReturnType.Name;

            if (mi.IsGenericMethod)
            {
                String typeParamsString = String.Join(",", mi.GetGenericArguments().Select(g => g.AssemblyQualifiedName).ToArray());


                // returns a string like this: "Assembly.YourSolution.YourProject.YourClass:YourMethod(Param1TypeName,...,ParamNTypeName):ReturnTypeName
                return $"{mi.DeclaringType.Namespace + mi.DeclaringType.Name}:{mi.Name}<{typeParamsString}>({signatureString}):{returnTypeName}";
            }

            return $"{mi.DeclaringType.Namespace + mi.DeclaringType.Name}:{mi.Name}({signatureString}):{returnTypeName}";
        }

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
            if (mi != null && mi.DeclaringType != null && mi.IsSpecialName && mi.Name.StartsWith("set_", System.StringComparison.Ordinal))
            {
                prop = mi.DeclaringType.GetProperty(mi.Name.Substring(4));
                return true;
            }

            prop = null;
            return false;
        }

        /// <summary>
        /// Get all the method of a class instance
        /// </summary>
        /// <param name="type">Type object of that class</param>
        /// <param name="getBaseType">is get baseType methods</param>
        /// <returns></returns>
        public static IEnumerable<MethodInfo> GetAllInstanceMethod(this Type type, bool getBaseType = true)
        {
            if (type == null)
            {
                return Enumerable.Empty<MethodInfo>();
            }

            BindingFlags flags = BindingFlags.Public |
                                 BindingFlags.NonPublic |
                                 BindingFlags.Instance |
                                 BindingFlags.DeclaredOnly;

            if (!getBaseType) return type.GetMethods(flags);
            return type.GetMethods(flags).Union(GetAllMethod(type.BaseType, getBaseType));
        }

        /// <summary>
        /// Get all the method of a class
        /// </summary>
        /// <param name="type">Type object of that class</param>
        /// <param name="getBaseType">is get baseType methods</param>
        /// <returns></returns>
        public static IEnumerable<MethodInfo> GetAllMethod(this Type type, bool getBaseType = true)
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

            if (!getBaseType) return type.GetMethods(flags);
            return type.GetMethods(flags).Union(GetAllMethod(type.BaseType, getBaseType));
        }

        /// <summary>
        /// 先获取当前类的method，没有的话在找父类的
        /// </summary>
        /// <param name="type"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public static MethodInfo GetMethod(this Type type, string methodName)
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

            return GetMethod(type.BaseType, methodName);
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
                   || type.IsGenericListOrCollectionInterfaceType() || type.IsTypeDefinitionEnumerable() ||
                   type.GetInterfaces()
                       .Any(t => t.IsSelfEnumerable() || t.IsTypeDefinitionEnumerable());
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

        private static bool IsSelfEnumerable(this Type type)
        {
            bool isDirectly = type == typeof(IEnumerable<>);
            return isDirectly;
        }

        private static bool IsTypeDefinitionEnumerable(this Type type)
        {
            bool isViaInterfaces = type.IsGenericType &&
                                   type.GetGenericTypeDefinition().IsSelfEnumerable();
            return isViaInterfaces;
        }


        /// <summary>
        /// 获取一个类型所有的父类和继承的接口
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetParentTypes(this Type type)
        {
            // is there any base type?
            if (type == null)
            {
                yield break;
            }

            // return all implemented or inherited interfaces
            foreach (var i in GetImplementedInterfaces(type))
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


        private static Type[] GetImplementedInterfaces(Type type)
        {
            var interfaces = type.GetTypeInfo().ImplementedInterfaces.Where(i => i != typeof(IDisposable));
            return type.GetTypeInfo().IsInterface ? interfaces.AppendItem(type).ToArray() : interfaces.ToArray();
        }

        /// <summary>
        /// Appends the item to the specified sequence.
        /// </summary>
        /// <typeparam name="T">The type of element in the sequence.</typeparam>
        /// <param name="sequence">The sequence.</param>
        /// <param name="trailingItem">The trailing item.</param>
        /// <returns>The sequence with an item appended to the end.</returns>
        private static IEnumerable<T> AppendItem<T>(this IEnumerable<T> sequence, T trailingItem)
        {
            if (sequence == null) throw new ArgumentNullException(nameof(sequence));

            foreach (var t in sequence)
                yield return t;

            yield return trailingItem;
        }
    }
}