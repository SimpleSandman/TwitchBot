using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot.Models
{
    public class BalanceResult
    {
        public string actionType { get; set; }
        public string username { get; set; }
        public int wallet { get; set; }
    }
}
