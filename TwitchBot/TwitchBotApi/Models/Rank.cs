using System;
using System.Collections.Generic;

namespace TwitchBotApi.Models
{
    public partial class Rank
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ExpCap { get; set; }
        public int Broadcaster { get; set; }

        public Broadcasters BroadcasterNavigation { get; set; }
    }
}
