using System;
using System.Collections.Generic;

namespace TwitchBotDb.Models
{
    public partial class PartyUp
    {
        public int Id { get; set; }
        public string PartyMember { get; set; }
        public int Game { get; set; }
        public int Broadcaster { get; set; }

        public Broadcasters BroadcasterNavigation { get; set; }
        public GameList GameNavigation { get; set; }
    }
}
