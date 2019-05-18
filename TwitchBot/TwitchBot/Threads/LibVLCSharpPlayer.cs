using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using LibVLCSharp.Shared;

using TwitchBot.Configuration;
using TwitchBot.Libraries;

namespace TwitchBot.Threads
{
    public class LibVLCSharpPlayer
    {
        private LibVLC _libVLC;
        private Thread _vlcPlayerThread;
        private TwitchBotConfigurationSection _botConfig;
        private YoutubeClient _youTubeClientInstance = YoutubeClient.Instance;

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

        public LibVLCSharpPlayer() { }

        public LibVLCSharpPlayer(TwitchBotConfigurationSection botConfig)
        {
            _botConfig = botConfig;
            _vlcPlayerThread = new Thread(new ThreadStart(this.Run));
        }

        private MediaPlayer MediaPlayer { get; set; }

        public void Start()
        {
            _vlcPlayerThread.IsBackground = true;
            _vlcPlayerThread.Start();
        }

        private async void Run()
        {
            List<string> songRequestPlaylistIds = await _youTubeClientInstance.GetPlaylistVideoIds(_botConfig.YouTubeBroadcasterPlaylistId);
            List<string> personalYoutubePlaylistIds = await _youTubeClientInstance.GetPlaylistVideoIds(_botConfig.YouTubePersonalPlaylistId);

            string videoId = songRequestPlaylistIds.FirstOrDefault();
            if (string.IsNullOrEmpty(videoId))
            {
                videoId = personalYoutubePlaylistIds.FirstOrDefault();
                if (string.IsNullOrEmpty(videoId))
                {
                    return; // exit without playing anything because either playlists aren't configured
                }
            }

            Core.Initialize();
            _libVLC = new LibVLC(_commandLineOptions);
            Media media = await SetMedia(_libVLC, "https://youtu.be/" + videoId);

            while (true)
            {
                while (MediaPlayer != null && MediaPlayer.IsPlaying)
                {
                    // wait
                }

                // ToDo: Load next song
                media = await SetMedia(_libVLC, "https://youtu.be/" + videoId);

                //Thread.Sleep(300000); // 5 minutes
            }
        }

        private async Task<Media> SetMedia(LibVLC libVLC, string url)
        {
            Media media = new Media(libVLC, url, FromType.FromLocation);
            await media.Parse(MediaParseOptions.ParseNetwork);

            return media;
        }

        public void Play()
        {
            MediaPlayer.Play();
        }
    }
}
