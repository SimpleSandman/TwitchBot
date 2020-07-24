using System.Collections.Generic;

using Newtonsoft.Json;

namespace TwitchBotConsoleApp.Models.JSON
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
