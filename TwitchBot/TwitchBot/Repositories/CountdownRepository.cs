using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot.Repositories
{
    public class CountdownRepository
    {
        private string _connStr;

        public CountdownRepository(string connStr)
        {
            _connStr = connStr;
        }

        public void AddCountdown(string countdownMessage, DateTime countdownDuration, int broadcasterId)
        {
            string query = "INSERT INTO tblCountdown (dueDate, message, broadcaster) VALUES (@dueDate, @message, @broadcaster)";

            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add("@dueDate", SqlDbType.DateTime).Value = countdownDuration;
                cmd.Parameters.Add("@message", SqlDbType.VarChar, 50).Value = countdownMessage;
                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public int GetCountdownId(int reqCountdownId, int broadcasterId)
        {
            int responseCountdownId = -1;

            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT id, broadcaster FROM tblCountdown "
                    + "WHERE broadcaster = @broadcaster", conn))
                {
                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (reqCountdownId.ToString().Equals(reader["id"].ToString()))
                                {
                                    responseCountdownId = int.Parse(reader["id"].ToString());
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return responseCountdownId;
        }

        public string ListCountdowns(int broadcasterId)
        {
            string countdownListMsg = "";

            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT Id, dueDate, message, broadcaster FROM tblCountdown "
                    + "WHERE broadcaster = @broadcaster ORDER BY Id", conn))
                {
                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                countdownListMsg += "ID: " + reader["Id"].ToString()
                                    + " Message: \"" + reader["message"].ToString()
                                    + "\" Time: \"" + reader["dueDate"].ToString()
                                    + "\" // ";
                            }
                            StringBuilder modCountdownListMsg = new StringBuilder(countdownListMsg);
                            modCountdownListMsg.Remove(countdownListMsg.Length - 4, 4); // remove extra " >< "
                            countdownListMsg = modCountdownListMsg.ToString(); // replace old countdown list string with new
                        }
                    }
                }
            }

            return countdownListMsg;
        }

        public void UpdateCountdown(int inputType, DateTime countdownDuration, string countdownInput, int responseCountdownId, int broadcasterId)
        {
            string strQuery = "";

            if (inputType == 1)
                strQuery = "UPDATE dbo.tblCountdown SET dueDate = @dueDate WHERE (Id = @id AND broadcaster = @broadcaster)";
            else if (inputType == 2)
                strQuery = "UPDATE dbo.tblCountdown SET message = @message WHERE (Id = @id AND broadcaster = @broadcaster)";

            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(strQuery, conn))
            {
                // append proper parameter
                if (inputType == 1)
                    cmd.Parameters.Add("@dueDate", SqlDbType.DateTime).Value = countdownDuration;
                else if (inputType == 2)
                    cmd.Parameters.Add("@message", SqlDbType.VarChar, 50).Value = countdownInput;

                cmd.Parameters.Add("@id", SqlDbType.Int).Value = responseCountdownId;
                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
