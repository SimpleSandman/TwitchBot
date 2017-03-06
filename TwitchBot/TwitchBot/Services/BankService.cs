using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public void CreateAccount(string strRecipient, int intBroadcasterID, int intDeposit)
        {
            _bank.CreateAccount(strRecipient, intBroadcasterID, intDeposit);
        }

        public void UpdateFunds(string strWalletOwner, int intBroadcasterID, int intNewWalletBalance)
        {
            _bank.UpdateFunds(strWalletOwner, intBroadcasterID, intNewWalletBalance);
        }

        public List<string> UpdateCreateBalance(List<string> lstUsernames, int intBroadcasterID, int intDeposit)
        {
            return _bank.UpdateCreateBalance(lstUsernames, intBroadcasterID, intDeposit)
                .AsEnumerable()
                .Select(x => x[0].ToString())
                .ToList();
        }

        public int CheckBalance(string username, int intBroadcasterID)
        {
            return _bank.CheckBalance(username, intBroadcasterID);
        }
    }
}
