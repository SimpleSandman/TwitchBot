using System;
using System.Collections.Generic;

namespace TwitchBotApi.Models
{
    public partial class Moderators
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public int Broadcaster { get; set; }
        public DateTime TimeAdded { get; set; }

        public Broadcasters BroadcasterNavigation { get; set; }
    }
}
