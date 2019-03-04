using System;
using System.Collections.Generic;

namespace TwitchBotDb.Models
{
    public partial class InGameUsername
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public int Broadcaster { get; set; }
        public int? GameId { get; set; }

        public Broadcaster BroadcasterNavigation { get; set; }
        public TwitchGameCategory Game { get; set; }
    }
}
