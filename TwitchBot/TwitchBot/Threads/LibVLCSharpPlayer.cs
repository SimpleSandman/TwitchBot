using System;
using System.Collections.Generic;
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

        public LibVLCSharpPlayer(LibVLC libVLC, TwitchBotConfigurationSection botConfig)
        {
            _libVLC = libVLC;
            _botConfig = botConfig;
            _vlcPlayerThread = new Thread(new ThreadStart(this.Run));
        }

        public void Start()
        {
            _vlcPlayerThread.IsBackground = true;
            _vlcPlayerThread.Start();
        }

        private async void Run()
        {
            Core.Initialize();
            _libVLC = new LibVLC(_commandLineOptions);
            
            // ToDo: Load YouTube song request playlist video IDs into memory
            List<string> songRequestPlaylistIds = await _youTubeClientInstance.GetPlaylistVideoIds(_botConfig.YouTubeBroadcasterPlaylistId);

            // ToDo: Load YouTube backup playlist video IDs into memory
            List<string> personalYoutubePlaylistIds = await _youTubeClientInstance.GetPlaylistVideoIds(_botConfig.YouTubePersonalPlaylistId);

            while (true)
            {
                Thread.Sleep(300000); // 5 minutes
            }
        }

        private async Task<Media> SetMedia(LibVLC libVLC, string url)
        {
            Media media = new Media(libVLC, url, FromType.FromLocation);
            await media.Parse(MediaParseOptions.ParseNetwork);

            return media;
        }
    }
}
