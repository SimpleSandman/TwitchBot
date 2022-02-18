using Newtonsoft.Json;

namespace TwitchBotShared.Models.JSON
{
    public class ChannelUpdateJSON
    {
        [JsonProperty("game_id")]
        public string GameId { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        //public string BroadcasterLanguage { get; set; }
        //public int Delay { get; set; }
    }
}
