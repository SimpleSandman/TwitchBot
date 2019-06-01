using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using RestSharp;

using TwitchBot.Configuration;
using TwitchBot.Enums;
using TwitchBot.Libraries;
using TwitchBot.Services;
using TwitchBot.Models;
using TwitchBot.Models.JSON;
using TwitchBot.Threads;

using TwitchBotDb.DTO;
using TwitchBotDb.Models;

using TwitchBotUtil.Extensions;
using Google.Apis.YouTube.v3.Data;

namespace TwitchBot.Commands
{
    public class CmdMod
    {
        private IrcClient _irc;
        private TimeoutCmd _timeout;
        private System.Configuration.Configuration _appConfig;
        private TwitchBotConfigurationSection _botConfig;
        private int _broadcasterId;
        private BankService _bank;
        private TwitchInfoService _twitchInfo;
        private ManualSongRequestService _manualSongRequest;
        private QuoteService _quote;
        private PartyUpService _partyUp;
        private GameDirectoryService _gameDirectory;
        private LibVLCSharpPlayer _libVLCSharpPlayer;
        private ErrorHandler _errHndlrInstance = ErrorHandler.Instance;
        private TwitterClient _twitter = TwitterClient.Instance;
        private BroadcasterSingleton _broadcasterInstance = BroadcasterSingleton.Instance;
        private TwitchChatterList _twitchChatterListInstance = TwitchChatterList.Instance;
        private YoutubeClient _youTubeClientInstance = YoutubeClient.Instance;

        public CmdMod(IrcClient irc, TimeoutCmd timeout, TwitchBotConfigurationSection botConfig, int broadcasterId, 
            System.Configuration.Configuration appConfig, BankService bank, TwitchInfoService twitchInfo, ManualSongRequestService manualSongRequest,
            QuoteService quote, PartyUpService partyUp, GameDirectoryService gameDirectory, LibVLCSharpPlayer libVLCSharpPlayer)
        {
            _irc = irc;
            _timeout = timeout;
            _botConfig = botConfig;
            _broadcasterId = broadcasterId;
            _appConfig = appConfig;
            _bank = bank;
            _twitchInfo = twitchInfo;
            _manualSongRequest = manualSongRequest;
            _quote = quote;
            _partyUp = partyUp;
            _gameDirectory = gameDirectory;
            _libVLCSharpPlayer = libVLCSharpPlayer;
        }

        /// <summary>
        /// Displays Discord link (if available)
        /// </summary>
        public async void CmdDiscord()
        {
            try
            {
                if (string.IsNullOrEmpty(_botConfig.DiscordLink) || _botConfig.DiscordLink.Equals("Link unavailable at the moment"))
                    _irc.SendPublicChatMessage("Discord link unavailable at the moment");
                else
                    _irc.SendPublicChatMessage("Wanna kick it with some awesome peeps like myself? Of course you do! Join this fantastic Discord! " + _botConfig.DiscordLink);
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdMod", "CmdDiscord()", false, "!discord");
            }
        }

