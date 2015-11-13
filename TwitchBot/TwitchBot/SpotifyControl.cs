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
        public void Connect()
        {
            if (!SpotifyLocalAPI.IsSpotifyRunning())
            {
                Console.WriteLine("Spotify isn't running!");
                Program.irc.sendPublicChatMessage("Spotify isn't running!");
                return;
            }
            if (!SpotifyLocalAPI.IsSpotifyWebHelperRunning())
            {
                Console.WriteLine("SpotifyWebHelper isn't running!");
                Program.irc.sendPublicChatMessage("Spotify isn't running!");
                return;
            }

            bool successful = Program.spotify.Connect();
            if (successful)
            {
                Console.WriteLine("Connection to Spotify successful");
                UpdateInfos();
                Program.spotify.ListenForEvents = true;
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
            ShowUpdatedTrack(e.NewTrack, true);
        }

        public void spotify_OnPlayStateChange(PlayStateEventArgs e)
        {
            UpdatePlayingStatus(e.Playing);
        }

        public void playBtn_Click()
        {
            Program.spotify.Play();
        }

        public void pauseBtn_Click()
        {
            Program.spotify.Pause();
        }

        public void prevBtn_Click()
        {
            Program.spotify.Previous();
        }

        public void skipBtn_Click()
        {
            Program.spotify.Skip();
        }

        public void UpdateInfos()
        {
            StatusResponse status = Program.spotify.GetStatus();
            if (status == null)
                return;

            // Basic Spotify Infos
            UpdatePlayingStatus(status.Playing);
            Console.WriteLine("Client Version: " + status.ClientVersion);
            Console.WriteLine("Version: " + status.Version.ToString() + "\n");

            if (status.Track != null) // Update track infos
                ShowUpdatedTrack(status.Track, false);
        }

        public void ShowUpdatedTrack(Track track, bool songChange)
        {
            if (track.IsAd())
                return; // Don't process further, maybe null values

            // display track
            Console.WriteLine("Song: " + track.TrackResource.Name);
            Console.WriteLine("Artist: " + track.ArtistResource.Name);
            Console.WriteLine("Album: " + track.AlbumResource.Name + "\n");

            // send message of current song info to chat
            if (songChange)
            {
                Program.irc.sendPublicChatMessage("Upcoming Song: " + track.TrackResource.Name
                    + " || Artist: " + track.ArtistResource.Name
                    + " || Album: " + track.AlbumResource.Name);
            }
            else
            {
                Program.irc.sendPublicChatMessage("Current Song: " + track.TrackResource.Name
                    + " || Artist: " + track.ArtistResource.Name
                    + " || Album: " + track.AlbumResource.Name);
            }
        }
    }
}
