using Newtonsoft.Json;

namespace TwitchBotUtil.Models.JSON
{
    public class ErrMsgJSON
    {
        [JsonProperty("error")]
        public string Error { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
