using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

using TwitchBot.Extensions;

namespace TwitchBot.Libraries
{
    public sealed class YoutubeClient
    {
        private static volatile YoutubeClient _instance;
        private static object _syncRoot = new Object();

        private YouTubeService _youtubeService;

        private ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

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
        /// <param name="youTubeClientId"></param>
        /// <param name="youTubeClientSecret"></param>
        public async Task<bool> GetAuth(string youTubeClientId, string youTubeClientSecret)
        {
            try
            {
                string clientSecrets = @"{ 'installed': {'client_id': '" + youTubeClientId + "', 'client_secret': '" + youTubeClientSecret + "'} }";

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

                return true;
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("access_denied")) // record unknown error
                    await _errHndlrInstance.LogError(ex, "YouTubeClient", "GetAuth(string, string)", false);

                return false;
            }
        }

        public async Task<Playlist> CreatePlaylist(string title, string desc, string privacyStatus = "public")
        {
            var newPlaylist = new Playlist();
            newPlaylist.Snippet = new PlaylistSnippet();
            newPlaylist.Snippet.Title = title;
            newPlaylist.Snippet.Description = desc;
            newPlaylist.Status = new PlaylistStatus();
            newPlaylist.Status.PrivacyStatus = privacyStatus;
            return await _youtubeService.Playlists.Insert(newPlaylist, "snippet,status").ExecuteAsync();
        }

        public async Task DeletePlaylist(string playlistId)
        {
            await _youtubeService.Playlists.Delete(playlistId).ExecuteAsync();
        }

        public async Task AddVideoToPlaylist(string videoId, string playlistId, string username, long position = -1)
        {
            var newPlaylistItem = new PlaylistItem();
            newPlaylistItem.Snippet = new PlaylistItemSnippet();
            newPlaylistItem.Snippet.PlaylistId = playlistId;
            newPlaylistItem.Snippet.ResourceId = new ResourceId();
            newPlaylistItem.Snippet.ResourceId.Kind = "youtube#video";
            newPlaylistItem.Snippet.ResourceId.VideoId = videoId;
            newPlaylistItem.ContentDetails = new PlaylistItemContentDetails();
            newPlaylistItem.ContentDetails.Note = $"Requested by: {username}";
            if (position > -1)
                newPlaylistItem.Snippet.Position = position;
            newPlaylistItem = await _youtubeService.PlaylistItems.Insert(newPlaylistItem, "snippet,contentDetails").ExecuteAsync();

            Console.WriteLine($"YouTube video has been added to playlist");
        }

        /// <summary>
        /// Get video ID by requested keywords
        /// </summary>
        /// <param name="searchQuery">Requested keywords for video search</param>
        /// <param name="partType">Part parameter types: 0 = "snippet", 1 = "contentDetails", 2 = "snippet,contentDetails", 3 = "id"</param>
        /// <returns></returns>
        public async Task<string> SearchVideoByKeyword(string searchQuery, int partType = 0)
        {
            var searchListRequest = _youtubeService.Search.List(GetPartParam(partType));
            searchListRequest.Q = searchQuery;
            searchListRequest.Type = "video";
            searchListRequest.MaxResults = 1;

            var searchListResponse = await searchListRequest.ExecuteAsync();

            SearchResult searchResult = searchListResponse.Items.FirstOrDefault();
            if (searchResult == null)
                return ""; // couldn't find requested video
            
            return searchResult.Id.VideoId;
        }

        /// <summary>
        /// Get a requested video by ID
        /// </summary>
        /// <param name="videoId">Video ID</param>
        /// <param name="partType">Part parameter types: 0 = "snippet", 1 = "contentDetails", 2 = "snippet,contentDetails", 3 = "id"</param>
        /// <returns></returns>
        public async Task<Video> GetVideoById(string videoId, int partType = 0)
        {
            var videoListRequest = _youtubeService.Videos.List(GetPartParam(partType));
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

        /// <summary>
        /// Get broadcaster's specified playlist by keyword
        /// </summary>
        /// <param name="playlistTitle">Title of playlist</param>
        /// <param name="partType">Part parameter types: 0 = "snippet", 1 = "contentDetails", 2 = "snippet,contentDetails", 3 = "id"</param>
        /// <returns></returns>
        public async Task<Playlist> GetBroadcasterPlaylistByKeyword(string playlistTitle, int partType = 0)
        {
            var userPlaylistRequest = _youtubeService.Playlists.List(GetPartParam(partType));
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

        /// <summary>
        /// Get broadcaster's specified playlist by ID
        /// </summary>
        /// <param name="playlistId">Playlist ID</param>
        /// <param name="partType">Part parameter types: 0 = "snippet", 1 = "contentDetails", 2 = "snippet,contentDetails", 3 = "id"</param>
        /// <returns></returns>
        public async Task<Playlist> GetBroadcasterPlaylistById(string playlistId, int partType = 0)
        {
            var userPlaylistRequest = _youtubeService.Playlists.List(GetPartParam(partType));
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

        /// <summary>
        /// Check if requested video is being requested again via video ID
        /// </summary>
        /// <param name="playlistId">Playlist ID</param>
        /// <param name="playlistItemVideoId">Requested video's ID</param>
        /// <param name="partType">Part parameter types: 0 = "snippet", 1 = "contentDetails", 2 = "snippet,contentDetails", 3 = "id"</param>
        /// <returns></returns>
        public async Task<bool> HasDuplicatePlaylistItem(string playlistId, string playlistItemVideoId, int partType = 0)
        {
            string nextPageToken = "";
            while (nextPageToken != null)
            {
                var userPlaylistItemsListRequest = _youtubeService.PlaylistItems.List(GetPartParam(partType));
                userPlaylistItemsListRequest.PlaylistId = playlistId;
                userPlaylistItemsListRequest.MaxResults = 50;
                userPlaylistItemsListRequest.PageToken = nextPageToken;

                var userPlaylistItemsListResponse = await userPlaylistItemsListRequest.ExecuteAsync();

                foreach (var playlistItem in userPlaylistItemsListResponse.Items)
                {
                    if (playlistItem.Snippet.ResourceId.VideoId.Equals(playlistItemVideoId))
                    {
                        return true;
                    }
                }

                nextPageToken = userPlaylistItemsListResponse.NextPageToken;
            }

            return false;
        }

        /// <summary>
        /// Get information for YouTube Resource based on specified part parameter
        /// </summary>
        /// <param name="partType">Set part parameter type: 0 = "snippet", 1 = "contentDetails", 2 = "snippet,contentDetails", 3 = "id"</param>
        /// <returns></returns>
        private string GetPartParam(int partType)
        {
            string part = "";

            if (partType == 0)
                part = "snippet";
            else if (partType == 1)
                part = "contentDetails";
            else if (partType == 2)
                part = "snippet,contentDetails";
            else if (partType == 3)
                part = "id";

            return part;
        }
    }
}
