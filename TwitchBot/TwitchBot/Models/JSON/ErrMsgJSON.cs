using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot.Models.JSON
{
    public class ErrMsgJSON
    {
        public string error { get; set; }
        public string status { get; set; }
        public string message { get; set; }
    }
}
