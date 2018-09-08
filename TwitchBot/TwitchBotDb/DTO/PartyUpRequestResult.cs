using System;

namespace TwitchBotDb.DTO
{
    public partial class PartyUpRequestResult
    {
        public int PartyRequestId { get; set; }
        public string Username { get; set; }
        public string PartyMemberName { get; set; }
        public int PartyMemberId { get; set; }
        public DateTime TimeRequested { get; set; }
    }
}
