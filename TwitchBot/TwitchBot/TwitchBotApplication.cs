using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.YouTube.v3.Data;

using Tweetinvi;
using Tweetinvi.Models;

using TwitchBot.Commands;
using TwitchBot.Configuration;
using TwitchBot.Libraries;
using TwitchBot.Models;
using TwitchBot.Models.JSON;
using TwitchBot.Services;
using TwitchBot.Threads;

using TwitchBotDb.Models;
using TwitchBotDb.Temp;

namespace TwitchBot
{
    public class TwitchBotApplication
    {
        private System.Configuration.Configuration _appConfig;
        private TwitchBotConfigurationSection _botConfig;
        private IrcClient _irc;
        private TimeoutCmd _timeout;
        private CmdBrdCstr _cmdBrdCstr;
        private CmdMod _cmdMod;
        private CmdGen _cmdGen;
        private CmdVip _cmdVip;
        private bool _isManualSongRequestAvail;
        private bool _isYouTubeSongRequestAvail;
        private bool _hasTwitterInfo;
        private bool _hasYouTubeAuth;
        private List<string> _multiStreamUsers;
        private List<string> _greetedUsers;
        private Queue<string> _gameQueueUsers;
        private List<CooldownUser> _cooldownUsers;
        private SpotifyWebClient _spotify;
        private TwitchInfoService _twitchInfo;
        private FollowerService _follower;
        private FollowerSubscriberListener _followerSubscriberListener;
        private BankService _bank;
        private SongRequestBlacklistService _songRequestBlacklist;
        private ManualSongRequestService _manualSongRequest;
        private PartyUpService _partyUp;
        private GameDirectoryService _gameDirectory;
        private QuoteService _quote;
        private SongRequestSettingService _songRequestSetting;
        private InGameUsernameService _ign;
        private BankHeist _bankHeist;
        private BossFight _bossFight;
        private TwitchChatterListener _twitchChatterListener;
        private TwitchChatterList _twitchChatterListInstance = TwitchChatterList.Instance;
        private ErrorHandler _errHndlrInstance = ErrorHandler.Instance;
        private YoutubeClient _youTubeClientInstance = YoutubeClient.Instance;
        private BroadcasterSingleton _broadcasterInstance = BroadcasterSingleton.Instance;
        private BankHeistSingleton _bankHeistInstance = BankHeistSingleton.Instance;
        private BossFightSingleton _bossFightInstance = BossFightSingleton.Instance;

        public TwitchBotApplication(System.Configuration.Configuration appConfig, TwitchInfoService twitchInfo, SongRequestBlacklistService songRequestBlacklist,
            FollowerService follower, BankService bank, FollowerSubscriberListener followerListener, ManualSongRequestService manualSongRequest, PartyUpService partyUp,
            GameDirectoryService gameDirectory, QuoteService quote, BankHeist bankHeist, TwitchChatterListener twitchChatterListener,
            BossFight bossFight, SongRequestSettingService songRequestSetting, InGameUsernameService ign)
        {
            _appConfig = appConfig;
            _botConfig = appConfig.GetSection("TwitchBotConfiguration") as TwitchBotConfigurationSection;
            _irc = new IrcClient();
            _isManualSongRequestAvail = false;
            _isYouTubeSongRequestAvail = false;
            _hasTwitterInfo = false;
            _hasYouTubeAuth = false;
            _timeout = new TimeoutCmd();
            _cooldownUsers = new List<CooldownUser>();
            _multiStreamUsers = new List<string>();
            _greetedUsers = new List<string>();
            _gameQueueUsers = new Queue<string>();
            _twitchInfo = twitchInfo;
            _follower = follower;
            _followerSubscriberListener = followerListener;
            _bank = bank;
            _songRequestBlacklist = songRequestBlacklist;
            _manualSongRequest = manualSongRequest;
            _partyUp = partyUp;
            _gameDirectory = gameDirectory;
            _quote = quote;
            _bankHeist = bankHeist;
            _twitchChatterListener = twitchChatterListener;
            _bossFight = bossFight;
            _songRequestSetting = songRequestSetting;
            _ign = ign;
        }

