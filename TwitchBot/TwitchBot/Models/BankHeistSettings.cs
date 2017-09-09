using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot.Models
{
    public class BankHeistSettings
    {
        // Entry Messages
        public int EntryCooldown { get; set; }
        public string EntryMessage { get; set; }
        public string MaxPointText { get; set; }
        public string EntryInstructions { get; set; }
        public string LateEntry { get; set; }
        public string CooldownEntry { get; set; }
        public string CooldownOver { get; set; }

        // Next Level Entry Messages (Level 2-5)
        public string[] NextLevelMessages { get; set; }

        // Game Outcomes
        public string GameStart { get; set; }
        public string SingleUserSuccess { get; set; }
        public string SingleUserFail { get; set; }
        public string Success100 { get; set; }
        public string Success34_99 { get; set; }
        public string Success1_33 { get; set; }
        public string Success0 { get; set; }
        public string Results { get; set; }

        // Game Levels (Level 1-5)
        public BankHeistLevel[] BankHeistLevels { get; set; }

        // Payouts
        public Payout[] Payouts { get; set; }
    }

    public class BankHeistLevel
    {
        public string LevelBankName { get; set; }
        public int MaxUsers { get; set; }
    }

    public class Payout
    {
        public int WinPercentage { get; set; }
        public decimal WinMultiplier { get; set; }
    }
}
