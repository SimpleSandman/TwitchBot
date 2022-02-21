using System.Collections.Generic;

using Newtonsoft.Json;

namespace TwitchBotShared.Models.JSON
{
    public class StreamJSON
    {
        //[JsonProperty("id")]
        //public string Id { get; set; }

        //[JsonProperty("user_id")]
        //public string UserId { get; set; }

        //[JsonProperty("user_login")]
        //public string UserLogin { get; set; }

        //[JsonProperty("user_name")]
        //public string UserName { get; set; }

        //[JsonProperty("game_id")]
        //public string GameId { get; set; }

        [JsonProperty("game_name")]
        public string GameName { get; set; }

        //[JsonProperty("type")]
        //public string Type { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        //[JsonProperty("viewer_count")]
        //public int ViewerCount { get; set; }

        [JsonProperty("started_at")]
        public string StartedAt { get; set; }

        //[JsonProperty("language")]
        //public string Language { get; set; }

        //[JsonProperty("thumbnail_url")]
        //public string ThumbnailUrl { get; set; }

        //[JsonProperty("tag_ids")]
        //public List<string> TagIds { get; set; }

        //[JsonProperty("is_mature")]
        //public bool IsMature { get; set; }
    }

    public class RootStreamJSON
    {
        [JsonProperty("data")]
        public List<StreamJSON> Stream { get; set; }

        [JsonProperty("pagination")]
        public Pagination Pagination { get; set; }
    }
}
