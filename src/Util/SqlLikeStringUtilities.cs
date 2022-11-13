using System;
using System.Collections.Generic;
using System.Linq;

namespace Autofac.Annotation.Util
{
    /// <summary>
    /// 
    /// </summary>
    internal class SqlLikeStringUtilities
    {
        /// <summary>
        /// 检查字符串匹配 和sql的like一样
        /// </summary>
        /// <param name="pattern">匹配模式</param>
        /// <param name="str">匹配字符串</param>
        /// <returns></returns>
        public static bool SqlLike(string pattern, string str)
        {
            pattern = pattern.Replace("*", "%");
            bool isMatch = true,
                isWildCardOn = false,
                isCharWildCardOn = false,
                isCharSetOn = false,
                isNotCharSetOn = false,
                endOfPattern = false;
            int lastWildCard = -1;
            int patternIndex = 0;
            List<char> set = new List<char>();
            // 专门处理 ^()
            List<string> kuo = new List<string>();
            var kuoStr = "";
            char p = '\0';

            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                endOfPattern = ( !string.IsNullOrEmpty(kuoStr) || patternIndex >= pattern.Length);
                if (!endOfPattern)
                {
                    p = pattern[patternIndex];

                    if (!isWildCardOn && p == '%')
                    {
                        lastWildCard = patternIndex;
                        isWildCardOn = true;
                        while (patternIndex < pattern.Length &&
                               pattern[patternIndex] == '%')
                        {
                            patternIndex++;
                        }

                        if (patternIndex >= pattern.Length) p = '\0';
                        else p = pattern[patternIndex];
                    }
                    else if (p == '_')
                    {
                        isCharWildCardOn = true;
                        patternIndex++;
                    }
                    else if (p == '[')
                    {
                        if (pattern[++patternIndex] == '^')
                        {
                            isNotCharSetOn = true;
                            patternIndex++;
                        }
                        else isCharSetOn = true;

                        kuo.Clear();
                        kuoStr = "";
                        while (pattern[patternIndex] == '(')
                        {
                            var indexKu = 1;
                            while (pattern[patternIndex + indexKu] != ')')
                            {
                                indexKu++;
                            }
                            string d = pattern.Substring(patternIndex + 1, indexKu -1);
                            patternIndex += d.Length+2;
                            kuo.Add(d);
                        }

                        if (kuo.Any() && kuo.Select(r => r.Length).Distinct().Count() > 1)
                        {
                            throw new InvalidOperationException($"pattern:{pattern} is invaild");
                        }
                        
                        set.Clear();
                        // 如果是 [A-D] 那么就是把A B C D 都加进去
                        if (pattern[patternIndex + 1] == '-' && pattern[patternIndex + 3] == ']')
                        {
                            char start = (pattern[patternIndex]);
                            patternIndex += 2;
                            char end = (pattern[patternIndex]);
                            if (start <= end)
                            {
                                for (char ci = start; ci <= end; ci++)
                                {
                                    set.Add(ci);
                                }
                            }

                            patternIndex++;
                        }

                        while (patternIndex < pattern.Length &&
                               pattern[patternIndex] != ']')
                        {
                            set.Add(pattern[patternIndex]);
                            patternIndex++;
                        }

                        patternIndex++;
                    }
                }

                if (isWildCardOn)
                {
                    if ((c) == (p))
                    {
                        isWildCardOn = false;
                        patternIndex++;
                    }
                }
                else if (isCharWildCardOn)
                {
                    isCharWildCardOn = false;
                }
                else if (isCharSetOn || isNotCharSetOn)
                {
                    if (kuo.Any())
                    {
                        if(kuoStr.Length!=kuo.First().Length)
                        {
                            kuoStr += c;
                            continue;
                        }
                    }
                    bool charMatch = !string.IsNullOrEmpty(kuoStr) ? kuo.Contains(kuoStr) : (set.Contains(c));
                    if ((isNotCharSetOn && charMatch) || (isCharSetOn && !charMatch))
                    {
                        if (lastWildCard >= 0) patternIndex = lastWildCard;
                        else
                        {
                            isMatch = false;
                            break;
                        }
                    }

                    kuoStr = "";
                    isNotCharSetOn = isCharSetOn = false;
                }
                else
                {
                    if ((c) == (p))
                    {
                        patternIndex++;
                    }
                    else
                    {
                        if (lastWildCard >= 0) patternIndex = lastWildCard;
                        else
                        {
                            isMatch = false;
                            break;
                        }
                    }
                }
            }

            endOfPattern = (patternIndex >= pattern.Length);

            if (isMatch && !endOfPattern)
            {
                bool isOnlyWildCards = true;
                for (int i = patternIndex; i < pattern.Length; i++)
                {
                    if (pattern[i] != '%')
                    {
                        isOnlyWildCards = false;
                        break;
                    }
                }

                if (isOnlyWildCards) endOfPattern = true;
            }

            return isMatch && endOfPattern;
        }
    }
}