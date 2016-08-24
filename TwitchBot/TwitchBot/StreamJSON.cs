using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot
{
    public class RootStreamJSON
    {
        public StreamJSON stream { get; set; }
    }

    public class StreamJSON
    {
        //public long _id { get; set; }
        //public string game { get; set; }
        //public int viewers { get; set; }
        public string created_at { get; set; }
        //public int video_height { get; set; }
        //public int average_fps { get; set; }
        //public int delay { get; set; }
        //public bool is_playlist { get; set; }
    }
}
