using System.Collections.Generic;
using System.Threading.Tasks;

using TwitchBotDb.DTO;
using TwitchBotDb.Models;

namespace TwitchBotDb.Repositories
{
    public class BankRepository
    {
        private readonly string _twitchBotApiLink;

        public BankRepository(string twitchBotApiLink)
        {
            _twitchBotApiLink = twitchBotApiLink;
        }

        public async Task CreateAccount(string username, int broadcasterId, int deposit)
        {
            Bank freshAccount = new Bank
            {
                Username = username,
                Wallet = deposit,
                Broadcaster = broadcasterId
            };

            await ApiBotRequest.PostExecuteAsync(_twitchBotApiLink + $"banks/createaccount", freshAccount);
        }

        public async Task UpdateAccount(string walletOwner, int broadcasterId, int newWalletBalance)
        {
            await ApiBotRequest.PutExecuteAsync<Bank>(_twitchBotApiLink + $"banks/updateaccount/{broadcasterId}?updatedwallet={newWalletBalance}&username={walletOwner}");
        }

        public async Task<List<BalanceResult>> UpdateCreateBalance(List<string> usernameList, int broadcasterId, int deposit, bool showOutput = false)
        {
            return await ApiBotRequest.PutExecuteAsync<List<BalanceResult>>(_twitchBotApiLink + $"banks/updatecreateaccount/{broadcasterId}?deposit={deposit}&showOutput={showOutput}", usernameList);
        }

        public async Task<int> CheckBalance(string username, int broadcasterId)
        {
            var response = await ApiBotRequest.GetExecuteAsync<List<Bank>>(_twitchBotApiLink + $"banks/get/{broadcasterId}?username={username}");

            if (response != null && response.Count > 0)
            {
                return response.Find(m => m.Username == username).Wallet;
            }

            return -1;
        }

        public async Task<List<Bank>> GetCurrencyLeaderboard(string broadcasterName, int broadcasterId, string botName)
        {
            var response = await ApiBotRequest.GetExecuteAsync<List<Bank>>(_twitchBotApiLink + $"banks/getleaderboard/{broadcasterId}?broadcastername={broadcasterName}&botname={botName}&topnumber=3");

            if (response != null && response.Count > 0)
            {
                return response;
            }

            return new List<Bank>();
        }
    }
}
