using System;
using System.Threading.Tasks;

using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;

using TwitchBotShared.ClientLibraries.Singletons;
using TwitchBotShared.Config;

namespace TwitchBotShared.ClientLibraries
{
    /* Example Code for Web Spotify API */
    // https://github.com/JohnnyCrazy/SpotifyAPI-NET/blob/master/SpotifyAPI.Web.Examples.CLI/Program.cs
    public class SpotifyWebClient
    {
        private TwitchBotConfigurationSection _botConfig;
        private SpotifyWebAPI _spotify;
        private ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        public SpotifyWebClient(TwitchBotConfigurationSection _botSection)
        {
            _botConfig = _botSection;
        }

        public async Task<DateTime> ConnectAsync()
        {
            try
            {
                if (!HasInitialConfig())
                {
                    return DateTime.Now;
                }

                ImplicitGrantAuth auth = new ImplicitGrantAuth(
                    _botConfig.SpotifyClientId,
                    _botConfig.SpotifyRedirectUri, 
                    _botConfig.SpotifyServerUri,
                    Scope.UserReadCurrentlyPlaying
                        | Scope.UserReadPlaybackState
                        | Scope.UserModifyPlaybackState);

                auth.AuthReceived += OnAuthReceived;
                auth.Start();
                auth.OpenBrowser();
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "TwitchBotApplication", "Connect()", false);
            }

            return DateTime.Now;
        }

        private void OnAuthReceived(object sender, Token token)
        {
            ImplicitGrantAuth auth = (ImplicitGrantAuth)sender;
            auth.Stop();

            _spotify = new SpotifyWebAPI
            {
                AccessToken = token.AccessToken,
                TokenType = token.TokenType
            };
        }

        public async Task<DateTime> PlayAsync()
        {
            await CheckForConnectionAsync();

            if (!string.IsNullOrEmpty(_spotify?.AccessToken))
            {
                ErrorResponse errorResponse = await _spotify.ResumePlaybackAsync("", "", null, "");

                if (errorResponse?.Error?.Status == 401)
                {
                    await CheckForConnectionAsync();
                    await _spotify.ResumePlaybackAsync("", "", null, "");
                }
            }

            return DateTime.Now;
        }

        public async Task<DateTime> PauseAsync()
        {
            await CheckForConnectionAsync();

            if (!string.IsNullOrEmpty(_spotify?.AccessToken))
            {
                ErrorResponse errorResponse = await _spotify.PausePlaybackAsync();

                if (errorResponse?.Error?.Status == 401)
                {
                    await CheckForConnectionAsync();
                    await _spotify.PausePlaybackAsync();
                }
            }

            return DateTime.Now;
        }

        public async Task<DateTime> SkipToPreviousPlaybackAsync()
        {
            await CheckForConnectionAsync();

            if (!string.IsNullOrEmpty(_spotify?.AccessToken))
            {
                ErrorResponse errorResponse = await _spotify.SkipPlaybackToPreviousAsync();

                if (errorResponse?.Error?.Status == 401)
                {
                    await CheckForConnectionAsync();
                    await _spotify.SkipPlaybackToPreviousAsync();
                }
            }

            return DateTime.Now;
        }

        public async Task<DateTime> SkipToNextPlaybackAsync()
        {
            await CheckForConnectionAsync();

            if (!string.IsNullOrEmpty(_spotify?.AccessToken))
            {
                ErrorResponse errorResponse = await _spotify.SkipPlaybackToNextAsync();
                
                if (errorResponse?.Error?.Status == 401)
                {
                    await CheckForConnectionAsync();
                    await _spotify.SkipPlaybackToNextAsync();
                }
            }

            return DateTime.Now;
        }

        public async Task<PlaybackContext> GetPlaybackAsync()
        {
            await CheckForConnectionAsync();

            if (!string.IsNullOrEmpty(_spotify?.AccessToken))
            {
                PlaybackContext playbackContext = await _spotify.GetPlaybackAsync();

                if (playbackContext?.Error?.Status == 401)
                {
                    await CheckForConnectionAsync();
                    playbackContext = await _spotify.GetPlaybackAsync();
                }

                return playbackContext;
            }

            return null;
        }

        public async Task<SimpleTrack> GetLastPlayedSongAsync()
        {
            await CheckForConnectionAsync();

            if (!string.IsNullOrEmpty(_spotify?.AccessToken))
            {
                CursorPaging<PlayHistory> playbackHistory = await _spotify.GetUsersRecentlyPlayedTracksAsync(1);

                if (playbackHistory.Error == null)
                {
                    await CheckForConnectionAsync();
                    if (playbackHistory.Items.Count > 0)
                    {
                        return playbackHistory.Items[0].Track;
                    }
                }
            }

            return null;
        }

        private async Task CheckForConnectionAsync()
        {
            if (HasInitialConfig() && string.IsNullOrEmpty(_spotify?.AccessToken))
            {
                await ConnectAsync();
                await Task.Delay(1000);
            }
        }

        private bool HasInitialConfig()
        {
            if (string.IsNullOrEmpty(_botConfig.SpotifyClientId)
                || string.IsNullOrEmpty(_botConfig.SpotifyRedirectUri)
                || string.IsNullOrEmpty(_botConfig.SpotifyServerUri))
            {
                Console.WriteLine("Warning: Spotify hasn't been set up for this bot.");
                Console.WriteLine("Please insert a Spotify client Id, redirect URI, and server URI in bot config\n");
                return false;
            }

            return true;
        }
    }
}
