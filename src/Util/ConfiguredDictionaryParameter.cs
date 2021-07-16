using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace Autofac.Annotation.Util
{
    [TypeConverter(typeof(DictionaryTypeConverter))]
    internal class ConfiguredDictionaryParameter
    {
        public Dictionary<string, string> Dictionary { get; set; }

        private class DictionaryTypeConverter : TypeConverter
        {
            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                var instantiatableType = GetInstantiableType(destinationType);

                var castValue = value as ConfiguredDictionaryParameter;
                if (castValue != null && instantiatableType != null)
                {
                    var dictionary = (IDictionary) Activator.CreateInstance(instantiatableType);
                    var generics = instantiatableType.GetGenericArguments();

                    foreach (var item in castValue.Dictionary)
                    {
                        if (String.IsNullOrEmpty(item.Key))
                        {
                            throw new FormatException("DictionaryKeyMayNotBeNullOrEmpty");
                        }

                        var convertedKey = TypeManipulation.ChangeToCompatibleType(item.Key, generics[0]);
                        var convertedValue = TypeManipulation.ChangeToCompatibleType(item.Value, generics[1]);

                        dictionary.Add(convertedKey, convertedValue);
                    }

                    return dictionary;
                }

                return base.ConvertTo(context, culture, value, destinationType);
            }

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                if (GetInstantiableType(destinationType) != null)
                {
                    return true;
                }

                return base.CanConvertTo(context, destinationType);
            }

            private static Type GetInstantiableType(Type destinationType)
            {
                if (typeof(IDictionary).IsAssignableFrom(destinationType) ||
                    (destinationType.IsConstructedGenericType && typeof(IDictionary<,>).IsAssignableFrom(destinationType.GetGenericTypeDefinition())))
                {
                    var generics = destinationType.IsConstructedGenericType ? destinationType.GetGenericArguments() : new[] {typeof(string), typeof(object)};
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
        }
    }
}