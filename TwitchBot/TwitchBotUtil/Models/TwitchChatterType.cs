using System.Collections.Generic;

using TwitchBotUtil.Enums;

namespace TwitchBotUtil.Models
{
    public class TwitchChatterType
    {
        public List<TwitchChatter> TwitchChatters { get; set; }
        public ChatterType ChatterType { get; set; }
    }
}
