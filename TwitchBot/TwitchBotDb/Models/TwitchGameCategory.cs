using System;
using System.Collections.Generic;

namespace TwitchBotDb.Models
{
    public partial class TwitchGameCategory
    {
        public TwitchGameCategory()
        {
            BossFightBossStats = new HashSet<BossFightBossStats>();
            PartyUp = new HashSet<PartyUp>();
            Reminder = new HashSet<Reminder>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public bool Multiplayer { get; set; }

        public ICollection<BossFightBossStats> BossFightBossStats { get; set; }
        public ICollection<PartyUp> PartyUp { get; set; }
        public ICollection<Reminder> Reminder { get; set; }
    }
}
