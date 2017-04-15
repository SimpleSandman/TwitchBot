using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpotifyAPI.Local;
using SpotifyAPI.Local.Enums;
using SpotifyAPI.Local.Models;

using TwitchBot.Configuration;

namespace TwitchBot.Libraries
{
    /* Example Code for Local Spotify API */
    // https://github.com/JohnnyCrazy/SpotifyAPI-NET/blob/master/SpotifyAPI.Example/LocalControl.cs
    public class LocalSpotifyClient
    {
        private TwitchBotConfigurationSection _botConfig;
        private SpotifyLocalAPI _spotify;
        private bool _trackChanged; // used to prevent a paused song to skip to the next song
                                    // from displaying both "upcoming" and "current" song stats
        private ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        public LocalSpotifyClient(TwitchBotConfigurationSection _botSection)
        {
            _botConfig = _botSection;
            _spotify = new SpotifyLocalAPI();
            _spotify.OnPlayStateChange += spotify_OnPlayStateChange;
            _spotify.OnTrackChange += spotify_OnTrackChange;
            _trackChanged = false;
        }

        public void Connect()
        {
            try
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

                if (_spotify.Connect())
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
                        Connect(); // attempt to connect again
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "TwitchBotApplication", "Connect()", true);
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
            _trackChanged = true;
        }

        private void spotify_OnPlayStateChange(object sender, PlayStateEventArgs e)
        {
            UpdatePlayingStatus(e.Playing);

            StatusResponse status = _spotify.GetStatus();
            if (status.Track != null && status.Playing && !_trackChanged) // Update track infos
                ShowUpdatedTrack(status.Track, _botConfig.EnableDisplaySong);

            _trackChanged = false;
        }

        public void UpdateInfos()
        {
            StatusResponse status = _spotify.GetStatus();
            if (status == null)
                return;

            // Basic Spotify Infos
            UpdatePlayingStatus(status.Playing);
            Console.WriteLine("Client Version: " + status.ClientVersion);
            Console.WriteLine("Version: " + status.Version.ToString());

            if (status.Track != null && status.Playing) // Update track infos
                ShowUpdatedTrack(status.Track, _botConfig.EnableDisplaySong);
        }

        public void ShowUpdatedTrack(Track track, bool isDisplayed)
        {
            string pendingMessage = "";

            if (track.IsAd())
                return; // Don't process further, maybe null values

            // display track
            Console.WriteLine("\nSong: " + track.TrackResource.Name);
            Console.WriteLine("Artist: " + track.ArtistResource.Name);
            Console.WriteLine("Album: " + track.AlbumResource.Name);

            // if song is allowed to be displayed to the chat
            if (isDisplayed)
            {
                pendingMessage = "Current Song: " + track.TrackResource.Name
                    + " >< Artist: " + track.ArtistResource.Name
                    + " >< Album: " + track.AlbumResource.Name;

                //Program.DelayMsgTupleList.Add(new Tuple<string, DateTime>(
                //        pendingMessage,
                //        DateTime.Now.AddSeconds(_botConfig.StreamLatency)
                //    )
                //);
            }
        }
    }
}
