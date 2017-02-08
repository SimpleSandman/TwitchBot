using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using TwitchBot.Configuration;
using TwitchBot.Libraries;
using TwitchBot.Models.JSON;
using TwitchBot.Services;

namespace TwitchBot.Threads
{
    public class FollowerListener
    {
        private IrcClient _irc;
        private TwitchBotConfigurationSection _botConfig;
        private string _connStr;
        private int _intBroadcasterID;
        private Thread _followerListener;
        private TwitchInfoService _twitchInfo;

        // Empty constructor makes instance of Thread
        public FollowerListener(IrcClient irc, TwitchBotConfigurationSection botConfig, string connString, int broadcasterId, TwitchInfoService twitchInfo)
        {
            _irc = irc;
            _botConfig = botConfig;
            _connStr = connString;
            _intBroadcasterID = broadcasterId;
            _followerListener = new Thread(new ThreadStart(this.Run));
            _twitchInfo = twitchInfo;
        }

        // Starts the thread
        public void Start()
        {
            _followerListener.IsBackground = true;
            _followerListener.Start();
        }

        /// <summary>
        /// Check if follower is watching. If so, give following viewer experience every iteration
        /// </summary>
        public void Run()
        {
            while (true)
            {
                try
                {
                    // Grab user's chatter info (viewers, mods, etc.)
                    List<List<string>> lstAvailChatterType = _twitchInfo.GetChatterListByType().Result;
                    if (lstAvailChatterType.Count > 0)
                    {
                        // Check for existing or new followers
                        for (int i = 0; i < lstAvailChatterType.Count(); i++)
                        {
                            foreach (string chatter in lstAvailChatterType[i])
                            {
                                using (HttpResponseMessage message = _twitchInfo.CheckFollowerStatus(chatter).Result)
                                {
                                    // check if chatter is a follower
                                    if (message.IsSuccessStatusCode)
                                    {
                                        int currExp = -1;

                                        // check if chatter has experience
                                        using (SqlConnection conn = new SqlConnection(_connStr))
                                        {
                                            conn.Open();
                                            using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblRankFollowers WHERE broadcaster = @broadcaster", conn))
                                            {
                                                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;
                                                using (SqlDataReader reader = cmd.ExecuteReader())
                                                {
                                                    if (reader.HasRows)
                                                    {
                                                        while (reader.Read())
                                                        {
                                                            if (chatter.Equals(reader["username"].ToString()))
                                                            {
                                                                currExp = int.Parse(reader["exp"].ToString());
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        if (currExp > -1)
                                        {
                                            // Give follower experience for watching
                                            string query = "UPDATE tblRankFollowers SET exp = @exp WHERE (username = @username AND broadcaster = @broadcaster)";

                                            using (SqlConnection conn = new SqlConnection(_connStr))
                                            using (SqlCommand cmd = new SqlCommand(query, conn))
                                            {
                                                cmd.Parameters.Add("@exp", SqlDbType.Int).Value = ++currExp; // add 1 experience every iteration
                                                cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = chatter;
                                                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;

                                                conn.Open();
                                                cmd.ExecuteNonQuery();
                                            }
                                        }
                                        else
                                        {
                                            // Add new follower to the ranks
                                            string query = "INSERT INTO tblRankFollowers (username, exp, broadcaster) "
                                                + "VALUES (@username, @exp, @broadcaster)";

                                            using (SqlConnection conn = new SqlConnection(_connStr))
                                            using (SqlCommand cmd = new SqlCommand(query, conn))
                                            {
                                                cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = chatter;
                                                cmd.Parameters.Add("@exp", SqlDbType.Int).Value = 0; // initial experience
                                                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;

                                                conn.Open();
                                                cmd.ExecuteNonQuery();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error inside FollowerListener Run(): " + ex.Message);
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    }
                }

                Thread.Sleep(300000); // 5 minutes
            }
        }
    }
}
