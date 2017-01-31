using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot
{
    public sealed class Moderator
    {
        private static volatile Moderator _instance;
        private static object _syncRoot = new Object();

        private List<string> _lstMod = new List<string>();

        public List<string> LstMod
        {
            get { return _lstMod; }
        }

        private Moderator() { }

        public static Moderator Instance
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
                            _instance = new Moderator();
                    }
                }

                return _instance;
            }
        }

        public void setLstMod(string connStr, int intBroadcasterID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblModerators WHERE broadcaster = @broadcaster", conn))
                    {
                        cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = intBroadcasterID;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    _lstMod.Add(reader["username"].ToString());
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

                _lstMod.Add(strRecipient);
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

                _lstMod.Remove(strRecipient);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
