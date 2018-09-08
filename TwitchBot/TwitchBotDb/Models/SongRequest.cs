using System;
using System.Collections.Generic;

namespace TwitchBotDb.Models
{
    public partial class SongRequest
    {
        public int Id { get; set; }
        public string Requests { get; set; }
        public string Chatter { get; set; }
        public int BroadcasterId { get; set; }

        public Broadcaster Broadcaster { get; set; }
    }
}
