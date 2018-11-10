using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Google.Apis.YouTube.v3.Data;

using TwitchBot.Configuration;
using TwitchBot.Libraries;
using TwitchBot.Models;
using TwitchBot.Models.JSON;
using TwitchBot.Services;

using TwitchBotDb.Models;

using TwitchBotUtil.Extensions;

namespace TwitchBot.Commands
{
    public class CmdBrdCstr
    {
        private IrcClient _irc;
        private System.Configuration.Configuration _appConfig;
        private TwitchBotConfigurationSection _botConfig;
        private int _broadcasterId;
        private SongRequestBlacklistService _songRequest;
        private TwitchInfoService _twitchInfo;
        private GameDirectoryService _gameDirectory;
        private SongRequestSettingService _songRequestSetting;
        private TwitterClient _twitter = TwitterClient.Instance;
        private YoutubeClient _youTubeClientInstance = YoutubeClient.Instance;
        private ErrorHandler _errHndlrInstance = ErrorHandler.Instance;
        private BroadcasterSingleton _broadcasterInstance = BroadcasterSingleton.Instance;
        private BossFightSingleton _bossFightSettingsInstance = BossFightSingleton.Instance;

        public CmdBrdCstr(IrcClient irc, TwitchBotConfigurationSection botConfig, int broadcasterId, 
            System.Configuration.Configuration appConfig, SongRequestBlacklistService songRequest, TwitchInfoService twitchInfo, 
            GameDirectoryService gameDirectory, SongRequestSettingService songRequestSetting)
        {
            _irc = irc;
            _botConfig = botConfig;
            _broadcasterId = broadcasterId;
            _appConfig = appConfig;
            _songRequest = songRequest;
            _twitchInfo = twitchInfo;
            _gameDirectory = gameDirectory;
            _songRequestSetting = songRequestSetting;
        }

        /// <summary>
        /// Display bot settings
        /// </summary>
        public async void CmdBotSettings()
        {
            try
            {
                _irc.SendPublicChatMessage($"Auto tweets set to \"{_botConfig.EnableTweets}\" "
                    + $">< Auto display songs set to \"{_botConfig.EnableDisplaySong}\" "
                    + $">< Currency set to \"{_botConfig.CurrencyType}\" "
                    + $">< Stream Latency set to \"{_botConfig.StreamLatency} second(s)\" "
                    + $">< Regular follower hours set to \"{_botConfig.RegularFollowerHours}\"");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdBotSettings()", false, "!settings");
            }
        }

        /// <summary>
        /// Stop running the bot
        /// </summary>
        public async void CmdExitBot()
        {
            try
            {
                _irc.SendPublicChatMessage("Bye! Have a beautiful time!");
                Environment.Exit(0); // exit program
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdExitBot()", false, "!exit");
            }
        }

