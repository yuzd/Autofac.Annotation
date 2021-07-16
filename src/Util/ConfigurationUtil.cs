using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Autofac.Annotation.Util
{
    internal class ConfigurationUtil
    {
        /// <summary>
        /// 从配置文件中获取类型值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object GetConfiguredParameterValue(IConfigurationSection value)
        {
            var subKeys = value.GetChildren().Select(sk => new Tuple<string, string>(GetKeyName(sk.Key), sk.Value)).ToArray();
            if (subKeys.Length == 0)
            {
                // No subkeys indicates a scalar value.
                return value.Value;
            }

            int parsed;
            if (subKeys.All(sk => int.TryParse(sk.Item1, out parsed)))
            {
                int i = 0;
                bool isList = true;
                foreach (int subKey in subKeys.Select(sk => int.Parse(sk.Item1, CultureInfo.InvariantCulture)))
                {
                    if (subKey != i)
                    {
                        isList = false;
                        break;
                    }

                    i++;
                }

                if (isList)
                {
                    var list = new List<string>();
                    foreach (var subKey in subKeys)
                    {
                        list.Add(subKey.Item2);
                    }

                    return new ConfiguredListParameter {List = list.ToArray()};
                }
            }

            // There are subkeys but not all zero-based sequential numbers - it's a dictionary.
            var dict = new Dictionary<string, string>();
            foreach (var subKey in subKeys)
            {
                dict[subKey.Item1] = subKey.Item2;
            }

            return new ConfiguredDictionaryParameter {Dictionary = dict};
        }

        private static string GetKeyName(string fullKey)
        {
            int index = fullKey.LastIndexOf(':');
            if (index < 0)
            {
                return fullKey;
            }

            return fullKey.Substring(index + 1);
        }
    }
}