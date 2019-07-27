using System;
using System.Collections.Generic;

namespace TwitchBotDb.Models
{
    public partial class TwitchGameCategory
    {
        public TwitchGameCategory()
        {
            BossFightBossStats = new HashSet<BossFightBossStats>();
            CustomCommand = new HashSet<CustomCommand>();
            InGameUsername = new HashSet<InGameUsername>();
            PartyUp = new HashSet<PartyUp>();
            Reminder = new HashSet<Reminder>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public bool Multiplayer { get; set; }

        public virtual ICollection<BossFightBossStats> BossFightBossStats { get; set; }
        public virtual ICollection<CustomCommand> CustomCommand { get; set; }
        public virtual ICollection<InGameUsername> InGameUsername { get; set; }
        public virtual ICollection<PartyUp> PartyUp { get; set; }
        public virtual ICollection<Reminder> Reminder { get; set; }
    }
}
