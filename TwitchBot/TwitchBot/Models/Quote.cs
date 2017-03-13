using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot.Models
{
    public class Quote
    {
        public string Message { get; set; }
        public string Author { get; set; }
        public DateTime TimeCreated { get; set; }
    }
}
