using System;
using System.Collections.Generic;

namespace TwitchBotDb.Models
{
    public partial class PartyUp
    {
        public PartyUp()
        {
            PartyUpRequest = new HashSet<PartyUpRequest>();
        }

        public int Id { get; set; }
        public string PartyMember { get; set; }
        public int GameId { get; set; }
        public int Broadcaster { get; set; }

        public Broadcaster BroadcasterNavigation { get; set; }
        public TwitchGameCategory Game { get; set; }
        public ICollection<PartyUpRequest> PartyUpRequest { get; set; }
    }
}
