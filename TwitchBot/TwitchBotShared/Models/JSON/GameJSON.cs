using System.Collections.Generic;

using Newtonsoft.Json;

namespace TwitchBotShared.Models.JSON
{
    public class GameJSON
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        //public string BoxArtUrl { get; set; }
    }

    public class RootGameJSON
    {
        [JsonProperty("data")]
        public List<GameJSON> Games { get; set; }
    }
}
