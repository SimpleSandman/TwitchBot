using System;
using System.Collections.Generic;

namespace TwitchBotApi.Models
{
    public partial class Quote
    {
        public int Id { get; set; }
        public string UserQuote { get; set; }
        public string Username { get; set; }
        public DateTime TimeCreated { get; set; }
        public int Broadcaster { get; set; }

        public Broadcasters BroadcasterNavigation { get; set; }
    }
}
