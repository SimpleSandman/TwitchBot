using System;
using System.Collections.Generic;

namespace TwitchBotApi.Models
{
    public partial class RankFollowers
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public int Exp { get; set; }
        public int Broadcaster { get; set; }

        public Broadcasters BroadcasterNavigation { get; set; }
    }
}
