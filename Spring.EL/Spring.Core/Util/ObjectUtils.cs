/*
 * Copyright  2002-2005 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections;
using System.Globalization;
using System.Reflection;



namespace Spring.Util
{
    /// <summary>
    /// Helper methods with regard to objects, types, properties, etc.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Not intended to be used directly by applications.
    /// </p>
    /// </remarks>
    /// <author>Rod Johnson</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Rick Evans (.NET)</author>
    internal sealed class ObjectUtils
    {
        

        /// <summary>
        /// An empty object array.
        /// </summary>
        public static readonly object[] EmptyObjects = { };

        private static readonly MethodInfo GetHashCodeMethodInfo;

        static ObjectUtils()
		{
			Type type = typeof(object);
			GetHashCodeMethodInfo = type.GetMethod("GetHashCode");
		}

        // CLOVER:OFF

        /// <summary>
        /// Creates a new instance of the <see cref="Spring.Util.ObjectUtils"/> class.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This is a utility class, and as such exposes no public constructors.
        /// </p>
        /// </remarks>
        private ObjectUtils()
        {
        }

       


        /// <summary>
        /// Determine if the given <see cref="System.Type"/> is assignable from the
        /// given value, assuming setting by reflection and taking care of transparent proxies.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Considers primitive wrapper classes as assignable to the
        /// corresponding primitive types.
        /// </p>
        /// <p>
        /// For example used in an object factory's constructor resolution.
        /// </p>
        /// </remarks>
        /// <param name="type">The target <see cref="System.Type"/>.</param>
        /// <param name="obj">The value that should be assigned to the type.</param>
        /// <returns>True if the type is assignable from the value.</returns>
        public static bool IsAssignable(Type type, object obj)
        {
            AssertUtils.ArgumentNotNull(type, "type");
            if (!type.IsPrimitive && obj == null)
            {
                return true;
            }

            if (type.IsInstanceOfType(obj))
            {
                return true;
            }

            return type.IsPrimitive &&
                   type == typeof(bool) && obj is bool ||
                   type == typeof(byte) && obj is byte ||
                   type == typeof(char) && obj is char ||
                   type == typeof(sbyte) && obj is sbyte ||
                   type == typeof(int) && obj is int ||
                   type == typeof(short) && obj is short ||
                   type == typeof(long) && obj is long ||
                   type == typeof(float) && obj is float ||
                   type == typeof(double) && obj is double;
        }

        /// <summary>
        /// Check if the given <see cref="System.Type"/> represents a
        /// "simple" property,
        /// i.e. a primitive, a <see cref="System.String"/>, a
        /// <see cref="System.Type"/>, or a corresponding array.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Used to determine properties to check for a "simple" dependency-check.
        /// </p>
        /// </remarks>
        /// <param name="type">
        /// The <see cref="System.Type"/> to check.
        /// </param>
        public static bool IsSimpleProperty(Type type)
        {
            return type.IsPrimitive
                   || type.Equals(typeof(string))
                   || type.Equals(typeof(string[]))
                   || IsPrimitiveArray(type)
                   || type.Equals(typeof(Type))
                   || type.Equals(typeof(Type[]));
        }

        /// <summary>
        /// Check if the given class represents a primitive array,
        /// i.e. boolean, byte, char, short, int, long, float, or double.
        /// </summary>
        public static bool IsPrimitiveArray(Type type)
        {
            return typeof(bool[]).Equals(type)
                   || typeof(sbyte[]).Equals(type)
                   || typeof(char[]).Equals(type)
                   || typeof(short[]).Equals(type)
                   || typeof(int[]).Equals(type)
                   || typeof(long[]).Equals(type)
                   || typeof(float[]).Equals(type)
                   || typeof(double[]).Equals(type);
        }


        /// <summary>
        /// Determines whether the specified array is null or empty.
        /// </summary>
        /// <param name="array">The array to check.</param>
        /// <returns>
        /// 	<c>true</c> if the specified array is null empty; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEmpty(object[] array)
        {
            return (array == null || array.Length == 0);
        }
        /// <summary>
        /// Determine if the given objects are equal, returning <see langword="true"/>
        /// if both are <see langword="null"/> respectively <see langword="false"/>
        /// if only one is <see langword="null"/>.
        /// </summary>
        /// <param name="o1">The first object to compare.</param>
        /// <param name="o2">The second object to compare.</param>
        /// <returns>
        /// <see langword="true"/> if the given objects are equal.
        /// </returns>
        public static bool NullSafeEquals(object o1, object o2)
        {
            return (o1 == o2 || (o1 != null && o1.Equals(o2)));
        }


        /// <summary>
	    /// Return as hash code for the given object; typically the value of
	    /// <code>{@link Object#hashCode()}</code>. If the object is an array,
	    /// this method will delegate to any of the <code>nullSafeHashCode</code>
	    /// methods for arrays in this class. If the object is <code>null</code>,
	    /// this method returns 0.
        /// </summary>
        public static int NullSafeHashCode(object o1)
        {
            return (o1 != null ? o1.GetHashCode() : 0);
        }

        /// <summary>
        /// Returns the first element in the supplied <paramref name="enumerator"/>.
        /// </summary>
        /// <param name="enumerator">
        /// The <see cref="System.Collections.IEnumerator"/> to use to enumerate
        /// elements.
        /// </param>
        /// <returns>
        /// The first element in the supplied <paramref name="enumerator"/>.
        /// </returns>
        /// <exception cref="System.IndexOutOfRangeException">
        /// If the supplied <paramref name="enumerator"/> did not have any elements.
        /// </exception>
        public static object EnumerateFirstElement(IEnumerator enumerator)
        {
            return EnumerateElementAtIndex(enumerator, 0);
        }

        /// <summary>
        /// Returns the first element in the supplied <paramref name="enumerable"/>.
        /// </summary>
        /// <param name="enumerable">
        /// The <see cref="System.Collections.IEnumerable"/> to use to enumerate
        /// elements.
        /// </param>
        /// <returns>
        /// The first element in the supplied <paramref name="enumerable"/>.
        /// </returns>
        /// <exception cref="System.IndexOutOfRangeException">
        /// If the supplied <paramref name="enumerable"/> did not have any elements.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="enumerable"/> is <see langword="null"/>.
        /// </exception>
        public static object EnumerateFirstElement(IEnumerable enumerable)
        {
            AssertUtils.ArgumentNotNull(enumerable, "enumerable");
            return EnumerateElementAtIndex(enumerable.GetEnumerator(), 0);
        }

        /// <summary>
        /// Returns the element at the specified index using the supplied
        /// <paramref name="enumerator"/>.
        /// </summary>
        /// <param name="enumerator">
        /// The <see cref="System.Collections.IEnumerator"/> to use to enumerate
        /// elements until the supplied <paramref name="index"/> is reached.
        /// </param>
        /// <param name="index">
        /// The index of the element in the enumeration to return.
        /// </param>
        /// <returns>
        /// The element at the specified index using the supplied
        /// <paramref name="enumerator"/>.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If the supplied <paramref name="index"/> was less than zero, or the
        /// supplied <paramref name="enumerator"/> did not contain enough elements
        /// to be able to reach the supplied <paramref name="index"/>.
        /// </exception>
        public static object EnumerateElementAtIndex(IEnumerator enumerator, int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            object element = null;
            int i = 0;
            while (enumerator.MoveNext())
            {
                element = enumerator.Current;
                if (++i > index)
                {
                    break;
                }
            }
            if (i < index)
            {
                throw new ArgumentOutOfRangeException();
            }
            return element;
        }

        /// <summary>
        /// Returns the element at the specified index using the supplied
        /// <paramref name="enumerable"/>.
        /// </summary>
        /// <param name="enumerable">
        /// The <see cref="System.Collections.IEnumerable"/> to use to enumerate
        /// elements until the supplied <paramref name="index"/> is reached.
        /// </param>
        /// <param name="index">
        /// The index of the element in the enumeration to return.
        /// </param>
        /// <returns>
        /// The element at the specified index using the supplied
        /// <paramref name="enumerable"/>.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If the supplied <paramref name="index"/> was less than zero, or the
        /// supplied <paramref name="enumerable"/> did not contain enough elements
        /// to be able to reach the supplied <paramref name="index"/>.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="enumerable"/> is <see langword="null"/>.
        /// </exception>
        public static object EnumerateElementAtIndex(IEnumerable enumerable, int index)
        {
            AssertUtils.ArgumentNotNull(enumerable, "enumerable");
            return EnumerateElementAtIndex(enumerable.GetEnumerator(), index);
        }

        /// <summary>
        /// Gets the qualified name of the given method, consisting of 
        /// fully qualified interface/class name + "." method name.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns>qualified name of the method.</returns>
        public static string GetQualifiedMethodName(MethodInfo method)
        {
            AssertUtils.ArgumentNotNull(method, "method", "MethodInfo must not be null");
            return method.DeclaringType.FullName + "." + method.Name;
        }

        /// <summary>
        /// Return a String representation of an object's overall identity.
        /// </summary>
        /// <param name="obj">The object (may be <code>null</code>).</param>
        /// <returns>The object's identity as String representation,
        /// or an empty String if the object was <code>null</code>
        /// </returns>
        public static string IdentityToString(object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }
            return obj.GetType().FullName + "@" + GetIdentityHexString(obj);
        }

        /// <summary>
        /// Gets a hex String form of an object's identity hash code.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns>The object's identity code in hex notation</returns>
        public static string GetIdentityHexString(object obj)
        {
            int hashcode = (int)GetHashCodeMethodInfo.Invoke(obj, null);
            return hashcode.ToString("X6");
        }
    }
}
