using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TwitchBot.Enums;

namespace TwitchBot.Models
{
    public class BossFighter
    {
        public string Username { get; set; }
        public int Gamble { get; set; }
        public FighterClass FighterClass { get; set; }
    }

    public class FighterClass
    {
        public ChatterType ChatterType { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int Evasion { get; set; }
        public int Health { get; set; }
    }
}
