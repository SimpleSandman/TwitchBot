using System;
using System.Net;
using System.Threading.Tasks;

using RestSharp;

using TwitchBot.Configuration;
using TwitchBot.Enums;
using TwitchBot.Libraries;
using TwitchBot.Models;
using TwitchBot.Services;
using TwitchBot.Threads;
using TwitchBotDb.Models;

namespace TwitchBot.Commands.Features
{
    /// <summary>
    /// The "Command Subsystem" for the "Twitch Channel" feature
    /// </summary>
    public sealed class TwitchChannelFeature : BaseFeature
    {
        private readonly GameDirectoryService _gameDirectory;
        private readonly BossFightSingleton _bossFightSettingsInstance = BossFightSingleton.Instance;
        private readonly BroadcasterSingleton _broadcasterInstance = BroadcasterSingleton.Instance;
        private readonly CustomCommandSingleton _customCommandInstance = CustomCommandSingleton.Instance;
        private readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        public TwitchChannelFeature(IrcClient irc, TwitchBotConfigurationSection botConfig, GameDirectoryService gameDirectory) : base(irc, botConfig)
        {
            _gameDirectory = gameDirectory;
            _rolePermission.Add("!game", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!title", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!updategame", new CommandPermission { General = ChatterType.Moderator });
            _rolePermission.Add("!updatetitle", new CommandPermission { General = ChatterType.Moderator });
        }

