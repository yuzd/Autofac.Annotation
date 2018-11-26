using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autofac.Annotation.Util
{
   [TypeConverter(typeof(ListTypeConverter))]
    internal class ConfiguredListParameter
    {
        public string[] List { get; set; }

        private class ListTypeConverter : TypeConverter
        {
            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                if (GetInstantiableListType(destinationType) != null || GetInstantiableDictionaryType(destinationType) != null)
                {
                    return true;
                }

                return base.CanConvertTo(context, destinationType);
            }

            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                var castValue = value as ConfiguredListParameter;
                if (castValue != null)
                {
                    // 99% of the time this type of parameter will be associated
                    // with an ordinal list - List<T> or T[] sort of thing...
                    var instantiatableType = GetInstantiableListType(destinationType);
                    if (instantiatableType != null)
                    {
                        var generics = instantiatableType.GetGenericArguments();
                        var collection = (IList)Activator.CreateInstance(instantiatableType);
                        foreach (string item in castValue.List)
                        {
                            collection.Add(TypeManipulation.ChangeToCompatibleType(item, generics[0]));
                        }

                        return collection;
                    }

                    // ...but there is a very small chance this is a Dictionary<int, T> where
                    // the keys are all 0-based and ordinal. This clause checks for
                    // that one edge case. We should only have gotten here if
                    // ConfigurationExtensions.GetConfiguredParameterValue saw
                    // a 0-based configuration dictionary.
                    instantiatableType = GetInstantiableDictionaryType(destinationType);
                    if (instantiatableType != null)
                    {
                        var dictionary = (IDictionary)Activator.CreateInstance(instantiatableType);
                        var generics = instantiatableType.GetGenericArguments();

                        for (int i = 0; i < castValue.List.Length; i++)
                        {
                            var convertedKey = TypeManipulation.ChangeToCompatibleType(i, generics[0]);
                            var convertedValue = TypeManipulation.ChangeToCompatibleType(castValue.List[i], generics[1]);

                            dictionary.Add(convertedKey, convertedValue);
                        }

                        return dictionary;
                    }
                }

                return base.ConvertTo(context, culture, value, destinationType);
            }

            /// <summary>
            /// Handles type determination for the case where the dictionary
            /// has numeric/ordinal keys.
            /// </summary>
            /// <param name="destinationType">
            /// The type to which the list content should be converted.
            /// </param>
            /// <returns>
            /// A dictionary type where the key can be numeric.
            /// </returns>
            private static Type GetInstantiableDictionaryType(Type destinationType)
            {
                if (typeof(IDictionary).IsAssignableFrom(destinationType) ||
                    (destinationType.IsConstructedGenericType && typeof(IDictionary<,>).IsAssignableFrom(destinationType.GetGenericTypeDefinition())))
                {
                    var generics = destinationType.IsConstructedGenericType ? destinationType.GetGenericArguments() : new[] { typeof(int), typeof(object) };
                    if (generics.Length != 2)
                    {
                        return null;
                    }

                    var dictType = typeof(Dictionary<,>).MakeGenericType(generics);
                    if (destinationType.IsAssignableFrom(dictType))
                    {
                        return dictType;
                    }
                }

                return null;
            }

            /// <summary>
            /// Handles type determination list conversion.
            /// </summary>
            /// <param name="destinationType">
            /// The type to which the list content should be converted.
            /// </param>
            /// <returns>
            /// A list type compatible with the data values.
            /// </returns>
            private static Type GetInstantiableListType(Type destinationType)
            {
                if (typeof(IEnumerable).IsAssignableFrom(destinationType))
                {
                    var generics = destinationType.IsConstructedGenericType ? destinationType.GetGenericArguments() : new[] { typeof(object) };
                    if (generics.Length != 1)
                    {
                        return null;
                    }

                    var listType = typeof(List<>).MakeGenericType(generics);

                    if (destinationType.IsAssignableFrom(listType))
                    {
                        return listType;
                    }
                }

                return null;
            }
        }
    }
}
