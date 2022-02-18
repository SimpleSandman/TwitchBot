using System.Collections.Generic;

using Newtonsoft.Json;

namespace TwitchBotShared.Models.JSON
{
    public class FollowerJSON
    {
        [JsonProperty("from_id")]
        public string FromId { get; set; }

        [JsonProperty("from_login")]
        public string FromLogin { get; set; }

        [JsonProperty("from_name")]
        public string FromName { get; set; }

        [JsonProperty("to_id")]
        public string ToId { get; set; }

        [JsonProperty("to_name")]
        public string ToName { get; set; }

        [JsonProperty("followed_at")]
        public string FollowedAt { get; set; }
    }

    public class RootFollowerJSON
    {
        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("data")]
        public List<FollowerJSON> Followers { get; set; }

        [JsonProperty("pagination")]
        public Pagination Pagination { get; set; }
    }
}
