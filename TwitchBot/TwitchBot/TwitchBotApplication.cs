﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
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
using TwitchBot.Services;
using TwitchBot.Threads;
using TwitchBot.Models.JSON;

using TwitchBotDb.Models;

namespace TwitchBot
{
    public class TwitchBotApplication
    {
        private System.Configuration.Configuration _appConfig;
        private TwitchBotConfigurationSection _botConfig;
        private string _connStr;
        private IrcClient _irc;
        private TimeoutCmd _timeout;
        private CmdBrdCstr _cmdBrdCstr;
        private CmdMod _cmdMod;
        private CmdGen _cmdGen;
        private bool _isManualSongRequestAvail;
        private bool _isYouTubeSongRequestAvail;
        private bool _hasTwitterInfo;
        private bool _hasYouTubeAuth;
        private List<string> _multiStreamUsers;
        private List<string> _greetedUsers;
        private Queue<string> _gameQueueUsers;
        private List<CooldownUser> _cooldownUsers;
        private LocalSpotifyClient _spotify;
        private TwitchInfoService _twitchInfo;
        private FollowerService _follower;
        private FollowerSubscriberListener _followerSubscriberListener;
        private BankService _bank;
        private SongRequestBlacklistService _songRequestBlacklist;
        private ManualSongRequestService _manualSongRequest;
        private PartyUpService _partyUp;
        private GameDirectoryService _gameDirectory;
        private QuoteService _quote;
        private BankHeist _bankHeist;
        private BossFight _bossFight;
        private TwitchChatterListener _twitchChatterListener;
        private ErrorHandler _errHndlrInstance = ErrorHandler.Instance;
        private ModeratorSingleton _modInstance = ModeratorSingleton.Instance;
        private YoutubeClient _youTubeClientInstance = YoutubeClient.Instance;
        private BroadcasterSingleton _broadcasterInstance = BroadcasterSingleton.Instance;
        private BankHeistSingleton _bankHeistInstance = BankHeistSingleton.Instance;
        private BossFightSingleton _bossFightInstance = BossFightSingleton.Instance;

        public TwitchBotApplication(System.Configuration.Configuration appConfig, TwitchInfoService twitchInfo, SongRequestBlacklistService songRequestBlacklist,
            FollowerService follower, BankService bank, FollowerSubscriberListener followerListener, ManualSongRequestService manualSongRequest, PartyUpService partyUp,
            GameDirectoryService gameDirectory, QuoteService quote, BankHeist bankHeist, TwitchChatterListener twitchChatterListener,
            BossFight bossFight)
        {
            _appConfig = appConfig;
            _connStr = appConfig.ConnectionStrings.ConnectionStrings[Program.ConnStrType].ConnectionString;
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
        }

