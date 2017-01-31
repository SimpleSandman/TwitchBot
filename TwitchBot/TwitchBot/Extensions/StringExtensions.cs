using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot
{
    public static class StringExtensions
    {
        public static bool IsInt(this string s)
        {
            int x = 0;
            return int.TryParse(s, out x);
        }
    }
}
