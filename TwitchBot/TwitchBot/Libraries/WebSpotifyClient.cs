using System;
using System.Threading.Tasks;

using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;

using TwitchBot.Configuration;

namespace TwitchBot.Libraries
{
    /* Example Code for Web Spotify API */
    // https://github.com/JohnnyCrazy/SpotifyAPI-NET/blob/master/SpotifyAPI.Web.Example/Program.cs
    public class WebSpotifyClient
    {
        private TwitchBotConfigurationSection _botConfig;
        private SpotifyWebAPI _spotify;
        private ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        public WebSpotifyClient(TwitchBotConfigurationSection _botSection)
        {
            _botConfig = _botSection;
        }

        public async Task Connect()
        {
            try
            {
                ImplictGrantAuth auth = new ImplictGrantAuth(
                    _botConfig.SpotifyClientId,
                    _botConfig.SpotifyRedirectUri, 
                    _botConfig.SpotifyServerUri,
                    Scope.PlaylistReadPrivate 
                        | Scope.PlaylistReadCollaborative 
                        | Scope.UserLibraryRead 
                        | Scope.UserReadCurrentlyPlaying
                        | Scope.UserReadPlaybackState
                        | Scope.UserModifyPlaybackState);

                auth.AuthReceived += OnAuthReceived;
                auth.Start();
                auth.OpenBrowser();
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "TwitchBotApplication", "Connect()", true);
            }
        }

        private void OnAuthReceived(object sender, Token token)
        {
            ImplictGrantAuth auth = (ImplictGrantAuth)sender;
            auth.Stop();

            _spotify = new SpotifyWebAPI
            {
                AccessToken = token.AccessToken,
                TokenType = token.TokenType
            };
        }

        public async Task Play()
        {
            await _spotify.ResumePlaybackAsync("", "", null, "");
        }

        public async Task Pause()
        {
            await _spotify.PausePlaybackAsync();
        }

        public async Task SkipToPreviousPlayback()
        {
            await _spotify.SkipPlaybackToPreviousAsync();
        }

        public async Task SkipToNextPlayback()
        {
            await _spotify.SkipPlaybackToNextAsync();
        }

        public async Task<PlaybackContext> GetPlayback()
        {
            return await _spotify.GetPlaybackAsync();
        }
    }
}
