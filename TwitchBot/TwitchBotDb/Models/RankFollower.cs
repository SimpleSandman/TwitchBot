using System;
using System.Collections.Generic;

namespace TwitchBotDb.Models
{
    public partial class RankFollower
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public int? TwitchId { get; set; }
        public int Experience { get; set; }
        public int BroadcasterId { get; set; }

        public Broadcaster Broadcaster { get; set; }
    }
}
