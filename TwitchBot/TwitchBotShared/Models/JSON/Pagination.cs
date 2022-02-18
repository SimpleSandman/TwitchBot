using Newtonsoft.Json;

namespace TwitchBotShared.Models.JSON
{
    public class Pagination
    {
        [JsonProperty("cursor")]
        public string Cursor { get; set; }
    }
}
