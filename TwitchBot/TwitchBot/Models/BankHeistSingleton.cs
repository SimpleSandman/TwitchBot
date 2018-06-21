using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TwitchBot.Libraries;

using TwitchBotDb.Models;

namespace TwitchBot.Models
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
        public async Task LoadSettings(int broadcasterId, string twitchBotApiLink, BankHeistSettings bankHeistSettings = null)
        {
            if (bankHeistSettings == null)
            {
                bankHeistSettings =
                    await ApiBotRequest.GetExecuteTaskAsync<BankHeistSettings>(twitchBotApiLink + $"bankheistsettings/get/{broadcasterId}");

                if (bankHeistSettings == null) return; // check if settings were loaded successfully, else attempt to create new settings
            }

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
            Id = bankHeistSettings.Id;
            CooldownTimePeriodMinutes = bankHeistSettings.CooldownPeriodMin;
            EntryPeriodSeconds = bankHeistSettings.EntryPeriodSec;
            EntryMessage = bankHeistSettings.EntryMessage;
            MaxGamble = bankHeistSettings.MaxGamble;
            MaxGambleText = bankHeistSettings.MaxGambleText;
            EntryInstructions = bankHeistSettings.EntryInstructions;
            CooldownEntry = bankHeistSettings.CooldownEntry;
            CooldownOver = bankHeistSettings.CooldownOver;

            // next level messages
            NextLevelMessages[0] = bankHeistSettings.NextLevelMessage2;
            NextLevelMessages[1] = bankHeistSettings.NextLevelMessage3;
            NextLevelMessages[2] = bankHeistSettings.NextLevelMessage4;
            NextLevelMessages[3] = bankHeistSettings.NextLevelMessage5;
            
            // game outcomes
            GameStart = bankHeistSettings.GameStart;
            ResultsMessage = bankHeistSettings.ResultsMessage;
            SingleUserSuccess = bankHeistSettings.SingleUserSuccess;
            SingleUserFail = bankHeistSettings.SingleUserFail;
            Success100 = bankHeistSettings.Success100;
            Success34 = bankHeistSettings.Success34;
            Success1 = bankHeistSettings.Success1;
            Success0 = bankHeistSettings.Success0;
            
            // game levels
            Levels[0].LevelBankName = bankHeistSettings.LevelName1;
            Levels[0].MaxUsers = bankHeistSettings.LevelMaxUsers1;
            Levels[1].LevelBankName = bankHeistSettings.LevelName2;
            Levels[1].MaxUsers = bankHeistSettings.LevelMaxUsers2;
            Levels[2].LevelBankName = bankHeistSettings.LevelName3;
            Levels[2].MaxUsers = bankHeistSettings.LevelMaxUsers3;
            Levels[3].LevelBankName = bankHeistSettings.LevelName4;
            Levels[3].MaxUsers = bankHeistSettings.LevelMaxUsers4;
            Levels[4].LevelBankName = bankHeistSettings.LevelName5;
            Levels[4].MaxUsers = bankHeistSettings.LevelMaxUsers5;
            
            // payout
            Payouts[0].SuccessRate = bankHeistSettings.PayoutSuccessRate1;
            Payouts[0].WinMultiplier = bankHeistSettings.PayoutMultiplier1;
            Payouts[1].SuccessRate = bankHeistSettings.PayoutSuccessRate2;
            Payouts[1].WinMultiplier = bankHeistSettings.PayoutMultiplier2;
            Payouts[2].SuccessRate = bankHeistSettings.PayoutSuccessRate3;
            Payouts[2].WinMultiplier = bankHeistSettings.PayoutMultiplier3;
            Payouts[3].SuccessRate = bankHeistSettings.PayoutSuccessRate4;
            Payouts[3].WinMultiplier = bankHeistSettings.PayoutMultiplier4;
            Payouts[4].SuccessRate = bankHeistSettings.PayoutSuccessRate5;
            Payouts[4].WinMultiplier = bankHeistSettings.PayoutMultiplier5;
        }

        public async Task CreateSettings(int broadcasterId, string twitchBotApiLink)
        {
            BankHeistSettings freshSettings = new BankHeistSettings { Broadcaster = broadcasterId };

            await LoadSettings(broadcasterId, twitchBotApiLink, await ApiBotRequest.PostExecuteTaskAsync(twitchBotApiLink + $"bankheistsettings/create", freshSettings));
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
