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
    public class SpotifyWebClient
    {
        private TwitchBotConfigurationSection _botConfig;
        private SpotifyWebAPI _spotify;
        private ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        public SpotifyWebClient(TwitchBotConfigurationSection _botSection)
        {
            _botConfig = _botSection;
        }

        public async Task Connect()
        {
            try
            {
                if (!HasInitialConfig())
                    return;

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

        public async Task Play()
        {
            if (HasInitialConfig() && !string.IsNullOrEmpty(_spotify?.AccessToken))
                await _spotify.ResumePlaybackAsync("", "", null, "");
        }

        public async Task Pause()
        {
            if (HasInitialConfig() && !string.IsNullOrEmpty(_spotify?.AccessToken))
                await _spotify.PausePlaybackAsync();
        }

        public async Task SkipToPreviousPlayback()
        {
            if (HasInitialConfig() && !string.IsNullOrEmpty(_spotify?.AccessToken))
                await _spotify.SkipPlaybackToPreviousAsync();
        }

        public async Task SkipToNextPlayback()
        {
            if (HasInitialConfig() && !string.IsNullOrEmpty(_spotify?.AccessToken))
                await _spotify.SkipPlaybackToNextAsync();
        }

        public async Task<PlaybackContext> GetPlayback()
        {
            if (HasInitialConfig() && !string.IsNullOrEmpty(_spotify?.AccessToken))
                return await _spotify.GetPlaybackAsync();

            return null;
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
