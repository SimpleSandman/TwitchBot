using System.Collections.Generic;

using TwitchBotShared.Enums;

namespace TwitchBotShared.Models
{
    public class TwitchChatterType
    {
        public List<TwitchChatter> TwitchChatters { get; set; }
        public ChatterType ChatterType { get; set; }
    }
}
