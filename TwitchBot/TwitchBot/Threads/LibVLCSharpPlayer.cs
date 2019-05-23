using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Google.Apis.YouTube.v3.Data;

using LibVLCSharp.Shared;
using LibVLCSharp.Shared.Structures;

using TwitchBot.Configuration;
using TwitchBot.Libraries;

using TwitchBotUtil.Extensions;

namespace TwitchBot.Threads
{
    public class LibVLCSharpPlayer
    {
        private LibVLC _libVLC;
        private Thread _vlcPlayerThread;
        private TwitchBotConfigurationSection _botConfig;
        private MediaPlayer _mediaPlayer;
        private Queue<PlaylistItem> _songRequestPlaylistVideoIds;
        private Queue<PlaylistItem> _personalYoutubePlaylistVideoIds;
        private YoutubeClient _youTubeClientInstance = YoutubeClient.Instance;

        public LibVLCSharpPlayer() { }

        public LibVLCSharpPlayer(TwitchBotConfigurationSection botConfig)
        {
            _botConfig = botConfig;

            Core.Initialize();
            _libVLC = new LibVLC(_commandLineOptions);
            _mediaPlayer = new MediaPlayer(_libVLC);

            _vlcPlayerThread = new Thread(new ThreadStart(this.Run));
        }

        public bool IsPersonalPlaylistShuffle { get; set; } = false;

        public PlaylistItem CurrentSongRequestPlaylistItem { get; private set; }

        // Reference (LibVLC YouTube playback): https://forum.videolan.org/viewtopic.php?t=148637#p488319
        // Reference (VLC command line): https://wiki.videolan.org/VLC_command-line_help
        private readonly string[] _commandLineOptions =
        {
                "--audio-filter=compressor",
                "--compressor-rms-peak=0.00",
                "--compressor-attack=24.00",
                "--compressor-release=250.00",
                "--compressor-threshold=-25.00",
                "--compressor-ratio=2.00",
                "--compressor-knee=4.50",
                "--compressor-makeup-gain=17.00"
        };

        public void Start()
        {
            _vlcPlayerThread.IsBackground = true;
            _vlcPlayerThread.Start();
        }

        private async void Run()
        {
            _songRequestPlaylistVideoIds = new Queue<PlaylistItem>(await _youTubeClientInstance.GetPlaylistItems(_botConfig.YouTubeBroadcasterPlaylistId));

            if (IsPersonalPlaylistShuffle)
            {
                List<PlaylistItem> shuffledList = await _youTubeClientInstance.GetPlaylistItems(_botConfig.YouTubePersonalPlaylistId);
                shuffledList.Shuffle();

                _personalYoutubePlaylistVideoIds = new Queue<PlaylistItem>(shuffledList);
            }
            else
            {
                _personalYoutubePlaylistVideoIds = new Queue<PlaylistItem>(await _youTubeClientInstance.GetPlaylistItems(_botConfig.YouTubePersonalPlaylistId));
            }

            SetNextVideoId();

            if (CurrentSongRequestPlaylistItem == null)
            {
                return; // don't try to start the VLC video player until there is something to play
            }

            while (true)
            {
                if (CurrentSongRequestPlaylistItem != null)
                {
                    PlayMedia();
                }

                while (_mediaPlayer?.Media?.State != VLCState.Ended)
                {
                    // wait
                }

                SetNextVideoId();
            }
        }

        private void SetNextVideoId()
        {
            if (_songRequestPlaylistVideoIds.Count > 0)
                CurrentSongRequestPlaylistItem = _songRequestPlaylistVideoIds.Dequeue();
            else if (_personalYoutubePlaylistVideoIds.Count > 0)
                CurrentSongRequestPlaylistItem = _personalYoutubePlaylistVideoIds.Dequeue();
            else
                CurrentSongRequestPlaylistItem = null;
        }

        private void PlayMedia()
        {
            if (CurrentSongRequestPlaylistItem?.ContentDetails?.VideoId != null)
            {
                if (_mediaPlayer.State == VLCState.Stopped || _mediaPlayer.State == VLCState.Paused)
                {
                    _mediaPlayer.Play();
                }
                else
                {
                    Media media = new Media(_libVLC, "https://youtu.be/" + CurrentSongRequestPlaylistItem.ContentDetails.VideoId, FromType.FromLocation);
                    media.Parse(MediaParseOptions.ParseNetwork).Wait();
                    _mediaPlayer.Media = media.SubItems.First();
                    _mediaPlayer.Play();

                    media.Dispose();
                }
            }
        }

        public void Play()
        {
            if (_mediaPlayer != null)
            {
                PlayMedia();
            }
        }

        public bool Pause()
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Pause();
                return true;
            }

            return false;
        }

        public void Skip()
        {
            if (_mediaPlayer != null)
            {
                SetNextVideoId();
                PlayMedia();
            }
        }

        public bool SetVolume(int volumePercentage)
        {
            if (_mediaPlayer != null && volumePercentage > 0 && volumePercentage <= 100)
            {
                _mediaPlayer.Volume = volumePercentage;
                return true;
            }

            return false;
        }

        public int DisplayVolume()
        {
            return _mediaPlayer.Volume;
        }

        public void ResetSongRequestQueue()
        {
            _songRequestPlaylistVideoIds = new Queue<PlaylistItem>();
        }

        public string SetAudioOutputDevice(string audioOutputDeviceName)
        {
            if (_mediaPlayer != null)
            {
                AudioOutputDevice defaultAudioOutputDevice = _mediaPlayer.AudioOutputDeviceEnum.FirstOrDefault(a => a.Description == "Default");

                if (audioOutputDeviceName.ToLower() == "default")
                {
                    _mediaPlayer.SetOutputDevice(defaultAudioOutputDevice.DeviceIdentifier);
                    return $"Successfully defaulted the song request media player's audio output device";
                }

                AudioOutputDevice audioOutputDevice = _mediaPlayer.AudioOutputDeviceEnum.FirstOrDefault(a => a.Description == audioOutputDeviceName);

                if (audioOutputDevice.Description != audioOutputDeviceName)
                {
                    _mediaPlayer.SetOutputDevice(defaultAudioOutputDevice.DeviceIdentifier);
                    return "Cannot set the requested audio output device for the song request media player. " 
                        + $"Setting it to \"{defaultAudioOutputDevice.Description}\"";
                }
                else
                {
                    _mediaPlayer.SetOutputDevice(audioOutputDevice.DeviceIdentifier);
                    return $"Successfully set the song request media player's audio output device to \"{audioOutputDeviceName}\"";
                }
            }

            return "I cannot find the song request media player";
        }

        public void AddSongRequest(PlaylistItem playlistItem)
        {
            _songRequestPlaylistVideoIds.Enqueue(playlistItem);
        }

        public bool MediaPlayerStatus()
        {
            return _mediaPlayer?.State == VLCState.Playing ? true : false;
        }
    }
}
