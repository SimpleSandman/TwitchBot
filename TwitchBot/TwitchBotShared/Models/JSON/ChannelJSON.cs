using Newtonsoft.Json;

namespace TwitchBotShared.Models.JSON
{
    public class ChannelJSON
    {
        //public bool mature { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
        //public string broadcaster_language { get; set; }
        //public string display_name { get; set; }
        [JsonProperty("game")]
        public string Game { get; set; }
        //public string language { get; set; }
        //public int _id { get; set; }
        //public string name { get; set; }
        //public DateTime created_at { get; set; }
        //public DateTime updated_at { get; set; }
        //public bool partner { get; set; }
        //public string logo { get; set; }
        //public string video_banner { get; set; }
        //public string profile_banner { get; set; }
        //public string profile_banner_background_color { get; set; }
        //public string url { get; set; }
        //public int views { get; set; }
        //public int followers { get; set; }
        [JsonProperty("broadcaster_type")]
        public string BroadcasterType { get; set; }
        //public string description { get; set; }
    }
}
