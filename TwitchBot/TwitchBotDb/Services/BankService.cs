using System.Collections.Generic;
using System.Threading.Tasks;

using TwitchBotDb.DTO;
using TwitchBotDb.Models;
using TwitchBotDb.Repositories;

namespace TwitchBotDb.Services
{
    public class BankService
    {
        private BankRepository _bank;

        public BankService(BankRepository bank)
        {
            _bank = bank;
        }

        public async Task CreateAccountAsync(string recipient, int broadcasterId, int deposit)
        {
            await _bank.CreateAccountAsync(recipient, broadcasterId, deposit);
        }

        public async Task UpdateFundsAsync(string walletOwner, int broadcasterId, int newWalletBalance)
        {
            await _bank.UpdateAccountAsync(walletOwner, broadcasterId, newWalletBalance);
        }

        public async Task<List<BalanceResult>> UpdateCreateBalanceAsync(List<string> usernameList, int broadcasterId, int deposit, bool showOutput = false)
        {
            return await _bank.UpdateCreateBalance(usernameList, broadcasterId, deposit, showOutput);
        }

        public async Task<int> CheckBalanceAsync(string username, int broadcasterId)
        {
            return await _bank.CheckBalanceAsync(username, broadcasterId);
        }

        public async Task<List<Bank>> GetCurrencyLeaderboardAsync(string broadcasterName, int broadcasterId, string botName)
        {
            return await _bank.GetCurrencyLeaderboardAsync(broadcasterName, broadcasterId, botName);
        }
    }
}
