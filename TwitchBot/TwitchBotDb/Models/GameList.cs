using System;
using System.Collections.Generic;

namespace TwitchBotDb.Models
{
    public partial class GameList
    {
        public GameList()
        {
            BossFightBossStats = new HashSet<BossFightBossStats>();
            PartyUp = new HashSet<PartyUp>();
            PartyUpRequests = new HashSet<PartyUpRequests>();
            Reminders = new HashSet<Reminders>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool Multiplayer { get; set; }

        public ICollection<BossFightBossStats> BossFightBossStats { get; set; }
        public ICollection<PartyUp> PartyUp { get; set; }
        public ICollection<PartyUpRequests> PartyUpRequests { get; set; }
        public ICollection<Reminders> Reminders { get; set; }
    }
}