        /// <summary>
        /// Takes money away from a user
        /// </summary>
        /// <param name="chatter"></param>
        public async Task CmdCharge(TwitchChatter chatter)
        {
            try
            {
                if (chatter.Message.StartsWith("!charge @", StringComparison.CurrentCultureIgnoreCase))
                    _irc.SendPublicChatMessage($"Please enter a valid amount @{chatter.DisplayName}");
                else
                {
                    int indexAction = chatter.Message.IndexOf(" ");
                    int fee = -1;
                    bool isValidFee = int.TryParse(chatter.Message.Substring(indexAction, chatter.Message.IndexOf("@") - indexAction - 1), out fee);
                    string recipient = chatter.Message.Substring(chatter.Message.IndexOf("@") + 1).ToLower();
                    int wallet = await _bank.CheckBalance(recipient, _broadcasterId);

                    // Check user's bank account exist or has currency
                    if (wallet == -1)
                        _irc.SendPublicChatMessage($"{recipient} is not currently banking with us @{chatter.DisplayName}");
                    else if (wallet == 0)
                        _irc.SendPublicChatMessage($"{recipient} is out of {_botConfig.CurrencyType} @{chatter.DisplayName}");
                    // Check if fee can be accepted
                    else if (fee > 0)
                        _irc.SendPublicChatMessage("Please insert a negative whole amount (no decimal numbers) "
                            + $" or use the !deposit command to add {_botConfig.CurrencyType} to a user's account");
                    else if (!isValidFee)
                        _irc.SendPublicChatMessage($"The fee wasn't accepted. Please try again with negative whole amount (no decimals) @{chatter.DisplayName}");
                    else if (chatter.Username != _botConfig.Broadcaster.ToLower() && _twitchChatterListInstance.GetUserChatterType(recipient) == ChatterType.Moderator)
                        _irc.SendPublicChatMessage($"Entire deposit voided. You cannot remove {_botConfig.CurrencyType} from another moderator's account @{chatter.DisplayName}");
                    else /* Deduct funds from wallet */
                    {
                        wallet += fee;

                        // Zero out account balance if user is being charged more than they have
                        if (wallet < 0)
                            wallet = 0;

                        await _bank.UpdateFunds(recipient, _broadcasterId, wallet);

                        // Prompt user's balance
                        if (wallet == 0)
                            _irc.SendPublicChatMessage($"Charged {fee.ToString().Replace("-", "")} {_botConfig.CurrencyType} to {recipient}"
                                + $"'s account! They are out of {_botConfig.CurrencyType} to spend");
                        else
                            _irc.SendPublicChatMessage($"Charged {fee.ToString().Replace("-", "")} {_botConfig.CurrencyType} to {recipient}"
                                + $"'s account! They only have {wallet} {_botConfig.CurrencyType} to spend");
                    }
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdMod", "CmdCharge(TwitchChatter)", false, "!charge");
            }
        }

        /// <summary>
        /// Gives a set amount of stream currency to user
        /// </summary>
        /// <param name="chatter"></param>
        public async Task CmdDeposit(TwitchChatter chatter)
        {
            try
            {
                List<string> userList = new List<string>();

                foreach (int index in chatter.Message.AllIndexesOf("@"))
                {
                    int lengthUsername = chatter.Message.IndexOf(" ", index) - index - 1;
                    if (lengthUsername < 0)
                        userList.Add(chatter.Message.Substring(index + 1).ToLower());
                    else
                        userList.Add(chatter.Message.Substring(index + 1, lengthUsername).ToLower());
                }

                // Check for valid command
                if (chatter.Message.StartsWith("!deposit @", StringComparison.CurrentCultureIgnoreCase))
                    _irc.SendPublicChatMessage($"Please enter a valid amount to a user @{chatter.DisplayName}");
                // Check if moderator is trying to give streamer currency to themselves
                else if (chatter.Username != _botConfig.Broadcaster.ToLower() && userList.Contains(chatter.Username))
                    _irc.SendPublicChatMessage($"Entire deposit voided. You cannot add {_botConfig.CurrencyType} to your own account @{chatter.DisplayName}");
                else
                {
                    // Check if moderator is trying to give streamer currency to other moderators
                    if (chatter.Username != _botConfig.Broadcaster.ToLower())
                    {
                        foreach (string user in userList)
                        {
                            if (_twitchChatterListInstance.GetUserChatterType(user) == ChatterType.Moderator)
                            {
                                _irc.SendPublicChatMessage($"Entire deposit voided. You cannot add {_botConfig.CurrencyType} to another moderator's account @{chatter.DisplayName}");
                                return;
                            }
                        }
                    }

                    int indexAction = chatter.Message.IndexOf(" ");
                    int deposit = -1;
                    bool isValidDeposit = int.TryParse(chatter.Message.Substring(indexAction, chatter.Message.IndexOf("@") - indexAction - 1), out deposit);

                    // Check if deposit amount is valid
                    if (deposit < 0)
                        _irc.SendPublicChatMessage("Please insert a positive whole amount (no decimals) " 
                            + $" or use the !charge command to remove {_botConfig.CurrencyType} from a user");
                    else if (!isValidDeposit)
                        _irc.SendPublicChatMessage($"The deposit wasn't accepted. Please try again with a positive whole amount (no decimals) @{chatter.DisplayName}");
                    else
                    {
                        if (userList.Count > 0)
                        {
                            List<BalanceResult> balResultList = await _bank.UpdateCreateBalance(userList, _broadcasterId, deposit, true);

                            string responseMsg = $"Gave {deposit.ToString()} {_botConfig.CurrencyType} to ";

                            if (balResultList.Count > 1)
                            {
                                foreach (BalanceResult userResult in balResultList)
                                    responseMsg += $"{userResult.Username}, ";

                                responseMsg = responseMsg.ReplaceLastOccurrence(", ", "");
                            }
                            else if (balResultList.Count == 1)
                            {
                                responseMsg += $"@{balResultList[0].Username} ";

                                if (balResultList[0].ActionType.Equals("UPDATE"))
                                    responseMsg += $"and now has {balResultList[0].Wallet} {_botConfig.CurrencyType}!";
                                else if (balResultList[0].ActionType.Equals("INSERT"))
                                    responseMsg += $"and can now gamble it all away! Kappa";
                            }
                            else
                                responseMsg = $"Unknown error has occurred in retrieving results. Please check your recipient's {_botConfig.CurrencyType}";

                            _irc.SendPublicChatMessage(responseMsg);
                        }
                        else
                        {
                            _irc.SendPublicChatMessage($"There are no chatters to deposit {_botConfig.CurrencyType} @{chatter.DisplayName}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdMod", "CmdDeposit(TwitchChatter)", false, "!deposit");
            }
        }

        /// <summary>
        /// Gives every viewer currently watching a set amount of currency
        /// </summary>
        /// <param name="chatter"></param>
        public async Task CmdBonusAll(TwitchChatter chatter)
        {
            try
            {
                // Check for valid command
                if (chatter.Message.StartsWith("!bonusall @", StringComparison.CurrentCultureIgnoreCase))
                    _irc.SendPublicChatMessage($"Please enter a valid amount to a user @{chatter.DisplayName}");
                else
                {
                    int indexAction = chatter.Message.IndexOf(" ");
                    int deposit = -1;
                    bool isValidDeposit = int.TryParse(chatter.Message.Substring(indexAction), out deposit);

                    // Check if deposit amount is valid
                    if (deposit < 0)
                        _irc.SendPublicChatMessage("Please insert a positive whole amount (no decimals) "
                            + $" or use the !charge command to remove {_botConfig.CurrencyType} from a user");
                    else if (!isValidDeposit)
                        _irc.SendPublicChatMessage($"The bulk deposit wasn't accepted. Please try again with positive whole amount (no decimals) @{chatter.DisplayName}");
                    else
                    {
                        // Wait until chatter lists are available
                        while (!_twitchChatterListInstance.AreListsAvailable)
                        {

                        }

                        List<string> chatterList = _twitchChatterListInstance.ChattersByName;

                        // broadcaster gives stream currency to all but themselves and the bot
                        if (chatter.Username == _botConfig.Broadcaster.ToLower())
                        {
                            chatterList = chatterList.Where(t => t != chatter.Username.ToLower() && t != _botConfig.BotName.ToLower()).ToList();
                        }
                        else // moderators gives stream currency to all but other moderators (including broadcaster)
                        {
                            chatterList = chatterList
                                .Where(t => _twitchChatterListInstance.GetUserChatterType(t) != ChatterType.Moderator 
                                    && t != _botConfig.BotName.ToLower()).ToList();
                        }

                        if (chatterList != null && chatterList.Count > 0)
                        {
                            await _bank.UpdateCreateBalance(chatterList, _broadcasterId, deposit);

                            _irc.SendPublicChatMessage($"{deposit.ToString()} {_botConfig.CurrencyType} for everyone! "
                                + $"Check your stream bank account with !{_botConfig.CurrencyType.ToLower()}");
                        }
                        else
                        {
                            _irc.SendPublicChatMessage($"There are no chatters to deposit {_botConfig.CurrencyType} @{chatter.DisplayName}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdMod", "CmdBonusAll(string, string)", false, "!bonusall");
            }
        }

        /// <summary>
        /// Resets the song request queue
        /// </summary>
        public async Task CmdResetManualSr()
        {
            try
            {
                List<SongRequest> removedSong = await _manualSongRequest.ResetSongRequests(_broadcasterId);

                if (removedSong != null && removedSong.Count > 0)
                    _irc.SendPublicChatMessage($"The song request queue has been reset @{_botConfig.Broadcaster}");
                else
                    _irc.SendPublicChatMessage($"Song requests are empty @{_botConfig.Broadcaster}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdMod", "CmdResetManualSr()", false, "!resetrbsr");
            }
        }

        /// <summary>
        /// Bot-specific timeout on a user for a set amount of time
        /// </summary>
        /// <param name="chatter"></param>
        public async Task CmdAddTimeout(TwitchChatter chatter)
        {
            try
            {
                if (chatter.Message.StartsWith("!addtimeout @"))
                    _irc.SendPublicChatMessage("I cannot make a user not talk to me without this format '!addtimeout [seconds] @[username]'");
                else if (chatter.Message.ToLower().Contains(_botConfig.Broadcaster.ToLower()))
                    _irc.SendPublicChatMessage($"I cannot betray @{_botConfig.Broadcaster} by not allowing him to communicate with me @{chatter.DisplayName}");
                else if (chatter.Message.ToLower().Contains(_botConfig.BotName.ToLower()))
                    _irc.SendPublicChatMessage($"You can't time me out @{chatter.DisplayName} PowerUpL Jebaited PowerUpR");
                else
                {
                    int indexAction = chatter.Message.IndexOf(" ");
                    string recipient = chatter.Message.Substring(chatter.Message.IndexOf("@") + 1).ToLower();
                    double seconds = -1;
                    bool isValidTimeout = double.TryParse(chatter.Message.Substring(indexAction, chatter.Message.IndexOf("@") - indexAction - 1), out seconds);

                    if (!isValidTimeout || seconds < 0.00)
                        _irc.SendPublicChatMessage("The timeout amount wasn't accepted. Please try again with positive seconds only");
                    else if (seconds < 15.00)
                        _irc.SendPublicChatMessage("The duration needs to be at least 15 seconds long. Please try again");
                    else
                    {
                        DateTime timeoutExpiration = await _timeout.AddTimeout(recipient, _broadcasterId, seconds, _botConfig.TwitchBotApiLink);

                        string response = $"I'm told not to talk to you until {timeoutExpiration.ToLocalTime()} ";

                        if (timeoutExpiration.ToLocalTime().IsDaylightSavingTime())
                            response += $"({TimeZone.CurrentTimeZone.DaylightName})";
                        else
                            response += $"({TimeZone.CurrentTimeZone.StandardName})";

                        _irc.SendPublicChatMessage($"{response} @{recipient}");
                    }
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdMod", "CmdAddTimeout(TwitchChatter)", false, "!addtimeout");
            }
        }

        /// <summary>
        /// Remove bot-specific timeout on a user for a set amount of time
        /// </summary>
        /// <param name="chatter"></param>
        public async Task CmdDeleteTimeout(TwitchChatter chatter)
        {
            try
            {
                string recipient = chatter.Message.Substring(chatter.Message.IndexOf("@") + 1).ToLower();

                recipient = await _timeout.DeleteUserTimeout(recipient, _broadcasterId, _botConfig.TwitchBotApiLink);

                if (!string.IsNullOrEmpty(recipient))
                    _irc.SendPublicChatMessage($"{recipient} can now interact with me again because of @{chatter.DisplayName} @{_botConfig.Broadcaster}");
                else
                    _irc.SendPublicChatMessage($"Cannot find the user you wish to timeout @{chatter.DisplayName}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdMod", "CmdDelTimeout(TwitchChatter)", false, "!deltimeout");
            }
        }

        /// <summary>
        /// Set delay for messages based on the latency of the stream
        /// </summary>
        /// <param name="chatter"></param>
        public async void CmdSetLatency(TwitchChatter chatter)
        {
            try
            {
                int latency = -1;
                bool isValidInput = int.TryParse(chatter.Message.Substring(chatter.Message.IndexOf(" ")), out latency);

                if (!isValidInput || latency < 0)
                    _irc.SendPublicChatMessage("Please insert a valid positive alloted amount of time (in seconds)");
                else
                {
                    _botConfig.StreamLatency = latency;
                    _appConfig.AppSettings.Settings.Remove("streamLatency");
                    _appConfig.AppSettings.Settings.Add("streamLatency", latency.ToString());
                    _appConfig.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection("TwitchBotConfiguration");

                    _irc.SendPublicChatMessage($"Bot settings for stream latency set to {_botConfig.StreamLatency} second(s) @{chatter.DisplayName}");
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdMod", "CmdSetLatency(TwitchChatter)", false, "!setlatency");
            }
        }

        /// <summary>
        /// Tell the stream the specified moderator will be AFK
        /// </summary>
        /// <param name="chatter"></param>
        public async void CmdModAfk(TwitchChatter chatter)
        {
            try
            {
                _irc.SendPublicChatMessage($"@{chatter.DisplayName} is going AFK @{_botConfig.Broadcaster}! SwiftRage");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdMod", "CmdModAfk(string)", false, "!modafk");
            }
        }

        /// <summary>
        /// Tell the stream the specified moderator is back
        /// </summary>
        /// <param name="chatter"></param>
        public async void CmdModBack(TwitchChatter chatter)
        {
            try
            {
                _irc.SendPublicChatMessage($"@{chatter.DisplayName} is back @{_botConfig.Broadcaster}! BlessRNG");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdMod", "CmdModBack(string)", false, "!modback");
            }
        }

        /// <summary>
        /// Update the title of the Twitch channel
        /// </summary>
        /// <param name="chatter"></param>
        public async Task CmdUpdateTitle(TwitchChatter chatter)
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
                    response = await client.ExecuteTaskAsync<Task>(request);
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
                await _errHndlrInstance.LogError(ex, "CmdMod", "CmdUpdateTitle(TwitchChatter)", false, "!updatetitle");
            }
        }

        /// <summary>
        /// Updates the game being played on the Twitch channel
        /// </summary>
        /// <param name="chatter"></param>
        /// <param name="hasTwitterInfo">Check for Twitter credentials</param>
        public async Task CmdUpdateGame(TwitchChatter chatter, bool hasTwitterInfo)
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
                    response = await client.ExecuteTaskAsync<Task>(request);
                    string statResponse = response.StatusCode.ToString();
                    if (statResponse.Contains("OK"))
                    {
                        _irc.SendPublicChatMessage($"Twitch channel game status updated to \"{game}\"");
                        if (_botConfig.EnableTweets && hasTwitterInfo)
                        {
                            Console.WriteLine(_twitter.SendTweet($"Just switched to \"{game}\" on " 
                                + $"twitch.tv/{_broadcasterInstance.Username}"));
                        }

                        await Threads.ChatReminder.RefreshReminders();
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
                await _errHndlrInstance.LogError(ex, "CmdMod", "CmdUpdateGame(TwitchChatter, bool)", false, "!updategame");
            }
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
                await _errHndlrInstance.LogError(ex, "CmdMod", "CmdResetJoin(TwitchChatter, Queue<string>)", false, "!resetjoin");
            }

            return gameQueueUsers;
        }

        public async Task CmdLibVLCSharpPlayerVolume(TwitchChatter chatter)
        {
            try
            {
                bool validMessage = int.TryParse(chatter.Message.Substring(chatter.Message.IndexOf(" ") + 1), out int volumePercentage);

                if (validMessage)
                {
                    if (await _libVLCSharpPlayer.SetVolume(volumePercentage))
                        _irc.SendPublicChatMessage($"Song request volume set to {volumePercentage}% @{chatter.DisplayName}");
                    else if (volumePercentage < 1 || volumePercentage > 100)
                        _irc.SendPublicChatMessage($"Please use a value for the song request volume from 1-100 @{chatter.DisplayName}");
                }
                else
                {
                    _irc.SendPublicChatMessage($"Requested volume not valid. Please set the song request volume from 1-100 @{chatter.DisplayName}");
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdMod", "CmdLibVLCSharpPlayerVolume(TwitchChatter)", false, "!srvolume", chatter.Message);
            }
        }

        public async void CmdLibVLCSharpPlayerSkip(TwitchChatter chatter)
        {
            try
            {
                bool validMessage = int.TryParse(chatter.Message.Substring(chatter.Message.IndexOf(" ") + 1), out int songSkipCount);

                if (!validMessage)
                    _libVLCSharpPlayer.Skip();
                else
                    _libVLCSharpPlayer.Skip(songSkipCount);

                PlaylistItem playlistItem = _libVLCSharpPlayer.CurrentSongRequestPlaylistItem;

                string songRequest = _youTubeClientInstance.ShowPlayingSongRequest(playlistItem);

                if (!string.IsNullOrEmpty(songRequest))
                    _irc.SendPublicChatMessage($"@{chatter.DisplayName} <-- Now playing: {songRequest}");
                else
                    _irc.SendPublicChatMessage($"Unable to display the current song @{chatter.DisplayName}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdMod", "CmdLibVLCSharpPlayerSkip(TwitchChatter)", false, "!srskip");
            }
        }

        public async Task CmdLibVLCSharpPlayerSetTime(TwitchChatter chatter)
        {
            try
            {
                bool validMessage = int.TryParse(chatter.Message.Substring(chatter.Message.IndexOf(" ") + 1), out int seekVideoTime);

                if (validMessage && await _libVLCSharpPlayer.SetVideoTime(seekVideoTime))
                    _irc.SendPublicChatMessage($"Video seek time set to {seekVideoTime} second(s) @{chatter.DisplayName}");
                else
                    _irc.SendPublicChatMessage($"Time not valid. Please set the time (in seconds) between 0 and the length of the video @{chatter.DisplayName}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdMod", "CmdLibVLCSharpPlayerSeek(TwitchChatter)", false, "!srtime", chatter.Message);
            }
        }
    }
}
