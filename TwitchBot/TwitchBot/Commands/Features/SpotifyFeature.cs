using SpotifyAPI.Web.Models;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using TwitchBot.Configuration;
using TwitchBot.Enums;
using TwitchBot.Libraries;
using TwitchBot.Models;

using TwitchBotUtil.Extensions;

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
            _rolePermission.Add("!spotifylastsong", new List<ChatterType> { ChatterType.Viewer });
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
        public async Task SpotifyCurrentSong(TwitchChatter chatter)
        {
            try
            {
                PlaybackContext playbackContext = await _spotify.GetPlayback();
                if (playbackContext != null && playbackContext.IsPlaying)
                {
                    string artistName = "";

                    foreach (SimpleArtist simpleArtist in playbackContext.Item.Artists)
                    {
                        artistName += $"{simpleArtist.Name}, ";
                    }

                    artistName = artistName.ReplaceLastOccurrence(", ", "");

                    TimeSpan progressTimeSpan = TimeSpan.FromMilliseconds(playbackContext.ProgressMs);
                    TimeSpan durationTimeSpan = TimeSpan.FromMilliseconds(playbackContext.Item.DurationMs);

                    _irc.SendPublicChatMessage($"@{chatter.DisplayName} <-- Now playing from Spotify: \"{playbackContext.Item.Name}\" by {artistName} "
                        + "https://open.spotify.com/track/" + playbackContext.Item.Id + " "
                        + $"Currently playing at {progressTimeSpan.ReformatTimeSpan()} of {durationTimeSpan.ReformatTimeSpan()}");
                }
                else
                    _irc.SendPublicChatMessage($"Nothing is playing at the moment @{chatter.DisplayName}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "SpotifyFeature", "SpotifyCurrentSong(TwitchChatter)", false, "!spotifysong");
            }
        }
    }
}
