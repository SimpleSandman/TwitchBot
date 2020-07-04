using SpotifyAPI.Web.Models;

using System;
using System.Threading.Tasks;

using TwitchBot.Libraries;
using TwitchBot.Models;

using TwitchBotUtil.Extensions;

namespace TwitchBot.Commands
{
    public static class SharedCommands
    {
        /// <summary>
        /// Displays the last song played from Spotify
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        public static async Task<string> SpotifyLastPlayedSong(TwitchChatter chatter, SpotifyWebClient spotify)
        {
            SimpleTrack simpleTrack = await spotify.GetLastPlayedSong();
            if (simpleTrack != null)
            {
                string artistName = "";

                foreach (SimpleArtist simpleArtist in simpleTrack.Artists)
                {
                    artistName += $"{simpleArtist.Name}, ";
                }

                artistName = artistName.ReplaceLastOccurrence(", ", "");

                return $"@{chatter.DisplayName} <-- Last played from Spotify: \"{simpleTrack.Name}\" by {artistName} "
                    + "https://open.spotify.com/track/" + simpleTrack.Id + " WARNING: This is currently a feature in BETA --> "
                    + "https://developer.spotify.com/documentation/web-api/reference/player/get-recently-played/";
            }
            else
            {
                return $"Nothing was played recently @{chatter.DisplayName}";
            }
        }

        /// <summary>
        /// Displays the current song being played from Spotify
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        public static async Task<string> SpotifyCurrentSong(TwitchChatter chatter, SpotifyWebClient spotify)
        {
            PlaybackContext playbackContext = await spotify.GetPlayback();
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

                return $"@{chatter.DisplayName} <-- Now playing from Spotify: \"{playbackContext.Item.Name}\" by {artistName} "
                    + "https://open.spotify.com/track/" + playbackContext.Item.Id + " "
                    + $"Currently playing at {progressTimeSpan.ReformatTimeSpan()} of {durationTimeSpan.ReformatTimeSpan()}";
            }
            else
            {
                return $"Nothing is playing at the moment @{chatter.DisplayName}";
            }
        }
    }
}
