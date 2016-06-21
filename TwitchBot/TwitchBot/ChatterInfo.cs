using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot
{
    public class Chatters
    {
        public List<string> moderators { get; set; }
        public List<object> staff { get; set; }
        public List<object> admins { get; set; }
        public List<object> global_mods { get; set; }
        public List<object> viewers { get; set; }
    }

    public class ChatterInfo
    {
        public int chatter_count { get; set; }
        public Chatters chatters { get; set; }
    }
}
