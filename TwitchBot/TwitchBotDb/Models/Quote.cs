using System;

namespace TwitchBotDb.Models
{
    public class Quote
    {
        public int Id { get; set; }
        public string UserQuote { get; set; }
        public string Username { get; set; }
        public DateTime TimeCreated { get; set; }
        public int BroadcasterId { get; set; }

        public virtual Broadcaster Broadcaster { get; set; }
    }
}
