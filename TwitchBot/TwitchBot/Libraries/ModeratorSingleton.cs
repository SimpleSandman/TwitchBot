using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace TwitchBot.Libraries
{
    public sealed class ModeratorSingleton
    {
        private static volatile ModeratorSingleton _instance;
        private static object _syncRoot = new Object();

        private List<string> _listMods = new List<string>();

        public List<string> ListMods
        {
            get { return _listMods; }
        }

        private ModeratorSingleton() { }

        public static ModeratorSingleton Instance
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
                            _instance = new ModeratorSingleton();
                    }
                }

                return _instance;
            }
        }

        public void SetModeratorList(string connStr, int broadcasterId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM Moderators WHERE Broadcaster = @broadcaster", conn))
                    {
                        cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    _listMods.Add(reader["Username"].ToString());
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void AddNewModToList(string recipient, int broadcasterId, string connStr)
        {
            try
            {
                string query = "INSERT INTO Moderators (Username, Broadcaster) VALUES (@username, @broadcaster)";

                // Create connection and command
                using (SqlConnection conn = new SqlConnection(connStr))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = recipient;
                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                _listMods.Add(recipient);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void DeleteOldModFromList(string recipient, int broadcasterId, string connStr)
        {
            try
            {
                string query = "DELETE FROM Moderators WHERE Username = @username AND Broadcaster = @broadcaster";

                // Create connection and command
                using (SqlConnection conn = new SqlConnection(connStr))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = recipient;
                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                _listMods.Remove(recipient);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
