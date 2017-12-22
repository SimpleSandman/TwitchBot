using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Net.Http;

using Google.Apis.YouTube.v3.Data;

using Newtonsoft.Json;

using SpotifyAPI.Local.Models;

using TwitchBot.Configuration;
using TwitchBot.Extensions;
using TwitchBot.Libraries;
using TwitchBot.Models;
using TwitchBot.Models.JSON;
using TwitchBot.Services;
using TwitchBot.Threads;
using TwitchBot.Enums;

namespace TwitchBot.Commands
{
    public class CmdGen
    {
        private IrcClient _irc;
        private LocalSpotifyClient _spotify;
        private TwitchBotConfigurationSection _botConfig;
        private string _connStr;
        private int _broadcasterId;
        private TwitchInfoService _twitchInfo;
        private BankService _bank;
        private FollowerService _follower;
        private SongRequestBlacklistService _songRequestBlacklist;
        private ManualSongRequestService _manualSongRequest;
        private PartyUpService _partyUp;
        private GameDirectoryService _gameDirectory;
        private QuoteService _quote;
        private Moderator _modInstance = Moderator.Instance;
        private ErrorHandler _errHndlrInstance = ErrorHandler.Instance;
        private YoutubeClient _youTubeClientInstance = YoutubeClient.Instance;
        private BankHeistSettings _heistSettingsInstance = BankHeistSettings.Instance;
        private BossFightSettings _bossSettingsInstance = BossFightSettings.Instance;
        private TwitchChatterList _twitchChatterListInstance = TwitchChatterList.Instance;

        public CmdGen(IrcClient irc, LocalSpotifyClient spotify, TwitchBotConfigurationSection botConfig, string connString, int broadcasterId,
            TwitchInfoService twitchInfo, BankService bank, FollowerService follower, SongRequestBlacklistService songRequestBlacklist,
            ManualSongRequestService manualSongRequest, PartyUpService partyUp, GameDirectoryService gameDirectory, QuoteService quote)
        {
            _irc = irc;
            _spotify = spotify;
            _botConfig = botConfig;
            _connStr = connString;
            _broadcasterId = broadcasterId;
            _twitchInfo = twitchInfo;
            _bank = bank;
            _follower = follower;
            _songRequestBlacklist = songRequestBlacklist;
            _manualSongRequest = manualSongRequest;
            _partyUp = partyUp;
            _gameDirectory = gameDirectory;
            _quote = quote;
        }

        public void CmdDisplayCmds()
        {
            try
            {
                _irc.SendPublicChatMessage("---> !hello >< !slap @[username] >< !stab @[username] >< !throw [item] @[username] >< !shoot @[username] "
                    + ">< !ytsr [youtube link/search] >< !ytsl >< !partyup [party member name] >< !gamble [money] "
                    + ">< !quote >< !8ball [question] >< !" + _botConfig.CurrencyType.ToLower() + " (check stream currency) <---"
                    + " Link to full list of commands: http://bit.ly/2bXLlEe");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdCmds()", false, "!cmds");
            }
        }

        public void CmdHello(string username)
        {
            try
            {
                _irc.SendPublicChatMessage($"Hey @{username}! Thanks for talking to me.");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdHello(string)", false, "!hello");
            }
        }

        public void CmdUtcTime()
        {
            try
            {
                _irc.SendPublicChatMessage($"UTC Time: {DateTime.UtcNow.ToString()}");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdUtcTime()", false, "!utctime");
            }
        }

        public void CmdHostTime()
        {
            try
            {
                _irc.SendPublicChatMessage($"{_botConfig.Broadcaster}'s Current Time: {DateTime.Now.ToString()} ({TimeZone.CurrentTimeZone.StandardName})");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdHostTime()", false, "!hosttime");
            }
        }

        public async Task CmdUptime()
        {
            try
            {
                RootStreamJSON streamJson = await _twitchInfo.GetBroadcasterStream();

                // Check if the channel is live
                if (streamJson.Stream != null)
                {
                    string duration = streamJson.Stream.CreatedAt;
                    TimeSpan ts = DateTime.UtcNow - DateTime.Parse(duration, new DateTimeFormatInfo(), DateTimeStyles.AdjustToUniversal);
                    string strResultDuration = String.Format("{0:h\\:mm\\:ss}", ts);
                    _irc.SendPublicChatMessage("This channel's current uptime (length of current stream) is " + strResultDuration);
                }
                else
                    _irc.SendPublicChatMessage("This channel is not streaming right now");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdUptime()", false, "!uptime");
            }
        }

