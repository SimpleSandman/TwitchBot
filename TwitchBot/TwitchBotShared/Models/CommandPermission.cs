using TwitchBotShared.Enums;

namespace TwitchBotShared.Models
{
    public class CommandPermission
    {
        public ChatterType General { get; set; }
        public ChatterType? Elevated { get; set; }
    }
}
