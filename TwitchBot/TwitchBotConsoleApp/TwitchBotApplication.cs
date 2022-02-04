using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading;
using System.Threading.Tasks;

using CoreTweet;

using Google.Apis.YouTube.v3.Data;

using TwitchBotDb.Models;
using TwitchBotDb.Services;

using TwitchBotShared.ClientLibraries;
using TwitchBotShared.ClientLibraries.Singletons;
using TwitchBotShared.Commands;
using TwitchBotShared.Config;
using TwitchBotShared.Extensions;
using TwitchBotShared.Models;
using TwitchBotShared.Models.JSON;
using TwitchBotShared.Threads;

namespace TwitchBotConsoleApp
{
    public class TwitchBotApplication
    {
        private CommandSystem _commandSystem;
        private SpotifyWebClient _spotify;
        private DiscordNetClient _discord;
        private TwitchStreamStatus _twitchStreamStatus;
        private readonly Configuration _appConfig;
        private readonly TwitchBotConfigurationSection _botConfig;
        private readonly IrcClient _irc;
        private readonly List<string> _greetedUsers;
        private readonly TwitchInfoService _twitchInfo;
        private readonly FollowerService _follower;
        private readonly FollowerSubscriberListener _followerSubscriberListener;
        private readonly BankService _bank;
        private readonly SongRequestBlacklistService _songRequestBlacklist;
        private readonly ManualSongRequestService _manualSongRequest;
        private readonly GameDirectoryService _gameDirectory;
        private readonly QuoteService _quote;
        private readonly SongRequestSettingService _songRequestSetting;
        private readonly InGameUsernameService _ign;
        private readonly BankHeist _bankHeist;
        private readonly BossFight _bossFight;
        private readonly LibVLCSharpPlayer _libVLCSharpPlayer;
        private readonly TwitchChatterListener _twitchChatterListener;
        private readonly PartyUpService _partyUp;
        private readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;
        private readonly TwitterClient _twitterInstance = TwitterClient.Instance;
        private readonly YoutubeClient _youTubeClientInstance = YoutubeClient.Instance;
        private readonly CooldownUsersSingleton _cooldownUsersInstance = CooldownUsersSingleton.Instance;
        private readonly BroadcasterSingleton _broadcasterInstance = BroadcasterSingleton.Instance;
        private readonly BankHeistSingleton _bankHeistInstance = BankHeistSingleton.Instance;
        private readonly BossFightSingleton _bossFightInstance = BossFightSingleton.Instance;
        private readonly CustomCommandSingleton _customCommandInstance = CustomCommandSingleton.Instance;

        public TwitchBotApplication(Configuration appConfig, TwitchInfoService twitchInfo, SongRequestBlacklistService songRequestBlacklist,
            FollowerService follower, BankService bank, FollowerSubscriberListener followerListener, ManualSongRequestService manualSongRequest, PartyUpService partyUp,
            GameDirectoryService gameDirectory, QuoteService quote, BankHeist bankHeist, TwitchChatterListener twitchChatterListener, IrcClient irc,
            BossFight bossFight, SongRequestSettingService songRequestSetting, InGameUsernameService ign, LibVLCSharpPlayer libVLCSharpPlayer)
        {
            _appConfig = appConfig;
            _botConfig = appConfig.GetSection("TwitchBotConfig") as TwitchBotConfigurationSection;
            _greetedUsers = new List<string>();
            _twitchInfo = twitchInfo;
            _follower = follower;
            _followerSubscriberListener = followerListener;
            _bank = bank;
            _songRequestBlacklist = songRequestBlacklist;
            _manualSongRequest = manualSongRequest;
            _gameDirectory = gameDirectory;
            _quote = quote;
            _bankHeist = bankHeist;
            _twitchChatterListener = twitchChatterListener;
            _bossFight = bossFight;
            _songRequestSetting = songRequestSetting;
            _ign = ign;
            _libVLCSharpPlayer = libVLCSharpPlayer;
            _irc = irc;
            _partyUp = partyUp;
        }

