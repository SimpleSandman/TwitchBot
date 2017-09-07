using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot.Libraries
{
    public class Broadcaster
    {
        public string Username { get; set; }
        public int DatabaseId { get; set; }
        public string TwitchId { get; set; }

        private static volatile Broadcaster _instance;
        private static object _syncRoot = new Object();

        private Broadcaster() { }

        public static Broadcaster Instance
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
                            _instance = new Broadcaster();
                    }
                }

                return _instance;
            }
        }

        public void FindBroadcaster(string username, string connStr)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblBroadcasters WHERE username = @username", conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (username.Equals(reader["username"].ToString().ToLower()))
                                {
                                    Username = username;
                                    DatabaseId = int.Parse(reader["id"].ToString());
                                    TwitchId = reader["twitchId"].ToString();

                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void AddBroadcaster(string connStr)
        {
            string query = "INSERT INTO tblBroadcasters (username, twitchId) VALUES (@username, @twitchId)";

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add("@twitchId", SqlDbType.Int).Value = TwitchId;
                cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = Username;

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateTwitchId(string connStr)
        {
            string query = "UPDATE tblBroadcasters SET twitchId = @twitchId WHERE username = @username";

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add("@twitchId", SqlDbType.Int).Value = TwitchId;
                cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = Username;

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