        public async Task RunAsync()
        {
            try
            {
                /* Check if developer attempted to set up the connection string for either production or test */
                if (Program.ConnStrType.Equals("TwitchBotConnStrTEST"))
                    Console.WriteLine("<<<< WARNING: Connecting to testing database >>>>");

                // Attempt to connect to server
                if (!IsServerConnected())
                {
                    _connStr = null; // clear sensitive data

                    Console.WriteLine("Datebase connection failed. Please try again");
                    Console.WriteLine();
                    Console.WriteLine("-- Common technical issues: --");
                    Console.WriteLine("1: Check if firewall settings has your client IP address.");
                    Console.WriteLine("2: Double check the connection string under 'Properties' inside 'Settings'");
                    Console.WriteLine();
                    Console.WriteLine("Shutting down now...");
                    Thread.Sleep(5000);
                    Environment.Exit(1);
                }

                // Check for tables needed to start this application
                // ToDo: Add more table names
                List<string> dbTables = GetTables();
                if (dbTables.Contains("Broadcasters")
                    && dbTables.Contains("Moderators")
                    && dbTables.Contains("UserBotTimeout")
                    && dbTables.Contains("ErrorLog"))
                {
                    Console.WriteLine("Found database tables");
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine("Error: Couldn't find necessary database tables");
                    Console.WriteLine();
                    Console.WriteLine("Shutting down now...");
                    Thread.Sleep(5000);
                    Environment.Exit(1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Message: " + ex.Message);
                Console.WriteLine();
                Console.WriteLine("Please check the connection string for the right format inside the config file");
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
                    Thread.Sleep(3000);
                    Environment.Exit(1);
                }

                // Configure error handler singleton class
                ErrorHandler.Configure(_broadcasterInstance.DatabaseId, _irc, _botConfig);

                /* Connect to local Spotify client */
                _spotify = new LocalSpotifyClient(_botConfig);
                await _spotify.Connect();

                // Password from www.twitchapps.com/tmi/
                // include the "oauth:" portion
                // Use chat bot's oauth
                /* main server: irc.twitch.tv, 6667 */
                _irc.Connect(_botConfig.BotName.ToLower(), _botConfig.TwitchOAuth, _botConfig.Broadcaster.ToLower());
                _cmdGen = new CmdGen(_irc, _spotify, _botConfig, _connStr, _broadcasterInstance.DatabaseId, _twitchInfo, _bank, _follower,
                    _songRequestBlacklist, _manualSongRequest, _partyUp, _gameDirectory, _quote);
                _cmdBrdCstr = new CmdBrdCstr(_irc, _botConfig, _connStr, _broadcasterInstance.DatabaseId, _appConfig, _songRequestBlacklist,
                    _twitchInfo, _gameDirectory);
                _cmdMod = new CmdMod(_irc, _timeout, _botConfig, _connStr, _broadcasterInstance.DatabaseId, _appConfig, _bank, _twitchInfo,
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

                /* Pull YouTube response tokens from user's account (request permission if needed) */
                // ToDo: Move YouTube client secrets away from bot config
                _hasYouTubeAuth = await _youTubeClientInstance.GetAuth(_botConfig.YouTubeClientId, _botConfig.YouTubeClientSecret);
                if (_hasYouTubeAuth && string.IsNullOrEmpty(_botConfig.YouTubeBroadcasterPlaylistId))
                {
                    // Check if user has a song request playlist, else create one
                    string playlistName = _botConfig.YouTubeBroadcasterPlaylistName;
                    string defaultPlaylistName = "Twitch Song Requests";

                    if (string.IsNullOrEmpty(playlistName))
                    {
                        playlistName = defaultPlaylistName;
                        _botConfig.YouTubeBroadcasterPlaylistName = playlistName;
                    }

                    Playlist broadcasterPlaylist = await _youTubeClientInstance.GetBroadcasterPlaylistByKeyword(playlistName);

                    if (broadcasterPlaylist.Id == null)
                        broadcasterPlaylist = await _youTubeClientInstance.CreatePlaylist(defaultPlaylistName, "Songs requested from Twitch chatters");

                    _botConfig.YouTubeBroadcasterPlaylistId = broadcasterPlaylist.Id;
                    _appConfig.AppSettings.Settings.Remove("youTubeBroadcasterPlaylistId");
                    _appConfig.AppSettings.Settings.Add("youTubeBroadcasterPlaylistId", broadcasterPlaylist.Id);
                    _appConfig.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection("TwitchBotConfiguration");
                }

                /* Start listening for delayed messages */
                DelayMsg delayMsg = new DelayMsg(_irc);
                delayMsg.Start();

                /* Grab list of chatters from channel */
                _twitchChatterListener.Start();

                /* Pull list of mods from database */
                await _modInstance.GetModerators(_broadcasterInstance.DatabaseId, _botConfig.TwitchBotApiLink);

                /* Pull list of followers and check experience points for stream leveling */
                _followerSubscriberListener.Start(_irc, _broadcasterInstance.DatabaseId);

                /* Get list of timed out users from database */
                await SetListTimeouts();

                /* Load/create settings and start the queue for the heist */
                await _bankHeistInstance.LoadSettings(_broadcasterInstance.DatabaseId, _botConfig.TwitchBotApiLink);

                if (_bankHeistInstance.Id == 0)
                    await _bankHeistInstance.CreateSettings(_broadcasterInstance.DatabaseId, _botConfig.TwitchBotApiLink);

                _bankHeist.Start(_irc, _broadcasterInstance.DatabaseId);

                /* Load/create settings and start the queue for the boss fight */
                // Get current game name
                ChannelJSON json = await _twitchInfo.GetBroadcasterChannelById();
                string gameTitle = json.Game;

                // Grab game id in order to find party member
                GameList game = await _gameDirectory.GetGameId(gameTitle);

                await _bossFightInstance.LoadSettings(_broadcasterInstance.DatabaseId, game?.Id, _botConfig.TwitchBotApiLink);

                if (_bossFightInstance.SettingsId == 0)
                    await _bossFightInstance.CreateSettings(_broadcasterInstance.DatabaseId, game?.Id, _botConfig.TwitchBotApiLink);

                _bossFight.Start(_irc, _broadcasterInstance.DatabaseId);

                /* Ping to twitch server to prevent auto-disconnect */
                PingSender ping = new PingSender(_irc);
                ping.Start();

                /* Send reminders of certain events */
                ChatReminder chatReminder = new ChatReminder(_irc, _broadcasterInstance.DatabaseId, _botConfig.TwitchBotApiLink, _botConfig.TwitchClientId, _gameDirectory);
                chatReminder.Start();

                /* Authenticate to Twitter if possible */
                if (!string.IsNullOrEmpty(_botConfig.TwitterConsumerKey) 
                    && !string.IsNullOrEmpty(_botConfig.TwitterConsumerSecret) 
                    && !string.IsNullOrEmpty(_botConfig.TwitterAccessToken) 
                    && !string.IsNullOrEmpty(_botConfig.TwitterAccessSecret))
                {
                    Auth.ApplicationCredentials = new TwitterCredentials(
                        _botConfig.TwitterConsumerKey, _botConfig.TwitterConsumerSecret,
                        _botConfig.TwitterAccessToken, _botConfig.TwitterAccessSecret
                    );

                    _hasTwitterInfo = true;
                }

                Console.WriteLine("=== Time to get to work! ===");
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
                    string message = await _irc.ReadMessage();
                    Console.WriteLine(message); // Print raw irc message

                    if (!string.IsNullOrEmpty(message))
                    {
                        /* 
                        * Get user name and message from chat 
                        * and check if user has access to certain functions
                        */
                        if (message.Contains("PRIVMSG"))
                        {
                            // Modify message to only show user and message
                            int indexParseSign = message.IndexOf('!');
                            StringBuilder modifiedMessage = new StringBuilder(message);
                            string username = message.Substring(1, indexParseSign - 1);

                            indexParseSign = message.IndexOf(" :");
                            modifiedMessage.Remove(0, indexParseSign + 2); // remove unnecessary info before and including the parse symbol
                            message = modifiedMessage.ToString();

                            await GreetNewUser(username, message);

                            /* 
                             * Broadcaster commands 
                             */
                            if (username.Equals(_botConfig.Broadcaster.ToLower()))
                            {
                                /* Display bot settings */
                                if (message.Equals("!botsettings"))
                                    _cmdBrdCstr.CmdBotSettings();

                                /* Stop running the bot */
                                else if (message.Equals("!exitbot"))
                                    _cmdBrdCstr.CmdExitBot();

                                /* Manually connect to Spotify */
                                else if (message.Equals("!spotifyconnect"))
                                    await _spotify.Connect();

                                /* Press local Spotify play button [>] */
                                else if (message.Equals("!spotifyplay"))
                                    _spotify.playBtn_Click();

                                /* Press local Spotify pause button [||] */
                                else if (message.Equals("!spotifypause"))
                                    _spotify.pauseBtn_Click();

                                /* Press local Spotify previous button [|<] */
                                else if (message.Equals("!spotifyprev"))
                                    _spotify.prevBtn_Click();

                                /* Press local Spotify next (skip) button [>|] */
                                else if (message.Equals("!spotifynext"))
                                    _spotify.skipBtn_Click();

                                /* Enables tweets to be sent out from this bot (both auto publish tweets and manual tweets) */
                                else if (message.Equals("!sendtweet on"))
                                    _cmdBrdCstr.CmdEnableTweet(hasTwitterInfo);

                                /* Disables tweets from being sent out from this bot */
                                else if (message.Equals("!sendtweet off"))
                                    _cmdBrdCstr.CmdDisableTweet(hasTwitterInfo);

                                /* Enables viewers to request songs (default off) */
                                else if (message.Equals("!rbsrmode on"))
                                    isManualSongRequestAvail = await _cmdBrdCstr.CmdEnableManualSrMode(isManualSongRequestAvail);

                                /* Disables viewers to request songs (default off) */
                                else if (message.Equals("!rbsrmode off"))
                                    isManualSongRequestAvail = await _cmdBrdCstr.CmdDisableManualSrMode(isManualSongRequestAvail);

                                /* Enables viewers to request songs (default off) */
                                else if (message.Equals("!ytsrmode on"))
                                    isYouTubeSongRequestAvail = await _cmdBrdCstr.CmdEnableYouTubeSrMode(isYouTubeSongRequestAvail);

                                /* Disables viewers to request songs (default off) */
                                else if (message.Equals("!ytsrmode off"))
                                    isYouTubeSongRequestAvail = await _cmdBrdCstr.CmdDisableYouTubeSrMode(isYouTubeSongRequestAvail);

                                /* Sends a manual tweet (if credentials have been provided) */
                                // Usage: !tweet "[message]" (use quotation marks)
                                else if (message.StartsWith("!tweet "))
                                    _cmdBrdCstr.CmdTweet(hasTwitterInfo, message);

                                /* Enables songs from local Spotify to be displayed inside the chat */
                                else if (message.Equals("!displaysongs on"))
                                    _cmdBrdCstr.CmdEnableDisplaySongs();

                                /* Disables songs from local Spotify to be displayed inside the chat */
                                else if (message.Equals("!displaysongs off"))
                                    _cmdBrdCstr.CmdDisableDisplaySongs();

                                /* Add viewer to moderator list so they can have access to bot moderator commands */
                                // Usage: !addmod @[username]
                                else if (message.StartsWith("!addmod ") && message.Contains("@"))
                                    await _cmdBrdCstr.CmdAddBotMod(message);

                                /* Remove moderator from list so they can't access the bot moderator commands */
                                // Usage: !delmod @[username]
                                else if (message.StartsWith("!delmod ") && message.Contains("@"))
                                    await _cmdBrdCstr.CmdDelBotMod(message);

                                /* List bot moderators */
                                else if (message.Equals("!listmod"))
                                    _cmdBrdCstr.CmdListMod();

                                /* Add song or artist to song request blacklist */
                                // Usage (artist): !srbl 1 [artist name]
                                // Usage (song): !srbl 2 "[song title]" <[artist name]>
                                else if (message.StartsWith("!srbl "))
                                    await _cmdBrdCstr.CmdAddSongRequestBlacklist(message);

                                /* Remove song or artist from song request blacklist */
                                // Usage (artist): !removesrbl 1 [artist name]
                                // Usage (song): !removesrbl 2 "[song title]" <[artist name]>
                                else if (message.StartsWith("!removesrbl "))
                                    await _cmdBrdCstr.CmdRemoveSongRequestBlacklist(message);

                                /* Reset the entire song request blacklist */
                                else if (message.Equals("!resetsrbl"))
                                    await _cmdBrdCstr.CmdResetSongRequestBlacklist();

                                /* Show the song request blacklist */
                                else if (message.Equals("!showsrbl"))
                                    await _cmdBrdCstr.CmdListSongRequestBlacklist();

                                /* Sends an announcement tweet saying the broadcaster is live */
                                else if (message.Equals("!live"))
                                    await _cmdBrdCstr.CmdLive(hasTwitterInfo);

                                /* Manually refresh reminders */
                                else if (message.Equals("!refreshreminders"))
                                    await _cmdBrdCstr.CmdRefreshReminders();

                                /* Set regular follower hours for dedicated followers */
                                else if (message.StartsWith("!setregularhours"))
                                    _cmdBrdCstr.CmdSetRegularFollowerHours(message);

                                /* Manually refresh boss fight */
                                else if (message.Equals("!refreshbossfight"))
                                    await _cmdBrdCstr.CmdRefreshBossFight();

                                /* insert more broadcaster commands here */
                            }

                            if (!await IsUserTimedout(message, username))
                            {
                                /*
                                 * Moderator commands (also checks if user has been timed out from using a command)
                                 */
                                if (username.Equals(_botConfig.Broadcaster) || _modInstance.Moderators.Contains(username.ToLower()))
                                {
                                    /* Takes money away from a user */
                                    // Usage: !charge [-amount] @[username]
                                    if (message.StartsWith("!charge ") && message.Contains("@"))
                                        await _cmdMod.CmdCharge(message, username);

                                    /* Gives money to user */
                                    // Usage: !deposit [amount] @[username]
                                    else if (message.StartsWith("!deposit ") && message.Contains("@"))
                                        await _cmdMod.CmdDeposit(message, username);

                                    /* Removes the first song in the queue of song requests */
                                    else if (message.Equals("!poprbsr"))
                                        await _cmdMod.CmdPopManualSr();

                                    /* Removes first party memeber in queue of party up requests */
                                    else if (message.Equals("!poppartyuprequest"))
                                        await _cmdMod.CmdPopPartyUpRequest();

                                    /* Bot-specific timeout on a user for a set amount of time */
                                    // Usage: !timeout [seconds] @[username]
                                    else if (message.StartsWith("!timeout ") && message.Contains("@"))
                                        await _cmdMod.CmdAddTimeout(message, username);

                                    /* Remove bot-specific timeout on a user for a set amount of time */
                                    // Usage: !deltimeout @[username]
                                    else if (message.StartsWith("!deltimeout @"))
                                        await _cmdMod.CmdDelTimeout(message, username);

                                    /* Set delay for messages based on the latency of the stream */
                                    // Usage: !setlatency [seconds]
                                    else if (message.StartsWith("!setlatency "))
                                        _cmdMod.CmdSetLatency(message, username);

                                    /* Add a broadcaster quote */
                                    // Usage: !addquote [quote]
                                    else if (message.StartsWith("!addquote "))
                                        await _cmdMod.CmdAddQuote(message, username);

                                    /* Tell the stream the specified moderator will be AFK */
                                    else if (message.Equals("!modafk"))
                                        _cmdMod.CmdModAfk(username);

                                    /* Tell the stream the specified moderator has returned */
                                    else if (message.Equals("!modback"))
                                        _cmdMod.CmdModBack(username);

                                    /* Gives every viewer a set amount of currency */
                                    else if (message.StartsWith("!bonusall "))
                                        await _cmdMod.CmdBonusAll(message, username);

                                    /* Add MultiStream user to link */
                                    // Usage: !addmsl @[username]
                                    else if (message.StartsWith("!addmsl "))
                                        _multiStreamUsers = await _cmdMod.CmdAddMultiStreamUser(message, username, _multiStreamUsers);

                                    /* Reset MultiStream link so link can be reconfigured */
                                    else if (message.Equals("!resetmsl"))
                                        _multiStreamUsers = await _cmdMod.CmdResetMultiStreamLink(username, _multiStreamUsers);

                                    /* Updates the title of the Twitch channel */
                                    // Usage: !updatetitle [title]
                                    else if (message.StartsWith("!updatetitle "))
                                        await _cmdMod.CmdUpdateTitle(message);

                                    /* Updates the game of the Twitch channel */
                                    // Usage: !updategame [game]
                                    else if (message.StartsWith("!updategame "))
                                        await _cmdMod.CmdUpdateGame(message, hasTwitterInfo);

                                    /* Pops user from the queue of users that want to play with the broadcaster */
                                    else if (message.Equals("!popgotnext"))
                                        _gameQueueUsers = await _cmdMod.CmdPopGotNextGame(username, _gameQueueUsers);

                                    /* Resets game queue of users that want to play with the broadcaster */
                                    else if (message.Equals("!resetgotnext"))
                                        _gameQueueUsers = await _cmdMod.CmdResetGotNextGame(username, _gameQueueUsers);

                                    /* Display the streamer's channel and game status */
                                    // Usage: !streamer @[username]
                                    else if (message.StartsWith("!streamer @"))
                                        await _cmdMod.CmdPromoteStreamer(message, username);

                                    /* insert moderator commands here */
                                }

                                /* 
                                 * General commands 
                                 */
                                /* Display some viewer commands a link to command documentation */
                                if (message.Equals("!cmds"))
                                    _cmdGen.CmdDisplayCmds();

                                /* Display a static greeting */
                                else if (message.Equals("!hello"))
                                    _cmdGen.CmdHello(username);

                                /* Displays Discord link into chat (if available) */
                                else if (message.Equals("!discord"))
                                    _cmdMod.CmdDiscord();

                                /* Display the current time in UTC (Coordinated Universal Time) */
                                else if (message.Equals("!utctime"))
                                    _cmdGen.CmdUtcTime();

                                /* Display the current time in the time zone the host is located */
                                else if (message.Equals("!hosttime"))
                                    _cmdGen.CmdHostTime();

                                /* Shows how long the broadcaster has been streaming */
                                else if (message.Equals("!uptime"))
                                    await _cmdGen.CmdUptime();

                                /* Display list of requested songs */
                                else if (message.Equals("!rbsrl"))
                                    await _cmdGen.CmdManualSrList(isManualSongRequestAvail, username);

                                /* Display link of list of songs to request */
                                else if (message.Equals("!rbsl"))
                                    _cmdGen.CmdManualSrLink(isManualSongRequestAvail, username);

                                /* Request a song for the host to play */
                                // Usage: !rbsr [artist] - [song title]
                                else if (message.StartsWith("!rbsr "))
                                    await _cmdGen.CmdManualSr(isManualSongRequestAvail, message, username);

                                /* Displays the current song being played from Spotify */
                                else if (message.Equals("!spotifycurr"))
                                    _cmdGen.CmdSpotifyCurr();

                                /* Slaps a user and rates its effectiveness */
                                // Usage: !slap @[username]
                                else if (message.StartsWith("!slap @") && !IsUserOnCooldown(username, "!slap"))
                                {
                                    DateTime cooldown = await _cmdGen.CmdSlap(message, username);
                                    if (cooldown > DateTime.Now)
                                    {
                                        _cooldownUsers.Add(new CooldownUser
                                        {
                                            Username = username,
                                            Cooldown = cooldown,
                                            Command = "!slap",
                                            Warned = false
                                        });
                                    }
                                }

                                /* Stabs a user and rates its effectiveness */
                                // Usage: !stab @[username]
                                else if (message.StartsWith("!stab @") && !IsUserOnCooldown(username, "!stab"))
                                {
                                    DateTime cooldown = await _cmdGen.CmdStab(message, username);
                                    if (cooldown > DateTime.Now)
                                    {
                                        _cooldownUsers.Add(new CooldownUser
                                        {
                                            Username = username,
                                            Cooldown = cooldown,
                                            Command = "!stab",
                                            Warned = false
                                        });
                                    }
                                }

                                /* Shoots a viewer's random body part */
                                // Usage !shoot @[username]
                                else if (message.StartsWith("!shoot @") && !IsUserOnCooldown(username, "!shoot"))
                                {
                                    DateTime cooldown = await _cmdGen.CmdShoot(message, username);
                                    if (cooldown > DateTime.Now)
                                    {
                                        _cooldownUsers.Add(new CooldownUser
                                        {
                                            Username = username,
                                            Cooldown = cooldown,
                                            Command = "!shoot",
                                            Warned = false
                                        });
                                    }
                                }

                                /* Throws an item at a viewer and rates its effectiveness against the victim */
                                // Usage: !throw [item] @username
                                else if (message.StartsWith("!throw ") && message.Contains("@") && !IsUserOnCooldown(username, "!throw"))
                                {
                                    DateTime cooldown = await _cmdGen.CmdThrow(message, username);
                                    if (cooldown > DateTime.Now)
                                    {
                                        _cooldownUsers.Add(new CooldownUser
                                        {
                                            Username = username,
                                            Cooldown = cooldown,
                                            Command = "!throw",
                                            Warned = false
                                        });
                                    }
                                }

                                /* Request party member if game and character exists in party up system */
                                // Usage: !partyup [party member name]
                                else if (message.StartsWith("!partyup "))
                                    await _cmdGen.CmdPartyUp(message, username);

                                /* Check what other user's have requested */
                                else if (message.Equals("!partyuprequestlist"))
                                    await _cmdGen.CmdPartyUpRequestList();

                                /* Check what party members are available (if game is part of the party up system) */
                                else if (message.Equals("!partyuplist"))
                                    await _cmdGen.CmdPartyUpList();

                                /* Check user's account balance */
                                else if (message.Equals($"!{_botConfig.CurrencyType.ToLower()}"))
                                    await _cmdGen.CmdCheckFunds(username);

                                /* Gamble money away */
                                // Usage: !gamble [money]
                                else if (message.StartsWith("!gamble ") && !IsUserOnCooldown(username, "!gamble"))
                                {
                                    DateTime cooldown = await _cmdGen.CmdGamble(message, username);
                                    if (cooldown > DateTime.Now)
                                    {
                                        _cooldownUsers.Add(new CooldownUser
                                        {
                                            Username = username,
                                            Cooldown = cooldown,
                                            Command = "!gamble",
                                            Warned = false
                                        });
                                    }
                                }

                                /* Display random broadcaster quote */
                                else if (message.Equals("!quote"))
                                    _cmdGen.CmdQuote();

                                /* Display how long a user has been following the broadcaster */
                                else if (message.Equals("!followsince"))
                                    _cmdGen.CmdFollowSince(username);

                                /* Display follower's stream rank */
                                else if (message.Equals("!rank"))
                                    _cmdGen.CmdViewRank(username);

                                /* Add song request to YouTube playlist */
                                // Usage: !ytsr [video title/YouTube link]
                                else if ((message.StartsWith("!ytsr ") || message.StartsWith("!sr ") || message.StartsWith("!songrequest "))
                                        && !IsUserOnCooldown(username, "!ytsr"))
                                {
                                    DateTime cooldown = await _cmdGen.CmdYouTubeSongRequest(message, username, hasYouTubeAuth, isYouTubeSongRequestAvail);
                                    if (cooldown > DateTime.Now)
                                    {
                                        _cooldownUsers.Add(new CooldownUser
                                        {
                                            Username = username,
                                            Cooldown = cooldown,
                                            Command = "!ytsr",
                                            Warned = false
                                        });
                                    }
                                }

                                /* Display YouTube link to song request playlist */
                                else if (message.Equals("!ytsl"))
                                    _cmdGen.CmdYouTubeSongRequestList(hasYouTubeAuth, isYouTubeSongRequestAvail);

                                /* Display MultiStream link */
                                else if (message.Equals("!msl"))
                                    _cmdGen.CmdMultiStreamLink(username, _multiStreamUsers);

                                /* Display Magic 8-ball response */
                                // Usage: !8ball [question]
                                else if (message.StartsWith("!8ball ") && !IsUserOnCooldown(username, "!8ball"))
                                {
                                    DateTime cooldown = await _cmdGen.CmdMagic8Ball(username);
                                    if (cooldown > DateTime.Now)
                                    {
                                        _cooldownUsers.Add(new CooldownUser
                                        {
                                            Username = username,
                                            Cooldown = cooldown,
                                            Command = "!8ball",
                                            Warned = false
                                        });
                                    }
                                }

                                /* Disply the top 3 richest users */
                                else if (message.Equals($"!{_botConfig.CurrencyType.ToLower()}top3"))
                                    _cmdGen.CmdLeaderboardCurrency(username);

                                /* Display the top 3 highest ranking users */
                                else if (message.Equals("!ranktop3"))
                                    _cmdGen.CmdLeaderboardRank(username);

                                /* Play russian roulette */
                                // Note: Chat moderators cannot be timed out by the bot (reason for being excluded)
                                else if (message.Equals("!roulette") && !IsUserOnCooldown(username, "!roulette"))
                                {
                                    DateTime cooldown = await _cmdGen.CmdRussianRoulette(username);
                                    if (cooldown > DateTime.Now)
                                    {
                                        _cooldownUsers.Add(new CooldownUser
                                        {
                                            Username = username,
                                            Cooldown = cooldown,
                                            Command = "!roulette",
                                            Warned = false
                                        });
                                    }
                                }

                                /* Show the users that want to play with the broadcaster */
                                else if (message.Equals("!listgotnext"))
                                    _cmdGen.CmdListGotNextGame(username, _gameQueueUsers);

                                /* Request to play with the broadcaster */
                                else if (message.Equals("!gotnextgame"))
                                    _gameQueueUsers = await _cmdGen.CmdGotNextGame(username, _gameQueueUsers);

                                /* Join the heist and gamble your currency for a higher payout */
                                // Usage: !bankheist [currency]
                                else if (message.StartsWith("!bankheist "))
                                    await _cmdGen.CmdBankHeist(message, username);

                                /* Show the subscribe link (if broadcaster is either Affiliate/Partnered) */
                                else if (message.Equals("!sub"))
                                    await _cmdGen.CmdSubscribe();

                                /* Display how long a user has been subscribed to the broadcaster */
                                else if (message.Equals("!subsince"))
                                    await _cmdGen.CmdSubscribeSince(username);

                                /* Join the boss fight with a pre-defined amount of currency set by broadcaster */
                                else if (message.Equals("!raid"))
                                    await _cmdGen.CmdBossFight(message, username);

                                /* Tell the broadcaster a user is lurking */
                                else if (message.Equals("!lurk") && !IsUserOnCooldown(username, "!lurk"))
                                {
                                    DateTime cooldown = await _cmdGen.CmdLurk(username);
                                    if (cooldown > DateTime.Now)
                                    {
                                        _cooldownUsers.Add(new CooldownUser
                                        {
                                            Username = username,
                                            Cooldown = cooldown,
                                            Command = "!lurk",
                                            Warned = false
                                        });
                                    }
                                }

                                /* Tell the broadcaster a user is no longer lurking */
                                else if (message.Equals("!unlurk") && !IsUserOnCooldown(username, "!unlurk"))
                                {
                                    DateTime cooldown = await _cmdGen.CmdUnlurk(username);
                                    if (cooldown > DateTime.Now)
                                    {
                                        _cooldownUsers.Add(new CooldownUser
                                        {
                                            Username = username,
                                            Cooldown = cooldown,
                                            Command = "!unlurk",
                                            Warned = false
                                        });
                                    }
                                }

                                /* Tell the chat about your amazing community */
                                else if (message.Equals("!community"))
                                    _cmdGen.CmdCommunity();

                                /* Give funds to another chatter */
                                // Usage: !give [amount] @[username]
                                else if (message.StartsWith("!give"))
                                    await _cmdGen.CmdGiveFunds(message, username);

                                /* add more general commands here */
                            }
                        }
                        else if (message.Contains("NOTICE"))
                        {
                            if (message.Contains("Error logging in"))
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
        /// <param name="message"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        private async Task<bool> IsUserTimedout(string message, string username)
        {
            TimeoutUser user = _timeout.TimedoutUsers.FirstOrDefault(u => u.Username.Equals(username));

            if (user == null) return false;
            else if (user.TimeoutExpiration < DateTime.UtcNow)
            {
                await _timeout.DeleteTimeout(username, _broadcasterInstance.DatabaseId, _botConfig.TwitchBotApiLink);
                return false;
            }
            else if (!user.HasBeenWarned)
            {
                user.HasBeenWarned = true; // prevent spamming timeout message
                string timeout = await _timeout.GetTimeout(username, _broadcasterInstance.DatabaseId, _botConfig.TwitchBotApiLink);

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
        /// <param name="cmd"></param>
        /// <returns></returns>
        private bool IsUserOnCooldown(string username, string cmd)
        {
            CooldownUser user = _cooldownUsers.FirstOrDefault(u => u.Username.Equals(username) && u.Command.Equals(cmd));

            if (user == null) return false;
            else if (user.Cooldown < DateTime.Now)
            {
                _cooldownUsers.Remove(user);
                return false;
            }

            if (!user.Warned)
            {
                user.Warned = true; // prevent spamming cooldown message
                TimeSpan ts = user.Cooldown - DateTime.Now;
                string tsMsg = "";

                if (ts.Minutes > 0)
                    tsMsg = $"{ts.Minutes} minute(s) and {ts.Seconds} second(s)";
                else
                    tsMsg = $"{ts.Seconds} second(s)";

                _irc.SendPublicChatMessage($"The {cmd} command is currently on cooldown @{username} for {tsMsg}");
            }

            return true;
        }

        /// <summary>
        /// Test that the server is connected
        /// </summary>
        private bool IsServerConnected()
        {
            using (SqlConnection connection = new SqlConnection(_connStr))
            {
                try
                {
                    connection.Open();
                    return true;
                }
                catch (SqlException)
                {
                    return false;
                }
            }
        }

        private List<string> GetTables()
        {
            using (SqlConnection connection = new SqlConnection(_connStr))
            {
                connection.Open();
                DataTable schema = connection.GetSchema("Tables");
                List<string> TableNames = new List<string>();
                foreach (DataRow row in schema.Rows)
                {
                    TableNames.Add(row[2].ToString());
                }
                return TableNames;
            }
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
                string query = "DELETE FROM UserBotTimeout WHERE Broadcaster = @broadcaster AND Timeout < GETDATE()";

                // Create connection and command
                using (SqlConnection conn = new SqlConnection(_connStr))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _broadcasterInstance.DatabaseId;

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                using (SqlConnection conn = new SqlConnection(_connStr))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM UserBotTimeout WHERE Broadcaster = @broadcaster", conn))
                    {
                        cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _broadcasterInstance.DatabaseId;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    _timeout.TimedoutUsers.Add(new TimeoutUser
                                    {
                                        Username = reader["Username"].ToString(),
                                        TimeoutExpiration = Convert.ToDateTime(reader["Timeout"]),
                                        HasBeenWarned = false
                                    });
                                }
                            }
                        }
                    }
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
        /// <param name="username"></param>
        /// <param name="message"></param>
        private async Task GreetNewUser(string username, string message)
        {
            try
            {
                if (!_greetedUsers.Any(u => u == username) && !username.Equals(_botConfig.Broadcaster.ToLower()) && message.Length > 1)
                {
                    // check if user has a stream currency account
                    int funds = await _bank.CheckBalance(username, _broadcasterInstance.DatabaseId);
                    int greetedDeposit = 500; // ToDo: Make greeted deposit config setting

                    if (funds > -1)
                    {
                        funds += greetedDeposit; // deposit 500 stream currency
                        await _bank.UpdateFunds(username, _broadcasterInstance.DatabaseId, funds);
                    }
                    else
                        await _bank.CreateAccount(username, _broadcasterInstance.DatabaseId, greetedDeposit);

                    _greetedUsers.Add(username);

                    _irc.SendPublicChatMessage($"Welcome to the channel @{username}! Thanks for saying something! "
                        + $"Let me show you my appreciation with {greetedDeposit} {_botConfig.CurrencyType}");
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "TwitchBotApplication", "GreetNewUser(string, string)", false);
            }
        }
    }
}
