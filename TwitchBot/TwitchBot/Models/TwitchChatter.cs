using System;

namespace TwitchBot.Models
{
    public class TwitchChatter
    {
        public string Username { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string Badges { get; set; }
        public string Message { get; set; }
        public string TwitchId { get; set; }
        public string MessageId { get; set; }
    }
}
