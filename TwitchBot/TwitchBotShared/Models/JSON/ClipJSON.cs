using Newtonsoft.Json;

using System.Collections.Generic;

namespace TwitchBotShared.Models.JSON
{
    public class ClipJSON
    {
        //[JsonProperty("id")]
        //public string Id { get; set; }

        //[JsonProperty("url")]
        //public string Url { get; set; }

        //[JsonProperty("embed_url")]
        //public string EmbedUrl { get; set; }

        //[JsonProperty("broadcaster_id")]
        //public string BroadcasterId { get; set; }

        [JsonProperty("broadcaster_name")]
        public string BroadcasterName { get; set; }

        //[JsonProperty("creator_id")]
        //public string CreatorId { get; set; }

        //[JsonProperty("creator_name")]
        //public string CreatorName { get; set; }

        //[JsonProperty("video_id")]
        //public string VideoId { get; set; }

        //[JsonProperty("game_id")]
        //public string GameId { get; set; }

        //[JsonProperty("language")]
        //public string Language { get; set; }

        //[JsonProperty("title")]
        //public string Title { get; set; }

        //[JsonProperty("view_count")]
        //public int ViewCount { get; set; }

        //[JsonProperty("created_at")]
        //public string CreatedAt { get; set; }

        //[JsonProperty("thumbnail_url")]
        //public string ThumbnailUrl { get; set; }

        //[JsonProperty("duration")]
        //public double Duration { get; set; }
    }

    public class RootClipJSON
    {
        [JsonProperty("data")]
        public List<ClipJSON> Clips { get; set; }

        [JsonProperty("pagination")]
        public Pagination Pagination { get; set; }
    }
}
