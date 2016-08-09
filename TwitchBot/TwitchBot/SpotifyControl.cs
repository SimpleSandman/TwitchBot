using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpotifyAPI.Local;
using SpotifyAPI.Local.Enums;
using SpotifyAPI.Local.Models;

namespace TwitchBot
{
    /* Example Code for Local Spotify API */
    // https://github.com/JohnnyCrazy/SpotifyAPI-NET/blob/master/SpotifyAPI.Example/LocalControl.cs

    class SpotifyControl
    {
        private bool trackChanged = false; // used to prevent a paused song to skip to the next song
                                           // from displaying both "upcoming" and "current" song stats

        public void Connect()
        {
            if (!SpotifyLocalAPI.IsSpotifyRunning())
            {
                Console.WriteLine("Spotify isn't running!");
                Program._irc.sendPublicChatMessage("Spotify isn't running!");
                return;
            }
            if (!SpotifyLocalAPI.IsSpotifyWebHelperRunning())
            {
                Console.WriteLine("SpotifyWebHelper isn't running!");
                Program._irc.sendPublicChatMessage("Spotify isn't running!");
                return;
            }

            bool successful = Program._spotify.Connect();
            if (successful)
            {
                Console.WriteLine("Connection to Spotify successful");
                UpdateInfos();
                Program._spotify.ListenForEvents = true;
            }
            else
            {
                Console.WriteLine("Couldn't connect to the spotify client. Retry? ('Y' = Yes and 'N' = No)");
                if (Console.ReadLine() == "Y")
                    Connect();
            }
        }

        public void UpdatePlayingStatus(bool playing)
        {
            Console.WriteLine("Playing Status: " + playing.ToString());

        }

        public void spotify_OnTrackChange(TrackChangeEventArgs e)
        {
            ShowUpdatedTrack(e.NewTrack, Program._isAutoDisplaySong);
            trackChanged = true;
        }

        public void spotify_OnPlayStateChange(PlayStateEventArgs e)
        {
            UpdatePlayingStatus(e.Playing);

            StatusResponse status = Program._spotify.GetStatus();
            if (status.Track != null && status.Playing && !trackChanged) // Update track infos
                ShowUpdatedTrack(status.Track, Program._isAutoDisplaySong);

            trackChanged = false;
        }

        public void playBtn_Click()
        {
            Program._spotify.Play();
        }

        public void pauseBtn_Click()
        {
            Program._spotify.Pause();
        }

        public void prevBtn_Click()
        {
            Program._spotify.Previous();
        }

        public void skipBtn_Click()
        {
            Program._spotify.Skip();
        }

        public void UpdateInfos()
        {
            StatusResponse status = Program._spotify.GetStatus();
            if (status == null)
                return;

            // Basic Spotify Infos
            UpdatePlayingStatus(status.Playing);
            Console.WriteLine("Client Version: " + status.ClientVersion);
            Console.WriteLine("Version: " + status.Version.ToString() + "\n");

            if (status.Track != null && status.Playing) // Update track infos
                ShowUpdatedTrack(status.Track, Program._isAutoDisplaySong);
        }

        public void ShowUpdatedTrack(Track track, bool isDisplayed)
        {
            string pendingMessage = "";

            if (track.IsAd())
                return; // Don't process further, maybe null values

            // display track
            Console.WriteLine("Song: " + track.TrackResource.Name);
            Console.WriteLine("Artist: " + track.ArtistResource.Name);
            Console.WriteLine("Album: " + track.AlbumResource.Name + "\n");

            // if song is allowed to be displayed to the chat
            if (isDisplayed)
            {
                pendingMessage = "Current Song: " + track.TrackResource.Name
                    + " || Artist: " + track.ArtistResource.Name
                    + " || Album: " + track.AlbumResource.Name;
            }

            Program._lstTupDelayMsg.Add(new Tuple<string, DateTime>(
                    pendingMessage, 
                    DateTime.Now.AddSeconds(Program._intStreamLatency)
                )
            );
        }
    }
}
