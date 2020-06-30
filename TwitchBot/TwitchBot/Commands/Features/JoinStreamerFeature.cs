using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TwitchBot.Configuration;
using TwitchBot.Enums;
using TwitchBot.Libraries;
using TwitchBot.Models;
using TwitchBot.Models.JSON;
using TwitchBot.Services;

using TwitchBotDb.Models;

namespace TwitchBot.Commands.Features
{
    /// <summary>
    /// The "Command Subsystem" for the "Join Streamer" feature
    /// </summary>
    public sealed class JoinStreamerFeature : BaseFeature
    {
        private readonly TwitchInfoService _twitchInfo;
        private readonly GameDirectoryService _gameDirectory;
        private readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        public JoinStreamerFeature(IrcClient irc, TwitchBotConfigurationSection botConfig, TwitchInfoService twitchInfo, 
            GameDirectoryService gameDirectory) : base(irc, botConfig)
        {
            _twitchInfo = twitchInfo;
            _gameDirectory = gameDirectory;
            _rolePermission.Add("!", new List<ChatterType> { ChatterType.Viewer });
        }

        public override async Task<(bool, DateTime)> ExecCommand(TwitchChatter chatter, string requestedCommand)
        {
            try
            {
                switch (requestedCommand)
                {
                    case "!":
                        //return (true, await SomethingCool(chatter));
                    default:
                        if (requestedCommand == "!")
                        {
                            //return (true, await OtherCoolThings(chatter));
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "JoinStreamerFeature", "ExecCommand(TwitchChatter, string)", false, requestedCommand, chatter.Message);
            }

            return (true, DateTime.Now);
        }

        public async Task<Queue<string>> CmdResetJoin(TwitchChatter chatter, Queue<string> gameQueueUsers)
        {
            try
            {
                if (gameQueueUsers.Count != 0)
                    gameQueueUsers.Clear();

                _irc.SendPublicChatMessage($"Queue is empty @{chatter.DisplayName}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "PartyUpFeature", "CmdResetJoin(TwitchChatter, Queue<string>)", false, "!resetjoin");
            }

            return gameQueueUsers;
        }

        /// <summary>
        /// Show a list of users that are queued to play with the broadcaster
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        /// <param name="gameQueueUsers">List of users that are queued to play with the broadcaster</param>
        public async Task CmdListJoin(TwitchChatter chatter, Queue<string> gameQueueUsers)
        {
            try
            {
                if (!await IsMultiplayerGame(chatter.Username)) return;

                if (gameQueueUsers.Count == 0)
                {
                    _irc.SendPublicChatMessage($"No one wants to play with the streamer at the moment. "
                        + "Be the first to play with !join");
                    return;
                }

                // Show list of queued users
                string message = $"List of users waiting to play with the streamer (in order from left to right): < ";

                foreach (string user in gameQueueUsers)
                {
                    message += user + " >< ";
                }

                _irc.SendPublicChatMessage(message.Remove(message.Length - 2));
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdGen", "CmdListJoin(TwitchChatter, Queue<string>)", false, "!listjoin");
            }
        }

        /// <summary>
        /// Add a user to the queue of users that want to play with the broadcaster
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        /// <param name="gameQueueUsers">List of users that are queued to play with the broadcaster</param>
        public async Task<Queue<string>> CmdInvite(TwitchChatter chatter, Queue<string> gameQueueUsers)
        {
            try
            {
                if (gameQueueUsers.Contains(chatter.Username))
                {
                    _irc.SendPublicChatMessage($"Don't worry @{chatter.DisplayName}. You're on the list to play with " +
                        $"the streamer with your current position at {gameQueueUsers.ToList().IndexOf(chatter.Username) + 1} " +
                        $"of {gameQueueUsers.Count} user(s)");
                }
                else if (await IsMultiplayerGame(chatter.Username))
                {
                    gameQueueUsers.Enqueue(chatter.Username);

                    _irc.SendPublicChatMessage($"Congrats @{chatter.DisplayName}! You're currently in line with your current position at " +
                        $"{gameQueueUsers.ToList().IndexOf(chatter.Username) + 1}");
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdGen", "CmdJoin(TwitchChatter, Queue<string>)", false, "!join");
            }

            return gameQueueUsers;
        }

        public async Task<Queue<string>> CmdPopJoin(TwitchChatter chatter, Queue<string> gameQueueUsers)
        {
            try
            {
                if (gameQueueUsers.Count == 0)
                    _irc.SendPublicChatMessage($"Queue is empty @{chatter.DisplayName}");
                else
                {
                    string poppedUser = gameQueueUsers.Dequeue();
                    _irc.SendPublicChatMessage($"{poppedUser} has been removed from the queue @{chatter.DisplayName}");
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdVip", "CmdPopJoin(TwitchChatter, Queue<string>)", false, "!popjoin");
            }

            return gameQueueUsers;
        }

        private async Task<bool> IsMultiplayerGame(string username)
        {
            // Get current game name
            ChannelJSON json = await _twitchInfo.GetBroadcasterChannelById();
            string gameTitle = json.Game;

            // Grab game id in order to find party member
            TwitchGameCategory game = await _gameDirectory.GetGameId(gameTitle);

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
                _irc.SendPublicChatMessage($"I cannot find the game, \"{gameTitle}\", in the database. "
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
    }
}
