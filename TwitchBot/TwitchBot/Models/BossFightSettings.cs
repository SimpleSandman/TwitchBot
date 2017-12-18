using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace TwitchBot.Models
{
    public class BossFightSettings
    {
        public BlockingCollection<BossFighter> Fighters { get; set; }

        /* Settings */
        public int CooldownTimePeriodMinutes { get; set; }
        public int EntryPeriodSeconds { get; set; }
        public DateTime CooldownTimePeriod { get; set; }
        public DateTime EntryPeriod { get; set; }

        // Entry Messages
        public string EntryMessage { get; set; }
        public int SetGamble { get; set; }
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

        // Fighter Classes
        public FighterClass[] ClassStats { get; set; }

        // Game Levels (Level 1-5)
        public BossFightLevel[] Levels { get; set; }

        // Payouts
        public BossFightPayout[] Payouts { get; set; }

        /* Singleton Instance */
        private static volatile BossFightSettings _instance;
        private static object _syncRoot = new Object();

        private BossFightSettings() { }

        public static BossFightSettings Instance
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
                            _instance = new BossFightSettings();
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
        /// Load all of the settings from the database for the bank heist mini-game
        /// </summary>
        /// <param name="broadcasterId"></param>
        /// <param name="connStr"></param>
        public void LoadSettings(int broadcasterId, string connStr)
        {
            // refresh arrays and lists
            NextLevelMessages = new string[4];
            Levels = new BossFightLevel[]
            {
                new BossFightLevel { },
                new BossFightLevel { },
                new BossFightLevel { },
                new BossFightLevel { },
                new BossFightLevel { }
            };
            Payouts = new BossFightPayout[]
            {
                new BossFightPayout{ },
                new BossFightPayout{ },
                new BossFightPayout{ },
                new BossFightPayout{ },
                new BossFightPayout{ }
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

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM BossFightSettings WHERE broadcaster = @broadcaster", conn))
                {
                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                // entry messages
                                CooldownTimePeriodMinutes = int.Parse(reader["cooldownPeriodMin"].ToString());
                                EntryPeriodSeconds = int.Parse(reader["entryPeriodSec"].ToString());
                                EntryMessage = reader["entryMessage"].ToString();
                                SetGamble = int.Parse(reader["setGamble"].ToString());
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
                                ResultsMessage = reader["resultsMessage"].ToString();
                                SingleUserSuccess = reader["singleUserSuccess"].ToString();
                                SingleUserFail = reader["singleUserFail"].ToString();
                                Success100 = reader["success100"].ToString();
                                Success34 = reader["success34"].ToString();
                                Success1 = reader["success1"].ToString();
                                Success0 = reader["success0"].ToString();
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
                                // class stats
                                ClassStats[0].ChatterType = Enums.ChatterType.Viewer;
                                ClassStats[0].Attack = int.Parse(reader["classStatsAttack1"].ToString());
                                ClassStats[0].Defense = int.Parse(reader["classStatsDefense1"].ToString());
                                ClassStats[0].Evasion = int.Parse(reader["classStatsEvasion1"].ToString());
                                ClassStats[0].Health = int.Parse(reader["classStatsHealth1"].ToString());
                                ClassStats[1].ChatterType = Enums.ChatterType.Follower;
                                ClassStats[1].Attack = int.Parse(reader["classStatsAttack2"].ToString());
                                ClassStats[1].Defense = int.Parse(reader["classStatsDefense2"].ToString());
                                ClassStats[1].Evasion = int.Parse(reader["classStatsEvasion2"].ToString());
                                ClassStats[1].Health = int.Parse(reader["classStatsHealth2"].ToString());
                                ClassStats[2].ChatterType = Enums.ChatterType.Regular;
                                ClassStats[2].Attack = int.Parse(reader["classStatsAttack3"].ToString());
                                ClassStats[2].Defense = int.Parse(reader["classStatsDefense3"].ToString());
                                ClassStats[2].Evasion = int.Parse(reader["classStatsEvasion3"].ToString());
                                ClassStats[2].Health = int.Parse(reader["classStatsHealth3"].ToString());
                                ClassStats[3].ChatterType = Enums.ChatterType.Moderator;
                                ClassStats[3].Attack = int.Parse(reader["classStatsAttack4"].ToString());
                                ClassStats[3].Defense = int.Parse(reader["classStatsDefense4"].ToString());
                                ClassStats[3].Evasion = int.Parse(reader["classStatsEvasion4"].ToString());
                                ClassStats[3].Health = int.Parse(reader["classStatsHealth4"].ToString());
                                ClassStats[4].ChatterType = Enums.ChatterType.Subscriber;
                                ClassStats[4].Attack = int.Parse(reader["classStatsAttack5"].ToString());
                                ClassStats[4].Defense = int.Parse(reader["classStatsDefense5"].ToString());
                                ClassStats[4].Evasion = int.Parse(reader["classStatsEvasion5"].ToString());
                                ClassStats[4].Health = int.Parse(reader["classStatsHealth5"].ToString());

                                break;
                            }
                        }
                    }
                }
            }
        }

        public void CreateSettings(int broadcasterId, string connStr)
        {
            string query = "INSERT INTO BossFightSettings (broadcaster) VALUES (@broadcaster)";

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }

    public class BossFightLevel
    {
        public string LevelBankName { get; set; }
        public int MaxUsers { get; set; }
    }

    public class BossFightPayout
    {
        public decimal SuccessRate { get; set; }
        public decimal WinMultiplier { get; set; }
    }
}
