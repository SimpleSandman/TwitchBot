using System.Collections.Generic;

using Newtonsoft.Json;

namespace TwitchBotCore.Models.JSON
{
    public class FollowerJSON
    {
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }
        //public bool notifications { get; set; }
        [JsonProperty("user")]
        public UserJSON User { get; set; }
    }

    public class RootFollowerJSON
    {
        //public int _total { get; set; }
        //public string _cursor { get; set; }
        [JsonProperty("follows")]
        public List<FollowerJSON> Followers { get; set; }
    }
}
