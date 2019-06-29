using System;
using System.Collections.Generic;

namespace TwitchBotDb.Models
{
    public partial class PartyUpRequest
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public int? TwitchId { get; set; }
        public int PartyMemberId { get; set; }
        public DateTime TimeRequested { get; set; }

        public virtual PartyUp PartyMember { get; set; }
    }
}
