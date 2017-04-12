using System;
using System.Collections.Generic;
using System.Data;

using TwitchBot.Models;
using TwitchBot.Repositories;

namespace TwitchBot.Services
{
    public class BankService
    {
        private BankRepository _bank;

        public BankService(BankRepository bank)
        {
            _bank = bank;
        }

        public void CreateAccount(string recipient, int broadcasterId, int deposit)
        {
            _bank.CreateAccount(recipient, broadcasterId, deposit);
        }

        public void UpdateFunds(string walletOwner, int broadcasterId, int newWalletBalance)
        {
            _bank.UpdateFunds(walletOwner, broadcasterId, newWalletBalance);
        }

        public List<BalanceResult> UpdateCreateBalance(List<string> usernameList, int broadcasterId, int deposit, bool showOutput = false)
        {
            DataTable dt = _bank.UpdateCreateBalance(usernameList, broadcasterId, deposit, showOutput);
            if (dt.Rows.Count == 0)
            {
                return new List<BalanceResult>();
            }

            List<BalanceResult> updatedBalanceList = new List<BalanceResult>(dt.Rows.Count);

            foreach (DataRow row in dt.Rows)
            {
                var values = row.ItemArray;
                BalanceResult br = new BalanceResult()
                {
                    ActionType = values[0].ToString(),
                    Username = values[1].ToString(),
                    Wallet = Convert.ToInt32(values[2])
                };
                updatedBalanceList.Add(br);
            }

            return updatedBalanceList;
        }

        public int CheckBalance(string username, int broadcasterId)
        {
            return _bank.CheckBalance(username, broadcasterId);
        }
    }
}
