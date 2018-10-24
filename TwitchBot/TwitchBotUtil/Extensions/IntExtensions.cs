using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBotUtil.Extensions
{
    public static class IntExtensions
    {
        public static int? ToNullableInt(this string s)
        {
            if (int.TryParse(s, out int i)) return i;

            return null;
        }
    }
}
