using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace TwitchBot.Models.JSON
{
    public class FollowingSinceJSON
    {
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }
        //public bool notifications { get; set; }
    }
}
