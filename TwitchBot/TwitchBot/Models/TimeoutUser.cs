using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot.Models
{
    public class TimeoutUser
    {
        public string Username { get; set; }
        public DateTime TimeoutExpiration { get; set; }
        public bool HasBeenWarned { get; set; }
    }
}
