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
        private Dictionary<string, DateTime> lstTimeout = new Dictionary<string, DateTime>();

        public Dictionary<string, DateTime> getLstTimeout()
        {
            return lstTimeout;
        }

        public void setLstTimeout(Dictionary<string, DateTime> value)
        {
            lstTimeout = value;
        }

        public void addTimeoutToLst(string strRecipient, int intBroadcaster, double dblSec, string connStr)
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
                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = intBroadcaster;
                    cmd.Parameters.Add("@timeout", SqlDbType.DateTime).Value = dtTimeout;

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }

                lstTimeout.Add(strRecipient, dtTimeout);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void delTimeoutFromLst(string strRecipient, int strBroadcaster, string connStr)
        {
            try
            {
                string query = "DELETE FROM tblTimeout WHERE username = @username AND broadcaster = @broadcaster";

                // Create connection and command
                using (SqlConnection conn = new SqlConnection(connStr))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = strRecipient;
                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = strBroadcaster;

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }

                lstTimeout.Remove(strRecipient);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
