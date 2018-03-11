using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using TwitchBot.Models;

namespace TwitchBot.Repositories
{
    public class SongRequestBlacklistRepository
    {
        private string _connStr;

        public SongRequestBlacklistRepository(string connStr)
        {
            _connStr = connStr;
        }

        public List<SongRequestBlacklistItem> GetSongRequestBlackList(int broadcasterId)
        {
            List<SongRequestBlacklistItem> blacklist = new List<SongRequestBlacklistItem>();

            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT Title, Artist FROM SongRequestBlacklist WHERE Broadcaster = @broadcaster", conn))
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
                                    Artist = reader["Artist"].ToString(),
                                    Title = reader["Title"].ToString()
                                });
                            }
                        }
                    }
                }
            }

            return blacklist;
        }

        public int AddArtistToBlacklist(string artist, int broadcasterId)
        {
            string query = "INSERT INTO SongRequestBlacklist (Artist, Broadcaster) VALUES (@artist, @broadcaster)";

            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add("@artist", SqlDbType.VarChar, 100).Value = artist;
                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;

                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public int AddSongToBlacklist(string title, string artist, int broadcasterId)
        {
            string query = "INSERT INTO SongRequestBlacklist (Title, Artist, Broadcaster) VALUES (@title, @artist, @broadcaster)";

            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add("@title", SqlDbType.VarChar, 100).Value = title;
                cmd.Parameters.Add("@artist", SqlDbType.VarChar, 100).Value = artist;
                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;

                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public int DeleteArtistFromBlacklist(string artist, int broadcasterId)
        {
            string query = "DELETE FROM SongRequestBlacklist WHERE Artist = @artist AND Broadcaster = @broadcaster";

            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add("@artist", SqlDbType.VarChar, 100).Value = artist;
                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;

                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public int DeleteSongFromBlacklist(string title, string artist, int broadcasterId)
        {
            string query = "DELETE FROM SongRequestBlacklist "
                        + "WHERE Title = @title AND Artist = @artist AND Broadcaster = @broadcaster";

            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add("@title", SqlDbType.VarChar, 100).Value = title;
                cmd.Parameters.Add("@artist", SqlDbType.VarChar, 100).Value = artist;
                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;

                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public int ResetBlacklist(int broadcasterId)
        {
            string query = "DELETE FROM SongRequestBlacklist WHERE Broadcaster = @broadcaster";

            // Create connection and command
            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;

                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }
    }
}
