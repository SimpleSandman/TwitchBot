using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace TwitchBot.Models.JSON
{
    public class UserJSON
    {
        //public string display_name { get; set; }
        [JsonProperty("_id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        //public string type { get; set; }
        //public string bio { get; set; }
        //public string created_at { get; set; }
        //public string updated_at { get; set; }
        //public string logo { get; set; }
    }

    public class RootUserJSON
    {
        //public int _total { get; set; }
        [JsonProperty("users")]
        public List<UserJSON> Users { get; set; }
    }
}
