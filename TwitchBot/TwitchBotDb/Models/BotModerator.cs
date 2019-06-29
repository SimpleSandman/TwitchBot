using System;
using System.Collections.Generic;

namespace TwitchBotDb.Models
{
    public partial class BotModerator
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public int TwitchId { get; set; }
        public int BroadcasterId { get; set; }

        public virtual Broadcaster Broadcaster { get; set; }
    }
}