        /// <summary>
        /// Enables tweets to be sent out from this bot (both auto publish tweets and manual tweets)
        /// </summary>
        /// <param name="hasTwitterInfo">Check for Twitter credentials</param>
        public async void CmdEnableTweet(bool hasTwitterInfo)
        {
            try
            {
                if (!hasTwitterInfo)
                    _irc.SendPublicChatMessage("You are missing twitter info @" + _botConfig.Broadcaster);
                else
                {
                    _botConfig.EnableTweets = true;
                    _appConfig.AppSettings.Settings.Remove("enableTweets");
                    _appConfig.AppSettings.Settings.Add("enableTweets", "true");
                    _appConfig.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection("TwitchBotConfiguration");

                    Console.WriteLine("Auto publish tweets is set to [" + _botConfig.EnableTweets + "]");
                    _irc.SendPublicChatMessage(_botConfig.Broadcaster + ": Automatic tweets is set to \"" + _botConfig.EnableTweets + "\"");
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdEnableTweet(bool)", false, "!sendtweet on");
            }
        }

        /// <summary>
        /// Disables tweets to be sent out from this bot (both auto publish tweets and manual tweets)
        /// </summary>
        /// <param name="hasTwitterInfo">Check for Twitter credentials</param>
        public async void CmdDisableTweet(bool hasTwitterInfo)
        {
            try
            {
                if (!hasTwitterInfo)
                    _irc.SendPublicChatMessage("You are missing twitter info @" + _botConfig.Broadcaster);
                else
                {
                    _botConfig.EnableTweets = false;
                    _appConfig.AppSettings.Settings.Remove("enableTweets");
                    _appConfig.AppSettings.Settings.Add("enableTweets", "false");
                    _appConfig.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection("TwitchBotConfiguration");

                    Console.WriteLine("Auto publish tweets is set to [" + _botConfig.EnableTweets + "]");
                    _irc.SendPublicChatMessage(_botConfig.Broadcaster + ": Automatic tweets is set to \"" + _botConfig.EnableTweets + "\"");
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdDisableTweet(bool)", false, "!sendtweet off");
            }
        }

        /// <summary>
        /// Enable manual song request mode
        /// </summary>
        public async Task<bool> CmdEnableManualSrMode(bool isManualSongRequestAvail)
        {
            try
            {
                _irc.SendPublicChatMessage("Song requests enabled");
                isManualSongRequestAvail = true;
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdEnableManualSrMode()", false, "!rbsrmode on");
            }

            return isManualSongRequestAvail;
        }

        /// <summary>
        /// Disable manual song request mode
        /// </summary>
        public async Task<bool> CmdDisableManualSrMode(bool isManualSongRequestAvail)
        {
            try
            {
                _irc.SendPublicChatMessage("Song requests disabled");
                isManualSongRequestAvail = false;
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdDisableSRMode()", false, "!rbsrmode off");
            }

            return isManualSongRequestAvail;
        }

        /// <summary>
        /// Enable manual song request mode
        /// </summary>
        public async Task<bool> CmdEnableYouTubeSrMode(bool isYouTubeSongRequestAvail)
        {
            try
            {
                _irc.SendPublicChatMessage("YouTube song requests enabled");
                isYouTubeSongRequestAvail = true;
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdEnableYouTubeSrMode()", false, "!ytsrmode on");
            }

            return isYouTubeSongRequestAvail;
        }

        /// <summary>
        /// Disable manual song request mode
        /// </summary>
        public async Task<bool> CmdDisableYouTubeSrMode(bool isYouTubeSongRequestAvail)
        {
            try
            {
                _irc.SendPublicChatMessage("YouTube song requests disabled");
                isYouTubeSongRequestAvail = false;
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdDisableYouTubeSrMode()", false, "!ytsrmode off");
            }

            return isYouTubeSongRequestAvail;
        }

        /// <summary>
        /// Manually send a tweet
        /// </summary>
        /// <param name="hasTwitterInfo">Check if user has provided the specific twitter credentials</param>
        /// <param name="message">Chat message from the user</param>
        public async void CmdTweet(bool hasTwitterInfo, string message)
        {
            try
            {
                if (!hasTwitterInfo)
                    _irc.SendPublicChatMessage("You are missing twitter info @" + _botConfig.Broadcaster);
                else
                    _irc.SendPublicChatMessage(_twitter.SendTweet(message.Replace("!tweet ", "")));
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdTweet(bool, string)", false, "!tweet");
            }
        }

        /// <summary>
        /// Enables displaying songs from Spotify into the IRC chat
        /// </summary>
        public async void CmdEnableDisplaySongs()
        {
            try
            {
                _botConfig.EnableDisplaySong = true;
                _appConfig.AppSettings.Settings.Remove("enableDisplaySong");
                _appConfig.AppSettings.Settings.Add("enableDisplaySong", "true");
                _appConfig.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("TwitchBotConfiguration");

                Console.WriteLine("Auto display songs is set to [" + _botConfig.EnableDisplaySong + "]");
                _irc.SendPublicChatMessage(_botConfig.Broadcaster + ": Automatic display Spotify songs is set to \"" + _botConfig.EnableDisplaySong + "\"");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdEnableDisplaySongs()", false, "!displaysongs on");
            }
        }

        /// <summary>
        /// Disables displaying songs from Spotify into the IRC chat
        /// </summary>
        public async void CmdDisableDisplaySongs()
        {
            try
            {
                _botConfig.EnableDisplaySong = false;
                _appConfig.AppSettings.Settings.Remove("enableDisplaySong");
                _appConfig.AppSettings.Settings.Add("enableDisplaySong", "false");
                _appConfig.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("TwitchBotConfiguration");

                Console.WriteLine("Auto display songs is set to [" + _botConfig.EnableDisplaySong + "]");
                _irc.SendPublicChatMessage(_botConfig.Broadcaster + ": Automatic display Spotify songs is set to \"" + _botConfig.EnableDisplaySong + "\"");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdDisableDisplaySongs()", false, "!displaysongs off");
            }
        }

        public async Task CmdAddSongRequestBlacklist(string message)
        {
            try
            {
                int requestIndex = message.GetNthCharIndex(' ', 2);

                if (requestIndex == -1)
                {
                    _irc.SendPublicChatMessage($"Please enter a request @{_botConfig.Broadcaster}");
                    return;
                }
                
                string requestType = message.Substring(message.IndexOf(" ") + 1, 1);
                string request = message.Substring(requestIndex + 1);

                // Check if request is based on an artist or just a song by an artist
                if (requestType.Equals("1")) // blackout any song by this artist
                {
                    // check if song-specific request is being used for artist blackout
                    if (request.Count(c => c == '"') == 2
                        || request.Count(c => c == '<') == 1
                        || request.Count(c => c == '>') == 1)
                    {
                        _irc.SendPublicChatMessage($"Please use request type 2 for song-specific blacklist-restrictions @{_botConfig.Broadcaster}");
                        return;
                    }

                    List<SongRequestIgnore> blacklist = await _songRequest.GetSongRequestIgnore(_broadcasterId);
                    if (blacklist.Count > 0 && blacklist.Exists(b => b.Artist.Equals(request, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        _irc.SendPublicChatMessage($"This song is already on the blacklist @{_botConfig.Broadcaster}");
                        return;
                    }

                    SongRequestIgnore response = await _songRequest.IgnoreArtist(request, _broadcasterId);

                    if (response != null)
                        _irc.SendPublicChatMessage($"The artist \"{response.Artist}\" has been added to the blacklist @{_botConfig.Broadcaster}");
                    else
                        _irc.SendPublicChatMessage($"I'm sorry. I'm not able to add this artist to the blacklist at this time @{_botConfig.Broadcaster}");
                }
                else if (requestType.Equals("2")) // blackout a song by an artist
                {
                    if (request.Count(c => c == '"') < 2 
                        || request.Count(c => c == '<') != 1 
                        || request.Count(c => c == '>') != 1)
                    {
                        _irc.SendPublicChatMessage($"Please surround the song title with \" (quotation marks) " + 
                            $"and the artist with \"<\" and \">\" @{_botConfig.Broadcaster}");
                        return;
                    }

                    int songTitleStartIndex = message.IndexOf('"');
                    int songTitleEndIndex = message.LastIndexOf('"');
                    int artistStartIndex = message.IndexOf('<');
                    int artistEndIndex = message.IndexOf('>');

                    string songTitle = message.Substring(songTitleStartIndex + 1, songTitleEndIndex - songTitleStartIndex - 1);
                    string artist = message.Substring(artistStartIndex + 1, artistEndIndex - artistStartIndex - 1);

                    // check if the request's exact song or artist-wide blackout-restriction has already been added
                    List<SongRequestIgnore> blacklist = await _songRequest.GetSongRequestIgnore(_broadcasterId);

                    if (blacklist.Count > 0)
                    { 
                        if (blacklist.Exists(b => b.Artist.Equals(artist, StringComparison.CurrentCultureIgnoreCase) 
                                && b.Title.Equals(songTitle, StringComparison.CurrentCultureIgnoreCase)))
                        {
                            _irc.SendPublicChatMessage($"This song is already on the blacklist @{_botConfig.Broadcaster}");
                            return;
                        }
                        else if (blacklist.Exists(b => b.Artist.Equals(artist, StringComparison.CurrentCultureIgnoreCase)))
                        {
                            _irc.SendPublicChatMessage($"This song's artist is already on the blacklist @{_botConfig.Broadcaster}");
                            return;
                        }
                    }

                    SongRequestIgnore response = await _songRequest.IgnoreSong(songTitle, artist, _broadcasterId);

                    if (response != null)
                        _irc.SendPublicChatMessage($"The song \"{response.Title} by {response.Artist}\" has been added to the blacklist @{_botConfig.Broadcaster}");
                    else
                        _irc.SendPublicChatMessage($"I'm sorry. I'm not able to add this song to the blacklist at this time @{_botConfig.Broadcaster}");
                }
                else
                {
                    _irc.SendPublicChatMessage($"Please insert request type (1 = artist/2 = song) @{_botConfig.Broadcaster}");
                    return;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdAddSongRequestBlacklist(string)", false, "!srbl");
            }
        }

        public async Task CmdRemoveSongRequestBlacklist(string message)
        {
            try
            {
                int requestIndex = message.GetNthCharIndex(' ', 2);

                if (requestIndex == -1)
                {
                    _irc.SendPublicChatMessage($"Please enter a request @{_botConfig.Broadcaster}");
                    return;
                }

                string requestType = message.Substring(message.IndexOf(" ") + 1, 1);
                string request = message.Substring(requestIndex + 1);

                // Check if request is based on an artist or just a song by an artist
                if (requestType.Equals("1")) // remove blackout for any song by this artist
                {
                    // remove artist from db
                    List<SongRequestIgnore> response = await _songRequest.AllowArtist(request, _broadcasterId);

                    if (response != null)
                        _irc.SendPublicChatMessage($"The artist \"{request}\" can now be requested @{_botConfig.Broadcaster}");
                    else
                        _irc.SendPublicChatMessage($"Couldn't find the requested artist for blacklist-removal @{_botConfig.Broadcaster}");
                }
                else if (requestType.Equals("2")) // remove blackout for a song by an artist
                {
                    if (request.Count(c => c == '"') < 2
                        || request.Count(c => c == '<') != 1
                        || request.Count(c => c == '>') != 1)
                    {
                        _irc.SendPublicChatMessage($"Please surround the song title with \" (quotation marks) " 
                            + $"and the artist with \"<\" and \">\" @{_botConfig.Broadcaster}");
                        return;
                    }

                    int songTitleStartIndex = message.IndexOf('"');
                    int songTitleEndIndex = message.LastIndexOf('"');
                    int artistStartIndex = message.IndexOf('<');
                    int artistEndIndex = message.IndexOf('>');

                    string songTitle = message.Substring(songTitleStartIndex + 1, songTitleEndIndex - songTitleStartIndex - 1);
                    string artist = message.Substring(artistStartIndex + 1, artistEndIndex - artistStartIndex - 1);

                    // remove artist from db
                    SongRequestIgnore response = await _songRequest.AllowSong(songTitle, artist, _broadcasterId);

                    if (response != null)
                        _irc.SendPublicChatMessage($"The song \"{response.Title} by {response.Artist}\" can now requested @{_botConfig.Broadcaster}");
                    else
                        _irc.SendPublicChatMessage($"Couldn't find the requested song for blacklist-removal @{_botConfig.Broadcaster}");
                }
                else
                {
                    _irc.SendPublicChatMessage($"Please insert request type (1 = artist/2 = song) @{_botConfig.Broadcaster}");
                    return;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdRemoveSongRequestBlacklist(string)", false, "!delsrbl");
            }
        }

        public async Task CmdResetSongRequestBlacklist()
        {
            try
            {
                List<SongRequestIgnore> response = await _songRequest.ResetIgnoreList(_broadcasterId);

                if (response?.Count > 0)
                    _irc.SendPublicChatMessage($"Song Request Blacklist has been reset @{_botConfig.Broadcaster}");
                else
                    _irc.SendPublicChatMessage($"Song Request Blacklist is empty @{_botConfig.Broadcaster}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdResetSongRequestBlacklist()", false, "!resetsrbl");
            }
        }

        public async Task CmdListSongRequestBlacklist()
        {
            try
            {
                List<SongRequestIgnore> blacklist = await _songRequest.GetSongRequestIgnore(_broadcasterId);

                if (blacklist.Count == 0)
                {
                    _irc.SendPublicChatMessage($"The song request blacklist is empty @{_botConfig.Broadcaster}");
                    return;
                }

                string songList = "";

                foreach (SongRequestIgnore item in blacklist.OrderBy(i => i.Artist))
                {
                    if (!string.IsNullOrEmpty(item.Title))
                        songList += $"\"{item.Title}\" - ";

                    songList += $"{item.Artist} >< ";
                }

                StringBuilder strBdrSongList = new StringBuilder(songList);
                strBdrSongList.Remove(songList.Length - 4, 4); // remove extra " >< "
                songList = strBdrSongList.ToString(); // replace old song list string with new

                _irc.SendPublicChatMessage($"Blacklisted Song/Artists: < {songList} >");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdListSongRequestBlacklist()", false, "!showsrbl");
            }
        }

        public async Task CmdLive(bool hasTwitterInfo)
        {
            try
            {
                RootStreamJSON streamJSON = await _twitchInfo.GetBroadcasterStream();

                if (streamJSON.Stream == null)
                    _irc.SendPublicChatMessage("This channel is not streaming right now");
                else if (!_botConfig.EnableTweets)
                    _irc.SendPublicChatMessage("Tweets are disabled at the moment");
                else if (_botConfig.EnableTweets && hasTwitterInfo)
                {
                    string tweetResult = _twitter.SendTweet($"Live on Twitch playing {streamJSON.Stream.Game} "
                        + $"\"{streamJSON.Stream.Channel.Status}\" twitch.tv/{_botConfig.Broadcaster}");

                    _irc.SendPublicChatMessage($"{tweetResult} @{_botConfig.Broadcaster}");
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdLive(bool)", false, "!live");
            }
        }

        public async Task CmdRefreshReminders()
        {
            try
            {
                await Threads.ChatReminder.RefreshReminders();
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdRefreshReminders()", false, "!refreshreminders");
            }
        }

        public async Task CmdRefreshBossFight()
        {
            try
            {
                // Check if any fighters are queued or fighting
                if (_bossFightSettingsInstance.Fighters.Count > 0)
                {
                    _irc.SendPublicChatMessage($"A boss fight is either queued or in progress @{_botConfig.Broadcaster}");
                    return;
                }

                // Get current game name
                ChannelJSON json = await _twitchInfo.GetBroadcasterChannelById();
                string gameTitle = json.Game;

                // Grab game id in order to find party member
                TwitchGameCategory game = await _gameDirectory.GetGameId(gameTitle);

                // During refresh, make sure no fighters can join
                _bossFightSettingsInstance.RefreshBossFight = true;
                await _bossFightSettingsInstance.LoadSettings(_broadcasterId, game?.Id, _botConfig.TwitchBotApiLink);
                _bossFightSettingsInstance.RefreshBossFight = false;

                _irc.SendPublicChatMessage($"Boss fight settings refreshed @{_botConfig.Broadcaster}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdRefreshBossFight()", false, "!refreshbossfight");
            }
        }

        public async void CmdSetRegularFollowerHours(string message)
        {
            try
            {
                bool validInput = int.TryParse(message.Substring(17), out int regularHours);
                if (!validInput)
                {
                    _irc.SendPublicChatMessage($"I can't process the time you've entered. " + 
                        $"Please insert positive hours @{_botConfig.Broadcaster}");
                    return;
                }
                else if (regularHours < 1)
                {
                    _irc.SendPublicChatMessage($"Please insert positive hours @{_botConfig.Broadcaster}");
                    return;
                }

                _botConfig.RegularFollowerHours = regularHours;
                _appConfig.AppSettings.Settings.Remove("regularFollowerHours");
                _appConfig.AppSettings.Settings.Add("regularFollowerHours", regularHours.ToString());
                _appConfig.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("TwitchBotConfiguration");

                Console.WriteLine($"Regular followers are set to {_botConfig.RegularFollowerHours}");
                _irc.SendPublicChatMessage($"{_botConfig.Broadcaster} : Regular followers now need {_botConfig.RegularFollowerHours} hours");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdSetRegularFollowerHours(string)", false, "!setregularhours");
            }
        }

        public async Task CmdResetYoutubeSongRequestList(bool hasYouTubeAuth)
        {
            try
            {
                if (!hasYouTubeAuth)
                {
                    _irc.SendPublicChatMessage("YouTube song requests have not been set up");
                    return;
                }

                // Check if user has a song request playlist, else create one
                string playlistName = _botConfig.YouTubeBroadcasterPlaylistName;
                string defaultPlaylistName = $"Twitch Song Requests";

                if (string.IsNullOrEmpty(playlistName))
                {
                    playlistName = defaultPlaylistName;
                }

                Playlist broadcasterPlaylist = null;

                // Check for existing playlist id
                if (!string.IsNullOrEmpty(_botConfig.YouTubeBroadcasterPlaylistId))
                {
                    broadcasterPlaylist = await _youTubeClientInstance.GetBroadcasterPlaylistById(_botConfig.YouTubeBroadcasterPlaylistId);
                }

                if (broadcasterPlaylist?.Id != null)
                    await _youTubeClientInstance.DeletePlaylist(broadcasterPlaylist.Id);

                broadcasterPlaylist = await _youTubeClientInstance.CreatePlaylist(playlistName,
                    "Songs requested via Twitch viewers on https://twitch.tv/" + _botConfig.Broadcaster
                        + " . Playlist automatically created courtesy of https://github.com/SimpleSandman/TwitchBot");

                _botConfig.YouTubeBroadcasterPlaylistId = broadcasterPlaylist.Id;
                _appConfig.AppSettings.Settings.Remove("youTubeBroadcasterPlaylistId");
                _appConfig.AppSettings.Settings.Add("youTubeBroadcasterPlaylistId", broadcasterPlaylist.Id);
                _botConfig.YouTubeBroadcasterPlaylistName = broadcasterPlaylist.Snippet.Title;
                _appConfig.AppSettings.Settings.Remove("youTubeBroadcasterPlaylistName");
                _appConfig.AppSettings.Settings.Add("youTubeBroadcasterPlaylistName", broadcasterPlaylist.Snippet.Title);
                _appConfig.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("TwitchBotConfiguration");

                await _songRequestSetting.UpdateSongRequestSetting(
                    _botConfig.YouTubeBroadcasterPlaylistId, _botConfig.YouTubePersonalPlaylistId, 
                    _broadcasterInstance.DatabaseId, false);

                _irc.SendPublicChatMessage($"YouTube playlist has been reset @{_botConfig.Broadcaster} " 
                    + "and is now at this link https://www.youtube.com/playlist?list=" + _botConfig.YouTubeBroadcasterPlaylistId);
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdResetYoutubeSongRequestList(bool)", false, "!resetytsr");
            }
        }

        public async Task CmdEnableDjMode()
        {
            try
            {
                // ToDo: Make HTTP PATCH request instead of full PUT
                await _songRequestSetting.UpdateSongRequestSetting(
                    _botConfig.YouTubeBroadcasterPlaylistId, _botConfig.YouTubePersonalPlaylistId,
                    _broadcasterInstance.DatabaseId, true);

                _irc.SendPublicChatMessage($"DJing has been enabled for YouTube song requests @{_botConfig.Broadcaster}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdEnableDjMode()", false, "!djmode on");
            }
        }

        public async Task CmdDisableDjMode()
        {
            try
            {
                // ToDo: Make HTTP PATCH request instead of full PUT
                await _songRequestSetting.UpdateSongRequestSetting(
                    _botConfig.YouTubeBroadcasterPlaylistId, _botConfig.YouTubePersonalPlaylistId,
                    _broadcasterInstance.DatabaseId, false);

                _irc.SendPublicChatMessage($"DJing has been disabled for YouTube song requests @{_botConfig.Broadcaster}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdDisableDjMode()", false, "!djmode off");
            }
        }

        public async Task CmdSetPersonalYoutubePlaylistById(string message)
        {
            try
            {
                string personalPlaylistId = message.Substring(message.IndexOf(" ") + 1);
                if (personalPlaylistId.Length != 34)
                {
                    _irc.SendPublicChatMessage("Please only insert the playlist ID that you want set " 
                        + $"when the song requests are finished/not available @{_botConfig.Broadcaster}");
                    return;
                }

                Playlist playlist = await _youTubeClientInstance.GetPlaylistById(personalPlaylistId);

                if (playlist?.Id == null)
                {
                    _irc.SendPublicChatMessage($"I'm sorry @{_botConfig.Broadcaster} I cannot find your playlist " 
                        + "you requested as a backup when song requests are finished/not available");
                    return;
                }

                SongRequestSetting songRequestSetting = await _songRequestSetting.GetSongRequestSetting(_broadcasterInstance.DatabaseId);

                // Save song request info into database
                if (songRequestSetting?.BroadcasterId == _broadcasterInstance.DatabaseId)
                {
                    // ToDo: Make HTTP PATCH request instead of full PUT
                    await _songRequestSetting.UpdateSongRequestSetting(
                        _botConfig.YouTubeBroadcasterPlaylistId,
                        _botConfig.YouTubePersonalPlaylistId,
                        _broadcasterInstance.DatabaseId,
                        songRequestSetting.DjMode);
                }
                else
                {
                    _irc.SendPublicChatMessage("Cannot find settings in database! Please contact my creator using " 
                        + $"the command \"!support\" if this problem persists @{_botConfig.Broadcaster}");
                    return;
                }

                _botConfig.YouTubePersonalPlaylistId = personalPlaylistId;
                _appConfig.AppSettings.Settings.Remove("youTubePersonalPlaylistId");
                _appConfig.AppSettings.Settings.Add("youTubePersonalPlaylistId", personalPlaylistId);
                _appConfig.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("TwitchBotConfiguration");

                _irc.SendPublicChatMessage($"Your personal playlist has been set https://www.youtube.com/playlist?list={personalPlaylistId} @{_botConfig.Broadcaster}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdSetPersonalYoutubePlaylistById(string)", false, "!setpersonalplaylistid");
            }
        }
    }
}
