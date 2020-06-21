using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Google.Apis.YouTube.v3.Data;

using TwitchBot.Configuration;
using TwitchBot.Libraries;
using TwitchBot.Models;
using TwitchBot.Services;
using TwitchBot.Threads;

using TwitchBotDb.Models;

using TwitchBotUtil.Extensions;

namespace TwitchBot.Commands.Features
{
    /// <summary>
    /// The "Command Subsystem" for the "Song Request" feature
    /// </summary>
    public sealed class SongRequestFeature : BaseFeature
    {
        private readonly SongRequestBlacklistService _songRequestBlacklist;
        private readonly LibVLCSharpPlayer _libVLCSharpPlayer;
        private readonly SongRequestSettingService _songRequestSetting;
        private readonly System.Configuration.Configuration _appConfig;
        private readonly ManualSongRequestService _manualSongRequest;
        private readonly BroadcasterSingleton _broadcasterInstance = BroadcasterSingleton.Instance;
        private readonly YoutubeClient _youTubeClientInstance = YoutubeClient.Instance;
        private readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        public SongRequestFeature(IrcClient irc, TwitchBotConfigurationSection botConfig, System.Configuration.Configuration appConfig,
            SongRequestBlacklistService songRequestBlacklist, LibVLCSharpPlayer libVLCSharpPlayer, SongRequestSettingService songRequestSetting,
            ManualSongRequestService manualSongRequest) : base(irc, botConfig)
        {
            _songRequestBlacklist = songRequestBlacklist;
            _libVLCSharpPlayer = libVLCSharpPlayer;
            _songRequestSetting = songRequestSetting;
            _appConfig = appConfig;
            _manualSongRequest = manualSongRequest;
            _rolePermission.Add("!srbl", "broadcaster");
            _rolePermission.Add("!delsrbl", "broadcaster");
            _rolePermission.Add("!resetsrbl", "broadcaster");
            _rolePermission.Add("!showsrbl", "broadcaster");
            _rolePermission.Add("!resetytsr", "broadcaster");
            _rolePermission.Add("!setpersonalplaylistid", "broadcaster");
            _rolePermission.Add("!djmode", "broadcaster");
            _rolePermission.Add("!msrmode", "broadcaster");
            _rolePermission.Add("!ytsrmode", "broadcaster");
            _rolePermission.Add("!displaysongs", "broadcaster");
            _rolePermission.Add("!resetmsr", "moderator");
        }

        public override async void ExecCommand(TwitchChatter chatter, string requestedCommand)
        {
            try
            {
                switch (requestedCommand)
                {
                    case "!resetsrbl":
                        await ResetSongRequestBlacklist();
                        break;
                    case "!showsrbl":
                        await ListSongRequestBlacklist();
                        break;
                    case "!resetytsr":
                        await ResetYoutubeSongRequestList();
                        break;
                    case "!msrmode":
                        SetManualSrMode(chatter);
                        break;
                    case "!ytsrmode":
                        SetYouTubeSrMode(chatter);
                        break;
                    case "!displaysongs":
                        SetAutoDisplaySongs(chatter);
                        break;
                    case "!djmode":
                        await SetDjMode(chatter);
                        break;
                    case "!srbl":
                        await AddSongRequestBlacklist(chatter);
                        break;
                    case "!delsrbl":
                        await RemoveSongRequestBlacklist(chatter);
                        break;
                    case "!setpersonalplaylistid":
                        await SetPersonalYoutubePlaylistById(chatter);
                        break;
                    case "!resetmsr":
                        await ResetManualSr();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "SongRequestFeature", "ExecCommand(TwitchChatter, string)", false, requestedCommand, chatter.Message);
            }
        }

