using System;
using System.Collections.Generic;

namespace TwitchBotDb.Models
{
    public partial class BossFightSetting
    {
        public BossFightSetting()
        {
            BossFightBossStats = new HashSet<BossFightBossStats>();
            BossFightClassStats = new HashSet<BossFightClassStats>();
        }

        public int Id { get; set; }
        public int CooldownPeriodMin { get; set; }
        public int EntryPeriodSec { get; set; }
        public string EntryMessage { get; set; }
        public int Cost { get; set; }
        public string CooldownEntry { get; set; }
        public string CooldownOver { get; set; }
        public string NextLevelMessage2 { get; set; }
        public string NextLevelMessage3 { get; set; }
        public string NextLevelMessage4 { get; set; }
        public string NextLevelMessage5 { get; set; }
        public string GameStart { get; set; }
        public string ResultsMessage { get; set; }
        public string SingleUserSuccess { get; set; }
        public string SingleUserFail { get; set; }
        public string Success100 { get; set; }
        public string Success34 { get; set; }
        public string Success1 { get; set; }
        public string Success0 { get; set; }
        public int Broadcaster { get; set; }

        public Broadcaster BroadcasterNavigation { get; set; }
        public ICollection<BossFightBossStats> BossFightBossStats { get; set; }
        public ICollection<BossFightClassStats> BossFightClassStats { get; set; }
    }
}
