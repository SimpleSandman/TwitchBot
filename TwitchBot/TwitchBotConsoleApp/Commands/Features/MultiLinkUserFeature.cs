using System;
using System.Threading.Tasks;

using TwitchBotConsoleApp.Libraries;

using TwitchBotUtil.Config;
using TwitchBotUtil.Enums;
using TwitchBotUtil.Models;

namespace TwitchBotConsoleApp.Commands.Features
{
    /// <summary>
    /// The "Command Subsystem" for the "MultiLink User" feature
    /// </summary>
    public sealed class MultiLinkUserFeature : BaseFeature
    {
        private readonly MultiLinkUserSingleton _multiLinkUser = MultiLinkUserSingleton.Instance;
        private readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        public MultiLinkUserFeature(IrcClient irc, TwitchBotConfigurationSection botConfig) : base(irc, botConfig)
        {
            _rolePermission.Add("!msl", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!addmsl", new CommandPermission { General = ChatterType.VIP });
            _rolePermission.Add("!resetmsl", new CommandPermission { General = ChatterType.VIP });
        }

        public override async Task<(bool, DateTime)> ExecCommand(TwitchChatter chatter, string requestedCommand)
        {
            try
            {
                switch (requestedCommand)
                {
                    case "!msl":
                    case "!addmsl":
                        if ((chatter.Message.StartsWith("!msl ") || chatter.Message.StartsWith("!addmsl "))
                            && HasPermission("!addmsl", DetermineChatterPermissions(chatter), _rolePermission))
                        {
                            return (true, await AddUser(chatter));
                        }
                        else if (chatter.Message == "!msl")
                        {
                            return (true, await ShowLink(chatter));
                        }
                        break;
                    case "!resetmsl":
                        return (true, await ResetLink(chatter));
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "MultiLinkUserFeature", "ExecCommand(TwitchChatter, string)", false, requestedCommand, chatter.Message);
            }

            return (false, DateTime.Now);
        }

        /// <summary>
        /// Displays MultiStream link so multiple streamers can be watched at once
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        private async Task<DateTime> ShowLink(TwitchChatter chatter)
        {
            try
            {
                _irc.SendPublicChatMessage(_multiLinkUser.ShowLink(chatter, _botConfig.Broadcaster.ToLower()));
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "MultiLinkUserFeature", "ShowLink(TwitchChatter)", false, "!msl");
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Add user(s) to a MultiStream link so viewers can watch multiple streamers at the same time
        /// </summary>
        /// <param name="chatter"></param>
        private async Task<DateTime> AddUser(TwitchChatter chatter)
        {
            try
            {
                _irc.SendPublicChatMessage(_multiLinkUser.AddUser(chatter, _botConfig.Broadcaster, _botConfig.BotName));
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "MultiLinkUserFeature", "AddUser(TwitchChatter)", false, "!addmsl", chatter.Message);
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Reset the MultiStream link to allow the link to be reconfigured
        /// </summary>
        /// <param name="chatter"></param>
        private async Task<DateTime> ResetLink(TwitchChatter chatter)
        {
            try
            {
                _multiLinkUser.ResetMultiLink();

                _irc.SendPublicChatMessage("MultiStream link has been reset. " +
                    $"Please reconfigure the link if you are planning on using it in the near future @{chatter.DisplayName}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "MultiLinkUserFeature", "ResetLink(TwitchChatter)", false, "!resetmsl");
            }

            return DateTime.Now;
        }
    }
}
