using System;
using System.Collections.Generic;

namespace TwitchBotDb.Models
{
    public partial class Rank
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ExpCap { get; set; }
        public int Broadcaster { get; set; }

        public Broadcaster BroadcasterNavigation { get; set; }
    }
}
