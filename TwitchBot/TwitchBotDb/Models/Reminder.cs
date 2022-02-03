using System;

namespace TwitchBotDb.Models
{
    public partial class Reminder
    {
        public int Id { get; set; }
        public bool Sunday { get; set; }
        public bool Monday { get; set; }
        public bool Tuesday { get; set; }
        public bool Wednesday { get; set; }
        public bool Thursday { get; set; }
        public bool Friday { get; set; }
        public bool Saturday { get; set; }
        public int? ReminderSec1 { get; set; }
        public int? ReminderSec2 { get; set; }
        public int? ReminderSec3 { get; set; }
        public int? ReminderSec4 { get; set; }
        public int? ReminderSec5 { get; set; }
        public int? RemindEveryMin { get; set; }
        public TimeSpan? TimeOfEventUtc { get; set; }
        public DateTime? ExpirationDateUtc { get; set; }
        public bool IsCountdownEvent { get; set; }
        public bool HasCountdownTicker { get; set; }
        public string Message { get; set; }
        public int BroadcasterId { get; set; }
        public int? GameId { get; set; }

        public virtual Broadcaster Broadcaster { get; set; }
        public virtual TwitchGameCategory Game { get; set; }
    }
}
