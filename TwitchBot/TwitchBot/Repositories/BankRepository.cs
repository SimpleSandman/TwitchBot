using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchBot.Extensions;

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

        public DataTable UpdateCreateBalance(List<string> lstUsernames, int intBroadcasterID, int intDeposit)
        {
            DataTable tblUsernames = lstUsernames.ToDataTable();
            DataTable tblResult = new DataTable();

            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand("uspUpdateCreateBalance", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new SqlParameter("@tvpUsernames", SqlDbType.Structured)).Value = tblUsernames;
                cmd.Parameters.Add("@intBroadcasterID", SqlDbType.Int).Value = intBroadcasterID;
                cmd.Parameters.Add("@intDeposit", SqlDbType.Int).Value = intDeposit;

                using (SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd))
                {
                    dataAdapter.Fill(tblResult);
                }
            }

            return tblResult;
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
