using System;
using System.Collections.Generic;

namespace TwitchBotApi.Models
{
    public partial class SongRequestBlacklist
    {
        public int Id { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }
        public int Broadcaster { get; set; }

        public Broadcasters BroadcasterNavigation { get; set; }
    }
}
