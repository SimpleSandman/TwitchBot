using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

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

        public void CreateAccount(string recipient, int broadcasterId, int deposit)
        {
            string query = "INSERT INTO tblBank (username, wallet, broadcaster) VALUES (@username, @wallet, @broadcaster)";

            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = recipient;
                cmd.Parameters.Add("@wallet", SqlDbType.Int).Value = deposit;
                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateFunds(string walletOwner, int broadcasterId, int newWalletBalance)
        {
            string query = "UPDATE tblBank SET wallet = @wallet WHERE (username = @username AND broadcaster = @broadcaster)";

            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add("@wallet", SqlDbType.Int).Value = newWalletBalance;
                cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = walletOwner;
                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public DataTable UpdateCreateBalance(List<string> usernameList, int broadcasterId, int deposit, bool showOutput = false)
        {
            DataTable usernamesTable = usernameList.ToDataTable();
            DataTable resultTable = new DataTable();

            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand("uspUpdateCreateBalance", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new SqlParameter("@tvpUsernames", SqlDbType.Structured)).Value = usernamesTable;
                cmd.Parameters.Add("@intBroadcasterID", SqlDbType.Int).Value = broadcasterId;
                cmd.Parameters.Add("@intDeposit", SqlDbType.Int).Value = deposit;
                cmd.Parameters.Add("@bitShowOutput", SqlDbType.Bit).Value = showOutput;

                using (SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd))
                {
                    dataAdapter.Fill(resultTable);
                }
            }

            return resultTable;
        }

        public int CheckBalance(string username, int broadcasterId)
        {
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblBank WHERE broadcaster = @broadcaster", conn))
                {
                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;
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
