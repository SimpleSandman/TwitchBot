using System;

namespace TwitchBotShared.Models
{
    public class CooldownUser
    {
        public string Username { get; set; }
        public DateTime Cooldown { get; set; }
        public string Command { get; set; }
        public bool Warned { get; set; }
    }
}
