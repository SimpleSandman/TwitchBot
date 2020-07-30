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

        public JoinStreamerFeature(IrcClient irc, TwitchBotConfigurationSection botConfig, TwitchInfoService twitchInfo, 
            GameDirectoryService gameDirectory) : base(irc, botConfig)
        {
            _twitchInfo = twitchInfo;
            _gameDirectory = gameDirectory;
            _rolePermissions.Add("!resetjoin", new CommandPermission { General = ChatterType.Moderator });
            _rolePermissions.Add("!listjoin", new CommandPermission { General = ChatterType.Viewer });
            _rolePermissions.Add("!invite", new CommandPermission { General = ChatterType.Viewer });
            _rolePermissions.Add("!popjoin", new CommandPermission { General = ChatterType.VIP });
        }

        public override async Task<(bool, DateTime)> ExecCommandAsync(TwitchChatter chatter, string requestedCommand)
        {
            try
            {
                switch (requestedCommand)
                {
                    case "!resetjoin":
                        return (true, await ResetJoinAsync(chatter));
                    case "!listjoin":
                        return (true, await ListJoinAsync(chatter));
                    case "!invite":
                        return (true, await InviteAsync(chatter));
                    case "!popjoin":
                        return (true, await PopJoinAsync(chatter));
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "JoinStreamerFeature", "ExecCommand(TwitchChatter, string)", false, requestedCommand, chatter.Message);
            }

            return (true, DateTime.Now);
        }

        private async Task<DateTime> ResetJoinAsync(TwitchChatter chatter)
        {
            try
            {
                _joinStreamerInstance.ResetList();
                _irc.SendPublicChatMessage($"Queue is empty @{chatter.DisplayName}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "JoinStreamerFeature", "ResetJoin(TwitchChatter)", false, "!resetjoin");
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
                await _errHndlrInstance.LogError(ex, "JoinStreamerFeature", "ListJoin(TwitchChatter)", false, "!listjoin");
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
                await _errHndlrInstance.LogError(ex, "JoinStreamerFeature", "Invite(TwitchChatter)", false, "!join");
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
                await _errHndlrInstance.LogError(ex, "JoinStreamerFeature", "PopJoin(TwitchChatter)", false, "!popjoin");
            }

            return DateTime.Now;
        }

        #region Private Methods
        private async Task<bool> IsMultiplayerGameAsync(string username)
        {
            // Get current game name
            ChannelJSON json = await _twitchInfo.GetBroadcasterChannelByIdAsync();
            string gameTitle = json.Game;

            // Grab game id in order to find party member
            TwitchGameCategory game = await _gameDirectory.GetGameIdAsync(gameTitle);

            if (string.IsNullOrEmpty(gameTitle))
            {
                _irc.SendPublicChatMessage("I cannot see the name of the game. It's currently set to either NULL or EMPTY. "
                    + "Please have the chat verify that the game has been set for this stream. "
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

            return true;
        }
        #endregion Private Methods
    }
}
