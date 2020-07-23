using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using TwitchBotConsoleApp.Enums;

using TwitchBotDb.Models;
using TwitchBotUtil.Libraries;

namespace TwitchBotConsoleApp.Models
{
    public class BossFightSingleton
    {
        public BlockingCollection<BossFighter> Fighters { get; set; }

        /* Settings */
        public int SettingsId { get; set; }
        public int CooldownTimePeriodMinutes { get; set; }
        public int EntryPeriodSeconds { get; set; }
        public DateTime CooldownTimePeriod { get; set; }
        public DateTime EntryPeriod { get; set; }
        public int Cost { get; set; }
        public bool RefreshBossFight { get; set; } = false;

        // Entry Messages
        public string EntryMessage { get; set; }
        public string CooldownEntry { get; set; }
        public string CooldownOver { get; set; }

        // Next Level Entry Messages (Level 2-5)
        public string[] NextLevelMessages { get; set; }

        // Game Outcomes
        public string GameStart { get; set; }
        public string ResultsMessage { get; set; }
        public string SingleUserSuccess { get; set; }
        public string SingleUserFail { get; set; }
        public string Success100 { get; set; }
        public string Success34 { get; set; }
        public string Success1 { get; set; }
        public string Success0 { get; set; }

        // Fighter Classes
        public FighterClass[] ClassStats { get; set; }

        // Boss Levels (Level 1-5)
        public Boss[] Bosses { get; set; }

        /* Singleton Instance */
        private static volatile BossFightSingleton _instance;
        private static object _syncRoot = new Object();

        private BossFightSingleton() { }

        public static BossFightSingleton Instance
        {
            get
            {
                // first check
                if (_instance == null)
                {
                    lock (_syncRoot)
                    {
                        // second check
                        if (_instance == null)
                            _instance = new BossFightSingleton();
                    }
                }

                return _instance;
            }
        }

        public bool IsEntryPeriodOver()
        {
            return EntryPeriod < DateTime.Now ? true : false;
        }

        public bool IsBossFightOnCooldown()
        {
            return CooldownTimePeriod > DateTime.Now ? true : false;
        }

