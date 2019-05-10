using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;


namespace TwitchBotUtil.Libraries
{
    public abstract class YoutubeClientApi
    {
        protected abstract YouTubeService YouTubeService { get; set; }

        public abstract Task<bool> GetAuthAsync(string youTubeClientId, string youTubeClientSecret);

        public virtual async Task<Playlist> CreatePlaylist(string title, string desc, string privacyStatus = "public")
        {
            var newPlaylist = new Playlist();
            newPlaylist.Snippet = new PlaylistSnippet();
            newPlaylist.Snippet.Title = title;
            newPlaylist.Snippet.Description = desc;
            newPlaylist.Status = new PlaylistStatus();
            newPlaylist.Status.PrivacyStatus = privacyStatus;
            return await YouTubeService.Playlists.Insert(newPlaylist, GetPartParam(4)).ExecuteAsync();
        }

        public virtual async Task DeletePlaylist(string playlistId)
        {
            await YouTubeService.Playlists.Delete(playlistId).ExecuteAsync();
        }

        public virtual async Task AddVideoToPlaylist(string videoId, string playlistId, string username, long position = -1)
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

            newPlaylistItem = await YouTubeService.PlaylistItems.Insert(newPlaylistItem, GetPartParam(2)).ExecuteAsync();

            Console.WriteLine($"YouTube video has been added to playlist");
        }

