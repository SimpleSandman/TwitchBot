using System;

namespace TwitchBotCore.Models
{
    public class DelayedMessage
    {
        public int ReminderId { get; set; }
        public string Message { get; set; }
        public DateTime SendDate { get; set; }
        public int? ReminderEveryMin { get; set; }
        public DateTime? ExpirationDateUtc { get; set; }
    }
}
