using System.Collections.Generic;

namespace TwitchBotDb.Models
{
    public class PartyUp
    {
        public PartyUp()
        {
            PartyUpRequests = new HashSet<PartyUpRequest>();
        }

        public int Id { get; set; }
        public string PartyMemberName { get; set; }
        public int GameId { get; set; }
        public int BroadcasterId { get; set; }

        public virtual Broadcaster Broadcaster { get; set; }
        public virtual TwitchGameCategory Game { get; set; }
        public virtual ICollection<PartyUpRequest> PartyUpRequests { get; set; }
    }
}
