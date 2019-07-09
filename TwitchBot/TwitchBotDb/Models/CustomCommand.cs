using System;
using System.Collections.Generic;

namespace TwitchBotDb.Models
{
    public partial class CustomCommand
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }
        public bool IsSound { get; set; }
        public bool IsGlobalCooldown { get; set; }
        public int CooldownSec { get; set; }
        public string Whitelist { get; set; }
        public int BroadcasterId { get; set; }

        public virtual Broadcaster Broadcaster { get; set; }
    }
}
