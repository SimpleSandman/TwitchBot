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

        public SpotifyFeature(IrcClient irc, TwitchBotConfigurationSection botConfig, SpotifyWebClient spotify) : base(irc, botConfig)
        {
            _spotify = spotify;
            _rolePermissions.Add("!spotifyconnect", new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermissions.Add("!spotifyplay", new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermissions.Add("!spotifypause", new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermissions.Add("!spotifyprev", new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermissions.Add("!spotifyback", new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermissions.Add("!spotifynext", new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermissions.Add("!spotifyskip", new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermissions.Add("!spotifysong", new CommandPermission { General = ChatterType.Viewer });
            _rolePermissions.Add("!spotifylastsong", new CommandPermission { General = ChatterType.Viewer });
        }

        public override async Task<(bool, DateTime)> ExecCommandAsync(TwitchChatter chatter, string requestedCommand)
        {
            try
            {
                switch (requestedCommand)
                {
                    case "!spotifyconnect": // Manually connect to Spotify
                        return (true, await _spotify.ConnectAsync());
                    case "!spotifyplay": // Press local Spotify play button [>]
                        return (true, await _spotify.PlayAsync());
                    case "!spotifypause": // Press local Spotify pause button [||]
                        return (true, await _spotify.PauseAsync());
                    case "!spotifyprev": // Press local Spotify previous button [|<]
                    case "!spotifyback":
                        return (true, await _spotify.SkipToPreviousPlaybackAsync());
                    case "!spotifynext": // Press local Spotify next (skip) button [>|]
                    case "!spotifyskip":
                        return (true, await _spotify.SkipToNextPlaybackAsync());
                    case "!spotifysong":
                        return (true, await SpotifyCurrentSongAsync(chatter));
                    case "!spotifylastsong":
                        return (true, await SpotifyLastLongAsync(chatter));
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "SpotifyFeature", "ExecCommand(TwitchChatter, string)", false, requestedCommand, chatter.Message);
            }

            return (false, DateTime.Now);
        }

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
                await _errHndlrInstance.LogError(ex, "SpotifyFeature", "SpotifyCurrentSong(TwitchChatter)", false, "!spotifysong");
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
                await _errHndlrInstance.LogError(ex, "SpotifyFeature", "SpotifyLastLong(TwitchChatter)", false, "!spotifylastsong");
            }

            return DateTime.Now;
        }
    }
}
