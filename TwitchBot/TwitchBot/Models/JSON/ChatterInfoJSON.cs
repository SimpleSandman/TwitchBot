using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot.Models.JSON
{
    public class Chatters
    {
        public List<string> moderators { get; set; }
        public List<string> staff { get; set; }
        public List<string> admins { get; set; }
        public List<string> global_mods { get; set; }
        public List<string> viewers { get; set; }
    }

    public class ChatterInfoJSON
    {
        public int chatter_count { get; set; }
        public Chatters chatters { get; set; }
    }
}
