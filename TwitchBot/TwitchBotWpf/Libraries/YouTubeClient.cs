using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;

using TwitchBotDb.Models;

using TwitchBotUtil.Extensions;
using TwitchBotUtil.Libraries;

namespace TwitchBotWpf.Libraries
{
    public class YoutubeClient : YoutubeClientApi
    {
        private static volatile YoutubeClient _instance;
        private static object _syncRoot = new object();

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

        public static SongRequestSetting SongRequestSetting { get; set; }

        public static bool HasLookedForNextVideo { get; set; } = false;

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
            catch
            {
                return false;
            }
        }
    }
}
