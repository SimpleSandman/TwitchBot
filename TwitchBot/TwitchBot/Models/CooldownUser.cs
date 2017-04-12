using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot.Models
{
    public class CooldownUser
    {
        public string Username { get; set; }
        public DateTime Cooldown { get; set; }
        public string Command { get; set; }
        public bool Warned { get; set; }
    }
}
