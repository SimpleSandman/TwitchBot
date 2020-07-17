using System;
using System.Threading.Tasks;

using TwitchBotCore.Config;
using TwitchBotCore.Enums;
using TwitchBotCore.Libraries;
using TwitchBotCore.Models;

namespace TwitchBotCore.Commands.Features
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
            _rolePermission.Add("!spotifyconnect", new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermission.Add("!spotifyplay", new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermission.Add("!spotifypause", new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermission.Add("!spotifyprev", new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermission.Add("!spotifyback", new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermission.Add("!spotifynext", new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermission.Add("!spotifyskip", new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermission.Add("!spotifysong", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!spotifylastsong", new CommandPermission { General = ChatterType.Viewer });
        }

        public override async Task<(bool, DateTime)> ExecCommand(TwitchChatter chatter, string requestedCommand)
        {
            try
            {
                switch (requestedCommand)
                {
                    case "!spotifyconnect": // Manually connect to Spotify
                        return (true, await _spotify.Connect());
                    case "!spotifyplay": // Press local Spotify play button [>]
                        return (true, await _spotify.Play());
                    case "!spotifypause": // Press local Spotify pause button [||]
                        return (true, await _spotify.Pause());
                    case "!spotifyprev": // Press local Spotify previous button [|<]
                    case "!spotifyback":
                        return (true, await _spotify.SkipToPreviousPlayback());
                    case "!spotifynext": // Press local Spotify next (skip) button [>|]
                    case "!spotifyskip":
                        return (true, await _spotify.SkipToNextPlayback());
                    case "!spotifysong":
                        return (true, await SpotifyCurrentSong(chatter));
                    case "!spotifylastsong":
                        return (true, await SpotifyLastLong(chatter));
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
        public async Task<DateTime> SpotifyCurrentSong(TwitchChatter chatter)
        {
            try
            {
                _irc.SendPublicChatMessage(await SharedCommands.SpotifyCurrentSong(chatter, _spotify));
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "SpotifyFeature", "SpotifyCurrentSong(TwitchChatter)", false, "!spotifysong");
            }

            return DateTime.Now;
        }

        public async Task<DateTime> SpotifyLastLong(TwitchChatter chatter)
        {
            try
            {
                _irc.SendPublicChatMessage(await SharedCommands.SpotifyLastPlayedSong(chatter, _spotify));
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "SpotifyFeature", "SpotifyLastLong(TwitchChatter)", false, "!spotifylastsong");
            }

            return DateTime.Now;
        }
    }
}
