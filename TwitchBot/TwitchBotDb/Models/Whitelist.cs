using System;
using System.Collections.Generic;

namespace TwitchBotDb.Models
{
    public partial class Whitelist
    {
        public int Id { get; set; }
        public int CustomCommandId { get; set; }
        public string Username { get; set; }
        public int TwitchId { get; set; }

        public virtual CustomCommand CustomCommand { get; set; }
    }
}
