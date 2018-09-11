using System;
using System.Collections.Generic;

namespace TwitchBotDb.Models
{
    public partial class BotTimeout
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public int TwitchId { get; set; }
        public DateTime Timeout { get; set; }
        public DateTime TimeAdded { get; set; }
        public int BroadcasterId { get; set; }

        public Broadcaster Broadcaster { get; set; }
    }
}