        public async Task AddSongRequestBlacklist(TwitchChatter chatter)
        {
            try
            {
                string message = chatter.Message;
                int requestIndex = message.GetNthCharIndex(' ', 2);

                if (requestIndex == -1)
                {
                    _irc.SendPublicChatMessage($"Please enter a request to block @{_botConfig.Broadcaster}");
                    return;
                }

                string requestType = message.Substring(message.IndexOf(" ") + 1, 1);
                string request = message.Substring(requestIndex + 1);

                // Check if request is based on an artist or just a song by an artist
                if (requestType == "1") // blackout any song by this artist
                {
                    // check if song-specific request is being used for artist blackout
                    if (request.Count(c => c == '"') == 2
                        || request.Count(c => c == '<') == 1
                        || request.Count(c => c == '>') == 1)
                    {
                        _irc.SendPublicChatMessage($"Please use request type 2 for song-specific blacklist restrictions @{_botConfig.Broadcaster}");
                        return;
                    }

                    List<SongRequestIgnore> blacklist = await _songRequestBlacklist.GetSongRequestIgnore(_broadcasterInstance.DatabaseId);
                    if (blacklist.Count > 0 && blacklist.Exists(b => b.Artist.Equals(request, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        _irc.SendPublicChatMessage($"This artist/video is already on the blacklist @{_botConfig.Broadcaster}");
                        return;
                    }

                    SongRequestIgnore response = await _songRequestBlacklist.IgnoreArtist(request, _broadcasterInstance.DatabaseId);

                    if (response != null)
                        _irc.SendPublicChatMessage($"The artist/video \"{response.Artist}\" has been added to the blacklist @{_botConfig.Broadcaster}");
                    else
                        _irc.SendPublicChatMessage($"I'm sorry. I'm not able to add this artist/video to the blacklist at this time @{_botConfig.Broadcaster}");
                }
                else if (requestType == "2") // blackout a song by an artist
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
                    List<SongRequestIgnore> blacklist = await _songRequestBlacklist.GetSongRequestIgnore(_broadcasterInstance.DatabaseId);

                    if (blacklist.Count > 0)
                    {
                        if (blacklist.Exists(b => b.Artist.Equals(artist, StringComparison.CurrentCultureIgnoreCase)
                                && b.Title.Equals(songTitle, StringComparison.CurrentCultureIgnoreCase)))
                        {
                            _irc.SendPublicChatMessage($"This song is already on the blacklist @{_botConfig.Broadcaster}");
                            return;
                        }
                    }

                    SongRequestIgnore response = await _songRequestBlacklist.IgnoreSong(songTitle, artist, _broadcasterInstance.DatabaseId);

                    if (response != null)
                        _irc.SendPublicChatMessage($"The song \"{response.Title}\" by \"{response.Artist}\" has been added to the blacklist @{_botConfig.Broadcaster}");
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
                await _errHndlrInstance.LogError(ex, "SongRequestFeature", "AddSongRequestBlacklist(TwitchChatter)", false, "!srbl", chatter.Message);
            }
        }

        public async Task RemoveSongRequestBlacklist(TwitchChatter chatter)
        {
            try
            {
                string message = chatter.Message;
                int requestIndex = message.GetNthCharIndex(' ', 2);

                if (requestIndex == -1)
                {
                    _irc.SendPublicChatMessage($"Please enter a request @{_botConfig.Broadcaster}");
                    return;
                }

                string requestType = message.Substring(message.IndexOf(" ") + 1, 1);
                string request = message.Substring(requestIndex + 1);

                // Check if request is based on an artist or just a song by an artist
                if (requestType == "1") // remove blackout for any song by this artist
                {
                    // remove artist from db
                    List<SongRequestIgnore> response = await _songRequestBlacklist.AllowArtist(request, _broadcasterInstance.DatabaseId);

                    if (response != null)
                        _irc.SendPublicChatMessage($"The artist \"{request}\" can now be requested @{_botConfig.Broadcaster}");
                    else
                        _irc.SendPublicChatMessage($"Couldn't find the requested artist for blacklist-removal @{_botConfig.Broadcaster}");
                }
                else if (requestType == "2") // remove blackout for a song by an artist
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
                    SongRequestIgnore response = await _songRequestBlacklist.AllowSong(songTitle, artist, _broadcasterInstance.DatabaseId);

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
                await _errHndlrInstance.LogError(ex, "SongRequestFeature", "RemoveSongRequestBlacklist(string)", false, "!delsrbl");
            }
        }

        public async Task ResetSongRequestBlacklist()
        {
            try
            {
                List<SongRequestIgnore> response = await _songRequestBlacklist.ResetIgnoreList(_broadcasterInstance.DatabaseId);

                if (response?.Count > 0)
                    _irc.SendPublicChatMessage($"Song Request Blacklist has been reset @{_botConfig.Broadcaster}");
                else
                    _irc.SendPublicChatMessage($"Song Request Blacklist is empty @{_botConfig.Broadcaster}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "SongRequestFeature", "ResetSongRequestBlacklist()", false, "!resetsrbl");
            }
        }

        public async Task ListSongRequestBlacklist()
        {
            try
            {
                List<SongRequestIgnore> blacklist = await _songRequestBlacklist.GetSongRequestIgnore(_broadcasterInstance.DatabaseId);

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
                await _errHndlrInstance.LogError(ex, "SongRequestFeature", "ListSongRequestBlacklist()", false, "!showsrbl");
            }
        }

        public async Task ResetYoutubeSongRequestList()
        {
            try
            {
                if (!_botConfig.IsYouTubeSongRequestAvail)
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
                {
                    await _youTubeClientInstance.DeletePlaylist(broadcasterPlaylist.Id);
                }

                broadcasterPlaylist = await _youTubeClientInstance.CreatePlaylist(playlistName,
                    "Songs requested via Twitch viewers on https://twitch.tv/" + _botConfig.Broadcaster
                        + " . Playlist automatically created courtesy of https://github.com/SimpleSandman/TwitchBot");

                // Save broadcaster playlist info to config
                _botConfig.YouTubeBroadcasterPlaylistId = broadcasterPlaylist.Id;
                CommandToolbox.SaveAppConfigSettings(broadcasterPlaylist.Id, "youTubeBroadcasterPlaylistId", _appConfig);

                _botConfig.YouTubeBroadcasterPlaylistName = broadcasterPlaylist.Snippet.Title;                
                CommandToolbox.SaveAppConfigSettings(broadcasterPlaylist.Snippet.Title, "youTubeBroadcasterPlaylistName", _appConfig);

                SongRequestSetting songRequestSetting = await _songRequestSetting.GetSongRequestSetting(_broadcasterInstance.DatabaseId);

                // ToDo: Make HTTP PATCH request instead of full PUT
                await _songRequestSetting.UpdateSongRequestSetting
                (
                    _botConfig.YouTubeBroadcasterPlaylistId, _botConfig.YouTubePersonalPlaylistId,
                    _broadcasterInstance.DatabaseId, songRequestSetting.DjMode
                );

                _libVLCSharpPlayer.ResetSongRequestQueue();

                _irc.SendPublicChatMessage($"YouTube song request playlist has been reset @{_botConfig.Broadcaster} "
                    + "and is now at this link https://www.youtube.com/playlist?list=" + _botConfig.YouTubeBroadcasterPlaylistId);
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "SongRequestFeature", "ResetYoutubeSongRequestList()", false, "!resetytsr");
            }
        }

        public async Task SetPersonalYoutubePlaylistById(TwitchChatter chatter)
        {
            try
            {
                string personalPlaylistId = CommandToolbox.ParseChatterCommandParameter(chatter);
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
                CommandToolbox.SaveAppConfigSettings(personalPlaylistId, "youTubePersonalPlaylistId", _appConfig);

                _irc.SendPublicChatMessage($"Your personal playlist has been set https://www.youtube.com/playlist?list={personalPlaylistId} @{_botConfig.Broadcaster}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "SongRequestFeature", "SetPersonalYoutubePlaylistById(TwitchChatter)", false, "!setpersonalplaylistid");
            }
        }

        /// <summary>
        /// Set DJ mode for YouTube song requests
        /// </summary>
        /// <param name="chatter"></param>
        /// <returns></returns>
        public async Task SetDjMode(TwitchChatter chatter)
        {
            try
            {
                string message = CommandToolbox.ParseChatterCommandParameter(chatter);
                bool hasDjModeEnabled = CommandToolbox.SetBooleanFromMessage(message);

                // ToDo: Make HTTP PATCH request instead of full PUT
                await _songRequestSetting.UpdateSongRequestSetting(
                    _botConfig.YouTubeBroadcasterPlaylistId, 
                    _botConfig.YouTubePersonalPlaylistId,
                    _broadcasterInstance.DatabaseId, 
                    hasDjModeEnabled);

                _irc.SendPublicChatMessage($"DJing has been set to {hasDjModeEnabled} for YouTube song requests. "
                    + $"Please wait a few seconds before this change is applied @{_botConfig.Broadcaster}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "SongRequestFeature", "SetDjMode(TwitchChatter)", false, "!djmode");
            }
        }

        /// <summary>
        /// Set manual song request mode
        /// </summary>
        /// <param name="chatter"></param>
        /// <returns></returns>
        public async void SetManualSrMode(TwitchChatter chatter)
        {
            try
            {
                string message = CommandToolbox.ParseChatterCommandParameter(chatter);
                bool shuffle = CommandToolbox.SetBooleanFromMessage(message);
                string boolValue = shuffle ? "true" : "false";

                _botConfig.IsManualSongRequestAvail = shuffle;
                CommandToolbox.SaveAppConfigSettings(boolValue, "isManualSongRequestAvail", _appConfig);

                _irc.SendPublicChatMessage($"{_botConfig.Broadcaster}: Song requests set to {boolValue}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "SongRequestFeature", "SetManualSrMode(TwitchChatter)", false, "!msrmode");
            }
        }

        /// <summary>
        /// Set YouTube song request mode
        /// </summary>
        /// <param name="chatter"></param>
        /// <returns></returns>
        public async void SetYouTubeSrMode(TwitchChatter chatter)
        {
            try
            {
                string message = CommandToolbox.ParseChatterCommandParameter(chatter);
                bool shuffle = CommandToolbox.SetBooleanFromMessage(message);
                string boolValue = shuffle ? "true" : "false";

                _botConfig.IsYouTubeSongRequestAvail = shuffle;
                CommandToolbox.SaveAppConfigSettings(boolValue, "isYouTubeSongRequestAvail", _appConfig);

                _irc.SendPublicChatMessage($"{_botConfig.Broadcaster}: YouTube song requests set to {boolValue}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "SongRequestFeature", "SetYouTubeSrMode(TwitchChatter)", false, "!ytsrmode");
            }
        }

        /// <summary>
        /// Enables displaying songs from Spotify into the IRC chat
        /// </summary>
        /// <param name="chatter"></param>
        /// <returns></returns>
        public async void SetAutoDisplaySongs(TwitchChatter chatter)
        {
            try
            {
                string message = CommandToolbox.ParseChatterCommandParameter(chatter);
                bool shuffle = CommandToolbox.SetBooleanFromMessage(message);
                string boolValue = shuffle ? "true" : "false";

                _botConfig.EnableDisplaySong = shuffle;
                CommandToolbox.SaveAppConfigSettings(boolValue, "enableDisplaySong", _appConfig);

                _irc.SendPublicChatMessage($"{_botConfig.Broadcaster}: Automatic display Spotify songs is set to \"{boolValue}\"");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "SongRequestFeature", "SetAutoDisplaySongs(TwitchChatter)", false, "!displaysongs");
            }
        }

        /// <summary>
        /// Resets the song request queue
        /// </summary>
        public async Task ResetManualSr()
        {
            try
            {
                List<SongRequest> removedSong = await _manualSongRequest.ResetSongRequests(_broadcasterInstance.DatabaseId);

                if (removedSong != null && removedSong.Count > 0)
                    _irc.SendPublicChatMessage($"The song request queue has been reset @{_botConfig.Broadcaster}");
                else
                    _irc.SendPublicChatMessage($"Song requests are empty @{_botConfig.Broadcaster}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "SongRequestFeature", "ResetManualSr()", false, "!resetrsr");
            }
        }
    }
}
