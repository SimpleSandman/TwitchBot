using System;
using System.Collections.Generic;
using System.IO;

namespace TwitchBotShared.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Get the index of a character based on the Nth occurrence
        /// </summary>
        /// <param name="s">The expression to be searched</param>
        /// <param name="findChar">The character in question</param>
        /// <param name="n">Nth index (zero-based)</param>
        /// <returns></returns>
        public static int GetNthCharIndex(this string s, char findChar, int n)
        {
            int count = 0;

            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == findChar)
                {
                    count++;
                    if (count == n)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        public static List<int> AllIndexesOf(this string s, string searchCriteria)
        {
            List<int> foundIndexes = new List<int>();

            for (int i = s.IndexOf(searchCriteria); i > -1; i = s.IndexOf(searchCriteria, i + 1))
            {
                foundIndexes.Add(i);
            }

            return foundIndexes;
        }

        public static Stream ToStream(this string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }

        public static string ReplaceLastOccurrence(this string source, string find, string replace)
        {
            int place = source.LastIndexOf(find);

            if (place == -1)
                return source;

            return source.Remove(place, find.Length).Insert(place, replace);
        }

        public static DateTime? ToNullableDateTime(this string s)
        {
            if (DateTime.TryParse(s, out DateTime i)) return i;

            return null;
        }

        public static int? ToNullableInt(this string s)
        {
            if (int.TryParse(s, out int i)) return i;

            return null;
        }        

        public static TimeSpan? ToNullableTimeSpan(this string s)
        {
            if (TimeSpan.TryParse(s, out TimeSpan i)) return i;

            return null;
        }
    }
}
