using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TwitchBot.Configuration;
using TwitchBot.Enums;
using TwitchBot.Libraries;
using TwitchBot.Models;

namespace TwitchBot.Commands.Features
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
            _rolePermission.Add("!spotifyconnect", new List<ChatterType> { ChatterType.Broadcaster });
            _rolePermission.Add("!spotifyplay", new List<ChatterType> { ChatterType.Broadcaster });
            _rolePermission.Add("!spotifypause", new List<ChatterType> { ChatterType.Broadcaster });
            _rolePermission.Add("!spotifyprev", new List<ChatterType> { ChatterType.Broadcaster });
            _rolePermission.Add("!spotifyback", new List<ChatterType> { ChatterType.Broadcaster });
            _rolePermission.Add("!spotifynext", new List<ChatterType> { ChatterType.Broadcaster });
            _rolePermission.Add("!spotifyskip", new List<ChatterType> { ChatterType.Broadcaster });
        }

        public override async Task<bool> ExecCommand(TwitchChatter chatter, string requestedCommand)
        {
            try
            {
                switch (requestedCommand)
                {
                    case "!spotifyconnect": // Manually connect to Spotify
                        await _spotify.Connect();
                        return true;
                    case "!spotifyplay": // Press local Spotify play button [>]
                        await _spotify.Play();
                        return true;
                    case "!spotifypause": // Press local Spotify pause button [||]
                        await _spotify.Pause();
                        return true;
                    case "!spotifyprev": // Press local Spotify previous button [|<]
                    case "!spotifyback":
                        await _spotify.SkipToPreviousPlayback();
                        return true;
                    case "!spotifynext": // Press local Spotify next (skip) button [>|]
                    case "!spotifyskip":
                        await _spotify.SkipToNextPlayback();
                        return true;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "SpotifyFeature", "ExecCommand(TwitchChatter, string)", false, requestedCommand, chatter.Message);
            }

            return false;
        }
    }
}
