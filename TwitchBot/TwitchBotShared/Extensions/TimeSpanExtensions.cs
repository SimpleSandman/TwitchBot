using System;

namespace TwitchBotShared.Extensions
{
    public static class TimeSpanExtensions
    {
        public static string ToReadableString(this TimeSpan span)
        {
            string formatted = string.Format("{0}{1}{2}{3}",
                span.Duration().Days > 0 ? string.Format("{0:0} day{1}, ", span.Days, span.Days == 1 ? String.Empty : "s") : string.Empty,
                span.Duration().Hours > 0 ? string.Format("{0:0} hour{1}, ", span.Hours, span.Hours == 1 ? String.Empty : "s") : string.Empty,
                span.Duration().Minutes > 0 ? string.Format("{0:0} minute{1}, ", span.Minutes, span.Minutes == 1 ? String.Empty : "s") : string.Empty,
                span.Duration().Seconds > 0 ? string.Format("{0:0} second{1}", span.Seconds, span.Seconds == 1 ? String.Empty : "s") : string.Empty);

            if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

            if (string.IsNullOrEmpty(formatted)) formatted = "0 seconds";

            return formatted;
        }

        public static string ReformatTimeSpan(this TimeSpan ts)
        {
            string response = "";

            // format minutes
            if (ts.Minutes < 1)
                response += $"[00:";
            else if (ts.Minutes > 0 && ts.Minutes < 10)
                response += $"[0{ts.Minutes}:";
            else
                response += $"[{ts.Minutes}:";

            // format seconds
            if (ts.Seconds < 1)
                response += $"00]";
            else if (ts.Seconds > 0 && ts.Seconds < 10)
                response += $"0{ts.Seconds}]";
            else
                response += $"{ts.Seconds}]";

            return response;
        }
    }
}
