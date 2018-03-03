using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace TwitchBot.Models.JSON
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
