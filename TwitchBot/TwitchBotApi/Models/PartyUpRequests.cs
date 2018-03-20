using System;
using System.Collections.Generic;

namespace TwitchBotApi.Models
{
    public partial class PartyUpRequests
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PartyMember { get; set; }
        public DateTime TimeRequested { get; set; }
        public int Broadcaster { get; set; }
        public int Game { get; set; }

        public Broadcasters BroadcasterNavigation { get; set; }
        public GameList GameNavigation { get; set; }
    }
}
