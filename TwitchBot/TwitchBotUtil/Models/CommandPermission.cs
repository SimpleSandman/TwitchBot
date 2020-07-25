using TwitchBotUtil.Enums;

namespace TwitchBotUtil.Models
{
    public partial class CommandPermission
    {
        public ChatterType General { get; set; }
        public ChatterType? Elevated { get; set; }
    }
}
