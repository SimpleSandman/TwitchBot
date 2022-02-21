using System.Collections.Generic;

namespace TwitchBotDb.Models
{
    public class TwitchGameCategory
    {
        public TwitchGameCategory()
        {
            BossFightBossStats = new HashSet<BossFightBossStats>();
            CustomCommands = new HashSet<CustomCommand>();
            InGameUsernames = new HashSet<InGameUsername>();
            PartyUps = new HashSet<PartyUp>();
            Reminders = new HashSet<Reminder>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public bool Multiplayer { get; set; }

        public virtual ICollection<BossFightBossStats> BossFightBossStats { get; set; }
        public virtual ICollection<CustomCommand> CustomCommands { get; set; }
        public virtual ICollection<InGameUsername> InGameUsernames { get; set; }
        public virtual ICollection<PartyUp> PartyUps { get; set; }
        public virtual ICollection<Reminder> Reminders { get; set; }
    }
}
