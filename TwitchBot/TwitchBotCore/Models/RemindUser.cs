using System;

namespace TwitchBotCore.Models
{
    public class RemindUser
    {
        public int Id { get; set; }
        public int? GameId { get; set; }
        public bool[] IsReminderDay { get; set; }
        public TimeSpan? TimeOfEvent { get; set; }
        public int?[] ReminderSeconds { get; set; }
        public int? RemindEveryMin { get; set; }
        public string Message { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public bool IsCountdownEvent { get; set; }
        public bool HasCountdownTicker { get; set; }
    }
}
