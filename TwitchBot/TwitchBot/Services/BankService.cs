using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public void CreateAccount(string strRecipient, int intBroadcasterID, int intDeposit)
        {
            _bank.CreateAccount(strRecipient, intBroadcasterID, intDeposit);
        }

        public void UpdateFunds(string strWalletOwner, int intBroadcasterID, int intNewWalletBalance)
        {
            _bank.UpdateFunds(strWalletOwner, intBroadcasterID, intNewWalletBalance);
        }

        public List<BalanceResult> UpdateCreateBalance(List<string> lstUsernames, int intBroadcasterID, int intDeposit, bool showOutput = false)
        {
            DataTable dt = _bank.UpdateCreateBalance(lstUsernames, intBroadcasterID, intDeposit, showOutput);
            if (dt.Rows.Count == 0)
            {
                return new List<BalanceResult>();
            }

            List<BalanceResult> lstUpdatedBalances = new List<BalanceResult>(dt.Rows.Count);

            foreach (DataRow row in dt.Rows)
            {
                var values = row.ItemArray;
                BalanceResult br = new BalanceResult()
                {
                    actionType = values[0].ToString(),
                    username = values[1].ToString(),
                    wallet = Convert.ToInt32(values[2])
                };
                lstUpdatedBalances.Add(br);
            }

            return lstUpdatedBalances;
        }

        public int CheckBalance(string username, int intBroadcasterID)
        {
            return _bank.CheckBalance(username, intBroadcasterID);
        }
    }
}
