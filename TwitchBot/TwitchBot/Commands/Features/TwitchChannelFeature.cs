using System;
using System.Net;
using System.Threading.Tasks;

using RestSharp;

using TwitchBot.Configuration;
using TwitchBot.Libraries;
using TwitchBot.Models;
using TwitchBot.Threads;

namespace TwitchBot.Commands.Features
{
    /// <summary>
    /// The "Command Subsystem" for the "Twitch Channel" feature
    /// </summary>
    public sealed class TwitchChannelFeature : BaseFeature
    {
        private readonly BroadcasterSingleton _broadcasterInstance = BroadcasterSingleton.Instance;
        private readonly CustomCommandSingleton _customCommandInstance = CustomCommandSingleton.Instance;
        private readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        public TwitchChannelFeature(IrcClient irc, TwitchBotConfigurationSection botConfig) : base(irc, botConfig)
        {
            _rolePermission.Add("!game", "");
            _rolePermission.Add("!title", "");
            _rolePermission.Add("!updategame", "moderator");
            _rolePermission.Add("!updatetitle", "moderator");
        }

        public override async void ExecCommand(TwitchChatter chatter, string requestedCommand)
        {
            try
            {
                switch (requestedCommand)
                {
                    case "!game":
                        ShowCurrentTwitchGame(chatter);
                        break;
                    case "!title":
                        ShowCurrentTwitchTitle(chatter);
                        break;
                    case "!updategame":
                        await UpdateGame(chatter);
                        break;
                    case "!updatetitle":
                        await UpdateTitle(chatter);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "TwitchChannelFeature", "ExecCommand(TwitchChatter, string)", false, requestedCommand, chatter.Message);
            }
        }

        /// <summary>
        /// Display the current game/category for the Twitch channel
        /// </summary>
        /// <param name="chatter"></param>
        /// <returns></returns>
        public async void ShowCurrentTwitchGame(TwitchChatter chatter)
        {
            try
            {
                _irc.SendPublicChatMessage($"We're currently playing \"{TwitchStreamStatus.CurrentCategory}\" @{chatter.DisplayName}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "TwitchChannelFeature", "ShowCurrentTwitchGame(TwitchChatter)", false, "!game");
            }
        }

        /// <summary>
        /// Display the current title for the Twitch channel
        /// </summary>
        /// <param name="chatter"></param>
        /// <returns></returns>
        public async void ShowCurrentTwitchTitle(TwitchChatter chatter)
        {
            try
            {
                _irc.SendPublicChatMessage($"The title of this stream is \"{TwitchStreamStatus.CurrentTitle}\" @{chatter.DisplayName}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "TwitchChannelFeature", "ShowCurrentTwitchTitle(TwitchChatter)", false, "!title");
            }
        }

        /// <summary>
        /// Update the title of the Twitch channel
        /// </summary>
        /// <param name="chatter"></param>
        public async Task UpdateTitle(TwitchChatter chatter)
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
        }

        /// <summary>
        /// Updates the game being played on the Twitch channel
        /// </summary>
        /// <param name="chatter"></param>
        public async Task UpdateGame(TwitchChatter chatter)
        {
            try
            {
                // Get game from command parameter
                string game = chatter.Message.Substring(chatter.Message.IndexOf(" ") + 1);

                // Send HTTP method PUT to base URI in order to change the game
                RestClient client = new RestClient("https://api.twitch.tv/kraken/channels/" + _broadcasterInstance.TwitchId);
                RestRequest request = new RestRequest(Method.PUT);
                request.AddHeader("Cache-Control", "no-cache");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", "OAuth " + _botConfig.TwitchAccessToken);
                request.AddHeader("Accept", "application/vnd.twitchtv.v5+json");
                request.AddHeader("Client-ID", _botConfig.TwitchClientId);
                request.AddParameter("application/json", "{\"channel\":{\"game\":\"" + game + "\"}}",
                    ParameterType.RequestBody);

                IRestResponse response = null;
                try
                {
                    response = await client.ExecuteAsync<Task>(request);
                    string statResponse = response.StatusCode.ToString();
                    if (statResponse.Contains("OK"))
                    {
                        _irc.SendPublicChatMessage($"Twitch channel game status updated to \"{game}\"");

                        await ChatReminder.RefreshReminders();
                        await _customCommandInstance.LoadCustomCommands(_botConfig.TwitchBotApiLink, _broadcasterInstance.DatabaseId);
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
        }
    }
}
