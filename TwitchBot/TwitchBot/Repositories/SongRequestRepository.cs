using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using TwitchBot.Models;

namespace TwitchBot.Repositories
{
    public class SongRequestRepository
    {
        private string _connStr;

        public SongRequestRepository(string connStr)
        {
            _connStr = connStr;
        }

        public List<SongRequestBlacklistItem> GetSongRequestBlackList(int broadcasterId)
        {
            List<SongRequestBlacklistItem> blacklist = new List<SongRequestBlacklistItem>();

            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT title, artist FROM tblSongRequestBlacklist WHERE broadcaster = @broadcaster", conn))
                {
                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                blacklist.Add(new SongRequestBlacklistItem
                                {
                                    Artist = reader["artist"].ToString(),
                                    Title = reader["title"].ToString()
                                });
                            }
                        }
                    }
                }
            }

            return blacklist;
        }
    }
}