        public override async Task<(bool, DateTime)> ExecCommand(TwitchChatter chatter, string requestedCommand)
        {
            try
            {
                switch (requestedCommand)
                {
                    case "!updategame":
                    case "!game":
                        if ((chatter.Message.StartsWith("!game ") || chatter.Message.StartsWith("!updategame ")) 
                            && HasPermission("!updategame", DetermineChatterPermissions(chatter), _rolePermission))
                        {
                            return (true, await UpdateGame(chatter));
                        }
                        else if (chatter.Message == "!game")
                        {
                            return (true, await ShowCurrentTwitchGame(chatter));
                        }
                        break;
                    case "!updatetitle":
                    case "!title":
                        if ((chatter.Message.StartsWith("!title ") || chatter.Message.StartsWith("!updatetitle ")) 
                            && HasPermission("!updatetitle", DetermineChatterPermissions(chatter), _rolePermission))
                        {
                            return (true, await UpdateTitle(chatter));
                        }
                        else if (chatter.Message == "!title")
                        {
                            return (true, await ShowCurrentTwitchTitle(chatter));
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "TwitchChannelFeature", "ExecCommand(TwitchChatter, string)", false, requestedCommand, chatter.Message);
            }

            return (false, DateTime.Now);
        }

        /// <summary>
        /// Display the current game/category for the Twitch channel
        /// </summary>
        /// <param name="chatter"></param>
        /// <returns></returns>
        public async Task<DateTime> ShowCurrentTwitchGame(TwitchChatter chatter)
        {
            try
            {
                _irc.SendPublicChatMessage($"We're currently playing \"{TwitchStreamStatus.CurrentCategory}\" @{chatter.DisplayName}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "TwitchChannelFeature", "ShowCurrentTwitchGame(TwitchChatter)", false, "!game");
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Display the current title for the Twitch channel
        /// </summary>
        /// <param name="chatter"></param>
        /// <returns></returns>
        public async Task<DateTime> ShowCurrentTwitchTitle(TwitchChatter chatter)
        {
            try
            {
                _irc.SendPublicChatMessage($"The title of this stream is \"{TwitchStreamStatus.CurrentTitle}\" @{chatter.DisplayName}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "TwitchChannelFeature", "ShowCurrentTwitchTitle(TwitchChatter)", false, "!title");
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Update the title of the Twitch channel
        /// </summary>
        /// <param name="chatter"></param>
        public async Task<DateTime> UpdateTitle(TwitchChatter chatter)
        {
            try
            {
                // Get title from command parameter
                string title = chatter.Message.Substring(chatter.Message.IndexOf(" ") + 1);

                // Send HTTP method PUT to base URI in order to change the title
                RestClient client = new RestClient("https://api.twitch.tv/kraken/channels/" + _broadcasterInstance.TwitchId);
                RestRequest request = new RestRequest(Method.PUT);
                request.AddHeader("Cache-Control", "no-cache");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", "OAuth " + _botConfig.TwitchAccessToken);
                request.AddHeader("Accept", "application/vnd.twitchtv.v5+json");
                request.AddHeader("Client-ID", _botConfig.TwitchClientId);
                request.AddParameter("application/json", "{\"channel\":{\"status\":\"" + title + "\"}}",
                    ParameterType.RequestBody);

                IRestResponse response = null;
                try
                {
                    response = await client.ExecuteAsync<Task>(request);
                    string statResponse = response.StatusCode.ToString();
                    if (statResponse.Contains("OK"))
                    {
                        _irc.SendPublicChatMessage($"Twitch channel title updated to \"{title}\"");
                    }
                    else
                        Console.WriteLine(response.ErrorMessage);
                }
                catch (WebException ex)
                {
                    if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.BadRequest)
                    {
                        Console.WriteLine("Error 400 detected!");
                    }
                    response = (IRestResponse)ex.Response;
                    Console.WriteLine("Error: " + response);
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "TwitchChannelFeature", "UpdateTitle(TwitchChatter)", false, "!updatetitle");
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Updates the game being played on the Twitch channel
        /// </summary>
        /// <param name="chatter"></param>
        public async Task<DateTime> UpdateGame(TwitchChatter chatter)
        {
            try
            {
                // Get game from command parameter
                string gameTitle = chatter.Message.Substring(chatter.Message.IndexOf(" ") + 1);

                // Send HTTP method PUT to base URI in order to change the game
                RestClient client = new RestClient("https://api.twitch.tv/kraken/channels/" + _broadcasterInstance.TwitchId);
                RestRequest request = new RestRequest(Method.PUT);
                request.AddHeader("Cache-Control", "no-cache");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", "OAuth " + _botConfig.TwitchAccessToken);
                request.AddHeader("Accept", "application/vnd.twitchtv.v5+json");
                request.AddHeader("Client-ID", _botConfig.TwitchClientId);
                request.AddParameter("application/json", "{\"channel\":{\"game\":\"" + gameTitle + "\"}}",
                    ParameterType.RequestBody);

                IRestResponse response = null;
                try
                {
                    response = await client.ExecuteAsync<Task>(request);
                    string statResponse = response.StatusCode.ToString();
                    if (statResponse.Contains("OK"))
                    {
                        _irc.SendPublicChatMessage($"Twitch channel game status updated to \"{gameTitle}\"");

                        await ChatReminder.RefreshReminders();
                        await _customCommandInstance.LoadCustomCommands(_botConfig.TwitchBotApiLink, _broadcasterInstance.DatabaseId);
                        _irc.SendPublicChatMessage($"Your commands have been refreshed @{chatter.DisplayName}");

                        // Grab game id in order to find party member
                        TwitchGameCategory game = await _gameDirectory.GetGameId(gameTitle);

                        // During refresh, make sure no fighters can join
                        _bossFightSettingsInstance.RefreshBossFight = true;
                        await _bossFightSettingsInstance.LoadSettings(_broadcasterInstance.DatabaseId, game?.Id, _botConfig.TwitchBotApiLink);
                        _bossFightSettingsInstance.RefreshBossFight = false;
                    }
                    else
                    {
                        Console.WriteLine(response.Content);
                    }
                }
                catch (WebException ex)
                {
                    if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.BadRequest)
                    {
                        Console.WriteLine("Error 400 detected!!");
                    }
                    response = (IRestResponse)ex.Response;
                    Console.WriteLine("Error: " + response);
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "TwitchChannelFeature", "UpdateGame(TwitchChatter)", false, "!updategame");
            }

            return DateTime.Now;
        }
    }
}
