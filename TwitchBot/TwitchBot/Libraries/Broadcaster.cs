using System;
using System.Data;
using System.Data.SqlClient;

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

        public bool FindBroadcasterByUserInfo(string username, string twitchId, string connStr)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM Broadcasters WHERE Username = @username AND TwitchId = @twitchId", conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@twitchId", twitchId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (username.Equals(reader["Username"].ToString().ToLower()) 
                                    && twitchId.Equals(reader["TwitchId"].ToString().ToLower()))
                                {
                                    Username = username;
                                    TwitchId = twitchId;
                                    DatabaseId = int.Parse(reader["Id"].ToString());

                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        public bool FindBroadcasterByTwitchId(string twitchId, string connStr)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM Broadcasters WHERE TwitchId = @twitchId", conn))
                {
                    cmd.Parameters.AddWithValue("@twitchId", twitchId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (twitchId.Equals(reader["TwitchId"].ToString().ToLower()))
                                {
                                    TwitchId = twitchId;
                                    DatabaseId = int.Parse(reader["Id"].ToString());
                                    Username = reader["Username"].ToString();

                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        public void AddBroadcaster(string connStr)
        {
            string query = "INSERT INTO Broadcasters (Username, TwitchId) VALUES (@username, @twitchId)";

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add("@twitchId", SqlDbType.Int).Value = TwitchId;
                cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = Username;

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateBroadcasterUsername(string connStr)
        {
            string query = "UPDATE Broadcasters SET Username = @username WHERE TwitchId = @twitchId";

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
