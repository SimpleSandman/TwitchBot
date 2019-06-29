using System;
using System.Collections.Generic;

namespace TwitchBotDb.Models
{
    public partial class BossFightBossStats
    {
        public int Id { get; set; }
        public int SettingsId { get; set; }
        public int? GameId { get; set; }
        public string Name1 { get; set; }
        public int MaxUsers1 { get; set; }
        public int Attack1 { get; set; }
        public int Defense1 { get; set; }
        public int Evasion1 { get; set; }
        public int Health1 { get; set; }
        public int TurnLimit1 { get; set; }
        public int Loot1 { get; set; }
        public int LastAttackBonus1 { get; set; }
        public string Name2 { get; set; }
        public int MaxUsers2 { get; set; }
        public int Attack2 { get; set; }
        public int Defense2 { get; set; }
        public int Evasion2 { get; set; }
        public int Health2 { get; set; }
        public int TurnLimit2 { get; set; }
        public int Loot2 { get; set; }
        public int LastAttackBonus2 { get; set; }
        public string Name3 { get; set; }
        public int MaxUsers3 { get; set; }
        public int Attack3 { get; set; }
        public int Defense3 { get; set; }
        public int Evasion3 { get; set; }
        public int Health3 { get; set; }
        public int TurnLimit3 { get; set; }
        public int Loot3 { get; set; }
        public int LastAttackBonus3 { get; set; }
        public string Name4 { get; set; }
        public int MaxUsers4 { get; set; }
        public int Attack4 { get; set; }
        public int Defense4 { get; set; }
        public int Evasion4 { get; set; }
        public int Health4 { get; set; }
        public int TurnLimit4 { get; set; }
        public int Loot4 { get; set; }
        public int LastAttackBonus4 { get; set; }
        public string Name5 { get; set; }
        public int MaxUsers5 { get; set; }
        public int Attack5 { get; set; }
        public int Defense5 { get; set; }
        public int Evasion5 { get; set; }
        public int Health5 { get; set; }
        public int TurnLimit5 { get; set; }
        public int Loot5 { get; set; }
        public int LastAttackBonus5 { get; set; }

        public virtual TwitchGameCategory Game { get; set; }
        public virtual BossFightSetting Settings { get; set; }
    }
}
