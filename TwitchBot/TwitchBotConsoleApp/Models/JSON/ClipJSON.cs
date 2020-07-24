using Newtonsoft.Json;

namespace TwitchBotConsoleApp.Models.JSON
{
    public class ClipJSON
    {
        //public string slug { get; set; }
        //public string tracking_id { get; set; }
        //public string url { get; set; }
        //public string embed_url { get; set; }
        //public string embed_html { get; set; }
        [JsonProperty("broadcaster")]
        public BroadcasterSnippetJSON Broadcaster { get; set; }
        //public Curator curator { get; set; }
        //public Vod vod { get; set; }
        //public string game { get; set; }
        //public string language { get; set; }
        //public string title { get; set; }
        //public int views { get; set; }
        //public double duration { get; set; }
        //public DateTime created_at { get; set; }
        //public Thumbnails thumbnails { get; set; }
    }

    public class BroadcasterSnippetJSON
    {
        //public string id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        //public string display_name { get; set; }
        //public string channel_url { get; set; }
        //public string logo { get; set; }
    }
}
