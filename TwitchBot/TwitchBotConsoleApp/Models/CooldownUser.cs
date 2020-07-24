using System;

namespace TwitchBotConsoleApp.Models
{
    public partial class CooldownUser
    {
        public string Username { get; set; }
        public DateTime Cooldown { get; set; }
        public string Command { get; set; }
        public bool Warned { get; set; }
    }
}
