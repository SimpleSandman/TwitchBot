using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot.Models
{
    public class DelayedMessage
    {
        public string Message { get; set; }
        public DateTime SendDate { get; set; }
        public int? ReminderEveryMin { get; set; }
    }
}
