using System;
using System.Collections.Generic;

namespace TwitchBotDb.Models
{
    public partial class SongRequestSetting
    {
        public int Id { get; set; }
        public string RequestPlaylistId { get; set; }
        public string PersonalPlaylistId { get; set; }
        public bool DjMode { get; set; }
        public int BroadcasterId { get; set; }

        public virtual Broadcaster Broadcaster { get; set; }
    }
}
