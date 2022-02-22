using System;
using System.Threading.Tasks;

using TwitchBotShared.ClientLibraries;
using TwitchBotShared.ClientLibraries.Singletons;
using TwitchBotShared.Config;
using TwitchBotShared.Enums;
using TwitchBotShared.Models;

namespace TwitchBotShared.Commands.Features
{
    /// <summary>
    /// The "Command Subsystem" for the "Spotify" feature
    /// </summary>
    public sealed class SpotifyFeature : BaseFeature
    {
        private readonly SpotifyWebClient _spotify;
        private readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        private const string SPOTIFY_CONNECT = "!spotifyconnect";
        private const string SPOTIFY_PLAY = "!spotifyplay";
        private const string SPOTIFY_PAUSE = "!spotifypause";
        private const string SPOTIFY_PREV = "!spotifyprev";
        private const string SPOTIFY_BACK = "!spotifyback";
        private const string SPOTIFY_NEXT = "!spotifynext";
        private const string SPOTIFY_SKIP = "!spotifyskip";
        private const string SPOTIFY_SONG = "!spotifysong";
        private const string SPOTIFY_LAST_SONG = "!spotifylastsong";

        public SpotifyFeature(IrcClient irc, TwitchBotConfigurationSection botConfig, SpotifyWebClient spotify) : base(irc, botConfig)
        {
            _spotify = spotify;
            _rolePermissions.Add(SPOTIFY_CONNECT, new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermissions.Add(SPOTIFY_PLAY, new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermissions.Add(SPOTIFY_PAUSE, new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermissions.Add(SPOTIFY_PREV, new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermissions.Add(SPOTIFY_BACK, new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermissions.Add(SPOTIFY_NEXT, new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermissions.Add(SPOTIFY_SKIP, new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermissions.Add(SPOTIFY_SONG, new CommandPermission { General = ChatterType.Viewer });
            _rolePermissions.Add(SPOTIFY_LAST_SONG, new CommandPermission { General = ChatterType.Viewer });
        }

        public override async Task<(bool, DateTime)> ExecCommandAsync(TwitchChatter chatter, string requestedCommand)
        {
            try
            {
                switch (requestedCommand)
                {
                    case SPOTIFY_CONNECT: // Manually connect to Spotify
                        return (true, await _spotify.ConnectAsync());
                    case SPOTIFY_PLAY: // Press local Spotify play button [>]
                        return (true, await _spotify.PlayAsync());
                    case SPOTIFY_PAUSE: // Press local Spotify pause button [||]
                        return (true, await _spotify.PauseAsync());
                    case SPOTIFY_PREV: // Press local Spotify previous button [|<]
                    case SPOTIFY_BACK:
                        return (true, await _spotify.SkipToPreviousPlaybackAsync());
                    case SPOTIFY_NEXT: // Press local Spotify next (skip) button [>|]
                    case SPOTIFY_SKIP:
                        return (true, await _spotify.SkipToNextPlaybackAsync());
                    case SPOTIFY_SONG:
                        return (true, await SpotifyCurrentSongAsync(chatter));
                    case SPOTIFY_LAST_SONG:
                        return (true, await SpotifyLastLongAsync(chatter));
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "SpotifyFeature", "ExecCommandAsync(TwitchChatter, string)", false, requestedCommand, chatter.Message);
            }

            return (false, DateTime.Now);
        }

        #region Private Methods
        /// <summary>
        /// Displays the current song being played from Spotify
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        private async Task<DateTime> SpotifyCurrentSongAsync(TwitchChatter chatter)
        {
            try
            {
                _irc.SendPublicChatMessage(await SharedCommands.SpotifyCurrentSongAsync(chatter, _spotify));
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "SpotifyFeature", "SpotifyCurrentSongAsync(TwitchChatter)", false, SPOTIFY_SONG);
            }

            return DateTime.Now;
        }

        private async Task<DateTime> SpotifyLastLongAsync(TwitchChatter chatter)
        {
            try
            {
                _irc.SendPublicChatMessage(await SharedCommands.SpotifyLastPlayedSongAsync(chatter, _spotify));
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "SpotifyFeature", "SpotifyLastLongAsync(TwitchChatter)", false, SPOTIFY_LAST_SONG);
            }

            return DateTime.Now;
        }
        #endregion
    }
}
