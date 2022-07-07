using System;
using System.Threading.Tasks;

using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;

using TwitchBotShared.ClientLibraries.Singletons;
using TwitchBotShared.Config;

namespace TwitchBotShared.ClientLibraries
{
    // Reference: https://github.com/JohnnyCrazy/SpotifyAPI-NET/tree/master/SpotifyAPI.Docs/docs
    public class SpotifyWebClient
    {
        private SpotifyClient _spotify;
        private SpotifyClientConfig _spotifyConfig = SpotifyClientConfig.CreateDefault();
        private readonly EmbedIOAuthServer _server;
        private readonly TwitchBotConfigurationSection _botConfig;
        private readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        public SpotifyWebClient(TwitchBotConfigurationSection _botSection)
        {
            _botConfig = _botSection;
            _server = new EmbedIOAuthServer(new Uri(_botConfig.SpotifyRedirectUri), 5000);
        }

        #region Public Methods
        public async Task<DateTime> ConnectAsync()
        {
            try
            {
                if (!await HasInitialConfig())
                {
                    return DateTime.Now;
                }

                await _server.Start();

                _server.ImplictGrantReceived += OnImplicitGrantReceived;
                _server.ErrorReceived += OnErrorReceived;

                LoginRequest request = new LoginRequest(_server.BaseUri, _botConfig.SpotifyClientId, LoginRequest.ResponseType.Token)
                {
                    Scope = new string[]
                    {
                        Scopes.UserReadCurrentlyPlaying,
                        Scopes.UserReadPlaybackState,
                        Scopes.UserModifyPlaybackState
                    }
                };

                BrowserUtil.Open(request.ToUri());
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "SpotifyWebClient", "ConnectAsync()", false);
            }

            return DateTime.Now;
        }

