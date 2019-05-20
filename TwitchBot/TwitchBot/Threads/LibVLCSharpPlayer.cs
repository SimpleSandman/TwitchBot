using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using LibVLCSharp.Shared;

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
        private Queue<string> _songRequestPlaylistVideoIds;
        private Queue<string> _personalYoutubePlaylistVideoIds;
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
            _songRequestPlaylistVideoIds = new Queue<string>(await _youTubeClientInstance.GetPlaylistVideoIds(_botConfig.YouTubeBroadcasterPlaylistId));

            if (IsPersonalPlaylistShuffle)
            {
                List<string> shuffledList = await _youTubeClientInstance.GetPlaylistVideoIds(_botConfig.YouTubePersonalPlaylistId);
                shuffledList.Shuffle();

                _personalYoutubePlaylistVideoIds = new Queue<string>(shuffledList);
            }
            else
            {
                _personalYoutubePlaylistVideoIds = new Queue<string>(await _youTubeClientInstance.GetPlaylistVideoIds(_botConfig.YouTubePersonalPlaylistId));
            }

            string videoId = GetVideoId();

            if (string.IsNullOrEmpty(videoId))
            {
                return; // don't try to start the VLC video player until there is something to play
            }

            while (true)
            {
                if (!string.IsNullOrEmpty(videoId))
                {
                    await PlayMedia(videoId);
                }

                await Task.Delay(1000); // check back every second for a new video in the queue

                while (_mediaPlayer?.Media?.State != VLCState.Ended)
                {
                    // wait
                }

                videoId = GetVideoId();
            }
        }

        private string GetVideoId()
        {
            string videoId = "";

            if (_songRequestPlaylistVideoIds.Count > 0)
            {
                videoId = _songRequestPlaylistVideoIds.Dequeue();
            }
            else if (_personalYoutubePlaylistVideoIds.Count > 0)
            {
                videoId = _personalYoutubePlaylistVideoIds.Dequeue();
            }

            return videoId;
        }

        private async Task<Media> SetMedia(LibVLC libVLC, string url)
        {
            Media media = new Media(libVLC, url, FromType.FromLocation);
            await media.Parse(MediaParseOptions.ParseNetwork);

            return media;
        }

        private async Task PlayMedia(string videoId)
        {
            if (_mediaPlayer.Media != null)
                _mediaPlayer.Media.Dispose();

            Media media = await SetMedia(_libVLC, "https://youtu.be/" + videoId);
            _mediaPlayer.Media = media.SubItems.First();
            _mediaPlayer.Play();

            media.Dispose();
        }

        public void Play()
        {
            if (_mediaPlayer != null)
                _mediaPlayer.Play();
        }

        public void Pause()
        {
            if (_mediaPlayer != null)
                _mediaPlayer.Pause();
        }

        public void Stop()
        {
            if (_mediaPlayer != null)
                _mediaPlayer.Stop();
        }

        public void Skip()
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Position = 1.0f;
            }
        }

        public void Volume(int volumePercentage)
        {
            if (_mediaPlayer != null && volumePercentage > 0 && volumePercentage <= 100)
            {
                _mediaPlayer.Volume = volumePercentage;
            }
        }

        public void SetAudioOutputDevice(string audioOutputDevice)
        {
            if (_mediaPlayer != null)
                _mediaPlayer.SetOutputDevice(_mediaPlayer.AudioOutputDeviceEnum.FirstOrDefault(a => a.Description == audioOutputDevice).DeviceIdentifier);
        }

        public void AddSongRequest(string videoId)
        {
            _songRequestPlaylistVideoIds.Enqueue(videoId);
        }
    }
}
