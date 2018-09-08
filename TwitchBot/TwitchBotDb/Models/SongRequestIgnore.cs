using System;
using System.Collections.Generic;

namespace TwitchBotDb.Models
{
    public partial class SongRequestIgnore
    {
        public int Id { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }
        public int BroadcasterId { get; set; }

        public Broadcaster Broadcaster { get; set; }
    }
}