        public async Task<DateTime> PlayAsync()
        {
            try
            {
                if (!await _spotify.Player.ResumePlayback())
                {
                    Console.WriteLine("WARN: Unable to resume playback");
                }
            }
            catch (APIUnauthorizedException ex)
            {
                Console.WriteLine($"Error Message: {ex.Message}");
                RetryAccess();
            }
            catch (APITooManyRequestsException ex)
            {
                Console.WriteLine($"Retry after {ex.RetryAfter.TotalSeconds} second(s)");
            }
            catch (APIException ex)
            {
                Console.WriteLine($"Error Message: {ex.Message}");
                Console.WriteLine($"Error Status Code: {ex.Response?.StatusCode}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "SpotifyWebClient", "PlayAsync()", false);
            }

            return DateTime.Now;
        }

        public async Task<DateTime> PauseAsync()
        {
            try
            {
                if (!await _spotify.Player.PausePlayback())
                {
                    Console.WriteLine("WARN: Unable to pause playback");
                }
            }
            catch (APIUnauthorizedException ex)
            {
                Console.WriteLine($"Error Message: {ex.Message}");
                RetryAccess();
            }
            catch (APITooManyRequestsException ex)
            {
                Console.WriteLine($"Retry after {ex.RetryAfter.TotalSeconds} second(s)");
            }
            catch (APIException ex)
            {
                Console.WriteLine($"Error Message: {ex.Message}");
                Console.WriteLine($"Error Status Code: {ex.Response?.StatusCode}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "SpotifyWebClient", "PauseAsync()", false);
            }

            return DateTime.Now;
        }

        public async Task<DateTime> SkipToPreviousPlaybackAsync()
        {
            try
            {
                if (!await _spotify.Player.SkipPrevious())
                {
                    Console.WriteLine("WARN: Unable to skip to previous playback");
                }
            }
            catch (APIUnauthorizedException ex)
            {
                Console.WriteLine($"Error Message: {ex.Message}");
                RetryAccess();
            }
            catch (APITooManyRequestsException ex)
            {
                Console.WriteLine($"Retry after {ex.RetryAfter.TotalSeconds} second(s)");
            }
            catch (APIException ex)
            {
                Console.WriteLine($"Error Message: {ex.Message}");
                Console.WriteLine($"Error Status Code: {ex.Response?.StatusCode}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "SpotifyWebClient", "SkipToPreviousPlaybackAsync()", false);
            }

            return DateTime.Now;
        }

        public async Task<DateTime> SkipToNextPlaybackAsync()
        {
            try
            {
                if (!await _spotify.Player.SkipNext())
                {
                    Console.WriteLine("WARN: Unable to skip to next playback");
                }
            }
            catch (APIUnauthorizedException ex)
            {
                Console.WriteLine($"Error Message: {ex.Message}");
                RetryAccess();
            }
            catch (APITooManyRequestsException ex)
            {
                Console.WriteLine($"Retry after {ex.RetryAfter.TotalSeconds} second(s)");
            }
            catch (APIException ex)
            {
                Console.WriteLine($"Error Message: {ex.Message}");
                Console.WriteLine($"Error Status Code: {ex.Response?.StatusCode}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "SpotifyWebClient", "SkipToNextPlaybackAsync()", false);
            }

            return DateTime.Now;
        }

        public async Task<CurrentlyPlayingContext> GetPlaybackAsync()
        {
            try
            {
                CurrentlyPlayingContext playbackContext = await _spotify.Player.GetCurrentPlayback();

                if (playbackContext != null)
                {
                    return playbackContext;
                }
            }
            catch (APIUnauthorizedException ex)
            {
                Console.WriteLine($"Error Message: {ex.Message}");
                RetryAccess();
            }
            catch (APITooManyRequestsException ex)
            {
                Console.WriteLine($"Retry after {ex.RetryAfter.TotalSeconds} second(s)");
            }
            catch (APIException ex)
            {
                Console.WriteLine($"Error Message: {ex.Message}");
                Console.WriteLine($"Error Status Code: {ex.Response?.StatusCode}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "SpotifyWebClient", "GetPlaybackAsync()", false);
            }

            return null;
        }

        public async Task<FullTrack> GetLastPlayedSongAsync()
        {
            try
            {
                CursorPaging<PlayHistoryItem> playbackHistory = await _spotify.Player.GetRecentlyPlayed();

                if (playbackHistory != null && playbackHistory.Items.Count > 0)
                {
                    return playbackHistory.Items[0].Track;
                }
            }
            catch (APIUnauthorizedException ex)
            {
                Console.WriteLine($"Error Message: {ex.Message}");
                RetryAccess();
            }
            catch (APITooManyRequestsException ex)
            {
                Console.WriteLine($"Retry after {ex.RetryAfter.TotalSeconds} second(s)");
            }
            catch (APIException ex)
            {
                Console.WriteLine($"Error Message: {ex.Message}");
                Console.WriteLine($"Error Status Code: {ex.Response?.StatusCode}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "SpotifyWebClient", "GetLastPlayedSongAsync()", false);
            }

            return null;
        }
        #endregion

        #region Private Methods
        private async Task OnImplicitGrantReceived(object sender, ImplictGrantResponse response)
        {
            try
            {
                await _server.Stop();
                _spotifyConfig = _spotifyConfig.WithToken(response.AccessToken);
                _spotify = new SpotifyClient(_spotifyConfig);
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "SpotifyWebClient", "OnImplicitGrantReceived(object, ImplictGrantResponse)", false);
            }
        }

        private async Task OnErrorReceived(object sender, string error, string state)
        {
            try
            {
                Console.WriteLine($"Aborting authorization, error received: {error}");
                await _server.Stop();
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "SpotifyWebClient", "OnErrorReceived(object, string, string)", false);
            }
        }

        private async void RetryAccess()
        {
            try
            {
                // Renew access token with retry
                _spotifyConfig = _spotifyConfig
                    .WithRetryHandler(new SimpleRetryHandler() { RetryAfter = TimeSpan.FromSeconds(1) });

                _spotify = new SpotifyClient(_spotifyConfig);
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "SpotifyWebClient", "RetryAccess()", false);
            }
        }

        private async Task<bool> HasInitialConfig()
        {
            try
            {
                if (string.IsNullOrEmpty(_botConfig.SpotifyClientId) || string.IsNullOrEmpty(_botConfig.SpotifyRedirectUri))
                {
                    Console.WriteLine("Warning: Spotify hasn't been set up for this bot.");
                    Console.WriteLine("Please insert a Spotify client Id and redirect URI in the bot config\n");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "SpotifyWebClient", "HasInitialConfig()", false);
            }

            return false;
        }
        #endregion
    }
}
