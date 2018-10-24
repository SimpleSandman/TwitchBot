using System;
using System.Collections.Generic;
using System.IO;

namespace TwitchBotUtil.Extensions
{
    public static class StringExtensions
    {
        public static bool IsInt(this string s)
        {
            int x = 0;
            return int.TryParse(s, out x);
        }

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

        public static Stream ToStream(this string str)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(str);
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
    }
}