        public async Task RunAsync()
        {
            try
            {
                // ToDo: Check version number of application
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Message: {ex.Message}");
                Console.WriteLine();
                Console.WriteLine("Cannot connect to database to verify the correct version of myself");
                Console.WriteLine("Local troubleshooting needed by author of this bot");
                Console.WriteLine();
                Console.WriteLine("Shutting down now...");
                Thread.Sleep(5000);
                Environment.Exit(1);
            }

            try
            {
                // Configure error handler singleton class
                ErrorHandler.Configure(_broadcasterInstance.DatabaseId, _irc, _botConfig);

                // Get broadcaster ID so the user can only see their data from the db
                await SetBroadcasterIdsAsync();

                if (_broadcasterInstance.DatabaseId == 0 || string.IsNullOrEmpty(_broadcasterInstance.TwitchId))
                {
                    Console.WriteLine("Cannot find a broadcaster ID for you. "
                        + "Please contact the author with a detailed description of the issue");
                    Console.WriteLine();
                    Console.WriteLine("Shutting down now...");
                    Thread.Sleep(5000);
                    Environment.Exit(1);
                }

                // Configure error handler singleton class
                ErrorHandler.Configure(_broadcasterInstance.DatabaseId, _irc, _botConfig);

                /* Connect to local Spotify client */
                _spotify = new SpotifyWebClient(_botConfig);
                await _spotify.ConnectAsync();

                /* Connect to Discord */
                _discord = new DiscordNetClient(_botConfig);
                await _discord.ConnectAsync();
                
                /* Load command classes */
                _commandSystem = new CommandSystem(_irc, _botConfig, _appConfig, _bank, _songRequestBlacklist,
                    _libVLCSharpPlayer, _songRequestSetting, _spotify, _twitchInfo, _follower, _gameDirectory, 
                    _ign, _manualSongRequest, _quote, _partyUp, _discord);

                /* Whisper broadcaster bot settings */
                Console.WriteLine();
                Console.WriteLine("---> Extra Bot Settings <---");
                Console.WriteLine($"Currency type: {_botConfig.CurrencyType}");
                Console.WriteLine($"Enable Auto Tweets: {_botConfig.EnableTweets}");
                Console.WriteLine($"Enable Auto Display Songs: {_botConfig.EnableDisplaySong}");
                Console.WriteLine($"Stream latency: {_botConfig.StreamLatency} second(s)");
                Console.WriteLine($"Regular follower hours: {_botConfig.RegularFollowerHours}");
                Console.WriteLine();

                /* Configure YouTube song request from user's YT account (request permission if needed) */
                await GetYouTubeAuthAsync();

                /* Start listening for delayed messages */
                DelayMessage delayMsg = new DelayMessage(_irc);
                delayMsg.Start();

                /* Grab list of chatters from channel */
                _twitchChatterListener.Start();

                /* Get the status of the Twitch stream */
                _twitchStreamStatus = new TwitchStreamStatus(_irc, _twitchInfo, _botConfig.Broadcaster);
                await _twitchStreamStatus.LoadChannelInfoAsync();
                _twitchStreamStatus.Start();

                /* Pull list of followers and check experience points for stream leveling */
                _followerSubscriberListener.Start(_irc, _broadcasterInstance.DatabaseId);

                /* Load/create settings and start the queue for the heist */
                await _bankHeistInstance.LoadSettings(_broadcasterInstance.DatabaseId, _botConfig.TwitchBotApiLink);
                _bankHeist.Start(_irc, _broadcasterInstance.DatabaseId);

                if (string.IsNullOrEmpty(TwitchStreamStatus.CurrentCategory))
                {
                    _irc.SendPublicChatMessage("WARNING: I cannot see the name of the game. It's currently set to either NULL or EMPTY. "
                        + "Please have the chat verify that the game has been set for this stream. "
                        + $"If the error persists, please have @{_botConfig.Broadcaster.ToLower()} retype the game in their Twitch Live Dashboard. "
                        + "If this error shows up again and your chat can see the game set for the stream, please contact my master with !support in this chat");
                }

                // Grab game id in order to find party member
                TwitchGameCategory game = await _gameDirectory.GetGameIdAsync(TwitchStreamStatus.CurrentCategory);

                /* Load/create settings and start the queue for the boss fight */
                await _bossFightInstance.LoadSettings(_broadcasterInstance.DatabaseId, game?.Id, _botConfig.TwitchBotApiLink);
                _bossFight.Start(_irc, _broadcasterInstance.DatabaseId);

                /* Ping to twitch server to prevent auto-disconnect */
                PingSender ping = new PingSender(_irc);
                ping.Start();

                /* Send reminders of certain events */
                ChatReminder chatReminder = new ChatReminder(_irc, _broadcasterInstance.DatabaseId, _botConfig.TwitchBotApiLink, _twitchInfo, _gameDirectory);
                chatReminder.Start();

                /* Load in custom commands */
                await _customCommandInstance.LoadCustomCommands(_botConfig.TwitchBotApiLink, _broadcasterInstance.DatabaseId);

                /* Authenticate to Twitter if possible */
                GetTwitterAuth();

                Console.WriteLine("===== Time to get to work! =====");
                Console.WriteLine();

                /* Finished setup, time to start */
                await GetChatBoxAsync();
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "TwitchBotApplication", "RunAsync()", true);
            }
        }        

