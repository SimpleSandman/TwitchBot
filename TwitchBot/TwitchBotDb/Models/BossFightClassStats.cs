using System;
using System.Collections.Generic;

namespace TwitchBotDb.Models
{
    public partial class BossFightClassStats
    {
        public int Id { get; set; }
        public int SettingsId { get; set; }
        public int ViewerAttack { get; set; }
        public int ViewerDefense { get; set; }
        public int ViewerEvasion { get; set; }
        public int ViewerHealth { get; set; }
        public int FollowerAttack { get; set; }
        public int FollowerDefense { get; set; }
        public int FollowerEvasion { get; set; }
        public int FollowerHealth { get; set; }
        public int RegularAttack { get; set; }
        public int RegularDefense { get; set; }
        public int RegularEvasion { get; set; }
        public int RegularHealth { get; set; }
        public int ModeratorAttack { get; set; }
        public int ModeratorDefense { get; set; }
        public int ModeratorEvasion { get; set; }
        public int ModeratorHealth { get; set; }
        public int SubscriberAttack { get; set; }
        public int SubscriberDefense { get; set; }
        public int SubscriberEvasion { get; set; }
        public int SubscriberHealth { get; set; }

        public BossFightSetting Settings { get; set; }
    }
}
