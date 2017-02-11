using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot.Repositories
{
    public class BankRepository
    {
        private string _connStr;

        public BankRepository(string connStr)
        {
            _connStr = connStr;
        }

        public void CreateAccount(string strRecipient, int intBroadcasterID, int intDeposit)
        {
            string query = "INSERT INTO tblBank (username, wallet, broadcaster) VALUES (@username, @wallet, @broadcaster)";

            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = strRecipient;
                cmd.Parameters.Add("@wallet", SqlDbType.Int).Value = intDeposit;
                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = intBroadcasterID;

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateFunds(string strWalletOwner, int intBroadcasterID, int intNewWalletBalance)
        {
            string query = "UPDATE tblBank SET wallet = @wallet WHERE (username = @username AND broadcaster = @broadcaster)";

            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add("@wallet", SqlDbType.Int).Value = intNewWalletBalance;
                cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = strWalletOwner;
                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = intBroadcasterID;

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public int CheckBalance(string username, int intBroadcasterID)
        {
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblBank WHERE broadcaster = @broadcaster", conn))
                {
                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = intBroadcasterID;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (username.Equals(reader["username"].ToString()))
                                {
                                    return int.Parse(reader["wallet"].ToString());
                                }
                            }
                        }
                    }
                }
            }

            return -1;
        }
    }
}
