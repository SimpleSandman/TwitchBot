using System;
using System.Collections.Generic;

namespace TwitchBotDb.Models
{
    public partial class BotTimeout
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public DateTime Timeout { get; set; }
        public DateTime TimeAdded { get; set; }
        public int Broadcaster { get; set; }

        public Broadcaster BroadcasterNavigation { get; set; }
    }
}