        /// <summary>
        /// Get video ID by requested keywords
        /// </summary>
        /// <param name="searchQuery">Requested keywords for video search</param>
        /// <returns></returns>
        public virtual async Task<string> SearchVideoByKeyword(string searchQuery)
        {
            var searchListRequest = YouTubeService.Search.List(GetPartParam(3));
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
        /// <returns></returns>
        public virtual async Task<Video> GetVideoById(string videoId)
        {
            var videoListRequest = YouTubeService.Videos.List(GetPartParam(2));
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
        public virtual async Task<Playlist> GetBroadcasterPlaylistByKeyword(string playlistTitle)
        {
            string nextPageToken = "";

            while (nextPageToken != null)
            {
                var userPlaylistRequest = YouTubeService.Playlists.List(GetPartParam(0));
                userPlaylistRequest.Mine = true;

                var userPlaylistResponse = await userPlaylistRequest.ExecuteAsync();

                foreach (var playlist in userPlaylistResponse.Items)
                {
                    if (playlist.Snippet.Title.Equals(playlistTitle))
                    {
                        return playlist;
                    }
                }

                nextPageToken = userPlaylistResponse.NextPageToken;
            }

            return new Playlist(); // couldn't find requested playlist
        }

        /// <summary>
        /// Get broadcaster's specified playlist by ID
        /// </summary>
        /// <param name="playlistId">Playlist ID</param>
        /// <returns></returns>
        public virtual async Task<Playlist> GetBroadcasterPlaylistById(string playlistId)
        {
            string nextPageToken = "";

            while (nextPageToken != null)
            {
                var userPlaylistRequest = YouTubeService.Playlists.List(GetPartParam(0));
                userPlaylistRequest.Mine = true;
                userPlaylistRequest.MaxResults = 50;
                userPlaylistRequest.PageToken = nextPageToken;

                var userPlaylistResponse = await userPlaylistRequest.ExecuteAsync();

                foreach (var playlist in userPlaylistResponse.Items)
                {
                    if (playlist.Id.Equals(playlistId))
                    {
                        return playlist;
                    }
                }

                nextPageToken = userPlaylistResponse.NextPageToken;
            }

            return new Playlist(); // couldn't find requested playlist
        }

        /// <summary>
        /// Get broadcaster's specified playlist by ID
        /// </summary>
        /// <param name="playlistId">Playlist ID</param>
        /// <returns></returns>
        public virtual async Task<Playlist> GetPlaylistById(string playlistId)
        {
            var userPlaylistRequest = YouTubeService.Playlists.List(GetPartParam(0));
            userPlaylistRequest.Id = playlistId;

            var userPlaylistResponse = await userPlaylistRequest.ExecuteAsync();

            foreach (var playlist in userPlaylistResponse.Items)
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
        /// <returns></returns>
        public virtual async Task<bool> HasDuplicatePlaylistItem(string playlistId, string playlistItemVideoId)
        {
            string nextPageToken = "";
            while (nextPageToken != null)
            {
                var userPlaylistItemsListRequest = YouTubeService.PlaylistItems.List(GetPartParam(0));
                userPlaylistItemsListRequest.PlaylistId = playlistId;
                userPlaylistItemsListRequest.MaxResults = 50;
                userPlaylistItemsListRequest.PageToken = nextPageToken;

                var userPlaylistItemListResponse = await userPlaylistItemsListRequest.ExecuteAsync();

                foreach (var playlistItem in userPlaylistItemListResponse.Items)
                {
                    if (playlistItem.Snippet.ResourceId.VideoId.Equals(playlistItemVideoId))
                    {
                        return true;
                    }
                }

                nextPageToken = userPlaylistItemListResponse.NextPageToken;
            }

            return false;
        }

        /// <summary>
        /// Check if requested video is being requested again via video ID
        /// </summary>
        /// <param name="playlistId">Playlist ID</param>
        /// <returns></returns>
        public virtual async Task<List<string>> GetPlaylistVideoIds(string playlistId)
        {
            List<string> playlistVideoIds = new List<string>();

            string nextPageToken = "";
            while (nextPageToken != null)
            {
                var userPlaylistItemsListRequest = YouTubeService.PlaylistItems.List(GetPartParam(0));
                userPlaylistItemsListRequest.PlaylistId = playlistId;
                userPlaylistItemsListRequest.MaxResults = 50;
                userPlaylistItemsListRequest.PageToken = nextPageToken;

                var userPlaylistItemListResponse = await userPlaylistItemsListRequest.ExecuteAsync();

                foreach (var playlistItem in userPlaylistItemListResponse.Items)
                {
                    playlistVideoIds.Add(playlistItem.ContentDetails.VideoId);
                }

                nextPageToken = userPlaylistItemListResponse.NextPageToken;
            }

            return playlistVideoIds;
        }

        /// <summary>
        /// Check if requested video is being requested again via video ID
        /// </summary>
        /// <param name="playlistId">Playlist ID</param>
        /// <returns></returns>
        public virtual string GetFirstPlaylistVideoId(string playlistId)
        {
            var userPlaylistItemsListRequest = YouTubeService.PlaylistItems.List(GetPartParam(0));
            userPlaylistItemsListRequest.PlaylistId = playlistId;
            userPlaylistItemsListRequest.MaxResults = 1;

            var userPlaylistItemListResponse = userPlaylistItemsListRequest.Execute();

            if (userPlaylistItemListResponse?.Items.Count > 0)
                return userPlaylistItemListResponse?.Items[0]?.Snippet.ResourceId.VideoId ?? "";
            else
                return "";
        }

        /// <summary>
        /// Check if requested video is being requested again via video ID
        /// </summary>
        /// <param name="playlistId">Playlist ID</param>
        /// <param name="lastPlaylistItemVideoId">Requested video's ID</param>
        /// <returns></returns>
        public virtual string GetNextPlaylistVideoId(string playlistId, string lastPlaylistItemVideoId)
        {
            string nextPageToken = "";
            long? nextVideoPosition = null;

            while (nextPageToken != null)
            {
                var userPlaylistItemsListRequest = YouTubeService.PlaylistItems.List(GetPartParam(0));
                userPlaylistItemsListRequest.PlaylistId = playlistId;
                userPlaylistItemsListRequest.MaxResults = 50;
                userPlaylistItemsListRequest.PageToken = nextPageToken;

                var userPlaylistItemListResponse = userPlaylistItemsListRequest.Execute();

                if (nextVideoPosition == null)
                {
                    foreach (var playlistItem in userPlaylistItemListResponse.Items)
                    {
                        if (playlistItem.Snippet.ResourceId.VideoId == lastPlaylistItemVideoId)
                        {
                            nextVideoPosition = playlistItem.Snippet.Position + 1;
                            break;
                        }
                    }
                }

                if (nextVideoPosition != null)
                {
                    foreach (var playlistItem in userPlaylistItemListResponse.Items)
                    {
                        if (playlistItem.Snippet.Position == nextVideoPosition)
                        {
                            return playlistItem.Snippet.ResourceId.VideoId;
                        }
                    }
                }

                nextPageToken = userPlaylistItemListResponse.NextPageToken;
            }

            return "";
        }

        /// <summary>
        /// Grab the YouTube video ID
        /// </summary>
        /// <param name="message">String containing the YouTube link</param>
        /// <returns></returns>
        public virtual string ParseYouTubeVideoId(string message)
        {
            int videoIdIndex = -1;

            if (message.Contains("?v=")) // full URL
            {
                videoIdIndex = message.IndexOf("?v=") + 3;
            }
            else if (message.Contains("&v=")) // full URL
            {
                videoIdIndex = message.IndexOf("&v=") + 3;
            }
            else if (message.Contains("youtu.be/")) // short URL
            {
                videoIdIndex = message.IndexOf("youtu.be/") + 9;
            }

            return videoIdIndex == -1 ? "" : message.Substring(videoIdIndex, 11);
        }

        /// <summary>
        /// Get information for YouTube Resource based on specified part parameter
        /// </summary>
        /// <param name="partType">Set part parameter type: 0 = "snippet", 1 = "contentDetails", 2 = "snippet,contentDetails", 3 = "id", 4 = "snippet,status"</param>
        /// <returns></returns>
        private string GetPartParam(int partType)
        {
            string part = "";

            switch(partType)
            {
                case 0:
                    part = "snippet";
                    break;
                case 1:
                    part = "contentDetails";
                    break;
                case 2:
                    part = "snippet,contentDetails";
                    break;
                case 3:
                    part = "id";
                    break;
                case 4:
                    part = "snippet,status";
                    break;
            }

            return part;
        }
    }
}
