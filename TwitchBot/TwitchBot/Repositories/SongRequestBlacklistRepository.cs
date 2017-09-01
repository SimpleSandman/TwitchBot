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

        public int AddArtistToBlacklist(string artist, int broadcasterId)
        {
            string query = "INSERT INTO tblSongRequestBlacklist (artist, broadcaster) VALUES (@artist, @broadcaster)";

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
            string query = "INSERT INTO tblSongRequestBlacklist (title, artist, broadcaster) VALUES (@title, @artist, @broadcaster)";

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
            string query = "DELETE FROM tblSongRequestBlacklist WHERE artist = @artist AND broadcaster = @broadcaster";

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
            string query = "DELETE FROM tblSongRequestBlacklist "
                        + "WHERE title = @title AND artist = @artist AND broadcaster = @broadcaster";

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
            string query = "DELETE FROM tblSongRequestBlacklist WHERE broadcaster = @broadcaster";

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
