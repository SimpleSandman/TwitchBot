using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace TwitchBot.Models.JSON
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
        //public int viewers { get; set; }
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }
        //public int video_height { get; set; }
        //public int average_fps { get; set; }
        //public int delay { get; set; }
        //public bool is_playlist { get; set; }
    }
}
