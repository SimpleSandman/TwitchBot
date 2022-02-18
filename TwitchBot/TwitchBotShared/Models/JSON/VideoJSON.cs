using Newtonsoft.Json;
using System.Collections.Generic;

namespace TwitchBotShared.Models.JSON
{
    public class VideoJSON
    {
        //[JsonProperty("id")]
        //public string Id { get; set; }

        //[JsonProperty("stream_id")]
        //public string StreamId { get; set; }

        //[JsonProperty("user_id")]
        //public string UserId { get; set; }

        [JsonProperty("user_login")]
        public string UserLogin { get; set; }

        //[JsonProperty("user_name")]
        //public string UserName { get; set; }

        //[JsonProperty("title")]
        //public string Title { get; set; }

        //[JsonProperty("description")]
        //public string Description { get; set; }

        //[JsonProperty("created_at")]
        //public string CreatedAt { get; set; }

        //[JsonProperty("published_at")]
        //public string PublishedAt { get; set; }

        //[JsonProperty("url")]
        //public string Url { get; set; }

        //[JsonProperty("thumbnail_url")]
        //public string ThumbnailUrl { get; set; }

        //[JsonProperty("viewable")]
        //public string Viewable { get; set; }

        //[JsonProperty("view_count")]
        //public int ViewCount { get; set; }

        //[JsonProperty("language")]
        //public string Language { get; set; }

        //[JsonProperty("type")]
        //public string Type { get; set; }

        //[JsonProperty("duration")]
        //public string Duration { get; set; }

        //[JsonProperty("muted_segments")]
        //public List<MutedSegment> MutedSegments { get; set; }
    }

    //public class MutedSegment
    //{
    //    [JsonProperty("duration")]
    //    public int Duration { get; set; }

    //    [JsonProperty("offset")]
    //    public int Offset { get; set; }
    //}

    public class RootVideoJSON
    {
        [JsonProperty("data")]
        public List<VideoJSON> Videos { get; set; }

        [JsonProperty("pagination")]
        public Pagination Pagination { get; set; }
    }
}
