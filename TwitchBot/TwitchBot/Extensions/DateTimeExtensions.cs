using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime? ToNullableDateTime(this string s)
        {
            if (DateTime.TryParse(s, out DateTime i)) return i;

            return null;
        }
    }
}