        public async Task RunAsync()
        {
            try
            {
                // ToDo: Check version number of application
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Message: " + ex.Message);
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
                await SetBroadcasterIds();

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
                await _spotify.Connect();

                // Password from www.twitchapps.com/tmi/
                // include the "oauth:" portion
                // Use chat bot's oauth
                /* main server: irc.chat.twitch.tv, 6667 */
                _irc.Connect(_botConfig.BotName.ToLower(), _botConfig.TwitchOAuth, _botConfig.Broadcaster.ToLower());
                _cmdGen = new CmdGen(_irc, _spotify, _botConfig, _broadcasterInstance.DatabaseId, _twitchInfo, _bank, _follower,
                    _songRequestBlacklist, _manualSongRequest, _partyUp, _gameDirectory, _quote, _ign);
                _cmdBrdCstr = new CmdBrdCstr(_irc, _botConfig, _broadcasterInstance.DatabaseId, _appConfig, _songRequestBlacklist,
                    _twitchInfo, _gameDirectory, _songRequestSetting, _ign);
                _cmdMod = new CmdMod(_irc, _timeout, _botConfig, _broadcasterInstance.DatabaseId, _appConfig, _bank, _twitchInfo,
                    _manualSongRequest, _quote, _partyUp, _gameDirectory);
                _cmdVip = new CmdVip(_irc, _timeout, _botConfig, _broadcasterInstance.DatabaseId, _appConfig, _bank, _twitchInfo,
                    _manualSongRequest, _quote, _partyUp, _gameDirectory);

                /* Whisper broadcaster bot settings */
                Console.WriteLine();
                Console.WriteLine("---> Extra Bot Settings <---");
                Console.WriteLine($"Discord link: {_botConfig.DiscordLink}");
                Console.WriteLine($"Currency type: {_botConfig.CurrencyType}");
                Console.WriteLine($"Enable Auto Tweets: {_botConfig.EnableTweets}");
                Console.WriteLine($"Enable Auto Display Songs: {_botConfig.EnableDisplaySong}");
                Console.WriteLine($"Stream latency: {_botConfig.StreamLatency} second(s)");
                Console.WriteLine($"Regular follower hours: {_botConfig.RegularFollowerHours}");
                Console.WriteLine();

                /* Configure YouTube song request from user's YT account (request permission if needed) */
                try
                {
                    _hasYouTubeAuth = await _youTubeClientInstance.GetAuthAsync(_botConfig.YouTubeClientId, _botConfig.YouTubeClientSecret);
                    if (_hasYouTubeAuth)
                    {
                        Playlist playlist = null;
                        string playlistName = _botConfig.YouTubeBroadcasterPlaylistName;
                        string defaultPlaylistName = "Twitch Song Requests";
                        SongRequestSetting songRequestSetting = await _songRequestSetting.GetSongRequestSetting(_broadcasterInstance.DatabaseId);

                        if (string.IsNullOrEmpty(playlistName))
                        {
                            playlistName = defaultPlaylistName;
                        }

                        // Check if YouTube song request playlist still exists
                        if (!string.IsNullOrEmpty(_botConfig.YouTubeBroadcasterPlaylistId))
                        {
                            playlist = await _youTubeClientInstance.GetBroadcasterPlaylistById(_botConfig.YouTubeBroadcasterPlaylistId);
                        }

                        if (playlist?.Id == null)
                        {
                            playlist = await _youTubeClientInstance.GetBroadcasterPlaylistById(songRequestSetting.RequestPlaylistId);

                            if (playlist?.Id == null)
                            {
                                playlist = await _youTubeClientInstance.GetBroadcasterPlaylistByKeyword(playlistName);

                                if (playlist?.Id == null)
                                {
                                    playlist = await _youTubeClientInstance.CreatePlaylist(playlistName,
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
                            playlist = await _youTubeClientInstance.GetPlaylistById(_botConfig.YouTubePersonalPlaylistId);
                        }

                        if (playlist?.Id == null && songRequestSetting.PersonalPlaylistId != null)
                        {
                            playlist = await _youTubeClientInstance.GetPlaylistById(songRequestSetting.PersonalPlaylistId);
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
                            await _songRequestSetting.UpdateSongRequestSetting(
                                _botConfig.YouTubeBroadcasterPlaylistId,
                                _botConfig.YouTubePersonalPlaylistId,
                                _broadcasterInstance.DatabaseId,
                                songRequestSetting.DjMode);
                        }
                        else if (songRequestSetting?.Id == 0)
                        {
                            await _songRequestSetting.CreateSongRequestSetting(
                                _botConfig.YouTubeBroadcasterPlaylistId,
                                _botConfig.YouTubePersonalPlaylistId,
                                _broadcasterInstance.DatabaseId);
                        }

                        // Save credentials into JSON file for WPF app to reference
                        YoutubePlaylistInfo.Save(_botConfig.YouTubeClientId, _botConfig.YouTubeClientSecret, 
                            _botConfig.TwitchBotApiLink, _broadcasterInstance.DatabaseId);
                    }
                }
                catch (Exception ex)
                {
                    _hasYouTubeAuth = false; // do not allow any YouTube features for this bot until error has been resolved
                    await _errHndlrInstance.LogError(ex, "TwitchBotApplication", "RunAsync()", false);
                }

                /* Start listening for delayed messages */
                DelayMsg delayMsg = new DelayMsg(_irc);
                delayMsg.Start();

                /* Grab list of chatters from channel */
                _twitchChatterListener.Start();

                /* Pull list of followers and check experience points for stream leveling */
                _followerSubscriberListener.Start(_irc, _broadcasterInstance.DatabaseId);

                /* Get list of timed out users from database */
                await SetListTimeouts();

                /* Load/create settings and start the queue for the heist */
                await _bankHeistInstance.LoadSettings(_broadcasterInstance.DatabaseId, _botConfig.TwitchBotApiLink);
                _bankHeist.Start(_irc, _broadcasterInstance.DatabaseId);

                /* Load/create settings and start the queue for the boss fight */
                // Get current game name
                ChannelJSON json = await _twitchInfo.GetBroadcasterChannelById();
                string gameTitle = json.Game;

                // Grab game id in order to find party member
                TwitchGameCategory game = await _gameDirectory.GetGameId(gameTitle);

                if (string.IsNullOrEmpty(gameTitle))
                {
                    _irc.SendPublicChatMessage("WARNING: I cannot see the name of the game. It's currently set to either NULL or EMPTY. "
                        + "Please have the chat verify that the game has been set for this stream. "
                        + $"If the error persists, please have @{_botConfig.Broadcaster.ToLower()} retype the game in their Twitch Live Dashboard. "
                        + "If this error shows up again and your chat can see the game set for the stream, please contact my master with !support in this chat");
                }

                /* Load/create settings and start the queue for the boss fight */
                await _bossFightInstance.LoadSettings(_broadcasterInstance.DatabaseId, game?.Id, _botConfig.TwitchBotApiLink);
                _bossFight.Start(_irc, _broadcasterInstance.DatabaseId);

                /* Ping to twitch server to prevent auto-disconnect */
                PingSender ping = new PingSender(_irc);
                ping.Start();

                /* Send reminders of certain events */
                ChatReminder chatReminder = new ChatReminder(_irc, _broadcasterInstance.DatabaseId, _botConfig.TwitchBotApiLink, _twitchInfo, _gameDirectory);
                chatReminder.Start();

                /* Authenticate to Twitter if possible */
                GetTwitterAuth();

                Console.WriteLine("===== Time to get to work! =====");
                Console.WriteLine();

                /* Finished setup, time to start */
                await GetChatBox(_isManualSongRequestAvail, _isYouTubeSongRequestAvail, _botConfig.TwitchAccessToken, _hasTwitterInfo, _hasYouTubeAuth);
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "TwitchBotApplication", "RunAsync()", true);
            }
        }        

        /// <summary>
        /// Monitor chat box for commands
        /// </summary>
        /// <param name="isManualSongRequestAvail"></param>
        /// <param name="isYouTubeSongRequestAvail"></param>
        /// <param name="twitchAccessToken"></param>
        /// <param name="hasTwitterInfo"></param>
        /// <param name="hasYouTubeAuth"></param>
        private async Task GetChatBox(bool isManualSongRequestAvail, bool isYouTubeSongRequestAvail, string twitchAccessToken, bool hasTwitterInfo, bool hasYouTubeAuth)
        {
            try
            {
                /* Master loop */
                while (true)
                {
                    // Read any message inside the chat room
                    string rawMessage = await _irc.ReadMessage();
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

                            // Purge any clips that aren't from the broadcaster that a viewer posts
                            if (_botConfig.Broadcaster.ToLower() != chatter.Username
                                && !chatter.Badges.Contains("moderator")
                                && !await IsAllowedChatMessage(chatter))
                            {
                                _irc.ClearMessage(chatter);
                                _irc.SendPublicChatMessage($"Please refrain from posting a message that isn't for this channel @{chatter.Username}");
                                continue;
                            }

                            await GreetNewUser(chatter);

                            #region Broadcaster Commands
                            if (username == _botConfig.Broadcaster.ToLower())
                            {
                                switch (message)
                                {
                                    case "!settings": // Display bot settings
                                        _cmdBrdCstr.CmdBotSettings();
                                        break;
                                    case "!exit": // Stop running the bot
                                        _cmdBrdCstr.CmdExitBot();
                                        break;
                                    case "!spotifyconnect": // Manually connect to Spotify
                                        await _spotify.Connect();
                                        break;
                                    case "!spotifyplay": // Press local Spotify play button [>]
                                        await _spotify.Play();
                                        break;
                                    case "!spotifypause": // Press local Spotify pause button [||]
                                        await _spotify.Pause();
                                        break;
                                    case "!spotifyprev": // Press local Spotify previous button [|<]
                                    case "!spotifyback":
                                        await _spotify.SkipToPreviousPlayback();
                                        break;
                                    case "!spotifynext": // Press local Spotify next (skip) button [>|]
                                    case "!spotifyskip":
                                        await _spotify.SkipToNextPlayback();
                                        break;
                                    case "!sendtweet on": // Enables tweets to be sent out from this bot (both auto publish tweets and manual tweets)
                                        _cmdBrdCstr.CmdEnableTweet(hasTwitterInfo);
                                        break;
                                    case "!sendtweet off": // Disables tweets from being sent out from this bot
                                        _cmdBrdCstr.CmdDisableTweet(hasTwitterInfo);
                                        break;
                                    case "!rbsrmode on": // Enables viewers to request songs (default off)
                                        isManualSongRequestAvail = await _cmdBrdCstr.CmdEnableManualSrMode(isManualSongRequestAvail);
                                        break;
                                    case "!rbsrmode off": // Disables viewers to request songs (default off)
                                        isManualSongRequestAvail = await _cmdBrdCstr.CmdDisableManualSrMode(isManualSongRequestAvail);
                                        break;
                                    case "!ytsrmode on": // Enables viewers to request songs (default off)
                                        isYouTubeSongRequestAvail = await _cmdBrdCstr.CmdEnableYouTubeSrMode(isYouTubeSongRequestAvail);
                                        break;
                                    case "!ytsrmode off": // Disables viewers to request songs (default off)
                                        isYouTubeSongRequestAvail = await _cmdBrdCstr.CmdDisableYouTubeSrMode(isYouTubeSongRequestAvail);
                                        break;
                                    case "!displaysongs on": // Enables songs from local Spotify to be displayed inside the chat
                                        _cmdBrdCstr.CmdEnableDisplaySongs();
                                        break;
                                    case "!displaysongs off": // Disables songs from local Spotify to be displayed inside the chat
                                        _cmdBrdCstr.CmdDisableDisplaySongs();
                                        break;
                                    case "!resetsrbl": // Reset the entire song request blacklist
                                        await _cmdBrdCstr.CmdResetSongRequestBlacklist();
                                        break;
                                    case "!showsrbl": // Show the song request blacklist
                                        await _cmdBrdCstr.CmdListSongRequestBlacklist();
                                        break;
                                    case "!live": // Sends an announcement tweet saying the broadcaster is live
                                        await _cmdBrdCstr.CmdLive(hasTwitterInfo);
                                        break;
                                    case "!refreshreminders": // Manually refresh reminders
                                        await _cmdBrdCstr.CmdRefreshReminders();
                                        break;
                                    case "!refreshbossfight": // Manually refresh boss fight
                                        await _cmdBrdCstr.CmdRefreshBossFight();
                                        break;
                                    case "!resetytsr": // Reset the YouTube song request playlist
                                        await _cmdBrdCstr.CmdResetYoutubeSongRequestList(hasYouTubeAuth);
                                        break;
                                    case "!djmode on": // Enable DJing mode for YouTube song requests 
                                        await _cmdBrdCstr.CmdEnableDjMode();
                                        break;
                                    case "!djmode off": // Disable DJing mode for YouTube song requests
                                        await _cmdBrdCstr.CmdDisableDjMode();
                                        break;
                                    case "!deleteign":
                                        await _cmdBrdCstr.CmdDeleteIgn();
                                        break;
                                    default: // Check commands that depend on special cases
                                        /* Sends a manual tweet (if credentials have been provided) */
                                        if (message.StartsWith("!tweet "))
                                            _cmdBrdCstr.CmdTweet(hasTwitterInfo, chatter.Message);

                                        /* Add song or artist to song request blacklist */
                                        else if (message.StartsWith("!srbl "))
                                            await _cmdBrdCstr.CmdAddSongRequestBlacklist(chatter.Message);

                                        /* Remove song or artist from song request blacklist */
                                        else if (message.StartsWith("!delsrbl "))
                                            await _cmdBrdCstr.CmdRemoveSongRequestBlacklist(chatter.Message);

                                        /* Set regular follower hours for dedicated followers */
                                        else if (message.StartsWith("!setregularhours "))
                                            _cmdBrdCstr.CmdSetRegularFollowerHours(chatter.Message);

                                        /* Set YouTube personal playlist as a backup when new requests  */
                                        else if (message.StartsWith("!setpersonalplaylistid "))
                                            await _cmdBrdCstr.CmdSetPersonalYoutubePlaylistById(chatter.Message);

                                        /* Set in-game (user) name for the current game */
                                        else if (message.StartsWith("!setgameign ") || message.StartsWith("!setgameid "))
                                            await _cmdBrdCstr.CmdSetGameIgn(chatter.Message);

                                        /* Set in-game (user) name for any non-specified game */
                                        else if (message.StartsWith("!setgenericign ") || message.StartsWith("!setgenericid "))
                                            await _cmdBrdCstr.CmdSetGenericIgn(chatter.Message);

                                        /* insert more broadcaster commands here */
                                        break;
                                } 
                            }
                            #endregion Broadcaster Commands

                            if (!await IsUserTimedout(chatter))
                            {
                                #region Moderator Commands
                                if (username == _botConfig.Broadcaster.ToLower() || chatter.Badges.Contains("moderator"))
                                {
                                    switch (message)
                                    {                                        
                                        case "!resetrbsr": // Resets the song request queue
                                            await _cmdMod.CmdResetManualSr();
                                            break;
                                        case "!modafk": // Tell the stream the specified moderator will be AFK
                                            _cmdMod.CmdModAfk(chatter);
                                            break;
                                        case "!modback": // Tell the stream the specified moderator has returned
                                            _cmdMod.CmdModBack(chatter);
                                            break;
                                        case "!resetjoin": // Resets game queue of users that want to play with the broadcaster
                                            _gameQueueUsers = await _cmdMod.CmdResetJoin(chatter, _gameQueueUsers);
                                            break;
                                        default: // Check commands that depend on special cases
                                            /* Takes money away from a user */
                                            if (message.StartsWith("!charge ") && message.Contains("@"))
                                                await _cmdMod.CmdCharge(chatter);

                                            /* Gives money to user */
                                            else if (message.StartsWith("!deposit ") && message.Contains("@"))
                                                await _cmdMod.CmdDeposit(chatter);

                                            /* Bot-specific timeout on a user for a set amount of time */
                                            else if (message.StartsWith("!addtimeout ") && message.Contains("@"))
                                                await _cmdMod.CmdAddTimeout(chatter);

                                            /* Remove bot-specific timeout on a user for a set amount of time */
                                            else if (message.StartsWith("!deltimeout @"))
                                                await _cmdMod.CmdDeleteTimeout(chatter);

                                            /* Set delay for messages based on the latency of the stream */
                                            else if (message.StartsWith("!setlatency "))
                                                _cmdMod.CmdSetLatency(chatter);

                                            /* Gives every viewer a set amount of currency */
                                            else if (message.StartsWith("!bonusall "))
                                                await _cmdMod.CmdBonusAll(chatter);

                                            /* Updates the title of the Twitch channel */
                                            else if (message.StartsWith("!updatetitle "))
                                                await _cmdMod.CmdUpdateTitle(chatter);

                                            /* Updates the game of the Twitch channel */
                                            else if (message.StartsWith("!updategame "))
                                                await _cmdMod.CmdUpdateGame(chatter, hasTwitterInfo);

                                            /* insert moderator commands here */
                                            break;
                                    }
                                    
                                }
                                #endregion Moderator Commands

                                #region VIP Commands
                                if (username == _botConfig.Broadcaster.ToLower() || chatter.Badges.Contains("moderator") || chatter.Badges.Contains("vip"))
                                {
                                    switch (message)
                                    {
                                        case "!resetmsl": // Reset MultiStream link so link can be reconfigured
                                            _multiStreamUsers = await _cmdVip.CmdResetMultiStreamLink(chatter, _multiStreamUsers);
                                            break;
                                        case "!popjoin": // Pops user from the queue of users that want to play with the broadcaster
                                            _gameQueueUsers = await _cmdVip.CmdPopJoin(chatter, _gameQueueUsers);
                                            break;
                                        case "!poprbsr": // Removes the first song in the queue of song requests
                                            await _cmdVip.CmdPopManualSr();
                                            break;
                                        case "!poppartyuprequest": // Removes first party memeber in queue of party up requests
                                            await _cmdVip.CmdPopPartyUpRequest();
                                            break;
                                        default:
                                            /* Add MultiStream user to link */
                                            if (message.StartsWith("!addmsl "))
                                                _multiStreamUsers = await _cmdVip.CmdAddMultiStreamUser(chatter, _multiStreamUsers);

                                            /* Add a broadcaster quote */
                                            else if (message.StartsWith("!addquote "))
                                                await _cmdVip.CmdAddQuote(chatter);

                                            /* Display the streamer's channel and game status */
                                            else if (message.StartsWith("!streamer @") || message.StartsWith("!so @"))
                                                await _cmdVip.CmdPromoteStreamer(chatter);

                                            /* insert vip commands here */
                                            break;
                                    }
                                }
                                #endregion VIP Commands

                                #region Viewer Commands
                                switch (message)
                                {
                                    case "!cmds": // Display some viewer commands a link to command documentation
                                    case "!commands":
                                        _cmdGen.CmdDisplayCmds();
                                        break;
                                    case "!hello": // Display a static greeting
                                        _cmdGen.CmdHello(chatter);
                                        break;
                                    case "!discord": // Displays Discord link into chat (if available)
                                        _cmdMod.CmdDiscord();
                                        break;
                                    case "!utctime": // Display the current time in UTC (Coordinated Universal Time)
                                        _cmdGen.CmdUtcTime();
                                        break;
                                    case "!hosttime": // Display the current time in the time zone the host is located
                                    case "!mytime":
                                        _cmdGen.CmdHostTime();
                                        break;
                                    case "!uptime": // Shows how long the broadcaster has been streaming
                                        await _cmdGen.CmdUptime();
                                        break;
                                    case "!rbsrl": // Display list of requested songs
                                        await _cmdGen.CmdManualSrList(isManualSongRequestAvail, chatter);
                                        break;
                                    case "!rbsl": // Display link of list of songs to request
                                        _cmdGen.CmdManualSrLink(isManualSongRequestAvail, chatter);
                                        break;
                                    case "!spotifysong": // Displays the current song being played from Spotify
                                        await _cmdGen.CmdSpotifyCurrentSong(chatter);
                                        break;
                                    case "!partyuprequestlist": // Check what other user's have requested
                                        await _cmdGen.CmdPartyUpRequestList();
                                        break;
                                    case "!partyuplist": // Check what party members are available (if game is part of the party up system)
                                        await _cmdGen.CmdPartyUpList();
                                        break;
                                    case "!followsince": // Display how long a user has been following the broadcaster
                                    case "!followage":
                                        await _cmdGen.CmdFollowSince(chatter);
                                        break;
                                    case "!rank": // Display follower's stream rank
                                        await _cmdGen.CmdViewRank(chatter);
                                        break;
                                    case "!ytsl": // Display YouTube link to song request playlist 
                                    case "!playlist":
                                    case "!songlist":
                                        _cmdGen.CmdYouTubeSongRequestList(hasYouTubeAuth);
                                        break;
                                    case "!msl": // Display MultiStream link
                                        _cmdGen.CmdMultiStreamLink(chatter, _multiStreamUsers);
                                        break;
                                    case "!ranktop3": // Display the top 3 highest ranking users
                                        await _cmdGen.CmdLeaderboardRank(chatter);
                                        break;
                                    case "!joinlist": // Show the users that want to play with the broadcaster
                                        await _cmdGen.CmdListJoin(chatter, _gameQueueUsers);
                                        break;
                                    case "!join": // Request to play with the broadcaster
                                        _gameQueueUsers = await _cmdGen.CmdJoin(chatter, _gameQueueUsers);
                                        break;
                                    case "!sub": // Show the subscribe link (if broadcaster is either Affiliate/Partnered)
                                        await _cmdGen.CmdSubscribe();
                                        break;
                                    case "!raid": // Join the boss fight with a pre-defined amount of currency set by broadcaster
                                        await _cmdGen.CmdBossFight(chatter);
                                        break;
                                    case "!twitter": // Display the broadcaster's twitter page
                                        _cmdGen.CmdTwitterLink(hasTwitterInfo, User.GetAuthenticatedUser()?.UserIdentifier?.ScreenName);
                                        break;
                                    case "!support": // Display this project and creator's info
                                        _cmdGen.CmdSupport();
                                        break;
                                    case "!song": // Display current song that's being played from WPF app
                                    case "!currentsong":
                                        await _cmdGen.CmdYouTubeCurrentSong(hasYouTubeAuth, chatter);
                                        break;
                                    case "!ign": // Display the broadcaster's in-game (user) name based on what they're streaming
                                    case "!gt":
                                    case "!fc":
                                    case "!allign": // Display all of the broadcaster's in-game (user) names
                                    case "!allgt":
                                    case "!allfc":
                                        await _cmdGen.CmdInGameUsername(chatter);
                                        break;
                                    case "!pika": // Proof of concept for sound commands
                                        // ToDo: Make sound commands dynamic (both sound and role access to command)
                                        if (chatter.DisplayName == "Teimoli" || chatter.DisplayName == "Simple_Sandman")
                                        {
                                            _cmdGen.PlayCommandSound(@"C:/temp/smashbros_pikachu.wav", 40);
                                        }
                                        break;
                                    default: // Check commands that depend on special cases
                                        /* Request a song for the host to play */
                                        if (message.StartsWith("!rbsr "))
                                            await _cmdGen.CmdManualSr(isManualSongRequestAvail, chatter);

                                        /* Slaps a user and rates its effectiveness */
                                        else if (message.StartsWith("!slap @") && !IsUserOnCooldown(chatter, "!slap"))
                                        {
                                            DateTime cooldown = await _cmdGen.CmdSlap(chatter);
                                            if (cooldown > DateTime.Now)
                                            {
                                                _cooldownUsers.Add(new CooldownUser
                                                {
                                                    Username = chatter.Username,
                                                    Cooldown = cooldown,
                                                    Command = "!slap",
                                                    Warned = false
                                                });
                                            }
                                        }

                                        /* Stabs a user and rates its effectiveness */
                                        else if (message.StartsWith("!stab @") && !IsUserOnCooldown(chatter, "!stab"))
                                        {
                                            DateTime cooldown = await _cmdGen.CmdStab(chatter);
                                            if (cooldown > DateTime.Now)
                                            {
                                                _cooldownUsers.Add(new CooldownUser
                                                {
                                                    Username = chatter.Username,
                                                    Cooldown = cooldown,
                                                    Command = "!stab",
                                                    Warned = false
                                                });
                                            }
                                        }

                                        /* Shoots a viewer's random body part */
                                        else if (message.StartsWith("!shoot @") && !IsUserOnCooldown(chatter, "!shoot"))
                                        {
                                            DateTime cooldown = await _cmdGen.CmdShoot(chatter);
                                            if (cooldown > DateTime.Now)
                                            {
                                                _cooldownUsers.Add(new CooldownUser
                                                {
                                                    Username = chatter.Username,
                                                    Cooldown = cooldown,
                                                    Command = "!shoot",
                                                    Warned = false
                                                });
                                            }
                                        }

                                        /* Throws an item at a viewer and rates its effectiveness against the victim */
                                        else if (message.StartsWith("!throw ") && message.Contains("@") && !IsUserOnCooldown(chatter, "!throw"))
                                        {
                                            DateTime cooldown = await _cmdGen.CmdThrow(chatter);
                                            if (cooldown > DateTime.Now)
                                            {
                                                _cooldownUsers.Add(new CooldownUser
                                                {
                                                    Username = chatter.Username,
                                                    Cooldown = cooldown,
                                                    Command = "!throw",
                                                    Warned = false
                                                });
                                            }
                                        }

                                        /* Request party member if game and character exists in party up system */
                                        else if (message.StartsWith("!partyup "))
                                            await _cmdGen.CmdPartyUp(chatter);

                                        /* Check user's account balance */
                                        else if (message.Equals($"!{_botConfig.CurrencyType.ToLower()}"))
                                            await _cmdGen.CmdCheckFunds(chatter);

                                        /* Gamble money away */
                                        else if (message.StartsWith("!gamble ") && !IsUserOnCooldown(chatter, "!gamble"))
                                        {
                                            DateTime cooldown = await _cmdGen.CmdGamble(chatter);
                                            if (cooldown > DateTime.Now)
                                            {
                                                _cooldownUsers.Add(new CooldownUser
                                                {
                                                    Username = chatter.Username,
                                                    Cooldown = cooldown,
                                                    Command = "!gamble",
                                                    Warned = false
                                                });
                                            }
                                        }

                                        /* Display random broadcaster quote */
                                        else if (message.Equals("!quote") && !IsUserOnCooldown(chatter, "!quote"))
                                        {
                                            DateTime cooldown = await _cmdGen.CmdQuote();
                                            if (cooldown > DateTime.Now)
                                            {
                                                _cooldownUsers.Add(new CooldownUser
                                                {
                                                    Username = chatter.Username,
                                                    Cooldown = cooldown,
                                                    Command = "!quote",
                                                    Warned = false
                                                });
                                            }
                                        }

                                        /* Add song request to YouTube playlist */
                                        else if ((message.StartsWith("!ytsr ") || message.StartsWith("!sr ") || message.StartsWith("!songrequest ")) && !IsUserOnCooldown(chatter, "!ytsr"))
                                        {
                                            DateTime cooldown = await _cmdGen.CmdYouTubeSongRequest(chatter, hasYouTubeAuth, isYouTubeSongRequestAvail);
                                            if (cooldown > DateTime.Now)
                                            {
                                                _cooldownUsers.Add(new CooldownUser
                                                {
                                                    Username = chatter.Username,
                                                    Cooldown = cooldown,
                                                    Command = "!ytsr",
                                                    Warned = false
                                                });
                                            }
                                        }

                                        /* Display Magic 8-ball response */
                                        else if (message.StartsWith("!8ball ") && !IsUserOnCooldown(chatter, "!8ball"))
                                        {
                                            DateTime cooldown = await _cmdGen.CmdMagic8Ball(chatter);
                                            if (cooldown > DateTime.Now)
                                            {
                                                _cooldownUsers.Add(new CooldownUser
                                                {
                                                    Username = chatter.Username,
                                                    Cooldown = cooldown,
                                                    Command = "!8ball",
                                                    Warned = false
                                                });
                                            }
                                        }

                                        /* Disply the top 3 richest users */
                                        else if (message.Equals($"!{_botConfig.CurrencyType.ToLower()}top3"))
                                            await _cmdGen.CmdLeaderboardCurrency(chatter);

                                        /* Play russian roulette */
                                        else if (message.Equals("!roulette") && !IsUserOnCooldown(chatter, "!roulette"))
                                        {
                                            DateTime cooldown = await _cmdGen.CmdRussianRoulette(chatter);
                                            if (cooldown > DateTime.Now)
                                            {
                                                _cooldownUsers.Add(new CooldownUser
                                                {
                                                    Username = chatter.Username,
                                                    Cooldown = cooldown,
                                                    Command = "!roulette",
                                                    Warned = false
                                                });
                                            }
                                        }

                                        /* Join the heist and gamble your currency for a higher payout */
                                        else if (message.StartsWith("!bankheist ") || message.StartsWith("!heist "))
                                            await _cmdGen.CmdBankHeist(chatter);

                                        /* Tell the broadcaster a user is lurking */
                                        else if (message.Equals("!lurk") && !IsUserOnCooldown(chatter, "!lurk"))
                                        {
                                            DateTime cooldown = await _cmdGen.CmdLurk(chatter);
                                            if (cooldown > DateTime.Now)
                                            {
                                                _cooldownUsers.Add(new CooldownUser
                                                {
                                                    Username = chatter.Username,
                                                    Cooldown = cooldown,
                                                    Command = "!lurk",
                                                    Warned = false
                                                });
                                            }
                                        }

                                        /* Tell the broadcaster a user is no longer lurking */
                                        else if (message.Equals("!unlurk") && !IsUserOnCooldown(chatter, "!unlurk"))
                                        {
                                            DateTime cooldown = await _cmdGen.CmdUnlurk(chatter);
                                            if (cooldown > DateTime.Now)
                                            {
                                                _cooldownUsers.Add(new CooldownUser
                                                {
                                                    Username = chatter.Username,
                                                    Cooldown = cooldown,
                                                    Command = "!unlurk",
                                                    Warned = false
                                                });
                                            }
                                        }

                                        /* Give funds to another chatter */
                                        else if (message.StartsWith("!give ") && !IsUserOnCooldown(chatter, "!give"))
                                        {
                                            DateTime cooldown = await _cmdGen.CmdGiveFunds(chatter);
                                            if (cooldown > DateTime.Now)
                                            {
                                                _cooldownUsers.Add(new CooldownUser
                                                {
                                                    Username = chatter.Username,
                                                    Cooldown = cooldown,
                                                    Command = "!give",
                                                    Warned = false
                                                });
                                            }
                                        }

                                        /* add more general commands here */
                                        break;
                                }
                                #endregion Viewer Commands
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
                Console.WriteLine(ex.Message);
                await _errHndlrInstance.LogError(ex, "TwitchBotApplication", "GetChatBox(bool, bool, string, bool, bool)", true);
            }
        }

        /// <summary>
        /// Checks if a user is timed out from all bot commands
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private async Task<bool> IsUserTimedout(TwitchChatter chatter)
        {
            if (chatter.Username == _botConfig.Broadcaster)
                return false;

            TimeoutUser user = _timeout.TimedoutUsers.FirstOrDefault(u => u.Username == chatter.Username);

            if (user == null) return false;
            else if (user.TimeoutExpirationUtc < DateTime.UtcNow)
            {
                await _timeout.DeleteUserTimeout(chatter.Username, _broadcasterInstance.DatabaseId, _botConfig.TwitchBotApiLink);
                return false;
            }
            else if (!user.HasBeenWarned)
            {
                user.HasBeenWarned = true; // prevent spamming timeout message
                string timeout = await _timeout.GetUserTimeout(chatter.Username, _broadcasterInstance.DatabaseId, _botConfig.TwitchBotApiLink);

                if (timeout.Equals("0 seconds"))
                    return false;
                else
                    _irc.SendPublicChatMessage("FYI: I am not allowed to talk to you for " + timeout);
            }

            return true;
        }

        /// <summary>
        /// Checks if a user is on a cooldown from a particular command
        /// </summary>
        /// <param name="username"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        private bool IsUserOnCooldown(TwitchChatter chatter, string command)
        {
            CooldownUser user = _cooldownUsers.FirstOrDefault(u => u.Username == chatter.Username && u.Command == command);

            if (user == null) return false;
            else if (user.Cooldown < DateTime.Now)
            {
                _cooldownUsers.Remove(user);
                return false;
            }

            if (!user.Warned)
            {
                user.Warned = true; // prevent spamming cooldown message
                string timespanMessage = "";
                TimeSpan timespan = user.Cooldown - DateTime.Now;

                if (timespan.Minutes > 0)
                    timespanMessage = $"{timespan.Minutes} minute(s) and {timespan.Seconds} second(s)";
                else if (timespan.Seconds == 0)
                    timespanMessage = $"{timespan.Milliseconds} millisecond(s)";
                else
                    timespanMessage = $"{timespan.Seconds} second(s)";

                _irc.SendPublicChatMessage($"The {command} command is currently on cooldown @{chatter.Username} for {timespanMessage}");
            }

            return true;
        }

        private async Task SetBroadcasterIds()
        {
            try
            {
                RootUserJSON json = await _twitchInfo.GetUsersByLoginName(_botConfig.Broadcaster);

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
                await _errHndlrInstance.LogError(ex, "TwitchBotApplication", "SetBroadcasterIds()", true);
            }
        }

        private async Task SetListTimeouts()
        {
            try
            {
                List<BotTimeout> botTimeouts = await _timeout.GetTimeouts(_broadcasterInstance.DatabaseId, _botConfig.TwitchBotApiLink);

                foreach (BotTimeout botTimeout in botTimeouts)
                {
                    _timeout.TimedoutUsers.Add(new TimeoutUser
                    {
                        Username = botTimeout.Username,
                        TimeoutExpirationUtc = botTimeout.Timeout,
                        HasBeenWarned = false
                    });
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "TwitchBotApplication", "SetListTimeouts()", true);
            }
        }

        /// <summary>
        /// Greet a new user with a welcome message and a "thank-you" deposit of stream currency
        /// </summary>
        ///<param name="chatter"></param>
        private async Task GreetNewUser(TwitchChatter chatter)
        {
            try
            {
                if (!_greetedUsers.Any(u => u == chatter.Username) && !chatter.Username.Equals(_botConfig.Broadcaster.ToLower()) && chatter.Message.Length > 1)
                {
                    // check if user has a stream currency account
                    int funds = await _bank.CheckBalance(chatter.Username, _broadcasterInstance.DatabaseId);
                    int greetedDeposit = 500; // ToDo: Make greeted deposit config setting

                    if (funds > -1)
                    {
                        funds += greetedDeposit; // deposit 500 stream currency
                        await _bank.UpdateFunds(chatter.Username, _broadcasterInstance.DatabaseId, funds);
                    }
                    else
                        await _bank.CreateAccount(chatter.Username, _broadcasterInstance.DatabaseId, greetedDeposit);

                    _greetedUsers.Add(chatter.Username);

                    _irc.SendPublicChatMessage($"Welcome to the channel @{chatter.Username}! Thanks for saying something! "
                        + $"Let me show you my appreciation with {greetedDeposit} {_botConfig.CurrencyType}");
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "TwitchBotApplication", "GreetNewUser(string, string)", false);
            }
        }

        /// <summary>
        /// Get access to user's Twitter credentials via PIN-based authentication
        /// </summary>
        private void GetTwitterAuth()
        {
            // Check if developer set up Twitter integration
            if (!string.IsNullOrEmpty(_botConfig.TwitterConsumerKey) && !string.IsNullOrEmpty(_botConfig.TwitterConsumerSecret))
            {
                // Check existing credentials
                if (!string.IsNullOrEmpty(_botConfig.TwitterAccessToken) && !string.IsNullOrEmpty(_botConfig.TwitterAccessSecret))
                {
                    TwitterCredentials userCredentials = new TwitterCredentials
                    (
                        _botConfig.TwitterConsumerKey, _botConfig.TwitterConsumerSecret,
                        _botConfig.TwitterAccessToken, _botConfig.TwitterAccessSecret
                    );

                    var authenticatedUser = new object();

                    // Try to set stored credentials
                    if (userCredentials != null)
                    {
                        // Use the user credentials in the application
                        Auth.SetCredentials(userCredentials);

                        authenticatedUser = User.GetAuthenticatedUser();
                    }

                    // Check if current credentials are valid
                    if (userCredentials == null || authenticatedUser == null)
                    {
                        // Remove access info from app settings on local computer
                        SaveTwitterAccessInfo();
                    }
                }

                // Get authentication to Twitter account
                if (string.IsNullOrEmpty(_botConfig.TwitterAccessToken) || string.IsNullOrEmpty(_botConfig.TwitterAccessSecret))
                {
                    // Create a new set of credentials for the application.
                    TwitterCredentials appCredentials = new TwitterCredentials(_botConfig.TwitterConsumerKey, _botConfig.TwitterConsumerSecret);

                    // Init the authentication process and store the related "AuthenticationContext".
                    IAuthenticationContext authenticationContext = AuthFlow.InitAuthentication(appCredentials);

                    // Go to the URL so that Twitter authenticates the user and gives him a PIN code.
                    Process.Start(authenticationContext.AuthorizationURL);

                    // Ask the user to enter the pin code given by Twitter
                    Console.WriteLine("Please enter the PIN given by Twitter (or press ENTER to continue using this bot without twitter):");
                    string pinCode = Console.ReadLine();

                    if (!string.IsNullOrWhiteSpace(pinCode))
                    {
                        // With this pin code, it is now possible to get the credentials back from Twitter
                        ITwitterCredentials userCredentials = AuthFlow.CreateCredentialsFromVerifierCode(pinCode, authenticationContext);

                        pinCode = ""; // clear pin code

                        if (userCredentials != null)
                        {
                            // Use the user credentials in the application
                            Auth.SetCredentials(userCredentials);

                            // Store access info into app settings on local computer
                            SaveTwitterAccessInfo(userCredentials.AccessToken, userCredentials.AccessTokenSecret);

                            // Allow Twitter-based commands to use user's credentials provided by the bot user
                            _hasTwitterInfo = true;

                            // ToDo: Add setting if user wants preset reminder
                            // ToDo: If !live was used before this reminder pops up, remove it from "Program.DelayedMessages"
                            Program.DelayedMessages.Add(new DelayedMessage
                            {
                                Message = $"@{_botConfig.Broadcaster} did you remind Twitter you're \"!live\" on " 
                                    + "https://twitter.com/" + $"{User.GetAuthenticatedUser().UserIdentifier.ScreenName}",
                                SendDate = DateTime.Now.AddMinutes(5)
                            });

                            Console.WriteLine();
                            Console.WriteLine("Twitter authentication granted for Twitter account (screen name): "
                                + $"{User.GetAuthenticatedUser().UserIdentifier.ScreenName}");
                            Console.WriteLine();
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
                    // Allow Twitter-based commands to use user's credentials provided by the bot user
                    _hasTwitterInfo = true;

                    Console.WriteLine($"Current authenticated Twitter's screen name: {User.GetAuthenticatedUser().UserIdentifier.ScreenName}");
                    Console.WriteLine();
                }
            }
            else
            {
                Console.WriteLine("Warning: Twitter integration not set. Continuing without Twitter features...");
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Save Twitter access token and secret values
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="accessSecret"></param>
        private void SaveTwitterAccessInfo(string accessToken = "", string accessSecret = "")
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
        private async Task<bool> IsAllowedChatMessage(TwitchChatter chatter)
        {
            if (chatter.Message.Contains("clips.twitch.tv/"))
            {
                return await IsBroadcasterClip(chatter);
            }
            else if (chatter.Message.Contains("twitch.tv/") 
                && chatter.Message.Contains("/clip/")
                && !chatter.Message.ToLower().Contains(_botConfig.Broadcaster.ToLower()))
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
        private async Task<bool> IsBroadcasterClip(TwitchChatter chatter)
        {
            string clipUrl = "clips.twitch.tv/";

            int slugIndex = chatter.Message.IndexOf(clipUrl) + clipUrl.Length;
            int endSlugIndex = chatter.Message.IndexOf(" ", slugIndex);

            string slug = endSlugIndex > 0 
                ? chatter.Message.Substring(slugIndex, endSlugIndex - slugIndex) 
                : chatter.Message.Substring(slugIndex);

            ClipJSON clip = await _twitchInfo.GetClip(slug);

            if (clip.Broadcaster.Name == _botConfig.Broadcaster.ToLower())
            {
                return true;
            }

            return false;
        }
    }
}
