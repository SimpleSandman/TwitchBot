using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot
{
    public class Moderator
    {
        private List<string> lstMod = new List<string>();

        public List<string> getLstMod()
        {
            return lstMod;
        }

        public void setLstMod(List<string> value)
        {
            lstMod = value;
        }

        public void addNewModToLst(string strRecipient, int intBroadcaster, string connStr)
        {
            try
            {
                string query = "INSERT INTO tblModerators (username, broadcaster) VALUES (@username, @broadcaster)";

                // Create connection and command
                using (SqlConnection conn = new SqlConnection(connStr))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = strRecipient;
                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = intBroadcaster;

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }

                lstMod.Add(strRecipient);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void delOldModFromLst(string strRecipient, int intBroadcaster, string connStr)
        {
            try
            {
                string query = "DELETE FROM tblModerators WHERE username = @username AND broadcaster = @broadcaster";

                // Create connection and command
                using (SqlConnection conn = new SqlConnection(connStr))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = strRecipient;
                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = intBroadcaster;

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }

                lstMod.Remove(strRecipient);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
