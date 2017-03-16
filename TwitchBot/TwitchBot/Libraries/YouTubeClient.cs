using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

using TwitchBot.Configuration;
using TwitchBot.Extensions;
using TwitchBot.Models;

namespace TwitchBot.Libraries
{
    public sealed class YoutubeClient
    {
        private static volatile YoutubeClient _instance;
        private static object _syncRoot = new Object();

        private YouTubeService _youtubeService;

        private ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        public YouTubeService YoutubeService
        {
            get { return _youtubeService; }
        }

        public YoutubeClient() { }

        public static YoutubeClient Instance
        {
            get
            {
                // first check
                if (_instance == null)
                {
                    lock (_syncRoot)
                    {
                        // second check
                        if (_instance == null)
                            _instance = new YoutubeClient();
                    }
                }

                return _instance;
            }
        }

        /// <summary>
        /// Get access and refresh tokens from user's account
        /// </summary>
        public async Task RetrieveTokens(TwitchBotConfigurationSection botConfig, System.Configuration.Configuration appConfig)
        {
            try
            {
                // ToDo: Move YouTube client secrets away from bot config
                string clientSecrets = @"{ 'installed': {'client_id': '" + botConfig.YouTubeClientId + "', 'client_secret': '" + botConfig.YouTubeClientSecret + "'} }";

                UserCredential credential;
                using (Stream stream = clientSecrets.ToStream())
                {
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        new[] { YouTubeService.Scope.Youtube },
                        "user",
                        CancellationToken.None,
                        new FileDataStore("Twitch Bot")
                    );
                }

                _youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Twitch Bot"
                });
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "YouTubeClient", "RetrieveTokens()", false);
            }
        }

        public async Task CreatePlaylist(string title, string desc, string privacyStatus = "public")
        {
            var newPlaylist = new Playlist();
            newPlaylist.Snippet = new PlaylistSnippet();
            newPlaylist.Snippet.Title = title;
            newPlaylist.Snippet.Description = desc;
            newPlaylist.Status = new PlaylistStatus();
            newPlaylist.Status.PrivacyStatus = privacyStatus;
            newPlaylist = await _youtubeService.Playlists.Insert(newPlaylist, "snippet,status").ExecuteAsync();

            Console.WriteLine($"New YouTube playlist '{title}' has been created with playlist ID: '{newPlaylist.Id}'");
        }

        public async Task AddVideoToPlaylist(string videoId, string playlistId, long position = -1)
        {
            var newPlaylistItem = new PlaylistItem();
            newPlaylistItem.Snippet = new PlaylistItemSnippet();
            newPlaylistItem.Snippet.PlaylistId = playlistId;
            newPlaylistItem.Snippet.ResourceId = new ResourceId();
            newPlaylistItem.Snippet.ResourceId.Kind = "youtube#video";
            newPlaylistItem.Snippet.ResourceId.VideoId = videoId;
            if (position > -1)
                newPlaylistItem.Snippet.Position = position;
            newPlaylistItem = await _youtubeService.PlaylistItems.Insert(newPlaylistItem, "snippet").ExecuteAsync();

            Console.WriteLine($"YouTube video has been added to playlist");
        }

        public async Task<YouTubeVideoSearchResult> SearchVideoByKeyword(string searchQuery)
        {
            var searchListRequest = _youtubeService.Search.List("snippet");
            searchListRequest.Q = searchQuery;
            searchListRequest.MaxResults = 10;

            var searchListResponse = await searchListRequest.ExecuteAsync();

            foreach (var searchResult in searchListResponse.Items)
            {
                if (searchResult.Id.Kind.Equals("youtube#video"))
                {
                    return new YouTubeVideoSearchResult
                    {
                        Id = searchResult.Id.VideoId,
                        Title = searchResult.Snippet.Title
                    };
                }
            }

            return new YouTubeVideoSearchResult(); // couldn't find requested video
        }

        public async Task<Video> SearchVideoById(string videoId)
        {
            var videoListRequest = _youtubeService.Videos.List("snippet,id");
            videoListRequest.Id = videoId;

            var videoListResponse = await videoListRequest.ExecuteAsync();

            foreach (var video in videoListResponse.Items)
            {
                if (video.Id.Equals(videoId))
                {
                    return video;
                }
            }

            return new Video(); // couldn't find requested video
        }

        public async Task<Playlist> GetBroadcasterPlaylistByKeyword(string playlistTitle)
        {
            var userPlaylistRequest = _youtubeService.Playlists.List("snippet");
            userPlaylistRequest.Mine = true;

            var userPlaylistListResponse = await userPlaylistRequest.ExecuteAsync();

            foreach (var playlist in userPlaylistListResponse.Items)
            {
                if (playlist.Snippet.Title.Equals(playlistTitle))
                {
                    return playlist;
                }
            }

            return new Playlist(); // couldn't find requested playlist
        }

        public async Task<Playlist> GetBroadcasterPlaylistById(string playlistId)
        {
            var userPlaylistRequest = _youtubeService.Playlists.List("snippet,id");
            userPlaylistRequest.Mine = true;

            var userPlaylistListResponse = await userPlaylistRequest.ExecuteAsync();

            foreach (var playlist in userPlaylistListResponse.Items)
            {
                if (playlist.Id.Equals(playlistId))
                {
                    return playlist;
                }
            }

            return new Playlist(); // couldn't find requested playlist
        }
    }
}
