using System.Collections.Generic;

using Newtonsoft.Json;

namespace TwitchBotShared.Models.JSON
{
    public class UserJSON
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("login")]
        public string Login { get; set; }

        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        //[JsonProperty("type")]
        //public string Type { get; set; }

        [JsonProperty("broadcaster_type")]
        public string BroadcasterType { get; set; }

        //[JsonProperty("description")]
        //public string Description { get; set; }

        //[JsonProperty("profile_image_url")]
        //public string ProfileImageUrl { get; set; }

        //[JsonProperty("offline_image_url")]
        //public string OfflineImageUrl { get; set; }

        //[JsonProperty("view_count")]
        //public int ViewCount { get; set; }

        //[JsonProperty("created_at")]
        //public string CreatedAt { get; set; }
    }

    public class RootUserJSON
    {
        [JsonProperty("data")]
        public List<UserJSON> Users { get; set; }
    }
}
