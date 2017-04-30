using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace TwitchBot.Libraries
{
    public class TimeoutCmd
    {
        private Dictionary<string, DateTime> _timeoutKeyValues = new Dictionary<string, DateTime>();

        public Dictionary<string, DateTime> TimeoutKeyValues
        {
            get { return _timeoutKeyValues; }
            set { _timeoutKeyValues = value; }
        }

        public void AddTimeoutToList(string recipient, int broadcasterId, double seconds, string connStr)
        {
            try
            {
                string query = "INSERT INTO tblTimeout (username, broadcaster, timeout) VALUES (@username, @broadcaster, @timeout)";
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
                    conn.Close();
                }

                _timeoutKeyValues.Add(recipient, timeoutDuration);
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
                string query = "DELETE FROM tblTimeout WHERE username = @username AND broadcaster = @broadcaster";

                // Create connection and command
                using (SqlConnection conn = new SqlConnection(connStr))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = recipient;
                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                _timeoutKeyValues.Remove(recipient);
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
                if (_timeoutKeyValues.ContainsKey(recipient))
                {
                    if (_timeoutKeyValues[recipient] < DateTime.UtcNow)
                    {
                        DeleteTimeoutFromList(recipient, broadcasterId, connStr);
                    }
                    else
                    {
                        TimeSpan timeout = _timeoutKeyValues[recipient] - DateTime.UtcNow;
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
