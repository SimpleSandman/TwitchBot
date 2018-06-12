using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

using TwitchBot.Libraries;

using TwitchBotDb.DTO;
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

        public async Task UpdateAccount(string walletOwner, int broadcasterId, int newWalletBalance)
        {
            await ApiBotRequest.PutExecuteTaskAsync<Bank>(_twitchBotApiLink + $"banks/updateaccount/{broadcasterId}?updatedwallet={newWalletBalance}&username={walletOwner}");
        }

        public async Task<List<BalanceResult>> UpdateCreateBalance(List<string> usernameList, int broadcasterId, int deposit, bool showOutput = false)
        {
            return await ApiBotRequest.PutExecuteTaskAsync<List<BalanceResult>>(_twitchBotApiLink + $"banks/updatecreateaccount/{broadcasterId}?deposit={deposit}&showOutput={showOutput}", usernameList);
        }

        public async Task<int> CheckBalance(string username, int broadcasterId)
        {
            var response = await ApiBotRequest.GetExecuteTaskAsync<List<Bank>>(_twitchBotApiLink + $"banks/get/{broadcasterId}?username={username}");

            if (response != null && response.Count > 0)
            {
                return response.Find(m => m.Username == username).Wallet;
            }

            return -1;
        }

        public async Task<List<Bank>> GetCurrencyLeaderboard(string broadcasterName, int broadcasterId, string botName)
        {
            var response = await ApiBotRequest.GetExecuteTaskAsync<List<Bank>>(_twitchBotApiLink + $"banks/getleaderboard/{broadcasterId}?broadcastername={broadcasterName}&botname={botName}&topnumber=3");

            if (response != null && response.Count > 0)
            {
                return response;
            }

            return new List<Bank>();
        }
    }
}
