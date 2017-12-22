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
        public int Cost { get; set; }

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

        // Game Levels (Level 1-5)
        public Boss[] Bosses { get; set; }

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
                                // entry messages and initial settings
                                CooldownTimePeriodMinutes = int.Parse(reader["cooldownPeriodMin"].ToString());
                                EntryPeriodSeconds = int.Parse(reader["entryPeriodSec"].ToString());
                                EntryMessage = reader["entryMessage"].ToString();
                                Cost = int.Parse(reader["cost"].ToString());
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

                                break;
                            }
                        }
                    }
                }

                // ToDo: Select where FK equals BossFightSettings Id instead of broadcaster
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM BossFightClassStats WHERE broadcaster = @broadcaster", conn))
                {
                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                // fighter class stats
                                ClassStats[0].ChatterType = Enums.ChatterType.Viewer;
                                ClassStats[0].Attack = int.Parse(reader["viewerAttack"].ToString());
                                ClassStats[0].Defense = int.Parse(reader["viewerDefense"].ToString());
                                ClassStats[0].Evasion = int.Parse(reader["viewerEvasion"].ToString());
                                ClassStats[0].Health = int.Parse(reader["viewerHealth"].ToString());
                                ClassStats[1].ChatterType = Enums.ChatterType.Follower;
                                ClassStats[1].Attack = int.Parse(reader["followerAttack"].ToString());
                                ClassStats[1].Defense = int.Parse(reader["followerDefense"].ToString());
                                ClassStats[1].Evasion = int.Parse(reader["followerEvasion"].ToString());
                                ClassStats[1].Health = int.Parse(reader["followerHealth"].ToString());
                                ClassStats[2].ChatterType = Enums.ChatterType.Regular;
                                ClassStats[2].Attack = int.Parse(reader["regularAttack"].ToString());
                                ClassStats[2].Defense = int.Parse(reader["regularDefense"].ToString());
                                ClassStats[2].Evasion = int.Parse(reader["regularEvasion"].ToString());
                                ClassStats[2].Health = int.Parse(reader["regularHealth"].ToString());
                                ClassStats[3].ChatterType = Enums.ChatterType.Moderator;
                                ClassStats[3].Attack = int.Parse(reader["moderatorAttack"].ToString());
                                ClassStats[3].Defense = int.Parse(reader["moderatorDefense"].ToString());
                                ClassStats[3].Evasion = int.Parse(reader["moderatorEvasion"].ToString());
                                ClassStats[3].Health = int.Parse(reader["moderatorHealth"].ToString());
                                ClassStats[4].ChatterType = Enums.ChatterType.Subscriber;
                                ClassStats[4].Attack = int.Parse(reader["subscriberAttack"].ToString());
                                ClassStats[4].Defense = int.Parse(reader["subscriberDefense"].ToString());
                                ClassStats[4].Evasion = int.Parse(reader["subscriberEvasion"].ToString());
                                ClassStats[4].Health = int.Parse(reader["subscriberHealth"].ToString());

                                break;
                            }
                        }
                    }
                }

                // ToDo: Select where FK equals BossFightSettings Id instead of broadcaster
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM BossFightBossStats WHERE broadcaster = @broadcaster", conn))
                {
                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                // boss stats
                                Bosses[0].Name = reader["name1"].ToString();
                                Bosses[0].MaxUsers = int.Parse(reader["maxUsers1"].ToString());
                                Bosses[0].Attack = int.Parse(reader["attack1"].ToString());
                                Bosses[0].Defense = int.Parse(reader["defense1"].ToString());
                                Bosses[0].Health = int.Parse(reader["health1"].ToString());
                                Bosses[0].TurnLimit = int.Parse(reader["turnLimit1"].ToString());
                                Bosses[0].Loot = int.Parse(reader["loot1"].ToString());
                                Bosses[0].LastAttackBonus = int.Parse(reader["lastAttackBonus1"].ToString());
                                Bosses[1].Name = reader["name2"].ToString();
                                Bosses[1].MaxUsers = int.Parse(reader["maxUsers2"].ToString());
                                Bosses[1].Attack = int.Parse(reader["attack2"].ToString());
                                Bosses[1].Defense = int.Parse(reader["defense2"].ToString());
                                Bosses[1].Health = int.Parse(reader["health2"].ToString());
                                Bosses[1].TurnLimit = int.Parse(reader["turnLimit2"].ToString());
                                Bosses[1].Loot = int.Parse(reader["loot2"].ToString());
                                Bosses[1].LastAttackBonus = int.Parse(reader["lastAttackBonus2"].ToString());
                                Bosses[2].Name = reader["name3"].ToString();
                                Bosses[2].MaxUsers = int.Parse(reader["maxUsers3"].ToString());
                                Bosses[2].Attack = int.Parse(reader["attack3"].ToString());
                                Bosses[2].Defense = int.Parse(reader["defense3"].ToString());
                                Bosses[2].Health = int.Parse(reader["health3"].ToString());
                                Bosses[2].TurnLimit = int.Parse(reader["turnLimit3"].ToString());
                                Bosses[2].Loot = int.Parse(reader["loot3"].ToString());
                                Bosses[2].LastAttackBonus = int.Parse(reader["lastAttackBonus3"].ToString());
                                Bosses[3].Name = reader["name4"].ToString();
                                Bosses[3].MaxUsers = int.Parse(reader["maxUsers4"].ToString());
                                Bosses[3].Attack = int.Parse(reader["attack4"].ToString());
                                Bosses[3].Defense = int.Parse(reader["defense4"].ToString());
                                Bosses[3].Health = int.Parse(reader["health4"].ToString());
                                Bosses[3].TurnLimit = int.Parse(reader["turnLimit4"].ToString());
                                Bosses[3].Loot = int.Parse(reader["loot4"].ToString());
                                Bosses[3].LastAttackBonus = int.Parse(reader["lastAttackBonus4"].ToString());
                                Bosses[4].Name = reader["name5"].ToString();
                                Bosses[4].MaxUsers = int.Parse(reader["maxUsers5"].ToString());
                                Bosses[4].Attack = int.Parse(reader["attack5"].ToString());
                                Bosses[4].Defense = int.Parse(reader["defense5"].ToString());
                                Bosses[4].Health = int.Parse(reader["health5"].ToString());
                                Bosses[4].TurnLimit = int.Parse(reader["turnLimit5"].ToString());
                                Bosses[4].Loot = int.Parse(reader["loot5"].ToString());
                                Bosses[4].LastAttackBonus = int.Parse(reader["lastAttackBonus5"].ToString());

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

            // ToDo: Get record ID from BossFightSettings and make foreign key for below tables

            query = "INSERT INTO BossFightClassStats (broadcaster) VALUES (@broadcaster)";

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            query = "INSERT INTO BossFightBossStats (broadcaster) VALUES (@broadcaster)";

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }

    public class Boss
    {
        public string Name { get; set; }
        public int MaxUsers { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int Health { get; set; }
        public int TurnLimit { get; set; }
        public int Loot { get; set; }
        public int LastAttackBonus { get; set; }
    }
}
