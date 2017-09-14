using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace TwitchBot.Models
{
    public class BankHeistSettings
    {
        /* Settings */
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
        public string SingleUserSuccess { get; set; }
        public string SingleUserFail { get; set; }
        public string Success100 { get; set; }
        public string Success34 { get; set; }
        public string Success1 { get; set; }
        public string Success0 { get; set; }
        public string Results { get; set; }

        // Game Levels (Level 1-5)
        public BankHeistLevel[] Levels { get; set; }

        // Payouts
        public BankHeistPayout[] Payouts { get; set; }

        /* Singleton Instance */
        private static volatile BankHeistSettings _instance;
        private static object _syncRoot = new Object();

        private BankHeistSettings() { }

        public static BankHeistSettings Instance
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
                            _instance = new BankHeistSettings();
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
            return CooldownTimePeriod < DateTime.Now ? true : false;
        }

        /// <summary>
        /// Load all of the settings from the database for the bank heist mini-game
        /// </summary>
        /// <param name="broadcasterId"></param>
        /// <param name="connStr"></param>
        public void LoadSettings(int broadcasterId, string connStr)
        {
            // refresh arrays
            NextLevelMessages = new string[4];
            Levels = new BankHeistLevel[5];
            Payouts = new BankHeistPayout[5];

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblBankHeistSettings WHERE broadcaster = @broadcaster", conn))
                {
                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                // entry messages
                                CooldownTimePeriodMinutes = int.Parse(reader["cooldownTimePeriodMin"].ToString());
                                EntryPeriodSeconds = int.Parse(reader["entryPeriodSec"].ToString());
                                EntryMessage = reader["entryMessage"].ToString();
                                MaxGamble = int.Parse(reader["maxGamble"].ToString());
                                MaxGambleText = reader["maxGambleText"].ToString();
                                EntryInstructions = reader["entryInstructions"].ToString();
                                CooldownEntry = reader["cooldownEntry"].ToString();
                                CooldownOver = reader["cooldownOver"].ToString();
                                // next level messages
                                NextLevelMessages[0] = reader["nextLevelMessage2"].ToString();
                                NextLevelMessages[1] = reader["nextLevelMessage3"].ToString();
                                NextLevelMessages[2] = reader["nextLevelMessage4"].ToString();
                                NextLevelMessages[3] = reader["nextLevelMessage5"].ToString();
                                // game outcomes
                                GameStart = reader["gameStart"].ToString();
                                SingleUserSuccess = reader["singleUserSuccess"].ToString();
                                SingleUserSuccess = reader["singleUserFail"].ToString();
                                Success100 = reader["success100"].ToString();
                                Success34 = reader["success34"].ToString();
                                Success1 = reader["success1"].ToString();
                                Success0 = reader["success0"].ToString();
                                Results = reader["results"].ToString();
                                // game levels
                                Levels[0].LevelBankName = reader["levelName1"].ToString();
                                Levels[0].MaxUsers = int.Parse(reader["levelMaxUsers1"].ToString());
                                Levels[1].LevelBankName = reader["levelName2"].ToString();
                                Levels[1].MaxUsers = int.Parse(reader["levelMaxUsers2"].ToString());
                                Levels[2].LevelBankName = reader["levelName3"].ToString();
                                Levels[2].MaxUsers = int.Parse(reader["levelMaxUsers3"].ToString());
                                Levels[3].LevelBankName = reader["levelName4"].ToString();
                                Levels[3].MaxUsers = int.Parse(reader["levelMaxUsers4"].ToString());
                                Levels[4].LevelBankName = reader["levelName5"].ToString();
                                Levels[4].MaxUsers = int.Parse(reader["levelMaxUsers5"].ToString());
                                // payout
                                Payouts[0].SuccessRate = decimal.Parse(reader["payoutSuccessRate1"].ToString());
                                Payouts[0].WinMultiplier = decimal.Parse(reader["payoutMultiplier1"].ToString());
                                Payouts[1].SuccessRate = decimal.Parse(reader["payoutSuccessRate2"].ToString());
                                Payouts[1].WinMultiplier = decimal.Parse(reader["payoutMultiplier2"].ToString());
                                Payouts[2].SuccessRate = decimal.Parse(reader["payoutSuccessRate3"].ToString());
                                Payouts[2].WinMultiplier = decimal.Parse(reader["payoutMultiplier3"].ToString());
                                Payouts[3].SuccessRate = decimal.Parse(reader["payoutSuccessRate4"].ToString());
                                Payouts[3].WinMultiplier = decimal.Parse(reader["payoutMultiplier4"].ToString());
                                Payouts[4].SuccessRate = decimal.Parse(reader["payoutSuccessRate5"].ToString());
                                Payouts[4].WinMultiplier = decimal.Parse(reader["payoutMultiplier5"].ToString());

                                break;
                            }
                        }
                    }
                }
            }
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
}
