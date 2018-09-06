using System;
using System.Collections.Generic;

namespace TwitchBotDb.Models
{
    public partial class SongRequest
    {
        public int Id { get; set; }
        public string Requests { get; set; }
        public string Chatter { get; set; }
        public int Broadcaster { get; set; }

        public Broadcaster BroadcasterNavigation { get; set; }
    }
}
