using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot.Models
{
    public class Reminder
    {
        public bool[] IsReminderDay { get; set; }
        public TimeSpan TimeToPost { get; set; }
        public int?[] ReminderSeconds { get; set; }
        public string Message { get; set; }
    }
}
