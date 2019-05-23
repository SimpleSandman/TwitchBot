using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwitchBotDb.DTO
{
    public partial class BalanceResult
    {
        public string ActionType { get; set; }
        public string Username { get; set; }
        public int Wallet { get; set; }
    }
}