        /// <summary>
        /// Monitor chat box for commands
        /// </summary>
        private async Task GetChatBoxAsync()
        {
            try
            {
                /* Master loop */
                while (true)
                {
                    // Read any message inside the chat room
                    string rawMessage = await _irc.ReadMessageAsync();
                    Console.WriteLine(rawMessage); // Print raw irc message

                    if (!string.IsNullOrEmpty(rawMessage))
                    {
                        /* 
                        * Get user name and message from chat 
                        * and check if user has access to certain functions
                        */
                        if (rawMessage.Contains("PRIVMSG"))
                        {
                            // Modify message to only show user and message
                            // Reference: https://dev.twitch.tv/docs/irc/tags/#privmsg-twitch-tags
                            int indexParseSign = rawMessage.IndexOf(" :");
                            string modifiedMessage = rawMessage.Remove(0, indexParseSign + 2);

                            indexParseSign = modifiedMessage.IndexOf('!');
                            string username = modifiedMessage.Substring(0, indexParseSign);

                            indexParseSign = modifiedMessage.IndexOf(" :");
                            string message = modifiedMessage.Substring(indexParseSign + 2);

                            TwitchChatter chatter = new TwitchChatter
                            {
                                Username = username,
                                Message = message,
                                DisplayName = PrivMsgParameterValue(rawMessage, "display-name"),
                                Badges = PrivMsgParameterValue(rawMessage, "badges"),
                                TwitchId = PrivMsgParameterValue(rawMessage, "user-id"),
                                MessageId = PrivMsgParameterValue(rawMessage, "id")
                            };

                            message = message.ToLower(); // make commands case-insensitive

                            try
                            {
                                // Purge any clips that aren't from the broadcaster that a viewer posts
                                if (_botConfig.Broadcaster.ToLower() != chatter.Username
                                    && !chatter.Badges.Contains("moderator") 
                                    && !await IsAllowedChatMessageAsync(chatter))
                                {
                                    _irc.ClearMessage(chatter);
                                    _irc.SendPublicChatMessage($"Please refrain from posting a message that isn't for this channel @{chatter.DisplayName}");
                                    continue;
                                }

                                await GreetUserAsync(chatter);

                                await _commandSystem.ExecRequestAsync(chatter);

                                FindCustomCommand(chatter);
                            }
                            catch (Exception ex)
                            {
                                await _errHndlrInstance.LogErrorAsync(ex, "TwitchBotApplication", "GetChatBox()", false, "N/A", chatter.Message);
                            }
                        }
                        else if (rawMessage.Contains("NOTICE"))
                        {
                            if (rawMessage.Contains("Error logging in"))
                            {
                                Console.WriteLine("\n------------> URGENT <------------");
                                Console.WriteLine("Please check your credentials and try again.");
                                Console.WriteLine("If this error persists, please check if you can access your channel's chat.");
                                Console.WriteLine("If not, then contact Twitch support.");
                                Console.WriteLine("Exiting bot application now...");
                                Thread.Sleep(7500);
                                Environment.Exit(0);
                            }
                        }
                    }
                } // end master while loop
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "TwitchBotApplication", "GetChatBox()", true);
            }
        }

        private async Task SetBroadcasterIdsAsync()
        {
            try
            {
                RootUserJSON json = await _twitchInfo.GetUsersByLoginNameAsync(_botConfig.Broadcaster);

                if (json?.Users.Count == 0)
                {
                    Console.WriteLine("Error: Couldn't find Twitch login name from Twitch. If this persists, please contact my creator");
                    Console.WriteLine("Shutting down now...");
                    Thread.Sleep(3000);
                    Environment.Exit(0);
                }

                await _broadcasterInstance.FindBroadcaster(json.Users.First().Id, _botConfig.TwitchBotApiLink);

                // check if user exists, but changed their username
                if (_broadcasterInstance.TwitchId != null)
                {
                    if (_broadcasterInstance.Username.ToLower() != json.Users.First().Name)
                    {
                        _broadcasterInstance.Username = json.Users.First().Name;

                        await _broadcasterInstance.UpdateBroadcaster(_botConfig.TwitchBotApiLink);
                    }
                    else
                        return;
                }                
                else // add new user
                {
                    _broadcasterInstance.Username = json.Users.First().Name;
                    _broadcasterInstance.TwitchId = json.Users.First().Id;

                    await _broadcasterInstance.AddBroadcaster(_botConfig.TwitchBotApiLink);
                }

                // check if user was inserted/updated correctly
                await _broadcasterInstance.FindBroadcaster(_broadcasterInstance.TwitchId, _botConfig.TwitchBotApiLink, _broadcasterInstance.Username);
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "TwitchBotApplication", "SetBroadcasterIds()", true);
            }
        }

        /// <summary>
        /// Greet a user (new or returning) with a welcome message and a "thank-you" deposit of stream currency
        /// </summary>
        ///<param name="chatter"></param>
        private async Task GreetUserAsync(TwitchChatter chatter)
        {
            try
            {
                if (!_greetedUsers.Any(u => u == chatter.Username) 
                    && chatter.Username != _botConfig.Broadcaster.ToLower() 
                    && chatter.Message.Length > 1 
                    && TwitchStreamStatus.IsLive)
                {
                    // check if user has a stream currency account
                    int funds = await _bank.CheckBalanceAsync(chatter.Username, _broadcasterInstance.DatabaseId);
                    int greetedDeposit = 500; // ToDo: Make greeted deposit config setting

                    if (funds > -1 || chatter.Badges.Contains("moderator") || chatter.Badges.Contains("vip") || chatter.Badges.Contains("bits"))
                    {
                        int currentExp = await _follower.CurrentExpAsync(chatter.Username, _broadcasterInstance.DatabaseId);
                        IEnumerable<Rank> rankList = await _follower.GetRankListAsync(_broadcasterInstance.DatabaseId);
                        Rank rank = _follower.GetCurrentRank(rankList, currentExp);

                        string rankName = "";
                        if (rank != null)
                        {
                            rankName = rank.Name;
                        }

                        funds += greetedDeposit; // deposit stream currency
                        await _bank.UpdateFundsAsync(chatter.Username, _broadcasterInstance.DatabaseId, funds);

                        _irc.SendPublicChatMessage($"Welcome back {rankName} @{chatter.DisplayName} ! " 
                            + $"Let me reward your return with {greetedDeposit} {_botConfig.CurrencyType}");
                    }
                    else
                    {
                        await _bank.CreateAccountAsync(chatter.Username, _broadcasterInstance.DatabaseId, greetedDeposit);

                        _irc.SendPublicChatMessage($"Welcome to the channel @{chatter.DisplayName} ! Thanks for saying something! "
                            + $"Let me show you my appreciation with {greetedDeposit} {_botConfig.CurrencyType}");
                    }

                    _greetedUsers.Add(chatter.Username); // make sure user doesn't get greeted again as long as this bot is alive
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "TwitchBotApplication", "GreetUser(TwitchChatter)", false);
            }
        }

        /// <summary>
        /// Get access to user's Twitter credentials via PIN-based authentication
        /// </summary>
        private async void GetTwitterAuth()
        {
            try
            { 
                // Check if developer set up Twitter integration
                if (!string.IsNullOrEmpty(_botConfig.TwitterConsumerKey) && !string.IsNullOrEmpty(_botConfig.TwitterConsumerSecret))
                {
                    Tokens tokens = new Tokens();

                    // Check existing credentials
                    if (!string.IsNullOrEmpty(_botConfig.TwitterAccessToken) && !string.IsNullOrEmpty(_botConfig.TwitterAccessSecret))
                    {
                        tokens = Tokens.Create(
                            _botConfig.TwitterConsumerKey, 
                            _botConfig.TwitterConsumerSecret,
                            _botConfig.TwitterAccessToken, 
                            _botConfig.TwitterAccessSecret
                        );
                    }

                    // Get authentication to Twitter account
                    if (string.IsNullOrEmpty(_botConfig.TwitterAccessToken) || string.IsNullOrEmpty(_botConfig.TwitterAccessSecret))
                    {
                        // create session
                        OAuth.OAuthSession session = await OAuth.AuthorizeAsync(_botConfig.TwitterConsumerKey, _botConfig.TwitterConsumerSecret);
                        string url = session.AuthorizeUri.AbsoluteUri;

                        Process process = new Process();
                        process.StartUrlCrossPlatform(url);

                        // Ask the user to enter the pin code given by Twitter
                        Console.WriteLine("Please enter the PIN given by Twitter (or simply press ENTER to continue using this bot without Twitter):");
                        string pinCode = Console.ReadLine();

                        if (!string.IsNullOrWhiteSpace(pinCode))
                        {
                            // assign tokens
                            tokens = await OAuth.GetTokensAsync(session, pinCode);

                            pinCode = ""; // clear pin code

                            if (tokens != null)
                            {
                                SaveTwitterAccessInfo(tokens.AccessToken, tokens.AccessTokenSecret);

                                if (await VerifyTwitterCredentialsAsync(tokens))
                                {
                                    Console.WriteLine();
                                    Console.WriteLine("Twitter authentication granted for Twitter account (screen name): "
                                        + $"{_twitterInstance.ScreenName}");
                                    Console.WriteLine();
                                }
                            }
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("Warning: Couldn't find Twitter credentials.");
                                Console.WriteLine("Either the PIN code wasn't entered correctly or unknown authentication error occurred");
                                Console.WriteLine("Continuing without Twitter features...");
                                Console.WriteLine();
                            }
                        }
                        else
                        {
                            Console.WriteLine("Warning: PIN code was not provided. Continuing without Twitter features...");
                            Console.WriteLine();
                        }
                    }
                    else
                    {
                        if (await VerifyTwitterCredentialsAsync(tokens))
                        {
                            Console.WriteLine($"Current authenticated Twitter's screen name: {_twitterInstance.ScreenName}");
                            Console.WriteLine();
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Warning: Twitter integration not set. Continuing without Twitter features...");
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "TwitchBotApplication", "GetTwitterAuth()", false);
            }
        }

        private async Task<bool> VerifyTwitterCredentialsAsync(Tokens tokens)
        {
            // Verify creds
            UserResponse response = await tokens.Account.VerifyCredentialsAsync();

            if (response.Id != null)
            {
                // Allow Twitter-based commands to use user's credentials provided by the bot user
                _twitterInstance.ScreenName = response.ScreenName;
                _twitterInstance.HasCredentials = true;
                _twitterInstance.Tokens = tokens;

                return true;
            }

            Console.WriteLine();
            Console.WriteLine("Warning: Couldn't validate Twitter credentials.");
            Console.WriteLine("Either the PIN code wasn't entered correctly or unknown authentication error occurred");
            Console.WriteLine("Continuing without Twitter features...");
            Console.WriteLine();
            return false;
        }

        /// <summary>
        /// Save Twitter access token and secret values
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="accessSecret"></param>
        private void SaveTwitterAccessInfo(string accessToken, string accessSecret)
        {
            _botConfig.TwitterAccessToken = accessToken;
            _botConfig.TwitterAccessSecret = accessSecret;
            _appConfig.AppSettings.Settings.Remove("twitterAccessToken");
            _appConfig.AppSettings.Settings.Add("twitterAccessToken", accessToken);
            _appConfig.AppSettings.Settings.Remove("twitterAccessSecret");
            _appConfig.AppSettings.Settings.Add("twitterAccessSecret", accessSecret);
            _appConfig.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("TwitchBotConfiguration");
        }

        private async Task GetYouTubeAuthAsync()
        {
            try
            {
                _youTubeClientInstance.HasCredentials = await _youTubeClientInstance.GetAuthAsync(_botConfig.YouTubeClientId, _botConfig.YouTubeClientSecret);
                if (_youTubeClientInstance.HasCredentials)
                {
                    Playlist playlist = null;
                    string playlistName = _botConfig.YouTubeBroadcasterPlaylistName;
                    string defaultPlaylistName = "Twitch Song Requests";
                    SongRequestSetting songRequestSetting = await _songRequestSetting.GetSongRequestSettingAsync(_broadcasterInstance.DatabaseId);

                    if (string.IsNullOrEmpty(playlistName))
                    {
                        playlistName = defaultPlaylistName;
                    }

                    // Check if YouTube song request playlist still exists
                    if (!string.IsNullOrEmpty(_botConfig.YouTubeBroadcasterPlaylistId))
                    {
                        playlist = await _youTubeClientInstance.GetBroadcasterPlaylistByIdAsync(_botConfig.YouTubeBroadcasterPlaylistId);
                    }

                    if (playlist?.Id == null)
                    {
                        playlist = await _youTubeClientInstance.GetBroadcasterPlaylistByIdAsync(songRequestSetting.RequestPlaylistId);

                        if (playlist?.Id == null)
                        {
                            playlist = await _youTubeClientInstance.GetBroadcasterPlaylistByKeywordAsync(playlistName);

                            if (playlist?.Id == null)
                            {
                                playlist = await _youTubeClientInstance.CreatePlaylistAsync(playlistName,
                                "Songs requested via Twitch viewers on https://twitch.tv/" + _botConfig.Broadcaster
                                    + " . Playlist automatically created courtesy of https://github.com/SimpleSandman/TwitchBot");
                            }
                        }
                    }

                    _botConfig.YouTubeBroadcasterPlaylistId = playlist.Id;
                    _appConfig.AppSettings.Settings.Remove("youTubeBroadcasterPlaylistId");
                    _appConfig.AppSettings.Settings.Add("youTubeBroadcasterPlaylistId", playlist.Id);

                    _botConfig.YouTubeBroadcasterPlaylistName = playlist.Snippet.Title;
                    _appConfig.AppSettings.Settings.Remove("youTubeBroadcasterPlaylistName");
                    _appConfig.AppSettings.Settings.Add("youTubeBroadcasterPlaylistName", playlist.Snippet.Title);

                    // Find personal playlist if requested
                    playlist = null;
                    playlistName = _botConfig.YouTubePersonalPlaylistName;

                    // Check if personal YouTube playlist still exists
                    if (!string.IsNullOrEmpty(_botConfig.YouTubePersonalPlaylistId))
                    {
                        playlist = await _youTubeClientInstance.GetPlaylistByIdAsync(_botConfig.YouTubePersonalPlaylistId);
                    }

                    if (playlist?.Id == null && songRequestSetting.PersonalPlaylistId != null)
                    {
                        playlist = await _youTubeClientInstance.GetPlaylistByIdAsync(songRequestSetting.PersonalPlaylistId);
                    }

                    if (playlist?.Id != null && playlist?.Snippet != null)
                    {
                        _botConfig.YouTubePersonalPlaylistId = playlist.Id;
                        _appConfig.AppSettings.Settings.Remove("youTubePersonalPlaylistId");
                        _appConfig.AppSettings.Settings.Add("youTubePersonalPlaylistId", playlist.Id);

                        _botConfig.YouTubePersonalPlaylistName = playlist.Snippet.Title;
                        _appConfig.AppSettings.Settings.Remove("youTubePersonalPlaylistName");
                        _appConfig.AppSettings.Settings.Add("youTubePersonalPlaylistName", playlist.Snippet.Title);
                    }

                    _appConfig.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection("TwitchBotConfiguration");

                    // Save song request info into database
                    if (songRequestSetting?.Id != 0
                        && (_botConfig.YouTubeBroadcasterPlaylistId != songRequestSetting.RequestPlaylistId
                            || _botConfig.YouTubePersonalPlaylistId != (songRequestSetting.PersonalPlaylistId ?? "")
                            || _broadcasterInstance.DatabaseId != songRequestSetting.BroadcasterId))
                    {
                        await _songRequestSetting.UpdateSongRequestSettingAsync
                        (
                            _botConfig.YouTubeBroadcasterPlaylistId,
                            _botConfig.YouTubePersonalPlaylistId,
                            _broadcasterInstance.DatabaseId,
                            songRequestSetting.DjMode
                        );
                    }
                    else if (songRequestSetting?.Id == 0)
                    {
                        await _songRequestSetting.CreateSongRequestSettingAsync
                        (
                            _botConfig.YouTubeBroadcasterPlaylistId,
                            _botConfig.YouTubePersonalPlaylistId,
                            _broadcasterInstance.DatabaseId
                        );
                    }

                    // Write to a text file to allow users to show the currently playing song as a song ticker
                    // ToDo: Add config variables
                    string filename = "Twitch Chat Bot Song Request.txt";
                    string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                    using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, filename)))
                    {
                        await outputFile.WriteAsync(""); // clear old song request or create new file with empty string
                    }
                }
            }
            catch (Exception ex)
            {
                _youTubeClientInstance.HasCredentials = false; // do not allow any YouTube features for this bot until error has been resolved
                await _errHndlrInstance.LogErrorAsync(ex, "TwitchBotApplication", "GetYouTubeAuth()", false);
            }
        }

        /// <summary>
        /// Get value(s) from any PRIVMSG parameters
        /// </summary>
        /// <param name="rawMessage"></param>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        private string PrivMsgParameterValue(string rawMessage, string parameterName)
        {
            int parameterParseIndex = rawMessage.IndexOf($"{parameterName}=") + parameterName.Length + 1;
            int indexParseSign = rawMessage.IndexOf(";", parameterParseIndex);
            return rawMessage.Substring(parameterParseIndex, indexParseSign - parameterParseIndex);
        }

        /// <summary>
        /// Check if chat message is allowed
        /// </summary>
        /// <param name="chatter"></param>
        /// <returns></returns>
        private async Task<bool> IsAllowedChatMessageAsync(TwitchChatter chatter)
        {
            if (chatter.Message.Contains("clips.twitch.tv/"))
            {
                return await IsBroadcasterClipAsync(chatter);
            }
            else if (chatter.Message.Contains("twitch.tv/videos/"))
            {
                return await IsBroadcasterVideoAsync(chatter);
            }
            else if ((chatter.Message.Contains("twitch.tv/") // find username in clip link
                && chatter.Message.Contains("/clip/")
                && !chatter.Message.ToLower().Contains(_botConfig.Broadcaster.ToLower()))
                ||
                (chatter.Message.Contains("twitch.tv/") // find username in video link
                && chatter.Message.Contains("/v/")
                && !chatter.Message.ToLower().Contains(_botConfig.Broadcaster.ToLower())))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check if broadcaster clip or not
        /// </summary>
        /// <param name="chatter"></param>
        /// <returns></returns>
        private async Task<bool> IsBroadcasterClipAsync(TwitchChatter chatter)
        {
            string clipUrl = "clips.twitch.tv/";

            int slugIndex = chatter.Message.IndexOf(clipUrl) + clipUrl.Length;
            int endSlugIndex = chatter.Message.IndexOf(" ", slugIndex);

            string slug = endSlugIndex > 0 
                ? chatter.Message.Substring(slugIndex, endSlugIndex - slugIndex) 
                : chatter.Message.Substring(slugIndex);

            ClipJSON clip = await _twitchInfo.GetClipAsync(slug);

            if (clip.Broadcaster.Name == _botConfig.Broadcaster.ToLower())
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if broadcaster clip or not
        /// </summary>
        /// <param name="chatter"></param>
        /// <returns></returns>
        private async Task<bool> IsBroadcasterVideoAsync(TwitchChatter chatter)
        {
            string videoUrl = "twitch.tv/videos/";

            int videoIndex = chatter.Message.IndexOf(videoUrl) + videoUrl.Length;
            int endVideoIndex = chatter.Message.IndexOf(" ", videoIndex);

            string videoId = endVideoIndex > 0
                ? chatter.Message.Substring(videoIndex, endVideoIndex - videoIndex)
                : chatter.Message.Substring(videoIndex);

            VideoJSON video = await _twitchInfo.GetVideoAsync(videoId);

            if (video.Channel.Name == _botConfig.Broadcaster.ToLower())
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Load a custom command made by the broadcaster
        /// </summary>
        /// <param name="chatter"></param>
        private async void FindCustomCommand(TwitchChatter chatter)
        {
            try
            {
                /* Display the sound commands to the chatter in alphabetical order */
                if (chatter.Message == "!sound")
                {
                    string orderedCommands = "";
                    foreach (string soundCommand in _customCommandInstance.GetSoundCommands().Select(c => c.Name))
                    {
                        orderedCommands += $"{soundCommand}, ";
                    }

                    string message = $"These are the available sound commands: {orderedCommands.ReplaceLastOccurrence(", ", "")}";
                    if (string.IsNullOrEmpty(orderedCommands))
                    {
                        message = $"There are no sound commands at this time @{chatter.Username}";
                    }

                    _irc.SendPublicChatMessage(message);
                    return;
                }

                /* Else use the custom command from the database */
                // Get current game name
                ChannelJSON json = await _twitchInfo.GetBroadcasterChannelByIdAsync();
                string gameTitle = json.Game;

                // Grab game id in order to find party member
                TwitchGameCategory game = await _gameDirectory.GetGameIdAsync(gameTitle);
                CustomCommand customCommand = _customCommandInstance.FindCustomCommand(chatter.Message.ToLower(), game?.Id);

                if (customCommand == null || _cooldownUsersInstance.IsCommandOnCooldown(customCommand.Name, chatter, _irc, customCommand.IsGlobalCooldown))
                {
                    return;
                }

                int balance = await _bank.CheckBalanceAsync(chatter.Username, _broadcasterInstance.DatabaseId);

                if (balance == -1)
                {
                    _irc.SendPublicChatMessage($"You are not currently banking with us at the moment. " 
                        + $"Please talk to a moderator about acquiring {_botConfig.CurrencyType} @{chatter.DisplayName}");

                    return;
                }
                else if (balance < customCommand.CurrencyCost)
                {
                    _irc.SendPublicChatMessage($"I'm sorry! {customCommand.Name} costs {customCommand.CurrencyCost} " 
                        + $"{_botConfig.CurrencyType} @{chatter.DisplayName}");

                    return;
                }              

                // Send either a sound command or a normal text command
                if (customCommand.IsSound)
                {
                    using (SoundPlayer player = new SoundPlayer(customCommand.Message))
                    {
                        player.Play();
                    }
                }
                else if (!customCommand.IsSound)
                {
                    _irc.SendPublicChatMessage(customCommand.Message);
                }

                // Check and add cooldown if necessary
                if (customCommand.CooldownSec > 0)
                {
                    _cooldownUsersInstance.AddCooldown(chatter, DateTime.Now.AddSeconds(customCommand.CooldownSec), customCommand.Name);
                }

                // Make the chatter pay the price if necessary
                if (customCommand.CurrencyCost > 0)
                {
                    balance -= customCommand.CurrencyCost;
                    await _bank.UpdateFundsAsync(chatter.Username, _broadcasterInstance.DatabaseId, balance);

                    _irc.SendPublicChatMessage($"@{chatter.DisplayName} spent {customCommand.CurrencyCost} " + 
                        $"{_botConfig.CurrencyType} for {customCommand.Name} and now has {balance} {_botConfig.CurrencyType}");
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "TwitchBotApplication", "FindCustomCommand(TwitchChatter)", false, "N/A", chatter.Message);
            }
        }
    }
}
