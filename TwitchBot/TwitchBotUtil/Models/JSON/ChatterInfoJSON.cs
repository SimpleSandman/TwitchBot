using System.Collections.Generic;

using Newtonsoft.Json;

namespace TwitchBotUtil.Models.JSON
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
        [JsonProperty("vips")]
        public List<string> VIPs { get; set; }
    }

    public class ChatterInfoJSON
    {
        [JsonProperty("chatter_count")]
        public int ChatterCount { get; set; }
        [JsonProperty("chatters")]
        public Chatters Chatters { get; set; }
    }
}
