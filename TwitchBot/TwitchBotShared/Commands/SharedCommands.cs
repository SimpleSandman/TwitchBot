using System;
using System.Threading.Tasks;

using SpotifyAPI.Web;

using TwitchBotShared.ClientLibraries;
using TwitchBotShared.Extensions;
using TwitchBotShared.Models;

namespace TwitchBotShared.Commands
{
    public static class SharedCommands
    {
        /// <summary>
        /// Displays the last song played from Spotify
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        public static async Task<string> SpotifyLastPlayedSongAsync(TwitchChatter chatter, SpotifyWebClient spotify)
        {
            FullTrack fullTrack = await spotify.GetLastPlayedSongAsync();
            if (fullTrack != null)
            {
                string artistName = "";

                foreach (SimpleArtist simpleArtist in fullTrack.Artists)
                {
                    artistName += $"{simpleArtist.Name}, ";
                }

                artistName = artistName.ReplaceLastOccurrence(", ", "");

                return $"@{chatter.DisplayName} <-- Last played from Spotify: \"{fullTrack.Name}\" by {artistName} "
                    + "https://open.spotify.com/track/" + fullTrack.Id + " WARNING: This is currently a feature in BETA --> "
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
        public static async Task<string> SpotifyCurrentSongAsync(TwitchChatter chatter, SpotifyWebClient spotify)
        {
            CurrentlyPlayingContext playbackContext = await spotify.GetPlaybackAsync();
            if (playbackContext != null && playbackContext.IsPlaying)
            {
                string artistName = "";
                FullTrack fullTrack = (FullTrack)playbackContext.Item;

                foreach (SimpleArtist simpleArtist in fullTrack.Artists)
                {
                    artistName += $"{simpleArtist.Name}, ";
                }

                artistName = artistName.ReplaceLastOccurrence(", ", "");

                TimeSpan progressTimeSpan = TimeSpan.FromMilliseconds(playbackContext.ProgressMs);
                TimeSpan durationTimeSpan = TimeSpan.FromMilliseconds(fullTrack.DurationMs);

                return $"@{chatter.DisplayName} <-- Now playing from Spotify: \"{fullTrack.Name}\" by {artistName} "
                    + "https://open.spotify.com/track/" + fullTrack.Id + " "
                    + $"Currently playing at {progressTimeSpan.ReformatTimeSpan()} of {durationTimeSpan.ReformatTimeSpan()}";
            }
            else
            {
                return $"Nothing is playing at the moment @{chatter.DisplayName}";
            }
        }
    }
}
