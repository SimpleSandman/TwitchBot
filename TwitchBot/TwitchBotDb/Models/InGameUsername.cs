using System;
using System.Collections.Generic;

namespace TwitchBotDb.Models
{
    public partial class InGameUsername
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public int BroadcasterId { get; set; }
        public int? GameId { get; set; }

        public Broadcaster Broadcaster { get; set; }
        public TwitchGameCategory Game { get; set; }
    }
}
