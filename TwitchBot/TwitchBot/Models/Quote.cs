using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot.Models
{
    public class Quote
    {
        public string strMessage { get; set; }
        public string strAuthor { get; set; }
        public DateTime dtTimeCreated { get; set; }
    }
}
