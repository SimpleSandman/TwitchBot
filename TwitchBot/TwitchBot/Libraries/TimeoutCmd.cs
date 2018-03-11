using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

using TwitchBot.Models;

namespace TwitchBot.Libraries
{
    public class TimeoutCmd
    {
        private List<TimeoutUser> _timedoutUsers = new List<TimeoutUser>();

        public List<TimeoutUser> TimedoutUsers
        {
            get { return _timedoutUsers; }
            set { _timedoutUsers = value; }
        }

        public void AddTimeoutToList(string recipient, int broadcasterId, double seconds, string connStr)
        {
            try
            {
                string query = "INSERT INTO UserBotTimeout (Username, Broadcaster, Timeout) VALUES (@username, @broadcaster, @timeout)";
                DateTime timeoutDuration = new DateTime();
                timeoutDuration = DateTime.UtcNow.AddSeconds(seconds);

                // Create connection and command
                using (SqlConnection conn = new SqlConnection(connStr))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = recipient;
                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;
                    cmd.Parameters.Add("@timeout", SqlDbType.DateTime).Value = timeoutDuration;

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                _timedoutUsers.Add(new TimeoutUser
                {
                    Username = recipient,
                    TimeoutExpiration = timeoutDuration,
                    HasBeenWarned = false
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void DeleteTimeoutFromList(string recipient, int broadcasterId, string connStr)
        {
            try
            {
                string query = "DELETE FROM UserBotTimeout WHERE Username = @username AND Broadcaster = @broadcaster";

                // Create connection and command
                using (SqlConnection conn = new SqlConnection(connStr))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = recipient;
                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                _timedoutUsers.RemoveAll(r => r.Username == recipient);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public string GetTimeoutFromUser(string recipient, int broadcasterId, string connStr)
        {
            try
            {
                TimeoutUser timeoutUser = _timedoutUsers.FirstOrDefault(r => r.Username == recipient);
                if (timeoutUser != null)
                {
                    if (timeoutUser.TimeoutExpiration < DateTime.UtcNow)
                    {
                        DeleteTimeoutFromList(recipient, broadcasterId, connStr);
                    }
                    else
                    {
                        TimeSpan timeout = timeoutUser.TimeoutExpiration - DateTime.UtcNow;
                        return timeout.ToString(@"hh\:mm\:ss");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return "0 seconds"; // if cannot find timeout
        }
    }
}
