using System.Collections.Generic;

using TwitchBotConsoleApp.Enums;

namespace TwitchBotConsoleApp.Models
{
    public class TwitchChatterType
    {
        public List<TwitchChatter> TwitchChatters { get; set; }
        public ChatterType ChatterType { get; set; }
    }
}
