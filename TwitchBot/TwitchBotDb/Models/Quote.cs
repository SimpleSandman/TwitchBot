using System;
using System.Collections.Generic;

namespace TwitchBotDb.Models
{
    public partial class Quote
    {
        public int Id { get; set; }
        public string UserQuote { get; set; }
        public string Username { get; set; }
        public DateTime TimeCreated { get; set; }
        public int BroadcasterId { get; set; }

        public Broadcaster Broadcaster { get; set; }
    }
}
