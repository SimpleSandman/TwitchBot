using System;
using System.Collections.Generic;

namespace TwitchBotApi.Models
{
    public partial class UserBotTimeout
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public DateTime Timeout { get; set; }
        public DateTime TimeAdded { get; set; }
        public int Broadcaster { get; set; }

        public Broadcasters BroadcasterNavigation { get; set; }
    }
}