        /// <summary>
        /// Load all of the settings from the database for the boss fight mini-game
        /// </summary>
        /// <param name="broadcasterId"></param>
        /// <param name="gameId"></param>
        /// <param name="twitchBotApiLink"></param>
        public async Task LoadSettings(int broadcasterId, int? gameId, string twitchBotApiLink)
        {
            BossFightClassStats bossFightClassStats = null;
            BossFightBossStats bossFightBossStats = null;

            BossFightSetting bossFightSetting = await ApiBotRequest.GetExecuteAsync<BossFightSetting>(twitchBotApiLink + $"bossfightsettings/get/{broadcasterId}");

            if (bossFightSetting == null)
            {
                bossFightSetting = new BossFightSetting { BroadcasterId = broadcasterId };
                bossFightSetting = await ApiBotRequest.PostExecuteAsync(twitchBotApiLink + $"bossfightsettings/create", bossFightSetting);
            }

            if (bossFightSetting == null)
                throw new Exception("Unable to create initial boss fight settings");

            SettingsId = bossFightSetting.Id;

            bossFightClassStats = await ApiBotRequest.GetExecuteAsync<BossFightClassStats>(twitchBotApiLink + $"bossfightclassstats/get/{SettingsId}");

            if (bossFightClassStats == null)
            {
                bossFightClassStats = new BossFightClassStats { SettingsId = SettingsId };
                bossFightClassStats = await ApiBotRequest.PostExecuteAsync(twitchBotApiLink + $"bossfightclassstats/create", bossFightClassStats);
            }

            if (bossFightClassStats == null)
                throw new Exception("Unable to create boss fight class stats");

            bossFightBossStats = await ApiBotRequest.GetExecuteAsync<BossFightBossStats>(twitchBotApiLink + $"bossfightbossstats/get/{SettingsId}?gameId={gameId}");

            if (bossFightBossStats == null)
            {
                bossFightBossStats = new BossFightBossStats { SettingsId = SettingsId };
                bossFightBossStats = await ApiBotRequest.PostExecuteAsync(twitchBotApiLink + $"bossfightbossstats/create", bossFightBossStats);
            }

            if (bossFightBossStats == null)
                throw new Exception("Unable to create boss fight boss stats");

            // refresh arrays and lists
            NextLevelMessages = new string[4];
            Bosses = new Boss[]
            {
                new Boss { },
                new Boss { },
                new Boss { },
                new Boss { },
                new Boss { }
            };
            ClassStats = new FighterClass[]
            {
                new FighterClass { },
                new FighterClass { },
                new FighterClass { },
                new FighterClass { },
                new FighterClass { }
            };
            Fighters = new BlockingCollection<BossFighter>();

            // entry messages and initial settings
            CooldownTimePeriodMinutes = bossFightSetting.CooldownPeriodMin;
            EntryPeriodSeconds = bossFightSetting.EntryPeriodSec;
            EntryMessage = bossFightSetting.EntryMessage;
            Cost = bossFightSetting.Cost;
            CooldownEntry = bossFightSetting.CooldownEntry;
            CooldownOver = bossFightSetting.CooldownOver;

            // next level messages
            NextLevelMessages[0] = bossFightSetting.NextLevelMessage2;
            NextLevelMessages[1] = bossFightSetting.NextLevelMessage3;
            NextLevelMessages[2] = bossFightSetting.NextLevelMessage4;
            NextLevelMessages[3] = bossFightSetting.NextLevelMessage5;

            // game outcomes
            GameStart = bossFightSetting.GameStart;
            ResultsMessage = bossFightSetting.ResultsMessage;
            SingleUserSuccess = bossFightSetting.SingleUserSuccess;
            SingleUserFail = bossFightSetting.SingleUserFail;
            Success100 = bossFightSetting.Success100;
            Success34 = bossFightSetting.Success34;
            Success1 = bossFightSetting.Success1;
            Success0 = bossFightSetting.Success0;
                
            // fighter class stats
            ClassStats[0].ChatterType = ChatterType.Viewer;
            ClassStats[0].Attack = bossFightClassStats.ViewerAttack;
            ClassStats[0].Defense = bossFightClassStats.ViewerDefense;
            ClassStats[0].Evasion = bossFightClassStats.ViewerEvasion;
            ClassStats[0].Health = bossFightClassStats.ViewerHealth;
            ClassStats[1].ChatterType = ChatterType.Follower;
            ClassStats[1].Attack = bossFightClassStats.FollowerAttack;
            ClassStats[1].Defense = bossFightClassStats.FollowerDefense;
            ClassStats[1].Evasion = bossFightClassStats.FollowerEvasion;
            ClassStats[1].Health = bossFightClassStats.FollowerHealth;
            ClassStats[2].ChatterType = ChatterType.RegularFollower;
            ClassStats[2].Attack = bossFightClassStats.RegularAttack;
            ClassStats[2].Defense = bossFightClassStats.RegularDefense;
            ClassStats[2].Evasion = bossFightClassStats.RegularEvasion;
            ClassStats[2].Health = bossFightClassStats.RegularHealth;
            ClassStats[3].ChatterType = ChatterType.Moderator;
            ClassStats[3].Attack = bossFightClassStats.ModeratorAttack;
            ClassStats[3].Defense = bossFightClassStats.ModeratorDefense;
            ClassStats[3].Evasion = bossFightClassStats.ModeratorEvasion;
            ClassStats[3].Health = bossFightClassStats.ModeratorHealth;
            ClassStats[4].ChatterType = ChatterType.Subscriber;
            ClassStats[4].Attack = bossFightClassStats.SubscriberAttack;
            ClassStats[4].Defense = bossFightClassStats.SubscriberDefense;
            ClassStats[4].Evasion = bossFightClassStats.SubscriberEvasion;
            ClassStats[4].Health = bossFightClassStats.SubscriberHealth;

            // boss stats
            Bosses[0].Name = bossFightBossStats.Name1;
            Bosses[0].MaxUsers = bossFightBossStats.MaxUsers1;
            Bosses[0].Attack = bossFightBossStats.Attack1;
            Bosses[0].Defense = bossFightBossStats.Defense1;
            Bosses[0].Evasion = bossFightBossStats.Evasion1;
            Bosses[0].Health = bossFightBossStats.Health1;
            Bosses[0].TurnLimit = bossFightBossStats.TurnLimit1;
            Bosses[0].Loot = bossFightBossStats.Loot1;
            Bosses[0].LastAttackBonus = bossFightBossStats.LastAttackBonus1;
            Bosses[1].Name = bossFightBossStats.Name2;
            Bosses[1].MaxUsers = bossFightBossStats.MaxUsers2;
            Bosses[1].Attack = bossFightBossStats.Attack2;
            Bosses[1].Defense = bossFightBossStats.Defense2;
            Bosses[1].Evasion = bossFightBossStats.Evasion2;
            Bosses[1].Health = bossFightBossStats.Health2;
            Bosses[1].TurnLimit = bossFightBossStats.TurnLimit2;
            Bosses[1].Loot = bossFightBossStats.Loot2;
            Bosses[1].LastAttackBonus = bossFightBossStats.LastAttackBonus2;
            Bosses[2].Name = bossFightBossStats.Name3;
            Bosses[2].MaxUsers = bossFightBossStats.MaxUsers3;
            Bosses[2].Attack = bossFightBossStats.Attack3;
            Bosses[2].Defense = bossFightBossStats.Defense3;
            Bosses[2].Evasion = bossFightBossStats.Evasion3;
            Bosses[2].Health = bossFightBossStats.Health3;
            Bosses[2].TurnLimit = bossFightBossStats.TurnLimit3;
            Bosses[2].Loot = bossFightBossStats.Loot3;
            Bosses[2].LastAttackBonus = bossFightBossStats.LastAttackBonus3;
            Bosses[3].Name = bossFightBossStats.Name4;
            Bosses[3].MaxUsers = bossFightBossStats.MaxUsers4;
            Bosses[3].Attack = bossFightBossStats.Attack4;
            Bosses[3].Defense = bossFightBossStats.Defense4;
            Bosses[3].Evasion = bossFightBossStats.Evasion4;
            Bosses[3].Health = bossFightBossStats.Health4;
            Bosses[3].TurnLimit = bossFightBossStats.TurnLimit4;
            Bosses[3].Loot = bossFightBossStats.Loot4;
            Bosses[3].LastAttackBonus = bossFightBossStats.LastAttackBonus4;
            Bosses[4].Name = bossFightBossStats.Name5;
            Bosses[4].MaxUsers = bossFightBossStats.MaxUsers5;
            Bosses[4].Attack = bossFightBossStats.Attack5;
            Bosses[4].Defense = bossFightBossStats.Defense5;
            Bosses[4].Evasion = bossFightBossStats.Evasion5;
            Bosses[4].Health = bossFightBossStats.Health5;
            Bosses[4].TurnLimit = bossFightBossStats.TurnLimit5;
            Bosses[4].Loot = bossFightBossStats.Loot5;
            Bosses[4].LastAttackBonus = bossFightBossStats.LastAttackBonus5;
        }
    }

    public class Boss
    {
        public string Name { get; set; }
        public int MaxUsers { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int Evasion { get; set; }
        public int Health { get; set; }
        public int TurnLimit { get; set; }
        public int Loot { get; set; }
        public int LastAttackBonus { get; set; }
    }

    public class BossFighter
    {
        public string Username { get; set; }
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
