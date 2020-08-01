using Newtonsoft.Json;

namespace TwitchBotShared.Models.JSON
{
    public partial class VideoJSON
    {
        //public string _id { get; set; }
        //public int broadcast_id { get; set; }
        //public string broadcast_type { get; set; }
        [JsonProperty("channel")]
        public ChannelJSON Channel { get; set; }
        //public DateTime created_at { get; set; }
        //public string description { get; set; }
        //public string description_html { get; set; }
        //public Fps fps { get; set; }
        //public string game { get; set; }
        //public string language { get; set; }
        //public int length { get; set; }
        //public List<MutedSegment> muted_segments { get; set; }
        //public Preview preview { get; set; }
        //public DateTime published_at { get; set; }
        //public Resolutions resolutions { get; set; }
        //public string status { get; set; }
        //public string tag_list { get; set; }
        //public Thumbnails thumbnails { get; set; }
        //public string title { get; set; }
        //public string url { get; set; }
        //public string viewable { get; set; }
        //public object viewable_at { get; set; }
        //public int views { get; set; }
    }
}
