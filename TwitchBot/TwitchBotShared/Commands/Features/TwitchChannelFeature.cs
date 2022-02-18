using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using RestSharp;

using TwitchBotDb.Models;
using TwitchBotDb.Services;

using TwitchBotShared.ClientLibraries;
using TwitchBotShared.ClientLibraries.Singletons;
using TwitchBotShared.Config;
using TwitchBotShared.Enums;
using TwitchBotShared.Models;
using TwitchBotShared.Models.JSON;
using TwitchBotShared.Threads;

namespace TwitchBotShared.Commands.Features
{
    /// <summary>
    /// The "Command Subsystem" for the "Twitch Channel" feature
    /// </summary>
    public sealed class TwitchChannelFeature : BaseFeature
    {
        private readonly GameDirectoryService _gameDirectory;
        private readonly TwitchInfoService _twitchInfo;

        private readonly BossFightSingleton _bossFightSettingsInstance = BossFightSingleton.Instance;
        private readonly BroadcasterSingleton _broadcasterInstance = BroadcasterSingleton.Instance;
        private readonly CustomCommandSingleton _customCommandInstance = CustomCommandSingleton.Instance;
        private readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        public TwitchChannelFeature(IrcClient irc, TwitchBotConfigurationSection botConfig, GameDirectoryService gameDirectory,
            TwitchInfoService twitchInfo) : base(irc, botConfig)
        {
            _twitchInfo = twitchInfo;
            _gameDirectory = gameDirectory;
            _rolePermissions.Add("!game", new CommandPermission { General = ChatterType.Viewer });
            _rolePermissions.Add("!title", new CommandPermission { General = ChatterType.Viewer });
            _rolePermissions.Add("!updategame", new CommandPermission { General = ChatterType.Moderator });
            _rolePermissions.Add("!updatetitle", new CommandPermission { General = ChatterType.Moderator });
        }

        public override async Task<(bool, DateTime)> ExecCommandAsync(TwitchChatter chatter, string requestedCommand)
        {
            try
            {
                switch (requestedCommand)
                {
                    case "!updategame":
                    case "!game":
                        if ((chatter.Message.StartsWith("!game ") || chatter.Message.StartsWith("!updategame ")) 
                            && HasPermission("!updategame", DetermineChatterPermissions(chatter), _rolePermissions))
                        {
                            return (true, await UpdateGameAsync(chatter));
                        }
                        else if (chatter.Message == "!game")
                        {
                            return (true, await ShowCurrentTwitchGameAsync(chatter));
                        }
                        break;
                    case "!updatetitle":
                    case "!title":
                        if ((chatter.Message.StartsWith("!title ") || chatter.Message.StartsWith("!updatetitle ")) 
                            && HasPermission("!updatetitle", DetermineChatterPermissions(chatter), _rolePermissions))
                        {
                            return (true, await UpdateTitleAsync(chatter));
                        }
                        else if (chatter.Message == "!title")
                        {
                            return (true, await ShowCurrentTwitchTitleAsync(chatter));
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "TwitchChannelFeature", "ExecCommandAsync(TwitchChatter, string)", false, requestedCommand, chatter.Message);
            }

            return (false, DateTime.Now);
        }

        /// <summary>
        /// Display the current game/category for the Twitch channel
        /// </summary>
        /// <param name="chatter"></param>
        /// <returns></returns>
        private async Task<DateTime> ShowCurrentTwitchGameAsync(TwitchChatter chatter)
        {
            try
            {
                _irc.SendPublicChatMessage($"We're currently playing \"{TwitchStreamStatus.CurrentCategory}\" @{chatter.DisplayName}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "TwitchChannelFeature", "ShowCurrentTwitchGameAsync(TwitchChatter)", false, "!game");
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Display the current title for the Twitch channel
        /// </summary>
        /// <param name="chatter"></param>
        /// <returns></returns>
        private async Task<DateTime> ShowCurrentTwitchTitleAsync(TwitchChatter chatter)
        {
            try
            {
                _irc.SendPublicChatMessage($"The title of this stream is \"{TwitchStreamStatus.CurrentTitle}\" @{chatter.DisplayName}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "TwitchChannelFeature", "ShowCurrentTwitchTitleAsync(TwitchChatter)", false, "!title");
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Update the stream title of the Twitch channel
        /// </summary>
        /// <param name="chatter"></param>
        private async Task<DateTime> UpdateTitleAsync(TwitchChatter chatter)
        {
            try
            {
                // Get title from command parameter
                string streamTitle = chatter.Message.Substring(chatter.Message.IndexOf(" ") + 1);

                await _twitchInfo.UpdateChannelInfoAsync(new ChannelUpdateJSON { Title = streamTitle });

                _irc.SendPublicChatMessage($"Twitch channel title updated to \"{streamTitle}\"");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "TwitchChannelFeature", "UpdateTitleAsync(TwitchChatter)", false, "!updatetitle");
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Updates the game being played on the Twitch channel
        /// </summary>
        /// <param name="chatter"></param>
        private async Task<DateTime> UpdateGameAsync(TwitchChatter chatter)
        {
            try
            {
                // Get game from command parameter
                string gameTitle = chatter.Message.Substring(chatter.Message.IndexOf(" ") + 1);

                RootGameJSON gameJson = await _twitchInfo.GetGameInfoAsync(WebUtility.HtmlEncode(gameTitle));
                if (gameJson == null || gameJson.Games.Count == 0)
                {
                    // TODO: Give invalid response
                    return DateTime.Now;
                }
                else if (gameJson.Games.Count > 1)
                {
                    // TODO: Give "too many games found on Twitch" response.
                    //       List possible games.
                    return DateTime.Now;
                }

                await _twitchInfo.UpdateChannelInfoAsync(new ChannelUpdateJSON { GameId = gameJson.Games.First().Id });

                _irc.SendPublicChatMessage($"Twitch channel game status updated to \"{gameTitle}\"");

                await ChatReminder.RefreshRemindersAsync();
                await _customCommandInstance.LoadCustomCommands(_botConfig.TwitchBotApiLink, _broadcasterInstance.DatabaseId);
                _irc.SendPublicChatMessage($"Your commands have been refreshed @{chatter.DisplayName}");

                // Grab game id in order to find party member
                TwitchGameCategory game = await _gameDirectory.GetGameIdAsync(gameTitle);

                // During refresh, make sure no fighters can join
                _bossFightSettingsInstance.RefreshBossFight = true;
                await _bossFightSettingsInstance.LoadSettings(_broadcasterInstance.DatabaseId, game?.Id, _botConfig.TwitchBotApiLink);
                _bossFightSettingsInstance.RefreshBossFight = false;
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "TwitchChannelFeature", "UpdateGameAsync(TwitchChatter)", false, "!updategame");
            }

            return DateTime.Now;
        }
    }
}
