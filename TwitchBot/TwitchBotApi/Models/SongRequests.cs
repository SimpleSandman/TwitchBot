using System;
using System.Collections.Generic;

namespace TwitchBotApi.Models
{
    public partial class SongRequests
    {
        public int Id { get; set; }
        public string Requests { get; set; }
        public string Chatter { get; set; }
        public int Broadcaster { get; set; }

        public Broadcasters BroadcasterNavigation { get; set; }
    }
}
