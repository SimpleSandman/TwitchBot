using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.YouTube.v3.Data;

using LibVLCSharp.Shared;

using TwitchBotCore.Config;
using TwitchBotCore.Enums;
using TwitchBotCore.Libraries;
using TwitchBotCore.Models;
using TwitchBotCore.Services;
using TwitchBotCore.Threads;

using TwitchBotDb.Models;

using TwitchBotUtil.Extensions;

namespace TwitchBotCore.Commands.Features
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
        private readonly BankService _bank;
        private readonly SpotifyWebClient _spotify;
        private readonly BroadcasterSingleton _broadcasterInstance = BroadcasterSingleton.Instance;
        private readonly YoutubeClient _youTubeClientInstance = YoutubeClient.Instance;
        private readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        public SongRequestFeature(IrcClient irc, TwitchBotConfigurationSection botConfig, System.Configuration.Configuration appConfig,
            SongRequestBlacklistService songRequestBlacklist, LibVLCSharpPlayer libVLCSharpPlayer, SongRequestSettingService songRequestSetting,
            ManualSongRequestService manualSongRequest, BankService bank, SpotifyWebClient spotify) : base(irc, botConfig)
        {
            _songRequestBlacklist = songRequestBlacklist;
            _libVLCSharpPlayer = libVLCSharpPlayer;
            _songRequestSetting = songRequestSetting;
            _appConfig = appConfig;
            _manualSongRequest = manualSongRequest;
            _bank = bank;
            _spotify = spotify;
            _rolePermission.Add("!srbl", new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermission.Add("!delsrbl", new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermission.Add("!resetsrbl", new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermission.Add("!showsrbl", new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermission.Add("!resetytsr", new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermission.Add("!setpersonalplaylistid", new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermission.Add("!djmode", new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermission.Add("!msrmode", new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermission.Add("!ytsrmode", new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermission.Add("!displaysongs", new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermission.Add("!resetmsr", new CommandPermission { General = ChatterType.Moderator });
            _rolePermission.Add("!sr", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!ytsr", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!songrequest", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!sl", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!ytsl", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!songlist", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!rsl", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!rsr", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!song", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!wrongsong", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!lastsong", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!poprsr", new CommandPermission { General = ChatterType.VIP });
        }

        public override async Task<(bool, DateTime)> ExecCommand(TwitchChatter chatter, string requestedCommand)
        {
            try
            {
                switch (requestedCommand)
                {
                    case "!resetsrbl":
                        return (true, await ResetSongRequestBlacklist());
                    case "!showsrbl":
                        return (true, await ListSongRequestBlacklist());
                    case "!resetytsr":
                        return (true, await ResetYoutubeSongRequestList());
                    case "!msrmode":
                        return (true, await SetManualSongRequestMode(chatter));
                    case "!ytsrmode":
                        return (true, await SetYouTubeSongRequestMode(chatter));
                    case "!displaysongs":
                        return (true, await SetAutoDisplaySongs(chatter));
                    case "!djmode":
                        return (true, await SetDjMode(chatter));
                    case "!srbl":
                        return (true, await AddSongRequestBlacklist(chatter));
                    case "!delsrbl":
                        return (true, await RemoveSongRequestBlacklist(chatter));
                    case "!setpersonalplaylistid":
                        return (true, await SetPersonalYoutubePlaylistById(chatter));
                    case "!resetmsr":
                        return (true, await ResetManualSongRequest());
                    case "!sr":
                    case "!ytsr":
                    case "!songrequest":
                        return (true, await YouTubeSongRequest(chatter));
                    case "!sl":
                    case "!ytsl":
                    case "!songlist":
                        return (true, await YouTubeSongRequestList());
                    case "!rsl":
                        return (true, await ManuallyRequestedSongRequestList(chatter));
                    case "!rsr":
                        return (true, await ManuallyRequestedSongRequest(chatter));
                    case "!song":
                        return (true, await YouTubeCurrentSong(chatter));
                    case "!wrongsong":
                        return (true, await YoutubeRemoveWrongSong(chatter));
                    case "!lastsong":
                        return (true, await YouTubeLastSong(chatter));
                    case "!poprsr":
                        return (true, await PopManuallyRequestedSongRequest());
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "SongRequestFeature", "ExecCommand(TwitchChatter, string)", false, requestedCommand, chatter.Message);
            }

            return (false, DateTime.Now);
        }

        private async Task<DateTime> AddSongRequestBlacklist(TwitchChatter chatter)
        {
            try
            {
                string message = chatter.Message;
                int requestIndex = message.GetNthCharIndex(' ', 2);

                if (requestIndex == -1)
                {
                    _irc.SendPublicChatMessage($"Please enter a request to block @{chatter.DisplayName}");
                    return DateTime.Now;
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
                        _irc.SendPublicChatMessage($"Please use request type 2 for song-specific blacklist restrictions @{chatter.DisplayName}");
                        return DateTime.Now;
                    }

                    List<SongRequestIgnore> blacklist = await _songRequestBlacklist.GetSongRequestIgnore(_broadcasterInstance.DatabaseId);
                    if (blacklist.Count > 0 && blacklist.Exists(b => b.Artist.Equals(request, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        _irc.SendPublicChatMessage($"This artist/video is already on the blacklist @{chatter.DisplayName}");
                        return DateTime.Now;
                    }

                    SongRequestIgnore response = await _songRequestBlacklist.IgnoreArtist(request, _broadcasterInstance.DatabaseId);

                    if (response != null)
                        _irc.SendPublicChatMessage($"The artist/video \"{response.Artist}\" has been added to the blacklist @{chatter.DisplayName}");
                    else
                        _irc.SendPublicChatMessage($"I'm sorry. I'm not able to add this artist/video to the blacklist at this time @{chatter.DisplayName}");
                }
                else if (requestType == "2") // blackout a song by an artist
                {
                    if (request.Count(c => c == '"') < 2
                        || request.Count(c => c == '<') != 1
                        || request.Count(c => c == '>') != 1)
                    {
                        _irc.SendPublicChatMessage($"Please surround the song title with \" (quotation marks) " +
                            $"and the artist with \"<\" and \">\" @{chatter.DisplayName}");
                        return DateTime.Now;
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
                            _irc.SendPublicChatMessage($"This song is already on the blacklist @{chatter.DisplayName}");
                            return DateTime.Now;
                        }
                    }

                    SongRequestIgnore response = await _songRequestBlacklist.IgnoreSong(songTitle, artist, _broadcasterInstance.DatabaseId);

                    if (response != null)
                        _irc.SendPublicChatMessage($"The song \"{response.Title}\" by \"{response.Artist}\" has been added to the blacklist @{chatter.DisplayName}");
                    else
                        _irc.SendPublicChatMessage($"I'm sorry. I'm not able to add this song to the blacklist at this time @{chatter.DisplayName}");
                }
                else
                {
                    _irc.SendPublicChatMessage($"Please insert request type (1 = artist/2 = song) @{chatter.DisplayName}");
                    return DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "SongRequestFeature", "AddSongRequestBlacklist(TwitchChatter)", false, "!srbl", chatter.Message);
            }

            return DateTime.Now;
        }

        private async Task<DateTime> RemoveSongRequestBlacklist(TwitchChatter chatter)
        {
            try
            {
                string message = chatter.Message;
                int requestIndex = message.GetNthCharIndex(' ', 2);

                if (requestIndex == -1)
                {
                    _irc.SendPublicChatMessage($"Please enter a request @{chatter.DisplayName}");
                    return DateTime.Now;
                }

                string requestType = message.Substring(message.IndexOf(" ") + 1, 1);
                string request = message.Substring(requestIndex + 1);

                // Check if request is based on an artist or just a song by an artist
                if (requestType == "1") // remove blackout for any song by this artist
                {
                    // remove artist from db
                    List<SongRequestIgnore> response = await _songRequestBlacklist.AllowArtist(request, _broadcasterInstance.DatabaseId);

                    if (response != null)
                        _irc.SendPublicChatMessage($"The artist \"{request}\" can now be requested @{chatter.DisplayName}");
                    else
                        _irc.SendPublicChatMessage($"Couldn't find the requested artist for blacklist-removal @{chatter.DisplayName}");
                }
                else if (requestType == "2") // remove blackout for a song by an artist
                {
                    if (request.Count(c => c == '"') < 2
                        || request.Count(c => c == '<') != 1
                        || request.Count(c => c == '>') != 1)
                    {
                        _irc.SendPublicChatMessage($"Please surround the song title with \" (quotation marks) "
                            + $"and the artist with \"<\" and \">\" @{chatter.DisplayName}");
                        return DateTime.Now;
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
                        _irc.SendPublicChatMessage($"The song \"{response.Title} by {response.Artist}\" can now requested @{chatter.DisplayName}");
                    else
                        _irc.SendPublicChatMessage($"Couldn't find the requested song for blacklist-removal @{chatter.DisplayName}");
                }
                else
                {
                    _irc.SendPublicChatMessage($"Please insert request type (1 = artist/2 = song) @{chatter.DisplayName}");
                    return DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "SongRequestFeature", "RemoveSongRequestBlacklist(string)", false, "!delsrbl");
            }

            return DateTime.Now;
        }

        private async Task<DateTime> ResetSongRequestBlacklist()
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

            return DateTime.Now;
        }

        private async Task<DateTime> ListSongRequestBlacklist()
        {
            try
            {
                List<SongRequestIgnore> blacklist = await _songRequestBlacklist.GetSongRequestIgnore(_broadcasterInstance.DatabaseId);

                if (blacklist.Count == 0)
                {
                    _irc.SendPublicChatMessage($"The song request blacklist is empty @{_botConfig.Broadcaster}");
                    return DateTime.Now;
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

            return DateTime.Now;
        }

        private async Task<DateTime> ResetYoutubeSongRequestList()
        {
            try
            {
                if (!_botConfig.IsYouTubeSongRequestAvail)
                {
                    _irc.SendPublicChatMessage("YouTube song requests have not been set up");
                    return DateTime.Now;
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
                SaveAppConfigSettings(broadcasterPlaylist.Id, "youTubeBroadcasterPlaylistId", _appConfig);

                _botConfig.YouTubeBroadcasterPlaylistName = broadcasterPlaylist.Snippet.Title;                
                SaveAppConfigSettings(broadcasterPlaylist.Snippet.Title, "youTubeBroadcasterPlaylistName", _appConfig);

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

            return DateTime.Now;
        }

        private async Task<DateTime> SetPersonalYoutubePlaylistById(TwitchChatter chatter)
        {
            try
            {
                string personalPlaylistId = ParseChatterCommandParameter(chatter);
                if (personalPlaylistId.Length != 34)
                {
                    _irc.SendPublicChatMessage("Please only insert the playlist ID that you want set "
                        + $"when the song requests are finished/not available @{chatter.DisplayName}");
                    return DateTime.Now;
                }

                Playlist playlist = await _youTubeClientInstance.GetPlaylistById(personalPlaylistId);

                if (playlist?.Id == null)
                {
                    _irc.SendPublicChatMessage($"I'm sorry @{chatter.DisplayName} I cannot find your playlist "
                        + "you requested as a backup when song requests are finished/not available");
                    return DateTime.Now;
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
                        + $"the command \"!support\" if this problem persists @{chatter.DisplayName}");
                    return DateTime.Now;
                }

                _botConfig.YouTubePersonalPlaylistId = personalPlaylistId;
                SaveAppConfigSettings(personalPlaylistId, "youTubePersonalPlaylistId", _appConfig);

                _irc.SendPublicChatMessage($"Your personal playlist has been set https://www.youtube.com/playlist?list={personalPlaylistId} @{chatter.DisplayName}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "SongRequestFeature", "SetPersonalYoutubePlaylistById(TwitchChatter)", false, "!setpersonalplaylistid");
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Set DJ mode for YouTube song requests
        /// </summary>
        /// <param name="chatter"></param>
        /// <returns></returns>
        private async Task<DateTime> SetDjMode(TwitchChatter chatter)
        {
            try
            {
                string message = ParseChatterCommandParameter(chatter);
                bool hasDjModeEnabled = SetBooleanFromMessage(message);

                // ToDo: Make HTTP PATCH request instead of full PUT
                await _songRequestSetting.UpdateSongRequestSetting(
                    _botConfig.YouTubeBroadcasterPlaylistId, 
                    _botConfig.YouTubePersonalPlaylistId,
                    _broadcasterInstance.DatabaseId, 
                    hasDjModeEnabled);

                _irc.SendPublicChatMessage($"DJing has been set to {hasDjModeEnabled} for YouTube song requests. "
                    + $"Please wait a few seconds before this change is applied @{chatter.DisplayName}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "SongRequestFeature", "SetDjMode(TwitchChatter)", false, "!djmode");
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Set manual song request mode
        /// </summary>
        /// <param name="chatter"></param>
        /// <returns></returns>
        private async Task<DateTime> SetManualSongRequestMode(TwitchChatter chatter)
        {
            try
            {
                string message = ParseChatterCommandParameter(chatter);
                bool shuffle = SetBooleanFromMessage(message);
                string boolValue = shuffle ? "true" : "false";

                _botConfig.IsManualSongRequestAvail = shuffle;
                SaveAppConfigSettings(boolValue, "isManualSongRequestAvail", _appConfig);

                _irc.SendPublicChatMessage($"{chatter.DisplayName}: Song requests set to {boolValue}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "SongRequestFeature", "SetManualSrMode(TwitchChatter)", false, "!msrmode");
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Set YouTube song request mode
        /// </summary>
        /// <param name="chatter"></param>
        /// <returns></returns>
        private async Task<DateTime> SetYouTubeSongRequestMode(TwitchChatter chatter)
        {
            try
            {
                string message = ParseChatterCommandParameter(chatter);
                bool shuffle = SetBooleanFromMessage(message);
                string boolValue = shuffle ? "true" : "false";

                _botConfig.IsYouTubeSongRequestAvail = shuffle;
                SaveAppConfigSettings(boolValue, "isYouTubeSongRequestAvail", _appConfig);

                _irc.SendPublicChatMessage($"{chatter.DisplayName}: YouTube song requests set to {boolValue}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "SongRequestFeature", "SetYouTubeSrMode(TwitchChatter)", false, "!ytsrmode");
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Enables displaying songs from Spotify into the IRC chat
        /// </summary>
        /// <param name="chatter"></param>
        /// <returns></returns>
        private async Task<DateTime> SetAutoDisplaySongs(TwitchChatter chatter)
        {
            try
            {
                string message = ParseChatterCommandParameter(chatter);
                bool shuffle = SetBooleanFromMessage(message);
                string boolValue = shuffle ? "true" : "false";

                _botConfig.EnableDisplaySong = shuffle;
                SaveAppConfigSettings(boolValue, "enableDisplaySong", _appConfig);

                _irc.SendPublicChatMessage($"{chatter.DisplayName}: Automatic display Spotify songs is set to \"{boolValue}\"");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "SongRequestFeature", "SetAutoDisplaySongs(TwitchChatter)", false, "!displaysongs");
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Resets the song request queue
        /// </summary>
        private async Task<DateTime> ResetManualSongRequest()
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

            return DateTime.Now;
        }

        /// <summary>
        /// Uses the Google API to add YouTube videos to the broadcaster's specified request playlist
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        /// <param name="hasYouTubeAuth">Checks if broadcaster allowed this bot to post videos to the playlist</param>
        /// <param name="isYouTubeSongRequestAvail">Checks if users can request songs</param>
        /// <returns></returns>
        private async Task<DateTime> YouTubeSongRequest(TwitchChatter chatter)
        {
            try
            {
                if (!_youTubeClientInstance.HasCredentials)
                {
                    _irc.SendPublicChatMessage("YouTube song requests have not been set up");
                    return DateTime.Now;
                }

                if (!_botConfig.IsYouTubeSongRequestAvail)
                {
                    _irc.SendPublicChatMessage("YouTube song requests are not turned on");
                    return DateTime.Now;
                }

                if (await _libVLCSharpPlayer.HasUserRequestedTooMany(chatter.DisplayName, 3))
                {
                    _irc.SendPublicChatMessage("You already have at least 3 song requests!"
                        + $" Please wait until there are less than 3 of your song requests in the queue before requesting more @{chatter.DisplayName}");
                    return DateTime.Now;
                }

                int cost = 250; // ToDo: Set YTSR currency cost into settings
                int funds = 0; // Set to minimum amount needed for song requests.
                               // This will allow chatters with VIP, moderator, or the broadcaster 
                               // to be exempt from paying the virtual currency

                // Force chatters that don't have a VIP, moderator, or broadcaster badge to use their virtul currency
                if (IsPrivilegedChatter(chatter))
                {
                    cost = 0;
                }
                else // Make the song request free for the mentioned chatter types above
                {
                    funds = await _bank.CheckBalance(chatter.Username, _broadcasterInstance.DatabaseId);
                }

                if (funds < cost)
                {
                    _irc.SendPublicChatMessage($"You do not have enough {_botConfig.CurrencyType} to make a song request. "
                        + $"You currently have {funds} {_botConfig.CurrencyType} @{chatter.DisplayName}");
                }
                else
                {
                    int spaceIndex = chatter.Message.IndexOf(" ");
                    string videoId = ParseYoutubeVideoId(chatter, spaceIndex);

                    Video video = null;

                    // Try to get video info using the parsed video ID
                    if (!string.IsNullOrEmpty(videoId))
                    {
                        video = await _youTubeClientInstance.GetVideoById(videoId);
                    }

                    // Default to search by keyword if parsed video ID was null or not available
                    if (video == null || video.Id == null)
                    {
                        string videoKeyword = chatter.Message.Substring(spaceIndex + 1);
                        videoId = await _youTubeClientInstance.SearchVideoByKeyword(videoKeyword);

                        if (string.IsNullOrEmpty(videoId))
                        {
                            _irc.SendPublicChatMessage($"Couldn't find video ID for song request @{chatter.DisplayName}");
                            return DateTime.Now;
                        }

                        video = await _youTubeClientInstance.GetVideoById(videoId);

                        if (video == null || video.Id == null)
                        {
                            _irc.SendPublicChatMessage($"Video wasn't available for song request @{chatter.DisplayName}");
                            return DateTime.Now;
                        }
                    }

                    // Confirm if video ID has been found and is a new song request
                    if (await _youTubeClientInstance.HasDuplicatePlaylistItem(_botConfig.YouTubeBroadcasterPlaylistId, videoId))
                    {
                        _irc.SendPublicChatMessage($"Song has already been requested @{chatter.DisplayName}");
                        return DateTime.Now;
                    }

                    // Check if video's title and account match song request blacklist
                    List<SongRequestIgnore> blacklist = await _songRequestBlacklist.GetSongRequestIgnore(_broadcasterInstance.DatabaseId);

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
                            _irc.SendPublicChatMessage($"I'm not allowing this artist/video to be queued on my master's behalf @{chatter.DisplayName}");
                            return DateTime.Now;
                        }
                        // Check for song-specific blacklist
                        else if (blacklist.Any(
                                b => (!string.IsNullOrEmpty(b.Title) && video.Snippet.Title.Contains(b.Artist, StringComparison.CurrentCultureIgnoreCase)
                                        && video.Snippet.Title.Contains(b.Title, StringComparison.CurrentCultureIgnoreCase)) // both song/artist in video title
                                    || (!string.IsNullOrEmpty(b.Title) && video.Snippet.ChannelTitle.Contains(b.Artist, StringComparison.CurrentCultureIgnoreCase)
                                        && video.Snippet.Title.Contains(b.Title, StringComparison.CurrentCultureIgnoreCase)) // song in title and artist in channel title
                            ))
                        {
                            _irc.SendPublicChatMessage($"I'm not allowing this song to be queued on my master's behalf @{chatter.DisplayName}");
                            return DateTime.Now;
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
                        _irc.SendPublicChatMessage($"Either couldn't find the video duration or it was way too long for the stream @{chatter.DisplayName}");
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

                        // Make sure song requests are no longer than a set amount
                        if (Convert.ToInt32(videoMin) >= videoMinLimit && Convert.ToInt32(videoSec) >= videoSecLimit)
                        {
                            _irc.SendPublicChatMessage("Song request is longer than or equal to "
                                + $"{videoMinLimit} minute(s) and {videoSecLimit} second(s) @{chatter.DisplayName}");

                            return DateTime.Now;
                        }

                        // Make sure song requests are no shorter than a set amount
                        if (Convert.ToInt32(videoMin) < 1 || (Convert.ToInt32(videoMin) == 1 && Convert.ToInt32(videoSec) < 30))
                        {
                            _irc.SendPublicChatMessage($"Song request is shorter than 1 minute and 30 seconds @{chatter.DisplayName}");

                            return DateTime.Now;
                        }

                        PlaylistItem playlistItem = await _youTubeClientInstance.AddVideoToPlaylist(videoId, _botConfig.YouTubeBroadcasterPlaylistId, chatter.DisplayName);

                        if (cost > 0)
                        {
                            await _bank.UpdateFunds(chatter.Username, _broadcasterInstance.DatabaseId, funds - cost);
                        }

                        int position = await _libVLCSharpPlayer.AddSongRequest(playlistItem);

                        string response = $"@{chatter.DisplayName} spent {cost} {_botConfig.CurrencyType} "
                            + $"and \"{video.Snippet.Title}\" by {video.Snippet.ChannelTitle} ({videoMin}M{videoSec}S) "
                            + $"was successfully added to the queue";

                        if (position == 1)
                            response += " and will be playing next!";
                        else if (position > 1)
                            response += $" at position #{position}!";

                        response += " https://youtu.be/" + video.Id;

                        _irc.SendPublicChatMessage(response);

                        // Return cooldown time by using one-third of the length of the video duration
                        TimeSpan totalTimeSpan = new TimeSpan(0, Convert.ToInt32(videoMin), Convert.ToInt32(videoSec));
                        TimeSpan cooldownTimeSpan = new TimeSpan(totalTimeSpan.Ticks / 3);

                        // Reduce the cooldown for privileged chatters
                        if (IsPrivilegedChatter(chatter))
                            cooldownTimeSpan = new TimeSpan(totalTimeSpan.Ticks / 4);

                        return DateTime.Now.AddSeconds(cooldownTimeSpan.TotalSeconds);
                    }
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "Gen", "YouTubeSongRequest(TwitchChatter, bool, bool)", false, "!ytsr");
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Display's link to broadcaster's YouTube song request playlist
        /// </summary>
        private async Task<DateTime> YouTubeSongRequestList()
        {
            try
            {
                if (_youTubeClientInstance.HasCredentials && !string.IsNullOrEmpty(_botConfig.YouTubeBroadcasterPlaylistId))
                {
                    _irc.SendPublicChatMessage($"{_botConfig.Broadcaster}'s song request list is at " +
                        "https://www.youtube.com/playlist?list=" + _botConfig.YouTubeBroadcasterPlaylistId);
                }
                else
                {
                    _irc.SendPublicChatMessage("There is no song request list at this time");
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "Gen", "YouTubeSongRequestList(bool)", false, "!ytsl");
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Display list of requested songs
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        private async Task<DateTime> ManuallyRequestedSongRequestList(TwitchChatter chatter)
        {
            try
            {
                if (!_botConfig.IsManualSongRequestAvail)
                    _irc.SendPublicChatMessage($"Song requests are not available at this time @{chatter.DisplayName}");
                else
                    _irc.SendPublicChatMessage(await _manualSongRequest.ListSongRequests(_broadcasterInstance.DatabaseId));
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "Gen", "ManualSrList(TwitchChatter)", false, "!rsl");
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Request a song for the host to play
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        private async Task<DateTime> ManuallyRequestedSongRequest(TwitchChatter chatter)
        {
            try
            {
                // Check if song request system is enabled
                if (_botConfig.IsManualSongRequestAvail)
                {
                    // Grab the song name from the request
                    int index = chatter.Message.IndexOf(" ");
                    string songRequest = chatter.Message.Substring(index + 1);

                    // Check if song request has more than allowed symbols
                    if (!Regex.IsMatch(songRequest, @"^[a-zA-Z0-9 \-\(\)\'\?\,\/\""]+$"))
                    {
                        _irc.SendPublicChatMessage("Only letters, numbers, commas, hyphens, parentheses, "
                            + "apostrophes, forward-slash, and question marks are allowed. Please try again. "
                            + "If the problem persists, please contact my creator");
                    }
                    else
                    {
                        await _manualSongRequest.AddSongRequest(songRequest, chatter.DisplayName, _broadcasterInstance.DatabaseId);

                        _irc.SendPublicChatMessage($"The song \"{songRequest}\" has been successfully requested @{chatter.DisplayName}");
                    }
                }
                else
                    _irc.SendPublicChatMessage($"Song requests are disabled at the moment @{chatter.DisplayName}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "Gen", "ManualSr(bool, TwitchChatter)", false, "!rsr", chatter.Message);
            }

            return DateTime.Now;
        }

        private async Task<DateTime> YouTubeCurrentSong(TwitchChatter chatter)
        {
            try
            {
                if (_libVLCSharpPlayer.MediaPlayerStatus() != VLCState.Playing)
                {
                    // fall back to see if Spotify is playing anything
                    _irc.SendPublicChatMessage(await SharedCommands.SpotifyCurrentSong(chatter, _spotify));
                    return DateTime.Now;
                }

                PlaylistItem playlistItem = _libVLCSharpPlayer.CurrentSongRequestPlaylistItem;

                string songRequest = _youTubeClientInstance.ShowPlayingSongRequest(playlistItem);

                if (!string.IsNullOrEmpty(songRequest))
                {
                    _irc.SendPublicChatMessage($"@{chatter.DisplayName} <-- Now playing: {songRequest} Currently {await _libVLCSharpPlayer.GetVideoTime()}");
                }
                else
                {
                    _irc.SendPublicChatMessage($"Unable to display the current song @{chatter.DisplayName}");
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "Gen", "YouTubeCurrentSong(TwitchChatter)", false, "!song");
            }

            return DateTime.Now;
        }

        private async Task<DateTime> YoutubeRemoveWrongSong(TwitchChatter chatter)
        {
            try
            {
                PlaylistItem removedWrongSong = await _libVLCSharpPlayer.RemoveWrongSong(chatter.DisplayName);

                if (removedWrongSong == null)
                {
                    _irc.SendPublicChatMessage($"It doesn't appear that you've requested a song for me to remove @{chatter.DisplayName}");
                    return DateTime.Now;
                }

                await _youTubeClientInstance.DeleteVideoFromPlaylist(removedWrongSong.Id);
                _irc.SendPublicChatMessage($"Successfully removed the wrong song request \"{removedWrongSong.Snippet.Title}\" @{chatter.DisplayName}");

                return DateTime.Now.AddMinutes(10);
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "Gen", "YoutubeRemoveWrongSong(TwitchChatter)", false, "!wrongsong");
            }

            return DateTime.Now;
        }

        private async Task<DateTime> YouTubeLastSong(TwitchChatter chatter)
        {
            try
            {
                if (_libVLCSharpPlayer.MediaPlayerStatus() != VLCState.Playing)
                {
                    // fall back to see if Spotify had something playing recently
                    _irc.SendPublicChatMessage(await SharedCommands.SpotifyLastPlayedSong(chatter, _spotify));
                    return DateTime.Now;
                }

                if (_libVLCSharpPlayer.LibVlc == null)
                {
                    _irc.SendPublicChatMessage($"Unable to display the last played song @{chatter.DisplayName}");
                    return DateTime.Now;
                }

                PlaylistItem playlistItem = _libVLCSharpPlayer.LastPlayedPlaylistItem;

                string songRequest = _youTubeClientInstance.ShowPlayingSongRequest(playlistItem);

                if (!string.IsNullOrEmpty(songRequest))
                {
                    _irc.SendPublicChatMessage($"@{chatter.DisplayName} <-- Last played: {songRequest}");
                }
                else
                {
                    _irc.SendPublicChatMessage($"Nothing was played before the current song from what I can see @{chatter.DisplayName}");
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "Gen", "YouTubeLastSong(TwitchChatter)", false, "!lastsong");
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Removes the first song in the queue of song requests
        /// </summary>
        private async Task<DateTime> PopManuallyRequestedSongRequest()
        {
            try
            {
                SongRequest removedSong = await _manualSongRequest.PopSongRequest(_broadcasterInstance.DatabaseId);

                if (removedSong != null)
                    _irc.SendPublicChatMessage($"The first song in the queue, \"{removedSong.Name}\" ({removedSong.Username}), has been removed");
                else
                    _irc.SendPublicChatMessage("There are no songs that can be removed from the song request list");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "Vip", "PopManualSr()", false, "!poprsr");
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Check if chatter is a VIP, moderator, or the broadcaster
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        /// <returns></returns>
        private bool IsPrivilegedChatter(TwitchChatter chatter)
        {
            return chatter.Badges.Contains("vip") || chatter.Badges.Contains("moderator") || chatter.Badges.Contains("broadcaster");
        }

        private string ParseYoutubeVideoId(TwitchChatter chatter, int spaceIndex)
        {
            // Parse video ID based on different types of requests
            if (chatter.Message.Contains("?v=") || chatter.Message.Contains("&v=") || chatter.Message.Contains("youtu.be/")) // full or short URL
            {
                return _youTubeClientInstance.ParseYouTubeVideoId(chatter.Message);
            }
            else if (chatter.Message.Substring(spaceIndex + 1).Length == 11
                && chatter.Message.Substring(spaceIndex + 1).IndexOf(" ") == -1
                && Regex.Match(chatter.Message, @"[\w\-]").Success) // assume only video ID
            {
                return chatter.Message.Substring(spaceIndex + 1);
            }

            return "";
        }
    }
}
