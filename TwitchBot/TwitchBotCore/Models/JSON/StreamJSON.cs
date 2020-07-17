using Newtonsoft.Json;

namespace TwitchBotCore.Models.JSON
{
    public class RootStreamJSON
    {
        [JsonProperty("stream")]
        public StreamJSON Stream { get; set; }
    }

    public class StreamJSON
    {
        //public long _id { get; set; }
        [JsonProperty("game")]
        public string Game { get; set; }
        //public string broadcast_platform { get; set; }
        //public string community_id { get; set; }
        //public List<object> community_ids { get; set; }
        //public int viewers { get; set; }
        //public int video_height { get; set; }
        //public double average_fps { get; set; }
        //public int delay { get; set; }
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }
        //public bool is_playlist { get; set; }
        //public string stream_type { get; set; }
        //public Preview preview { get; set; }
        [JsonProperty("channel")]
        public ChannelJSON Channel { get; set; }
    }
}
