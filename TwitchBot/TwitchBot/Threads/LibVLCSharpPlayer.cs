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

        public bool IsPersonalPlaylistShuffle { get; set; }

        public void Start()
        {
            _vlcPlayerThread.IsBackground = true;
            _vlcPlayerThread.Start();
        }

        private async void Run()
        {
            List<string> songRequestPlaylistVideoIds = await _youTubeClientInstance.GetPlaylistVideoIds(_botConfig.YouTubeBroadcasterPlaylistId);
            List<string> personalYoutubePlaylistVideoIds = await _youTubeClientInstance.GetPlaylistVideoIds(_botConfig.YouTubePersonalPlaylistId);

            if (IsPersonalPlaylistShuffle)
            {
                personalYoutubePlaylistVideoIds.Shuffle(new Random());
            }

            string videoId = songRequestPlaylistVideoIds.FirstOrDefault();
            if (string.IsNullOrEmpty(videoId))
            {
                videoId = personalYoutubePlaylistVideoIds.FirstOrDefault();
                if (string.IsNullOrEmpty(videoId))
                {
                    return; // exit without playing anything because either playlists aren't configured
                }
            }

            Core.Initialize();
            _libVLC = new LibVLC(_commandLineOptions);

            while (true)
            {
                await PlayMedia(videoId);

                await Task.Delay(1000);

                while (MediaPlayer != null && MediaPlayer.IsPlaying)
                {
                    // wait
                }

                // ToDo: Move onto the next video
            }
        }

        private async Task<Media> SetMedia(LibVLC libVLC, string url)
        {
            Media media = new Media(libVLC, url, FromType.FromLocation);
            await media.Parse(MediaParseOptions.ParseNetwork);

            return media;
        }

        private async Task PlayMedia(string videoId)
        {
            Media media = await SetMedia(_libVLC, "https://youtu.be/" + videoId);
            MediaPlayer = new MediaPlayer(media.SubItems.First());
            MediaPlayer.Play();
        }

        public void Play()
        {
            MediaPlayer.Play();
        }
    }
}
