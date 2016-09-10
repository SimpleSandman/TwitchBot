using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpotifyAPI.Local;
using SpotifyAPI.Local.Enums;
using SpotifyAPI.Local.Models;
using TwitchBot.Configuration;

namespace TwitchBot
{
    /* Example Code for Local Spotify API */
    // https://github.com/JohnnyCrazy/SpotifyAPI-NET/blob/master/SpotifyAPI.Example/LocalControl.cs

    public class SpotifyControl
    {
        private TwitchBotConfigurationSection _botConfig;
        private SpotifyLocalAPI _spotify;
        private bool trackChanged = false; // used to prevent a paused song to skip to the next song
                                           // from displaying both "upcoming" and "current" song stats

        public SpotifyControl(TwitchBotConfigurationSection _botSection)
        {
            _botConfig = _botConfig;
            _spotify = new SpotifyLocalAPI();
            _spotify.OnPlayStateChange += spotify_OnPlayStateChange;
            _spotify.OnTrackChange += spotify_OnTrackChange;
        }

        public void Connect()
        {
            if (!SpotifyLocalAPI.IsSpotifyRunning())
            {
                Console.WriteLine("Spotify isn't running!");
                return;
            }

            if (!SpotifyLocalAPI.IsSpotifyWebHelperRunning())
            {
                Console.WriteLine("SpotifyWebHelper isn't running!");
                return;
            }

            bool successful = _spotify.Connect();
            if (successful)
            {
                Console.WriteLine("Connection to Spotify successful");
                UpdateInfos();
                _spotify.ListenForEvents = true;
            }
            else
            {
                Console.WriteLine("Couldn't connect to the spotify client. Retry? (Yes = 'y' and No = 'n')");
                Console.WriteLine("If this problem persists, try reinstalling Spotify to the latest version");
                if (Console.ReadLine().Equals("y"))
                    Connect();
            }
        }

        public void playBtn_Click()
        {
            _spotify.Play();
        }

        public void pauseBtn_Click()
        {
            _spotify.Pause();
        }

        public void prevBtn_Click()
        {
            _spotify.Previous();
        }

        public void skipBtn_Click()
        {
            _spotify.Skip();
        }

        public StatusResponse GetStatus()
        {
            return _spotify.GetStatus();
        }

        public void UpdatePlayingStatus(bool playing)
        {
            Console.WriteLine("Playing Status: " + playing.ToString());

        }

        private void spotify_OnTrackChange(object sender, TrackChangeEventArgs e)
        {
            ShowUpdatedTrack(e.NewTrack, _botConfig.EnableDisplaySong);
            trackChanged = true;
        }

        private void spotify_OnPlayStateChange(object sender, PlayStateEventArgs e)
        {
            UpdatePlayingStatus(e.Playing);

            StatusResponse status = _spotify.GetStatus();
            if (status.Track != null && status.Playing && !trackChanged) // Update track infos
                ShowUpdatedTrack(status.Track, _botConfig.EnableDisplaySong);

            trackChanged = false;
        }

        public void UpdateInfos()
        {
            StatusResponse status = _spotify.GetStatus();
            if (status == null)
                return;

            // Basic Spotify Infos
            UpdatePlayingStatus(status.Playing);
            Console.WriteLine("Client Version: " + status.ClientVersion);
            Console.WriteLine("Version: " + status.Version.ToString() + "\n");

            if (status.Track != null && status.Playing) // Update track infos
                ShowUpdatedTrack(status.Track, _botConfig.EnableDisplaySong);
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

                Program._lstTupDelayMsg.Add(new Tuple<string, DateTime>(
                        pendingMessage,
                        DateTime.Now.AddSeconds(_botConfig.StreamLatency)
                    )
                );
            }
        }
    }
}
