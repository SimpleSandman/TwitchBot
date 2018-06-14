using System.Collections.Generic;
using System.Threading.Tasks;

using TwitchBot.Repositories;

using TwitchBotDb.DTO;
using TwitchBotDb.Models;

namespace TwitchBot.Services
{
    public class BankService
    {
        private BankRepository _bank;

        public BankService(BankRepository bank)
        {
            _bank = bank;
        }

        public async Task CreateAccount(string recipient, int broadcasterId, int deposit)
        {
            await _bank.CreateAccount(recipient, broadcasterId, deposit);
        }

        public async Task UpdateFunds(string walletOwner, int broadcasterId, int newWalletBalance)
        {
            await _bank.UpdateAccount(walletOwner, broadcasterId, newWalletBalance);
        }

        public async Task<List<BalanceResult>> UpdateCreateBalance(List<string> usernameList, int broadcasterId, int deposit, bool showOutput = false)
        {
            return await _bank.UpdateCreateBalance(usernameList, broadcasterId, deposit, showOutput);
        }

        public async Task<int> CheckBalance(string username, int broadcasterId)
        {
            return await _bank.CheckBalance(username, broadcasterId);
        }

        public async Task<List<Bank>> GetCurrencyLeaderboard(string broadcasterName, int broadcasterId, string botName)
        {
            return await _bank.GetCurrencyLeaderboard(broadcasterName, broadcasterId, botName);
        }
    }
}
