using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;

using TwitchBotUtil.Extensions;
using TwitchBotUtil.Libraries;

namespace TwitchBot.Libraries
{
    public class YoutubeClient : YoutubeClientApi
    {
        private static volatile YoutubeClient _instance;
        private static object _syncRoot = new object();

        private ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        public bool HasCredentials { get; set; }

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

        protected override YouTubeService YouTubeService { get; set; }

        /// <summary>
        /// Get access and refresh tokens from user's account
        /// </summary>
        /// <param name="youTubeClientId"></param>
        /// <param name="youTubeClientSecret"></param>
        public override async Task<bool> GetAuthAsync(string youTubeClientId, string youTubeClientSecret)
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

                YouTubeService = new YouTubeService(new BaseClientService.Initializer()
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
    }
}
