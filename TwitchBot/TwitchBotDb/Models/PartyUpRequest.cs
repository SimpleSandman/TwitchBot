using System;
using System.Collections.Generic;

namespace TwitchBotDb.Models
{
    public partial class PartyUpRequest
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public int PartyMember { get; set; }
        public DateTime TimeRequested { get; set; }

        public PartyUpRequest IdNavigation { get; set; }
        public PartyUp PartyMemberNavigation { get; set; }
        public PartyUpRequest InverseIdNavigation { get; set; }
    }
}
