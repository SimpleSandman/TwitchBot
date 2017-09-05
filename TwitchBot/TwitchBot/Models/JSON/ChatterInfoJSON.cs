using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace TwitchBot.Models.JSON
{
    public class Chatters
    {
        [JsonProperty("moderators")]
        public List<string> Moderators { get; set; }
        [JsonProperty("staff")]
        public List<string> Staff { get; set; }
        [JsonProperty("admins")]
        public List<string> Admins { get; set; }
        [JsonProperty("global_mods")]
        public List<string> GlobalMods { get; set; }
        [JsonProperty("viewers")]
        public List<string> Viewers { get; set; }
    }

    public class ChatterInfoJSON
    {
        [JsonProperty("chatter_count")]
        public int ChatterCount { get; set; }
        [JsonProperty("chatters")]
        public Chatters Chatters { get; set; }
    }
}
