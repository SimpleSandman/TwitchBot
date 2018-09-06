using System;
using System.Collections.Generic;

namespace TwitchBotDb.Models
{
    public partial class RankFollower
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public int Experience { get; set; }
        public int? TwitchId { get; set; }
        public int Broadcaster { get; set; }

        public Broadcaster BroadcasterNavigation { get; set; }
    }
}
