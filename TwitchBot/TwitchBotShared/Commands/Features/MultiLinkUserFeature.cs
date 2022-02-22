using System;
using System.Threading.Tasks;

using TwitchBotShared.ClientLibraries;
using TwitchBotShared.ClientLibraries.Singletons;
using TwitchBotShared.Config;
using TwitchBotShared.Enums;
using TwitchBotShared.Models;

namespace TwitchBotShared.Commands.Features
{
    /// <summary>
    /// The "Command Subsystem" for the "MultiLink User" feature
    /// </summary>
    public sealed class MultiLinkUserFeature : BaseFeature
    {
        private readonly MultiLinkUserSingleton _multiLinkUser = MultiLinkUserSingleton.Instance;
        private readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        private const string MSL = "!msl";
        private const string ADD_MSL = "!addmsl";
        private const string RESET_MSL = "!resetmsl";

        public MultiLinkUserFeature(IrcClient irc, TwitchBotConfigurationSection botConfig) : base(irc, botConfig)
        {
            _rolePermissions.Add(MSL, new CommandPermission { General = ChatterType.Viewer });
            _rolePermissions.Add(ADD_MSL, new CommandPermission { General = ChatterType.VIP });
            _rolePermissions.Add(RESET_MSL, new CommandPermission { General = ChatterType.VIP });
        }

        public override async Task<(bool, DateTime)> ExecCommandAsync(TwitchChatter chatter, string requestedCommand)
        {
            try
            {
                switch (requestedCommand)
                {
                    case MSL:
                    case ADD_MSL:
                        if ((chatter.Message.StartsWith($"{MSL} ") || chatter.Message.StartsWith($"{ADD_MSL} "))
                            && HasPermission(ADD_MSL, DetermineChatterPermissions(chatter), _rolePermissions))
                        {
                            return (true, await AddUsersAsync(chatter));
                        }
                        else if (chatter.Message == MSL)
                        {
                            return (true, await ShowLinkAsync(chatter));
                        }
                        break;
                    case RESET_MSL:
                        return (true, await ResetLinkAsync(chatter));
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "MultiLinkUserFeature", "ExecCommandAsync(TwitchChatter, string)", false, requestedCommand, chatter.Message);
            }

            return (false, DateTime.Now);
        }

        #region Private Methods
        /// <summary>
        /// Displays MultiStream link so multiple streamers can be watched at once
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        private async Task<DateTime> ShowLinkAsync(TwitchChatter chatter)
        {
            try
            {
                _irc.SendPublicChatMessage(_multiLinkUser.ShowLink(chatter, _botConfig.Broadcaster.ToLower()));
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "MultiLinkUserFeature", "ShowLinkAsync(TwitchChatter)", false, MSL);
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Add user(s) to a MultiStream link so viewers can watch multiple streamers at the same time
        /// </summary>
        /// <param name="chatter"></param>
        private async Task<DateTime> AddUsersAsync(TwitchChatter chatter)
        {
            try
            {
                _irc.SendPublicChatMessage(_multiLinkUser.AddUser(chatter, _botConfig.Broadcaster, _botConfig.BotName));
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "MultiLinkUserFeature", "AddUsersAsync(TwitchChatter)", false, ADD_MSL, chatter.Message);
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Reset the MultiStream link to allow the link to be reconfigured
        /// </summary>
        /// <param name="chatter"></param>
        private async Task<DateTime> ResetLinkAsync(TwitchChatter chatter)
        {
            try
            {
                _multiLinkUser.ResetMultiLink();

                _irc.SendPublicChatMessage("MultiStream link has been reset. " +
                    $"Please reconfigure the link if you are planning on using it in the near future @{chatter.DisplayName}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "MultiLinkUserFeature", "ResetLinkAsync(TwitchChatter)", false, RESET_MSL);
            }

            return DateTime.Now;
        }
        #endregion
    }
}
