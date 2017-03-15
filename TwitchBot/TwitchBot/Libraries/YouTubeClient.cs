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

                botConfig.YouTubeAccessToken = credential.Token.AccessToken;
                botConfig.YouTubeRefreshToken = credential.Token.RefreshToken;
                appConfig.Save();
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
        }
    }
}
