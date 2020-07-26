using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using TwitchBotDb;
using TwitchBotDb.Models;

namespace TwitchBotShared.ClientLibraries.Singletons
{
    public class BankHeistSingleton
    {
        public BlockingCollection<BankRobber> Robbers { get; set; }

        /* Settings */
        public int Id { get; set; }
        public int CooldownTimePeriodMinutes { get; set; }
        public int EntryPeriodSeconds { get; set; }
        public DateTime CooldownTimePeriod { get; set; }
        public DateTime EntryPeriod { get; set; }

        // Entry Messages
        public string EntryMessage { get; set; }
        public int MaxGamble { get; set; }
        public string MaxGambleText { get; set; }
        public string EntryInstructions { get; set; }
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

        // Game Levels (Level 1-5)
        public BankHeistLevel[] Levels { get; set; }

        // Payouts
        public BankHeistPayout[] Payouts { get; set; }

        /* Singleton Instance */
        private static volatile BankHeistSingleton _instance;
        private static object _syncRoot = new Object();

        private BankHeistSingleton() { }

        public static BankHeistSingleton Instance
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
                            _instance = new BankHeistSingleton();
                    }
                }

                return _instance;
            }
        }

        public bool IsEntryPeriodOver()
        {
            return EntryPeriod < DateTime.Now ? true : false;
        }

        public bool IsHeistOnCooldown()
        {
            return CooldownTimePeriod > DateTime.Now ? true : false;
        }

        /// <summary>
        /// Load all of the settings from the database for the bank heist mini-game
        /// </summary>
        /// <param name="broadcasterId"></param>
        public async Task LoadSettings(int broadcasterId, string twitchBotApiLink)
        {
            BankHeistSetting bankHeistSetting = await ApiBotRequest.GetExecuteAsync<BankHeistSetting>(twitchBotApiLink + $"bankheistsettings/get/{broadcasterId}");

            if (bankHeistSetting == null)
            {
                bankHeistSetting = new BankHeistSetting { BroadcasterId = broadcasterId };
                bankHeistSetting = await ApiBotRequest.PostExecuteAsync(twitchBotApiLink + $"bankheistsettings/create", bankHeistSetting);
            }

            if (bankHeistSetting == null)
                throw new Exception("Unable to create initial boss fight settings");

            // refresh arrays and lists
            NextLevelMessages = new string[4];
            Levels = new BankHeistLevel[] 
            {
                new BankHeistLevel { },
                new BankHeistLevel { },
                new BankHeistLevel { },
                new BankHeistLevel { },
                new BankHeistLevel { }
            };
            Payouts = new BankHeistPayout[] 
            {
                new BankHeistPayout{ },
                new BankHeistPayout{ },
                new BankHeistPayout{ },
                new BankHeistPayout{ },
                new BankHeistPayout{ }
            };
            Robbers = new BlockingCollection<BankRobber>();

            // settings
            Id = bankHeistSetting.Id;
            CooldownTimePeriodMinutes = bankHeistSetting.CooldownPeriodMin;
            EntryPeriodSeconds = bankHeistSetting.EntryPeriodSec;
            EntryMessage = bankHeistSetting.EntryMessage;
            MaxGamble = bankHeistSetting.MaxGamble;
            MaxGambleText = bankHeistSetting.MaxGambleText;
            EntryInstructions = bankHeistSetting.EntryInstructions;
            CooldownEntry = bankHeistSetting.CooldownEntry;
            CooldownOver = bankHeistSetting.CooldownOver;

            // next level messages
            NextLevelMessages[0] = bankHeistSetting.NextLevelMessage2;
            NextLevelMessages[1] = bankHeistSetting.NextLevelMessage3;
            NextLevelMessages[2] = bankHeistSetting.NextLevelMessage4;
            NextLevelMessages[3] = bankHeistSetting.NextLevelMessage5;
            
            // game outcomes
            GameStart = bankHeistSetting.GameStart;
            ResultsMessage = bankHeistSetting.ResultsMessage;
            SingleUserSuccess = bankHeistSetting.SingleUserSuccess;
            SingleUserFail = bankHeistSetting.SingleUserFail;
            Success100 = bankHeistSetting.Success100;
            Success34 = bankHeistSetting.Success34;
            Success1 = bankHeistSetting.Success1;
            Success0 = bankHeistSetting.Success0;
            
            // game levels
            Levels[0].LevelBankName = bankHeistSetting.LevelName1;
            Levels[0].MaxUsers = bankHeistSetting.LevelMaxUsers1;
            Levels[1].LevelBankName = bankHeistSetting.LevelName2;
            Levels[1].MaxUsers = bankHeistSetting.LevelMaxUsers2;
            Levels[2].LevelBankName = bankHeistSetting.LevelName3;
            Levels[2].MaxUsers = bankHeistSetting.LevelMaxUsers3;
            Levels[3].LevelBankName = bankHeistSetting.LevelName4;
            Levels[3].MaxUsers = bankHeistSetting.LevelMaxUsers4;
            Levels[4].LevelBankName = bankHeistSetting.LevelName5;
            Levels[4].MaxUsers = bankHeistSetting.LevelMaxUsers5;
            
            // payout
            Payouts[0].SuccessRate = bankHeistSetting.PayoutSuccessRate1;
            Payouts[0].WinMultiplier = bankHeistSetting.PayoutMultiplier1;
            Payouts[1].SuccessRate = bankHeistSetting.PayoutSuccessRate2;
            Payouts[1].WinMultiplier = bankHeistSetting.PayoutMultiplier2;
            Payouts[2].SuccessRate = bankHeistSetting.PayoutSuccessRate3;
            Payouts[2].WinMultiplier = bankHeistSetting.PayoutMultiplier3;
            Payouts[3].SuccessRate = bankHeistSetting.PayoutSuccessRate4;
            Payouts[3].WinMultiplier = bankHeistSetting.PayoutMultiplier4;
            Payouts[4].SuccessRate = bankHeistSetting.PayoutSuccessRate5;
            Payouts[4].WinMultiplier = bankHeistSetting.PayoutMultiplier5;
        }
    }

    public class BankHeistLevel
    {
        public string LevelBankName { get; set; }
        public int MaxUsers { get; set; }
    }

    public class BankHeistPayout
    {
        public decimal SuccessRate { get; set; }
        public decimal WinMultiplier { get; set; }
    }

    public class BankRobber
    {
        public string Username { get; set; }
        public int Gamble { get; set; }
    }
}
