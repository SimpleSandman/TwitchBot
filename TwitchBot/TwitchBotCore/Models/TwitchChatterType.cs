using System.Collections.Generic;

using TwitchBotCore.Enums;

namespace TwitchBotCore.Models
{
    public class TwitchChatterType
    {
        public List<TwitchChatter> TwitchChatters { get; set; }
        public ChatterType ChatterType { get; set; }
    }
}
