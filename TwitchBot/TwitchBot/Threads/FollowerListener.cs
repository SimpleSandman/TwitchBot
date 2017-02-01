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

namespace TwitchBot.Threads
{
    public class FollowerListener
    {
        private IrcClient _irc;
        private TwitchBotConfigurationSection _botConfig;
        private string _connStr;
        private int _intBroadcasterID;
        private Thread _followerListener;

        // Empty constructor makes instance of Thread
        public FollowerListener(IrcClient irc, TwitchBotConfigurationSection botConfig, string connString, int broadcasterId)
        {
            _irc = irc;
            _botConfig = botConfig;
            _connStr = connString;
            _intBroadcasterID = broadcasterId;
            _followerListener = new Thread(new ThreadStart(this.Run));
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
                    ChatterInfoJSON chatterInfo = TaskJSON.GetChatters(_botConfig.Broadcaster, _botConfig.TwitchClientId).Result;

                    if (chatterInfo.chatter_count > 0)
                    {
                        Chatters chatters = chatterInfo.chatters; // get list of chatters

                        // Make list of available chatters by chatter type
                        List<List<string>> lstAvailChatterType = new List<List<string>>();
                        if (chatters.viewers.Count() > 0)
                            lstAvailChatterType.Add(chatters.viewers);
                        if (chatters.moderators.Count() > 0)
                            lstAvailChatterType.Add(chatters.moderators);
                        if (chatters.global_mods.Count() > 0)
                            lstAvailChatterType.Add(chatters.global_mods);
                        if (chatters.admins.Count() > 0)
                            lstAvailChatterType.Add(chatters.admins);
                        if (chatters.staff.Count() > 0)
                            lstAvailChatterType.Add(chatters.staff);

                        // Check for existing or new followers
                        for (int i = 0; i < lstAvailChatterType.Count(); i++)
                        {
                            foreach (string chatter in lstAvailChatterType[i])
                            {
                                using (HttpResponseMessage message = checkFollowerStatus(chatter))
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
                }

                Thread.Sleep(300000); // 5 minutes
            }
        }

        /// <summary>
        /// Check if viewer is a follower via HttpResponseMessage
        /// </summary>
        /// <param name="strUserName"></param>
        /// <returns></returns>
        public HttpResponseMessage checkFollowerStatus(string strUserName)
        {
            return TaskJSON.GetFollowerStatus(_botConfig.Broadcaster.ToLower(), _botConfig.TwitchClientId, strUserName).Result;
        }
    }
}
