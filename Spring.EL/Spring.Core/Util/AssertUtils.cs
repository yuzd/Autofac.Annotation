#region License

/*
 * Copyright � 2002-2011 the original author or authors.
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

#endregion

using System;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Spring.Util
{
    /// <summary>
    /// Assertion utility methods that simplify things such as argument checks.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Not intended to be used directly by applications.
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    /// <author>Erich Eichinger</author>
    internal static class AssertUtils
	{
        

		/// <summary>
		/// Checks the value of the supplied <paramref name="argument"/> and throws an
		/// <see cref="System.ArgumentNullException"/> if it is <see langword="null"/>.
		/// </summary>
		/// <param name="argument">The object to check.</param>
		/// <param name="name">The argument name.</param>
		/// <exception cref="System.ArgumentNullException">
		/// If the supplied <paramref name="argument"/> is <see langword="null"/>.
		/// </exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ArgumentNotNull(object argument, string name)
		{
			if (argument == null)
			{
				ThrowArgumentNullException(name);
			}
		}

		/// <summary>
		/// Checks the value of the supplied <paramref name="argument"/> and throws an
		/// <see cref="System.ArgumentNullException"/> if it is <see langword="null"/>.
		/// </summary>
		/// <param name="argument">The object to check.</param>
		/// <param name="name">The argument name.</param>
		/// <param name="message">
		/// An arbitrary message that will be passed to any thrown
		/// <see cref="System.ArgumentNullException"/>.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// If the supplied <paramref name="argument"/> is <see langword="null"/>.
		/// </exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ArgumentNotNull(object argument, string name, string message)
		{
			if (argument == null)
			{
				ThrowArgumentNullException(name, message);
			}
		}

		/// <summary>
		/// Checks the value of the supplied string <paramref name="argument"/> and throws an
        /// <see cref="System.ArgumentNullException"/> if it is <see langword="null"/> or
		/// contains only whitespace character(s).
		/// </summary>
		/// <param name="argument">The string to check.</param>
		/// <param name="name">The argument name.</param>
		/// <exception cref="System.ArgumentNullException">
		/// If the supplied <paramref name="argument"/> is <see langword="null"/> or
		/// contains only whitespace character(s).
		/// </exception>
		public static void ArgumentHasText(string argument, string name)
		{
			if (StringUtils.IsNullOrEmpty(argument))
			{
				ThrowArgumentNullException(
					name,
					$"Argument '{name}' cannot be null or resolve to an empty string : '{argument}'.");
			}
		}

		/// <summary>
		/// Checks the value of the supplied string <paramref name="argument"/> and throws an
		/// <see cref="System.ArgumentNullException"/> if it is <see langword="null"/> or
		/// contains only whitespace character(s).
		/// </summary>
		/// <param name="argument">The string to check.</param>
		/// <param name="name">The argument name.</param>
		/// <param name="message">
		/// An arbitrary message that will be passed to any thrown
		/// <see cref="System.ArgumentNullException"/>.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// If the supplied <paramref name="argument"/> is <see langword="null"/> or
		/// contains only whitespace character(s).
		/// </exception>
		public static void ArgumentHasText(string argument, string name, string message)
		{
			if (StringUtils.IsNullOrEmpty(argument))
			{
				ThrowArgumentNullException(name, message);
			}
		}

        /// <summary>
        /// Checks the value of the supplied <see cref="ICollection"/> <paramref name="argument"/> and throws
        /// an <see cref="ArgumentNullException"/> if it is <see langword="null"/> or contains no elements.
        /// </summary>
        /// <param name="argument">The array or collection to check.</param>
        /// <param name="name">The argument name.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="argument"/> is <see langword="null"/> or
        /// contains no elements.
        /// </exception>
        public static void ArgumentHasLength(ICollection argument, string name)
        {
            if (!ArrayUtils.HasLength(argument))
            {
                ThrowArgumentNullException(
                    name,
	                $"Argument '{name}' cannot be null or resolve to an empty array");
            }
        }

        /// <summary>
        /// Checks the value of the supplied <see cref="ICollection"/> <paramref name="argument"/> and throws
        /// an <see cref="ArgumentNullException"/> if it is <see langword="null"/> or contains no elements.
        /// </summary>
        /// <param name="argument">The array or collection to check.</param>
        /// <param name="name">The argument name.</param>
        /// <param name="message">An arbitrary message that will be passed to any thrown <see cref="System.ArgumentNullException"/>.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="argument"/> is <see langword="null"/> or
        /// contains no elements.
        /// </exception>
        public static void ArgumentHasLength(ICollection argument, string name, string message)
        {
            if(!ArrayUtils.HasLength(argument))
            {
                ThrowArgumentNullException(name, message);
            }
        }

        /// <summary>
        /// Checks the value of the supplied <see cref="ICollection"/> <paramref name="argument"/> and throws
        /// an <see cref="ArgumentException"/> if it is <see langword="null"/>, contains no elements or only null elements.
        /// </summary>
        /// <param name="argument">The array or collection to check.</param>
        /// <param name="name">The argument name.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="argument"/> is <see langword="null"/>, 
        /// contains no elements or only null elements.
        /// </exception>
        public static void ArgumentHasElements(ICollection argument, string name)
        {
            if (!ArrayUtils.HasElements(argument))
            {
                ThrowArgumentException(
                    name,
	                $"Argument '{name}' must not be null or resolve to an empty collection and must contain non-null elements");
            }            
        }

	    /// <summary>
	    /// Checks whether the specified <paramref name="argument"/> can be cast 
	    /// into the <paramref name="requiredType"/>.
	    /// </summary>
	    /// <param name="argument">
	    /// The argument to check.
	    /// </param>
	    /// <param name="argumentName">
	    /// The name of the argument to check.
	    /// </param>
	    /// <param name="requiredType">
	    /// The required type for the argument.
	    /// </param>
        /// <param name="message">
        /// An arbitrary message that will be passed to any thrown
        /// <see cref="System.ArgumentException"/>.
        /// </param>
        public static void AssertArgumentType(object argument, string argumentName, Type requiredType, string message)
        {
            if (argument != null && requiredType != null && !requiredType.IsInstanceOfType(argument))
            {
                ThrowArgumentException(message, argumentName);
            }
        }

		/// <summary>
        ///  Assert a boolean expression, throwing <code>ArgumentException</code>
	    ///  if the test result is <code>false</code>.
        /// </summary>
        /// <param name="expression">a boolean expression.</param>
        /// <param name="message">The exception message to use if the assertion fails.</param>
        /// <exception cref="ArgumentException">
        /// if expression is <code>false</code>
        /// </exception>
        public static void IsTrue(bool expression, string message)
        {
            if (!expression)
            {
                ThrowArgumentException(message);
            }
        }

        /// <summary>
        ///  Assert a boolean expression, throwing <code>ArgumentException</code>
        ///  if the test result is <code>false</code>.
        /// </summary>
        /// <param name="expression">a boolean expression.</param>
        /// <exception cref="ArgumentException">
        /// if expression is <code>false</code>
        /// </exception>
        public static void IsTrue(bool expression)
        {
            IsTrue(expression, "[Assertion failed] - this expression must be true");
        }

        /// <summary>
        /// Assert a bool expression, throwing <code>InvalidOperationException</code>
        /// if the expression is <code>false</code>.
        /// </summary>
        /// <param name="expression">a boolean expression.</param>
        /// <param name="message">The exception message to use if the assertion fails</param>
        /// <exception cref="InvalidOperationException">if expression is <code>false</code></exception>
        public static void State(bool expression, string message)
        {
            if (!expression)
            {
	            ThrowInvalidOperationException(message);
            }
        }

		private static void ThrowInvalidOperationException(string message)
		{
			throw new InvalidOperationException(message);
		}

		private static void ThrowNotSupportedException(string message)
		{
			throw new NotSupportedException(message);
		}

		private static void ThrowArgumentNullException(string paramName)
		{
			throw new ArgumentNullException(paramName, $"Argument '{paramName}' cannot be null.");
		}
		
		private static void ThrowArgumentNullException(string paramName, string message)
		{
			throw new ArgumentNullException(paramName, message);
		}
		
		private static void ThrowArgumentException(string message)
		{
			throw new ArgumentException(message);
		}
		
		private static void ThrowArgumentException(string message, string paramName)
		{
			throw new ArgumentException(message, paramName);
		}
	}
}