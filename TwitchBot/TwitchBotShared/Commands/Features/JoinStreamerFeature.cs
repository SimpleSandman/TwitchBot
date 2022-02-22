using System;
using System.Threading.Tasks;

using TwitchBotDb.Models;
using TwitchBotDb.Services;

using TwitchBotShared.ClientLibraries;
using TwitchBotShared.ClientLibraries.Singletons;
using TwitchBotShared.Config;
using TwitchBotShared.Enums;
using TwitchBotShared.Models;
using TwitchBotShared.Models.JSON;

namespace TwitchBotShared.Commands.Features
{
    /// <summary>
    /// The "Command Subsystem" for the "Join Streamer" feature
    /// </summary>
    public sealed class JoinStreamerFeature : BaseFeature
    {
        private readonly TwitchInfoService _twitchInfo;
        private readonly GameDirectoryService _gameDirectory;
        private readonly JoinStreamerSingleton _joinStreamerInstance = JoinStreamerSingleton.Instance;
        private readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        private const string RESET_JOIN = "!resetjoin";
        private const string LIST_JOIN = "!listjoin";
        private const string INVITE = "!invite";
        private const string POP_JOIN = "!popjoin";

        public JoinStreamerFeature(IrcClient irc, TwitchBotConfigurationSection botConfig, TwitchInfoService twitchInfo, 
            GameDirectoryService gameDirectory) : base(irc, botConfig)
        {
            _twitchInfo = twitchInfo;
            _gameDirectory = gameDirectory;
            _rolePermissions.Add(RESET_JOIN, new CommandPermission { General = ChatterType.Moderator });
            _rolePermissions.Add(LIST_JOIN, new CommandPermission { General = ChatterType.Viewer });
            _rolePermissions.Add(INVITE, new CommandPermission { General = ChatterType.Viewer });
            _rolePermissions.Add(POP_JOIN, new CommandPermission { General = ChatterType.VIP });
        }

        public override async Task<(bool, DateTime)> ExecCommandAsync(TwitchChatter chatter, string requestedCommand)
        {
            try
            {
                switch (requestedCommand)
                {
                    case RESET_JOIN:
                        return (true, await ResetJoinAsync(chatter));
                    case LIST_JOIN:
                        return (true, await ListJoinAsync(chatter));
                    case INVITE:
                        return (true, await InviteAsync(chatter));
                    case POP_JOIN:
                        return (true, await PopJoinAsync(chatter));
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "JoinStreamerFeature", "ExecCommandAsync(TwitchChatter, string)", false, requestedCommand, chatter.Message);
            }

            return (true, DateTime.Now);
        }

        #region Private Methods
        private async Task<DateTime> ResetJoinAsync(TwitchChatter chatter)
        {
            try
            {
                _joinStreamerInstance.ResetList();
                _irc.SendPublicChatMessage($"Queue is empty @{chatter.DisplayName}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "JoinStreamerFeature", "ResetJoinAsync(TwitchChatter)", false, RESET_JOIN);
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Show a list of users that are queued to play with the broadcaster
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        /// <param name="gameQueueUsers">List of users that are queued to play with the broadcaster</param>
        private async Task<DateTime> ListJoinAsync(TwitchChatter chatter)
        {
            try
            {
                if (!await IsMultiplayerGameAsync(chatter.Username))
                {
                    return DateTime.Now;
                }

                string message = _joinStreamerInstance.ListJoin();

                _irc.SendPublicChatMessage(message.Remove(message.Length - 2));
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "JoinStreamerFeature", "ListJoinAsync(TwitchChatter)", false, LIST_JOIN);
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Add a user to the queue of users that want to play with the broadcaster
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        private async Task<DateTime> InviteAsync(TwitchChatter chatter)
        {
            try
            {
                if (await IsMultiplayerGameAsync(chatter.Username))
                {
                    _joinStreamerInstance.Invite(chatter);
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "JoinStreamerFeature", "InviteAsync(TwitchChatter)", false, INVITE);
            }

            return DateTime.Now;
        }

        private async Task<DateTime> PopJoinAsync(TwitchChatter chatter)
        {
            try
            {
                _joinStreamerInstance.PopJoin(chatter);
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "JoinStreamerFeature", "PopJoinAsync(TwitchChatter)", false, POP_JOIN);
            }

            return DateTime.Now;
        }

        private async Task<bool> IsMultiplayerGameAsync(string username)
        {
            try
            {
                // Get current game name
                ChannelJSON json = await _twitchInfo.GetBroadcasterChannelByIdAsync();
                string gameTitle = json.GameName;

                // Grab game id in order to find party member
                TwitchGameCategory game = await _gameDirectory.GetGameIdAsync(gameTitle);

                if (string.IsNullOrEmpty(gameTitle))
                {
                    _irc.SendPublicChatMessage("I cannot see the name of the game. It's currently set to either NULL or EMPTY. "
                        + "Please have the chat verify that the game has been set for this stream or if this is intentional. "
                        + $"If the error persists, please have @{_botConfig.Broadcaster.ToLower()} retype the game in their Twitch Live Dashboard. "
                        + "If this error shows up again and your chat can see the game set for the stream, please contact my master with !support in this chat");
                    return false;
                }
                else if (game == null || game.Id == 0)
                {
                    _irc.SendPublicChatMessage($"I cannot find the game, \"{gameTitle.TrimEnd()}\", in the database. "
                        + $"Have my master resolve this issue by typing !support in this chat @{username}");
                    return false;
                }

                if (!game.Multiplayer)
                {
                    _irc.SendPublicChatMessage("This game is set to single-player only. "
                        + $"Contact my master with !support in this chat if this isn't correct @{username}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "JoinStreamerFeature", "IsMultiplayerGameAsync(string)", false);
            }

            return true;
        }
        #endregion
    }
}