        /// <summary>
        /// Display list of requested songs
        /// </summary>
        /// <param name="isManualSongRequestAvail">Check if song requests are available</param>
        /// <param name="username">User that sent the message</param>
        public void CmdManualSrList(bool isManualSongRequestAvail, string username)
        {
            try
            {
                if (!isManualSongRequestAvail)
                    _irc.SendPublicChatMessage($"Song requests are not available at this time @{username}");
                else
                {
                    string songList = _manualSongRequest.ListSongRequests(_broadcasterId);

                    if (!string.IsNullOrEmpty(songList))
                        _irc.SendPublicChatMessage($"Current list of requested songs: {songList}");
                    else
                        _irc.SendPublicChatMessage($"No song requests have been made @{username}");
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdManualSrList(bool, string)", false, "!rbsrl");
            }
        }

        /// <summary>
        /// Displays link to the list of songs that can be requested manually
        /// </summary>
        /// <param name="isManualSongRequestAvail">Check if song requests are available</param>
        /// <param name="username">User that sent the message</param>
        public void CmdManualSrLink(bool isManualSongRequestAvail, string username)
        {
            try
            {
                if (!isManualSongRequestAvail)
                    _irc.SendPublicChatMessage($"Song requests are not available at this time @{username}");
                else
                    _irc.SendPublicChatMessage($"Here is the link to the songs you can manually request {_botConfig.ManualSongRequestLink}");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdManualSrLink(bool, string)", false, "!rbsl");
            }
        }

        /// <summary>
        /// Request a song for the host to play
        /// </summary>
        /// <param name="isSongRequestAvail">Check if song request system is enabled</param>
        /// <param name="message">Chat message from the user</param>
        /// <param name="username">User that sent the message</param>
        public void CmdManualSr(bool isSongRequestAvail, string message, string username)
        {
            try
            {
                // Check if song request system is enabled
                if (isSongRequestAvail)
                {
                    // Grab the song name from the request
                    int index = message.IndexOf(" ");
                    string songRequest = message.Substring(index + 1);
                    Console.WriteLine("New song request: " + songRequest);

                    // Check if song request has more than allowed symbols
                    if (!Regex.IsMatch(songRequest, @"^[a-zA-Z0-9 \-\(\)\'\?\,\/\""]+$"))
                    {
                        _irc.SendPublicChatMessage("Only letters, numbers, commas, hyphens, parentheses, "
                            + "apostrophes, forward-slash, and question marks are allowed. Please try again. "
                            + "If the problem persists, please contact my creator");
                    }
                    else
                    {
                        _manualSongRequest.AddSongRequest(songRequest, username, _broadcasterId);

                        _irc.SendPublicChatMessage("The song \"" + songRequest + "\" has been successfully requested!");
                    }
                }
                else
                    _irc.SendPublicChatMessage("Song requests are disabled at the moment");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdManualSr(bool, string, string)", false, "!rbsr", message);
            }
        }

        /// <summary>
        /// Displays the current song being played from Spotify
        /// </summary>
        public void CmdSpotifyCurr()
        {
            try
            {
                StatusResponse status = _spotify.GetStatus();
                if (status != null)
                {
                    _irc.SendPublicChatMessage("Current Song: " + status.Track.TrackResource.Name
                        + " >< Artist: " + status.Track.ArtistResource.Name
                        + " >< Album: " + status.Track.AlbumResource.Name);
                }
                else
                    _irc.SendPublicChatMessage("The broadcaster is not playing a song at the moment");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdSpotifyCurr()", false, "!spotifycurr");
            }
        }

        /// <summary>
        /// Slaps a user and rates its effectiveness
        /// </summary>
        /// <param name="message">Chat message from the user</param>
        /// <param name="username">User that sent the message</param>
        public DateTime CmdSlap(string message, string username)
        {
            try
            {
                string recipient = message.Substring(message.IndexOf("@") + 1).ToLower();
                ReactionCmd(username, recipient, "Stop smacking yourself", "slaps", Effectiveness());
                return DateTime.Now.AddSeconds(20);
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdSlap(string, string)", false, "!slap", message);
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Stabs a user and rates its effectiveness
        /// </summary>
        /// <param name="message">Chat message from the user</param>
        /// <param name="username">User that sent the message</param>
        public DateTime CmdStab(string message, string username)
        {
            try
            {
                string recipient = message.Substring(message.IndexOf("@") + 1).ToLower();
                ReactionCmd(username, recipient, "Stop stabbing yourself! You'll bleed out", "stabs", Effectiveness());
                return DateTime.Now.AddSeconds(20);
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdStab(string, string)", false, "!stab", message);
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Shoots a viewer's random body part
        /// </summary>
        /// <param name="message">Chat message from the user</param>
        /// <param name="username">User that sent the message</param>
        public DateTime CmdShoot(string message, string username)
        {
            try
            {
                string bodyPart = "'s ";
                string recipient = message.Substring(message.IndexOf("@") + 1).ToLower();
                Random rnd = new Random(DateTime.Now.Millisecond);
                int bodyPartId = rnd.Next(8); // between 0 and 7

                if (bodyPartId == 0)
                    bodyPart += "head";
                else if (bodyPartId == 1)
                    bodyPart += "left leg";
                else if (bodyPartId == 2)
                    bodyPart += "right leg";
                else if (bodyPartId == 3)
                    bodyPart += "left arm";
                else if (bodyPartId == 4)
                    bodyPart += "right arm";
                else if (bodyPartId == 5)
                    bodyPart += "stomach";
                else if (bodyPartId == 6)
                    bodyPart += "neck";
                else // found largest random value
                    bodyPart = " but missed";

                if (bodyPart.Equals(" but missed"))
                {
                    _irc.SendPublicChatMessage("Ha! You missed @" + username);
                }
                else
                {
                    // bot makes a special response if shot at
                    if (recipient.Equals(_botConfig.BotName.ToLower()))
                    {
                        _irc.SendPublicChatMessage("You think shooting me in the " + bodyPart.Replace("'s ", "") + " would hurt me? I am a bot!");
                    }
                    else // viewer is the target
                    {
                        ReactionCmd(username, recipient, "You just shot your " + bodyPart.Replace("'s ", ""), "shoots", bodyPart);
                        return DateTime.Now.AddSeconds(20);
                    }
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdShoot(string, string)", false, "!shoot", message);
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Throws an item at a viewer and rates its effectiveness against the victim
        /// </summary>
        /// <param name="message">Chat message from the user</param>
        /// <param name="username">User that sent the message</param>
        public DateTime CmdThrow(string message, string username)
        {
            try
            {
                int indexAction = 7;

                if (message.StartsWith("!throw @"))
                    _irc.SendPublicChatMessage("Please throw an item to a user @" + username);
                else
                {
                    string recipient = message.Substring(message.IndexOf("@") + 1).ToLower();
                    string item = message.Substring(indexAction, message.IndexOf("@") - indexAction - 1);

                    ReactionCmd(username, recipient, "Stop throwing " + item + " at yourself", "throws " + item + " at", ". " + Effectiveness());
                    return DateTime.Now.AddSeconds(20);
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdThrow(string, string)", false, "!throw", message);
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Request party member if game and character exists in party up system
        /// </summary>
        /// <param name="message">Chat message from the user</param>
        /// <param name="username">User that sent the message</param>
        public async Task CmdPartyUp(string message, string username)
        {
            try
            {
                int inputIndex = message.IndexOf(" ") + 1;

                // check if user entered something
                if (message.Length < inputIndex)
                    _irc.SendPublicChatMessage($"Please enter a party member @{username}");
                else
                {
                    // get current game info
                    ChannelJSON json = await _twitchInfo.GetBroadcasterChannelById();
                    string gameTitle = json.Game;
                    string partyMember = message.Substring(inputIndex);
                    int gameId = _gameDirectory.GetGameId(gameTitle, out bool hasMultiplayer);

                    // if the game is not found
                    // tell users this game is not accepting party up requests
                    if (gameId == 0)
                        _irc.SendPublicChatMessage("This game is currently not a part of the 'Party Up' system");
                    else // check if user has already requested a party member
                    {
                        if (_partyUp.HasPartyMemberBeenRequested(username, gameId, _broadcasterId))
                            _irc.SendPublicChatMessage($"You have already requested a party member. " 
                                + $"Please wait until your request has been completed @{username}");
                        else // search for party member user is requesting
                        {
                            if (!_partyUp.HasRequestedPartyMember(partyMember, gameId, _broadcasterId))
                                _irc.SendPublicChatMessage($"I couldn't find the requested party member \"{partyMember}\" @{username}. "
                                    + "Please check with the broadcaster for possible spelling errors");
                            else // insert party member if they exists from database
                            {
                                _partyUp.AddPartyMember(username, partyMember, gameId, _broadcasterId);

                                _irc.SendPublicChatMessage($"@{username}: {partyMember} has been added to the party queue");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdPartyUp(string, string)", false, "!partyup", message);
            }
        }

        /// <summary>
        /// Check what other user's have requested
        /// </summary>
        public async Task CmdPartyUpRequestList()
        {
            try
            {
                // get current game info
                ChannelJSON json = await _twitchInfo.GetBroadcasterChannelById();
                string gameTitle = json.Game;
                int gameId = _gameDirectory.GetGameId(gameTitle, out bool hasMultiplayer);

                if (gameId == 0)
                    _irc.SendPublicChatMessage("This game is currently not a part of the \"Party Up\" system");
                else
                    _irc.SendPublicChatMessage(_partyUp.GetRequestList(gameId, _broadcasterId));
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdPartyUpRequestList()", false, "!partyuprequestlist");
            }
        }

        /// <summary>
        /// Check what party members are available (if game is part of the party up system)
        /// </summary>
        public async Task CmdPartyUpList()
        {
            try
            {
                // get current game info
                ChannelJSON json = await _twitchInfo.GetBroadcasterChannelById();
                string gameTitle = json.Game;
                int gameId = _gameDirectory.GetGameId(gameTitle, out bool hasMultiplayer);

                if (gameId == 0)
                    _irc.SendPublicChatMessage("This game is currently not a part of the \"Party Up\" system");
                else
                    _irc.SendPublicChatMessage(_partyUp.GetPartyList(gameId, _broadcasterId));
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdPartyUpList()", false, "!partyuplist");
            }
        }

        /// <summary>
        /// Check user's account balance
        /// </summary>
        /// <param name="username">User that sent the message</param>
        public void CmdCheckFunds(string username)
        {
            try
            {
                int balance = _bank.CheckBalance(username, _broadcasterId);

                if (balance == -1)
                    _irc.SendPublicChatMessage("You are not currently banking with us at the moment. Please talk to a moderator about acquiring " + _botConfig.CurrencyType);
                else
                    _irc.SendPublicChatMessage("@" + username + " currently has " + balance.ToString() + " " + _botConfig.CurrencyType);
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdCheckFunds(string)", false, "![currency name]");
            }
        }

        /// <summary>
        /// Gamble away currency
        /// </summary>
        /// <param name="message">Chat message from the user</param>
        /// <param name="username">User that sent the message</param>
        public DateTime CmdGamble(string message, string username)
        {
            try
            {
                int gambledMoney = 0; // Money put into the gambling system
                bool isValidMsg = int.TryParse(message.Substring(message.IndexOf(" ") + 1), out gambledMoney);
                int walletBalance = _bank.CheckBalance(username, _broadcasterId);

                if (!isValidMsg || gambledMoney < 1)
                    _irc.SendPublicChatMessage($"Please insert a positive whole amount (no decimal numbers) to gamble @{username}");
                else if (gambledMoney > walletBalance)
                    _irc.SendPublicChatMessage($"You do not have the sufficient funds to gamble {gambledMoney} {_botConfig.CurrencyType} @{username}");
                else
                {
                    Random rnd = new Random(DateTime.Now.Millisecond);
                    int diceRoll = rnd.Next(1, 101); // between 1 and 100
                    int newBalance = 0;

                    string result = $"@{username} gambled {gambledMoney} {_botConfig.CurrencyType} and the dice roll was {diceRoll}. They ";

                    // Check the 100-sided die roll result
                    if (diceRoll < 61) // lose gambled money
                    {
                        newBalance = walletBalance - gambledMoney;
                        
                        result += $"lost {gambledMoney} {_botConfig.CurrencyType}";
                    }
                    else if (diceRoll >= 61 && diceRoll <= 98) // earn double
                    {
                        walletBalance -= gambledMoney; // put money into the gambling pot (remove money from wallet)
                        newBalance = walletBalance + (gambledMoney * 2); // recieve 2x earnings back into wallet
                        
                        result += $"won {gambledMoney * 2} {_botConfig.CurrencyType}";
                    }
                    else if (diceRoll == 99 || diceRoll == 100) // earn triple
                    {
                        walletBalance -= gambledMoney; // put money into the gambling pot (remove money from wallet)
                        newBalance = walletBalance + (gambledMoney * 3); // recieve 3x earnings back into wallet
                        
                        result += $"won {gambledMoney * 3} {_botConfig.CurrencyType}";
                    }

                    _bank.UpdateFunds(username, _broadcasterId, newBalance);

                    result += $" and now has {newBalance} {_botConfig.CurrencyType}";

                    _irc.SendPublicChatMessage(result);
                    return DateTime.Now.AddSeconds(20);
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdGamble(string, string)", false, "!gamble", message);
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Display random broadcaster quote
        /// </summary>
        public void CmdQuote()
        {
            try
            {
                List<Quote> quoteList = _quote.GetQuotes(_broadcasterId);

                // Check if there any quotes inside the system
                if (quoteList.Count == 0)
                    _irc.SendPublicChatMessage("There are no quotes to be displayed at the moment");
                else
                {
                    // Randomly pick a quote from the list to display
                    Random rnd = new Random(DateTime.Now.Millisecond);
                    int index = rnd.Next(quoteList.Count);

                    Quote resultingQuote = new Quote();
                    resultingQuote = quoteList.ElementAt(index); // grab random quote from list of quotes
                    string quoteResult = $"\"{resultingQuote.Message}\" - {_botConfig.Broadcaster} " +
                        $"({resultingQuote.TimeCreated.ToString("MMMM", CultureInfo.InvariantCulture)} {resultingQuote.TimeCreated.Year})";

                    _irc.SendPublicChatMessage(quoteResult);
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdQuote()", false, "!quote");
            }
        }

        /// <summary>
        /// Tell the user how long they have been following the broadcaster
        /// </summary>
        /// <param name="username">User that sent the message</param>
        /// <returns></returns>
        public void CmdFollowSince(string username)
        {
            try
            {
                TwitchChatter chatter = _twitchChatterListInstance.TwitchFollowers.FirstOrDefault(c => c.Username.Equals(username));

                if (chatter != null)
                {
                    DateTime startedFollowing = Convert.ToDateTime(chatter.CreatedAt);
                    _irc.SendPublicChatMessage($"@{username} has been following since {startedFollowing.ToLongDateString()}");
                }
                else
                {
                    _irc.SendPublicChatMessage($"{username} is not following {_botConfig.Broadcaster.ToLower()}");
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdFollowSince(string)", false, "!followsince");
            }
        }

        /// <summary>
        /// Display the follower's stream rank
        /// </summary>
        /// <param name="username">User that sent the message</param>
        /// <returns></returns>
        public void CmdViewRank(string username)
        {
            try
            {
                TwitchChatter chatter = _twitchChatterListInstance.TwitchFollowers.FirstOrDefault(c => c.Username.Equals(username));

                if (chatter != null)
                {
                    int currExp = _follower.CurrentExp(username, _broadcasterId);

                    // Grab the follower's associated rank
                    if (currExp > -1)
                    {
                        IEnumerable<Rank> rankList = _follower.GetRankList(_broadcasterId);
                        Rank currFollowerRank = _follower.GetCurrentRank(rankList, currExp);
                        decimal hoursWatched = _follower.GetHoursWatched(currExp);

                        _irc.SendPublicChatMessage($"@{username}: \"{currFollowerRank.Name}\" "
                            + $"{currExp}/{currFollowerRank.ExpCap} EXP ({hoursWatched} hours watched)");
                    }
                    else
                    {
                        _follower.EnlistRecruit(username, _broadcasterId);

                        _irc.SendPublicChatMessage($"Welcome to the army @{username}. View your new rank using !rank");
                    }
                }
                else
                {
                    _irc.SendPublicChatMessage($"{username} is not following {_botConfig.Broadcaster.ToLower()}");
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdViewRank(string)", false, "!rank");
            }
        }

        /// <summary>
        /// Uses the Google API to add YouTube videos to the broadcaster's specified request playlist
        /// </summary>
        /// <param name="message">Chat message from the user</param>
        /// <param name="username">User that sent the message</param>
        /// <param name="hasYouTubeAuth">Checks if broadcaster allowed this bot to post videos to the playlist</param>
        /// <param name="isYouTubeSongRequestAvail">Checks if users can request songs</param>
        /// <returns></returns>
        public async Task<DateTime> CmdYouTubeSongRequest(string message, string username, bool hasYouTubeAuth, bool isYouTubeSongRequestAvail)
        {
            try
            {
                if (!hasYouTubeAuth)
                {
                    _irc.SendPublicChatMessage("YouTube song requests have not been set up");
                    return DateTime.Now;
                }

                if (!isYouTubeSongRequestAvail)
                {
                    _irc.SendPublicChatMessage("YouTube song requests are not turned on");
                    return DateTime.Now;
                }

                int funds = _bank.CheckBalance(username, _broadcasterId);
                int cost = 250; // ToDo: Set YTSR currency cost into settings

                if (funds < cost)
                {
                    _irc.SendPublicChatMessage($"You do not have enough {_botConfig.CurrencyType} to make a song request. "
                        + $"You currently have {funds} {_botConfig.CurrencyType} @{username}");
                }
                else
                {
                    string videoId = "";

                    // Parse video ID based on different types of requests
                    if (message.Contains("youtube.com/watch?v=")) // full URL
                    {
                        int videoIdIndex = message.IndexOf("?v=") + 3;
                        int addParam = message.IndexOf("&", videoIdIndex);

                        if (addParam == -1)
                            videoId = message.Substring(videoIdIndex);
                        else
                            videoId = message.Substring(videoIdIndex, addParam - videoIdIndex);
                    }
                    else if (message.Contains("youtu.be/")) // short URL
                    {
                        int videoIdIndex = message.IndexOf("youtu.be/") + 9;
                        int addParam = message.IndexOf("?", videoIdIndex);

                        if (addParam == -1)
                            videoId = message.Substring(videoIdIndex);
                        else
                            videoId = message.Substring(videoIdIndex, addParam - videoIdIndex);
                    }
                    else if (message.Replace("!ytsr ", "").Length == 11
                        && message.Replace("!ytsr ", "").IndexOf(" ") == -1
                        && Regex.Match(message, @"[\w\-]").Success) // assume only video ID
                    {
                        videoId = message.Replace("!ytsr ", "");
                    }
                    else // search by keyword
                    {
                        string videoKeyword = message.Substring(6);
                        videoId = await _youTubeClientInstance.SearchVideoByKeyword(videoKeyword, 3);
                    }

                    // Confirm if video ID has been found and is a new song request
                    if (string.IsNullOrEmpty(videoId))
                    {
                        _irc.SendPublicChatMessage($"Couldn't find video ID for song request @{username}");
                    }
                    else if (await _youTubeClientInstance.HasDuplicatePlaylistItem(_botConfig.YouTubeBroadcasterPlaylistId, videoId))
                    {
                        _irc.SendPublicChatMessage($"Song has already been requested @{username}");
                    }
                    else
                    {
                        Video video = await _youTubeClientInstance.GetVideoById(videoId, 2);

                        // Check if video's title and account match song request blacklist
                        List<SongRequestBlacklistItem> blacklist = _songRequestBlacklist.GetSongRequestBlackList(_broadcasterId);

                        if (blacklist.Count > 0)
                        {
                            // Check for artist-wide blacklist
                            if (blacklist.Any(
                                    b => (string.IsNullOrEmpty(b.Title)
                                            && video.Snippet.Title.Contains(b.Artist, StringComparison.CurrentCultureIgnoreCase))
                                        || (string.IsNullOrEmpty(b.Title)
                                            && video.Snippet.ChannelTitle.Contains(b.Artist, StringComparison.CurrentCultureIgnoreCase))
                                ))
                            {
                                _irc.SendPublicChatMessage($"This artist cannot be requested at this time @{username}");
                            }
                            // Check for song-specific blacklist
                            else if (blacklist.Any(
                                    b => (!string.IsNullOrEmpty(b.Title) && video.Snippet.Title.Contains(b.Artist, StringComparison.CurrentCultureIgnoreCase)
                                            && video.Snippet.Title.Contains(b.Title, StringComparison.CurrentCultureIgnoreCase)) // both song/artist in video title
                                        || (!string.IsNullOrEmpty(b.Title) && video.Snippet.ChannelTitle.Contains(b.Artist, StringComparison.CurrentCultureIgnoreCase)
                                            && video.Snippet.Title.Contains(b.Title, StringComparison.CurrentCultureIgnoreCase)) // song in title and artist in channel title
                                ))
                            {
                                _irc.SendPublicChatMessage($"This song cannot be requested at this time @{username}");
                            }
                        }

                        // Check if video is blocked in the broadcaster's country
                        CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
                        RegionInfo regionInfo = new RegionInfo(cultureInfo.Name);
                        var regionRestriction = video.ContentDetails.RegionRestriction;

                        if ((regionRestriction?.Allowed != null && !regionRestriction.Allowed.Contains(regionInfo.TwoLetterISORegionName))
                            || (regionRestriction?.Blocked != null && regionRestriction.Blocked.Contains(regionInfo.TwoLetterISORegionName)))
                        {
                            _irc.SendPublicChatMessage($"Your song request is blocked in this broadcaster's country. Please request a different song");
                            return DateTime.Now;
                        }

                        string videoDuration = video.ContentDetails.Duration;

                        // Check if time limit has been reached
                        // ToDo: Make bot setting for duration limit based on minutes (if set)
                        if (!videoDuration.Contains("PT") || videoDuration.Contains("H"))
                        {
                            _irc.SendPublicChatMessage($"Either couldn't find the video duration or it was way too long for the stream @{username}");
                        }
                        else
                        {
                            int timeIndex = videoDuration.IndexOf("T") + 1;
                            string parsedDuration = videoDuration.Substring(timeIndex);
                            int minIndex = parsedDuration.IndexOf("M");

                            string videoMin = "0";
                            string videoSec = "0";
                            int videoMinLimit = 10;
                            int videoSecLimit = 0;

                            if (minIndex > 0)
                                videoMin = parsedDuration.Substring(0, minIndex);

                            if (parsedDuration.IndexOf("S") > 0)
                                videoSec = parsedDuration.Substring(minIndex + 1).TrimEnd('S');

                            if (Convert.ToInt32(videoMin) >= videoMinLimit && Convert.ToInt32(videoSec) >= videoSecLimit)
                            {
                                _irc.SendPublicChatMessage($"Song request is longer than or equal to {videoMinLimit} minute(s) and {videoSecLimit} second(s)");
                            }
                            else
                            {
                                await _youTubeClientInstance.AddVideoToPlaylist(videoId, _botConfig.YouTubeBroadcasterPlaylistId, username);
                                _bank.UpdateFunds(username, _broadcasterId, funds - cost);

                                _irc.SendPublicChatMessage($"@{username} spent {cost} {_botConfig.CurrencyType} " + 
                                    $"and \"{video.Snippet.Title}\" by {video.Snippet.ChannelTitle} was successfully requested!");

                                // Return cooldown time by using one-third of the length of the video duration
                                TimeSpan totalTimeSpan = new TimeSpan(0, Convert.ToInt32(videoMin), Convert.ToInt32(videoSec));
                                TimeSpan oneThirdTimeSpan = new TimeSpan(totalTimeSpan.Ticks / 3);

                                return DateTime.Now.AddSeconds(oneThirdTimeSpan.TotalSeconds);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdYouTubeSongRequest(string, string, bool, bool)", false, "!ytsr");
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Display's link to broadcaster's YouTube song request playlist
        /// </summary>
        /// <param name="hasYouTubeAuth">Checks if broadcaster allowed this bot to post videos to the playlist</param>
        /// <param name="isYouTubeSongRequestAvail">Checks if users can request songs</param>
        public void CmdYouTubeSongRequestList(bool hasYouTubeAuth, bool isYouTubeSongRequestAvail)
        {
            try
            {
                if (hasYouTubeAuth && isYouTubeSongRequestAvail && !string.IsNullOrEmpty(_botConfig.YouTubeBroadcasterPlaylistId))
                {
                    _irc.SendPublicChatMessage($"{_botConfig.Broadcaster.ToLower()}'s song request list is at " +
                        "https://www.youtube.com/playlist?list=" + _botConfig.YouTubeBroadcasterPlaylistId);
                }
                else
                {
                    _irc.SendPublicChatMessage("There is no song request list at this time");
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdYouTubeSongRequestList(bool, bool)", false, "!ytsl");
            }
        }

        /// <summary>
        /// Displays MultiStream link so multiple streamers can be watched at once
        /// </summary>
        /// <param name="username">User that sent the message</param>
        /// <param name="multiStreamUsers">List of broadcasters that are a part of the link</param>
        public void CmdMultiStreamLink(string username, List<string> multiStreamUsers)
        {
            try
            {
                if (multiStreamUsers.Count == 0)
                    _irc.SendPublicChatMessage($"MultiStream link is not set up @{username}");
                else
                {
                    string multiStreamLink = "https://multistre.am/" + _botConfig.Broadcaster.ToLower() + "/";

                    foreach (string multiStreamUser in multiStreamUsers)
                        multiStreamLink += $"{multiStreamUser}/";

                    // Layouts used specifically for multistre.am
                    if (multiStreamUsers.Count == 3)
                        multiStreamLink += "layout11";
                    else if (multiStreamUsers.Count == 2)
                        multiStreamLink += "layout7";
                    else if (multiStreamUsers.Count == 1)
                        multiStreamLink += "layout4";

                    _irc.SendPublicChatMessage($"Check out these awesome streamers at the same time! (Use desktop for best results) {multiStreamLink}");
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdMultiStream(string, List<string>)", false, "!msl");
            }
        }

        /// <summary>
        /// Ask any question and the Magic 8 Ball will give a fortune
        /// </summary>
        /// <param name="username">User that sent the message</param>
        public DateTime CmdMagic8Ball(string username)
        {
            try
            {
                Random rnd = new Random(DateTime.Now.Millisecond);
                int answerId = rnd.Next(20); // between 0 and 19

                string[] possibleAnswers = new string[20]
                {
                    $"It is certain @{username}",
                    $"It is decidedly so @{username}",
                    $"Without a doubt @{username}",
                    $"Yes definitely @{username}",
                    $"You may rely on it @{username}",
                    $"As I see it, yes @{username}",
                    $"Most likely @{username}",
                    $"Outlook good @{username}",
                    $"Yes @{username}",
                    $"Signs point to yes @{username}",
                    $"Reply hazy try again @{username}",
                    $"Ask again later @{username}",
                    $"Better not tell you now @{username}",
                    $"Cannot predict now @{username}",
                    $"Concentrate and ask again @{username}",
                    $"Don't count on it @{username}",
                    $"My reply is no @{username}",
                    $"My sources say no @{username}",
                    $"Outlook not so good @{username}",
                    $"Very doubtful @{username}"
                };

                _irc.SendPublicChatMessage(possibleAnswers[answerId]);
                return DateTime.Now.AddSeconds(20);
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdMagic8Ball(string)", false, "!8ball");
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Disply the top 3 richest users (if available)
        /// </summary>
        /// <param name="username">User that sent the message</param>
        public void CmdLeaderboardCurrency(string username)
        {
            try
            {
                List<BalanceResult> richestUsers = _bank.GetCurrencyLeaderboard(_botConfig.Broadcaster, _broadcasterId, _botConfig.BotName);

                if (richestUsers.Count == 0)
                {
                    _irc.SendPublicChatMessage($"Everyone's broke! @{username}");
                    return;
                }

                string resultMsg = "";
                foreach (BalanceResult user in richestUsers)
                {
                    resultMsg += $"\"{user.Username}\" with {user.Wallet} {_botConfig.CurrencyType}, ";
                }

                resultMsg = resultMsg.Remove(resultMsg.Length - 2); // remove extra ","

                // improve list grammar
                if (richestUsers.Count == 2)
                    resultMsg = resultMsg.ReplaceLastOccurrence(", ", " and ");
                else if (richestUsers.Count > 2)
                    resultMsg = resultMsg.ReplaceLastOccurrence(", ", ", and ");

                if (richestUsers.Count == 1)
                    _irc.SendPublicChatMessage($"The richest user is {resultMsg}");
                else
                    _irc.SendPublicChatMessage($"The richest users are: {resultMsg}");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdLeaderboardCurrency(string)", false, "![currency name]top3");
            }
        }

        /// <summary>
        /// Display the top 3 highest ranking members (if available)
        /// </summary>
        /// <param name="username">User that sent the message</param>
        public void CmdLeaderboardRank(string username)
        {
            try
            {
                IEnumerable<Follower> highestRankedFollowers = _follower.GetFollowersLeaderboard(_botConfig.Broadcaster, _broadcasterId, _botConfig.BotName);

                if (highestRankedFollowers.Count() == 0)
                {
                    _irc.SendPublicChatMessage($"There's no one in your ranks. Start recruiting today! @{username}");
                    return;
                }

                IEnumerable<Rank> rankList = _follower.GetRankList(_broadcasterId);

                string resultMsg = "";
                foreach (Follower follower in highestRankedFollowers)
                {
                    Rank currFollowerRank = _follower.GetCurrentRank(rankList, follower.Exp);
                    decimal hoursWatched = _follower.GetHoursWatched(follower.Exp);

                    resultMsg += $"\"{currFollowerRank.Name} {follower.Username}\" with {hoursWatched} hour(s), ";
                }

                resultMsg = resultMsg.Remove(resultMsg.Length - 2); // remove extra ","

                // improve list grammar
                if (highestRankedFollowers.Count() == 2)
                    resultMsg = resultMsg.ReplaceLastOccurrence(", ", " and ");
                else if (highestRankedFollowers.Count() > 2)
                    resultMsg = resultMsg.ReplaceLastOccurrence(", ", ", and ");

                if (highestRankedFollowers.Count() == 1)
                    _irc.SendPublicChatMessage($"This leader's highest ranking member is {resultMsg}");
                else
                    _irc.SendPublicChatMessage($"This leader's highest ranking members are: {resultMsg}");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdLeaderboardRank(string)", false, "!ranktop3");
            }
        }

        /// <summary>
        /// Play a friendly game of Russian Roulette and risk chat privileges for stream currency
        /// </summary>
        /// <param name="username">User that sent the message</param>
        /// <param name="rouletteUsers">List of roulette users that have attempted and survived</param>
        public DateTime CmdRussianRoulette(string username, ref List<RouletteUser> rouletteUsers)
        {
            try
            {
                RouletteUser rouletteUser = rouletteUsers.FirstOrDefault(u => u.Username.Equals(username));

                Random rnd = new Random(DateTime.Now.Millisecond);
                int bullet = rnd.Next(6); // between 0 and 5

                if (bullet == 0) // user was shot
                {
                    if (rouletteUser != null)
                        rouletteUsers.Remove(rouletteUser);

                    if (_modInstance.ListMods.Contains(username) || _botConfig.Broadcaster.ToLower().Equals(username))
                    {
                        _irc.SendPublicChatMessage($"Enjoy your 15 minutes without russian roulette @{username}");
                        return DateTime.Now.AddMinutes(15);
                    }

                    _irc.SendChatTimeout(username, 300); // 5 minute timeout
                    _irc.SendPublicChatMessage($"You are dead @{username}. Enjoy your 5 minutes in limbo (cannot talk)");
                    return DateTime.Now;
                }

                if (rouletteUser == null) // new roulette user
                {
                    rouletteUser = new RouletteUser() { Username = username, ShotsTaken = 1 };
                    rouletteUsers.Add(rouletteUser);

                    _irc.SendPublicChatMessage($"@{username} -> 1/6 attempts");
                }
                else // existing roulette user
                {
                    if (rouletteUser.ShotsTaken < 6)
                    {
                        foreach (var user in rouletteUsers)
                        {
                            if (user.Username.Equals(username))
                            {
                                user.ShotsTaken++;
                                break;
                            }
                        }
                    }

                    string responseMessage = $"@{username} -> {rouletteUser.ShotsTaken}/6 attempts";

                    if (rouletteUser.ShotsTaken == 6)
                    {
                        int funds = _bank.CheckBalance(username, _broadcasterId);
                        int reward = 3000; // ToDo: Make roulette reward deposit config setting

                        if (funds > -1)
                        {
                            funds += reward; // deposit 500 stream currency
                            _bank.UpdateFunds(username, _broadcasterId, funds);
                        }
                        else
                            _bank.CreateAccount(username, _broadcasterId, reward);

                        rouletteUsers.RemoveAll(u => u.Username.Equals(username));

                        responseMessage = $"Congrats on surviving russian roulette. Here's {reward} {_botConfig.CurrencyType}!";

                        // Special cooldown for moderators/broadcasters after they win
                        if (_modInstance.ListMods.Contains(username) || _botConfig.Broadcaster.ToLower().Equals(username))
                        {
                            _irc.SendPublicChatMessage(responseMessage);
                            return DateTime.Now.AddMinutes(5);
                        }
                    }

                    _irc.SendPublicChatMessage(responseMessage);
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdRussianRoulette(string, ref List<RouletteUser>)", false, "!roulette");
            }

            return DateTime.Now;
        }

        public void CmdListGotNextGame(string username, Queue<string> gameQueueUsers)
        {
            try
            {
                if (!IsMultiplayerGame(username)) return;

                if (gameQueueUsers.Count == 0)
                {
                    _irc.SendPublicChatMessage($"No one wants to play with the streamer at the moment. "
                        + "Be the first to play with !gotnextgame");
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
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdListGotNextGame(string, Queue<string>)", false, "!listgotnext");
            }
        }

        public void CmdGotNextGame(string username, ref Queue<string> gameQueueUsers)
        {
            try
            {
                if (!IsMultiplayerGame(username)) return;

                if (gameQueueUsers.Contains(username))
                {
                    _irc.SendPublicChatMessage($"Don't worry @{username}. You're on the list to play with " +
                        $"the streamer with your current position at {gameQueueUsers.ToList().IndexOf(username) + 1} " +
                        $"of {gameQueueUsers.Count} user(s)");
                }
                else
                {
                    gameQueueUsers.Enqueue(username);

                    _irc.SendPublicChatMessage($"Congrats @{username}! You're currently in line with your current position at " +
                        $"{gameQueueUsers.ToList().IndexOf(username) + 1} of {gameQueueUsers.Count} user(s)");
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdGotNextGame(string, Queue<string>)", false, "!gotnextgame");
            }
        }

        public void CmdBankHeist(string message, string username)
        {
            try
            {
                BankHeist bankHeist = new BankHeist();
                int funds = _bank.CheckBalance(username, _broadcasterId);
                bool isValid = int.TryParse(message.Substring(message.IndexOf(" ")), out int gamble);

                if (_heistSettingsInstance.IsHeistOnCooldown())
                {
                    TimeSpan cooldown = _heistSettingsInstance.CooldownTimePeriod.Subtract(DateTime.Now);

                    if (cooldown.Minutes >= 1)
                    {
                        _irc.SendPublicChatMessage(_heistSettingsInstance.CooldownEntry
                            .Replace("@timeleft@", cooldown.Minutes.ToString()));
                    }
                    else
                    {
                        _irc.SendPublicChatMessage(_heistSettingsInstance.CooldownEntry
                            .Replace("@timeleft@", cooldown.Seconds.ToString())
                            .Replace("minutes", "seconds"));
                    }

                    return;
                }

                if (bankHeist.HasRobberAlreadyEntered(username))
                {
                    _irc.SendPublicChatMessage($"You are already in this heist @{username}");
                    return;
                }

                // check if funds and gambling amount are valid
                if (!isValid)
                {
                    _irc.SendPublicChatMessage($"Please gamble with a positive amount of {_botConfig.CurrencyType} @{username}");
                    return;
                }
                else if (gamble < 1)
                {
                    _irc.SendPublicChatMessage($"You cannot gamble less than one {_botConfig.CurrencyType} @{username}");
                    return;
                }
                else if (funds < 1)
                {
                    _irc.SendPublicChatMessage($"You need at least one {_botConfig.CurrencyType} to join the heist @{username}");
                    return;
                }
                else if (funds < gamble)
                {
                    _irc.SendPublicChatMessage($"You do not have enough to gamble {gamble} {_botConfig.CurrencyType} @{username}");
                    return;
                }
                else if (gamble > _heistSettingsInstance.MaxGamble)
                {
                    _irc.SendPublicChatMessage($"{_heistSettingsInstance.MaxGamble} {_botConfig.CurrencyType} is the most you can put in. " + 
                        $"Please try again with less {_botConfig.CurrencyType} @{username}");
                    return;
                }
                
                if (!bankHeist.IsEntryPeriodOver())
                {
                    // make heist announcement if first robber and start recruiting members
                    if (_heistSettingsInstance.Robbers.Count == 0)
                    {
                        _heistSettingsInstance.EntryPeriod = DateTime.Now.AddSeconds(_heistSettingsInstance.EntryPeriodSeconds);
                        _irc.SendPublicChatMessage(_heistSettingsInstance.EntryMessage.Replace("user@", username));
                    }

                    // join bank heist
                    BankRobber robber = new BankRobber { Username = username, Gamble = gamble };
                    bankHeist.Produce(robber);
                    _bank.UpdateFunds(username, _broadcasterId, funds - gamble);

                    // display new heist level
                    if (!string.IsNullOrEmpty(bankHeist.NextLevelMessage()))
                    {
                        _irc.SendPublicChatMessage(bankHeist.NextLevelMessage());
                    }

                    // display if more than one robber joins
                    if (_heistSettingsInstance.Robbers.Count > 1)
                    {
                        _irc.SendPublicChatMessage($"@{username} has joined the heist");
                    }
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdBankHeist(string, string)", false, "!bankheist");
            }
        }

        public async Task CmdSubscribe()
        {
            try
            {
                // Get broadcaster type
                ChannelJSON json = await _twitchInfo.GetBroadcasterChannelById();
                string broadcasterType = json.BroadcasterType;

                if (broadcasterType.Equals("partner") || broadcasterType.Equals("affiliate"))
                {
                    _irc.SendPublicChatMessage("Subscribe here! https://www.twitch.tv/subs/" + _botConfig.Broadcaster);
                }
                else
                {
                    _irc.SendPublicChatMessage($"{_botConfig.Broadcaster} is not a Twitch Affiliate/Partner. "
                        + "Please stick around and make their dream not a meme Kappa");
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdSubscribe()", false, "!sub");
            }
        }

        /// <summary>
        /// Tell the user how long they have been subscribing to the broadcaster
        /// </summary>
        /// <param name="username">User that sent the message</param>
        /// <returns></returns>
        public void CmdSubscribeSince(string username)
        {
            try
            {
                TwitchChatter chatter = _twitchChatterListInstance.TwitchSubscribers.FirstOrDefault(c => c.Username.Equals(username));

                if (chatter != null)
                {
                    DateTime startedSubscribing = Convert.ToDateTime(chatter.CreatedAt);
                    _irc.SendPublicChatMessage($"@{username} has been a subscriber since {startedSubscribing.ToLongDateString()}");
                }
                else
                {
                    _irc.SendPublicChatMessage($"{username} is not subscriber to {_botConfig.Broadcaster.ToLower()}");
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdSubscribeSince(string)", false, "!subsince");
            }
        }

        public void CmdBossFight(string message, string username)
        {
            try
            {
                BossFight bossFight = new BossFight();
                int funds = _bank.CheckBalance(username, _broadcasterId);

                if (_bossSettingsInstance.IsBossFightOnCooldown())
                {
                    TimeSpan cooldown = _bossSettingsInstance.CooldownTimePeriod.Subtract(DateTime.Now);

                    if (cooldown.Minutes >= 1)
                    {
                        _irc.SendPublicChatMessage(_bossSettingsInstance.CooldownEntry
                            .Replace("@timeleft@", cooldown.Minutes.ToString()));
                    }
                    else
                    {
                        _irc.SendPublicChatMessage(_bossSettingsInstance.CooldownEntry
                            .Replace("@timeleft@", cooldown.Seconds.ToString())
                            .Replace("minutes", "seconds"));
                    }

                    return;
                }

                if (bossFight.HasFighterAlreadyEntered(username))
                {
                    _irc.SendPublicChatMessage($"You are already in this fight @{username}");
                    return;
                }

                if (funds < _bossSettingsInstance.Cost)
                {
                    _irc.SendPublicChatMessage($"You do need {_bossSettingsInstance.Cost} {_botConfig.CurrencyType} to enter this fight @{username}");
                    return;
                }

                if (!bossFight.IsEntryPeriodOver())
                {
                    // make boss fight announcement if first fighter and start recruiting members
                    if (_bossSettingsInstance.Fighters.Count == 0)
                    {
                        _bossSettingsInstance.EntryPeriod = DateTime.Now.AddSeconds(_bossSettingsInstance.EntryPeriodSeconds);
                        _irc.SendPublicChatMessage(_bossSettingsInstance.EntryMessage.Replace("user@", username));
                    }

                    // join boss fight
                    // ToDo: check chatter types in this order (subscribers, moderators, regulars, followers, viewers)
                    ChatterType chatterType = _twitchChatterListInstance.ChattersByType.First(c => c.TwitchChatters.Equals(username)).ChatterType;
                    FighterClass fighterClass = _bossSettingsInstance.ClassStats.Single(c => c.ChatterType.Equals(chatterType));
                    BossFighter fighter = new BossFighter { Username = username, FighterClass = fighterClass };
                    bossFight.Produce(fighter);
                    _bank.UpdateFunds(username, _broadcasterId, funds - _bossSettingsInstance.Cost);

                    // display new boss level
                    if (!string.IsNullOrEmpty(bossFight.NextLevelMessage()))
                    {
                        _irc.SendPublicChatMessage(bossFight.NextLevelMessage());
                    }

                    // display if more than one fighter joins
                    if (_bossSettingsInstance.Fighters.Count > 1)
                    {
                        _irc.SendPublicChatMessage($"@{username} has joined the fight");
                    }
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdGen", "CmdBossFight(string, string)", false, "!bossfight");
            }
        }

        private bool IsMultiplayerGame(string username)
        {
            // Get current game name
            ChannelJSON json = _twitchInfo.GetBroadcasterChannelById().Result;
            string gameTitle = json.Game;

            // Grab game id in order to find party member
            int gameId = _gameDirectory.GetGameId(gameTitle, out bool hasMultiplayer);

            if (gameId == 0)
            {
                _irc.SendPublicChatMessage($"I cannot find this game in the database. Have my master resolve this issue @{username}");
                return false;
            }

            if (hasMultiplayer == false)
            {
                _irc.SendPublicChatMessage($"This game is set to single-player only. Verify with my master with this game @{username}");
                return false;
            }

            return true;
        }

        private bool ReactionCmd(string origUser, string recipient, string msgToSelf, string action, string addlMsg = "")
        {
            // check if user is trying to use a command on themselves
            if (origUser.Equals(recipient))
            {
                _irc.SendPublicChatMessage(msgToSelf + " @" + origUser);
                return true;
            }

            // check if recipient is the broadcaster before checking the viewer channel
            if (ChatterValid(origUser, recipient))
            {
                _irc.SendPublicChatMessage(origUser + " " + action + " @" + recipient + " " + addlMsg);
                return true;
            }

            return false;
        }

        private bool ChatterValid(string origUser, string recipient)
        {
            // Check if the requested user is this bot
            if (recipient.Equals(_botConfig.BotName.ToLower()) || recipient.Equals(_botConfig.Broadcaster.ToLower()))
                return true;

            // Wait until chatter lists are available
            while (!_twitchChatterListInstance.AreListsAvailable)
            {
                
            }

            // Grab user's chatter info (viewers, mods, etc.)
            List<string> chatterList = _twitchChatterListInstance.ChattersByName;
            if (chatterList.Count > 0)
            {
                // Search for user
                foreach (string chatter in chatterList)
                {
                    if (chatter.Equals(recipient.ToLower()))
                        return true;
                }
            }

            // finished searching with no results
            _irc.SendPublicChatMessage($"@{origUser}: I cannot find the user you wanted to interact with. Perhaps the user left us?");
            return false;
        }

        private string Effectiveness()
        {
            Random rnd = new Random(DateTime.Now.Millisecond);
            int effectiveLvl = rnd.Next(3); // between 0 and 2
            string effectiveness = "";

            if (effectiveLvl == 0)
                effectiveness = "It's super effective!";
            else if (effectiveLvl == 1)
                effectiveness = "It wasn't very effective";
            else
                effectiveness = "It had no effect";

            return effectiveness;
        }
    }
}
