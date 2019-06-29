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
        public string PartyMemberName { get; set; }
        public int GameId { get; set; }
        public int BroadcasterId { get; set; }

        public virtual Broadcaster Broadcaster { get; set; }
        public virtual TwitchGameCategory Game { get; set; }
        public virtual ICollection<PartyUpRequest> PartyUpRequest { get; set; }
    }
}
