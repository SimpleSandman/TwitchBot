using TwitchBotCore.Enums;

namespace TwitchBotCore.Models
{
    public partial class CommandPermission
    {
        public ChatterType General { get; set; }
        public ChatterType? Elevated { get; set; }
    }
}
