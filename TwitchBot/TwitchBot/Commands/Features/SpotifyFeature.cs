using System;

using TwitchBot.Configuration;
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
            _rolePermission.Add("!spotifyconnect", "broadcaster");
            _rolePermission.Add("!spotifyplay", "broadcaster");
            _rolePermission.Add("!spotifypause", "broadcaster");
            _rolePermission.Add("!spotifyprev", "broadcaster");
            _rolePermission.Add("!spotifyback", "broadcaster");
            _rolePermission.Add("!spotifynext", "broadcaster");
            _rolePermission.Add("!spotifyskip", "broadcaster");
        }

        public override async void ExecCommand(TwitchChatter chatter, string requestedCommand)
        {
            try
            {
                switch (requestedCommand)
                {
                    case "!spotifyconnect": // Manually connect to Spotify
                        await _spotify.Connect();
                        break;
                    case "!spotifyplay": // Press local Spotify play button [>]
                        await _spotify.Play();
                        break;
                    case "!spotifypause": // Press local Spotify pause button [||]
                        await _spotify.Pause();
                        break;
                    case "!spotifyprev": // Press local Spotify previous button [|<]
                    case "!spotifyback":
                        await _spotify.SkipToPreviousPlayback();
                        break;
                    case "!spotifynext": // Press local Spotify next (skip) button [>|]
                    case "!spotifyskip":
                        await _spotify.SkipToNextPlayback();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "SpotifyFeature", "ExecCommand(TwitchChatter, string)", false, requestedCommand, chatter.Message);
            }
        }
    }
}
