using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot
{
    public class Timeout
    {
        private Dictionary<string, DateTime> _dictTimeout = new Dictionary<string, DateTime>();

        public Dictionary<string, DateTime> getLstTimeout()
        {
            return _dictTimeout;
        }

        public void setLstTimeout(Dictionary<string, DateTime> value)
        {
            _dictTimeout = value;
        }

        public void addTimeoutToLst(string strRecipient, int intBroadcasterID, double dblSec, string connStr)
        {
            try
            {
                string query = "INSERT INTO tblTimeout (username, broadcaster, timeout) VALUES (@username, @broadcaster, @timeout)";
                DateTime dtTimeout = new DateTime();
                dtTimeout = DateTime.UtcNow.AddSeconds(dblSec);

                // Create connection and command
                using (SqlConnection conn = new SqlConnection(connStr))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = strRecipient;
                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = intBroadcasterID;
                    cmd.Parameters.Add("@timeout", SqlDbType.DateTime).Value = dtTimeout;

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }

                _dictTimeout.Add(strRecipient, dtTimeout);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void delTimeoutFromLst(string strRecipient, int intBroadcasterID, string connStr)
        {
            try
            {
                string query = "DELETE FROM tblTimeout WHERE username = @username AND broadcaster = @broadcaster";

                // Create connection and command
                using (SqlConnection conn = new SqlConnection(connStr))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = strRecipient;
                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = intBroadcasterID;

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }

                _dictTimeout.Remove(strRecipient);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public string getTimoutFromUser(string strRecipient, int intBroadcasterID, string connStr)
        {
            try
            {
                if (_dictTimeout.ContainsKey(strRecipient))
                {
                    if (_dictTimeout[strRecipient] < DateTime.UtcNow)
                    {
                        delTimeoutFromLst(strRecipient, intBroadcasterID, connStr);
                    }
                    else
                    {
                        TimeSpan timeout = _dictTimeout[strRecipient] - DateTime.UtcNow;
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
