using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

using TwitchBot.Extensions;
using TwitchBot.Libraries;
using TwitchBot.Models;

using TwitchBotDb.Models;

namespace TwitchBot.Repositories
{
    public class BankRepository
    {
        private readonly string _connStr;
        private readonly string _twitchBotApiLink;

        public BankRepository(string connStr, string twitchBotApiLink)
        {
            _connStr = connStr;
            _twitchBotApiLink = twitchBotApiLink;
        }

        public void CreateAccount(string recipient, int broadcasterId, int deposit)
        {
            string query = "INSERT INTO Bank (Username, Wallet, Broadcaster) VALUES (@username, @wallet, @broadcaster)";

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
            string query = "UPDATE Bank SET Wallet = @wallet WHERE (Username = @username AND Broadcaster = @broadcaster)";

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
            using (SqlCommand cmd = new SqlCommand("UpdateCreateBalance", conn))
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

        public async Task<int> CheckBalance(string username, int broadcasterId)
        {
            var response = await ApiRequest.GetBotExecuteTaskAsync<List<Bank>>(_twitchBotApiLink + $"banks/get/{broadcasterId}?username={username}");

            if (response != null && response.Count > 0)
            {
                return response.Find(m => m.Username == username).Wallet;
            }

            return -1;
        }

        public async Task<List<Bank>> GetCurrencyLeaderboard(string broadcasterName, int broadcasterId, string botName)
        {
            var response = await ApiRequest.GetBotExecuteTaskAsync<List<Bank>>(_twitchBotApiLink + $"banks/getleaderboard/{broadcasterId}?broadcastername={broadcasterName}&botname={botName}&topnumber=3");

            if (response != null && response.Count > 0)
            {
                return response;
            }

            return new List<Bank>();
        }
    }
}
