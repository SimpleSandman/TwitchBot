using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
        private List<PlaylistItem> _songRequestPlaylistVideoIds;
        private List<PlaylistItem> _personalYoutubePlaylistVideoIds;
        private YoutubeClient _youTubeClientInstance = YoutubeClient.Instance;
        private ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

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

        public PlaylistItem CurrentSongRequestPlaylistItem { get; private set; }

        public async Task Start()
        {
            try
            {
                if (_libVLC == null)
                {
                    Core.Initialize();
                    _libVLC = new LibVLC(_commandLineOptions);
                    _mediaPlayer = new MediaPlayer(_libVLC);

                    await SetAudioOutputDevice(_botConfig.LibVLCAudioOutputDevice);

                    _vlcPlayerThread.IsBackground = true;
                    _vlcPlayerThread.Start();
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "LibVLCSharpPlayer", "Start()", false);
            }
        }

        private async void Run()
        {
            try
            {
                _songRequestPlaylistVideoIds = await _youTubeClientInstance.GetPlaylistItems(_botConfig.YouTubeBroadcasterPlaylistId);

                if (_botConfig.EnablePersonalPlaylistShuffle)
                {
                    List<PlaylistItem> shuffledList = await _youTubeClientInstance.GetPlaylistItems(_botConfig.YouTubePersonalPlaylistId);
                    shuffledList.Shuffle();

                    _personalYoutubePlaylistVideoIds = shuffledList;
                }
                else
                {
                    _personalYoutubePlaylistVideoIds = await _youTubeClientInstance.GetPlaylistItems(_botConfig.YouTubePersonalPlaylistId);
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
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "LibVLCSharpPlayer", "Run()", false);
            }
        }

        private async void SetNextVideoId()
        {
            try
            {
                if (_songRequestPlaylistVideoIds.Count > 0)
                {
                    CurrentSongRequestPlaylistItem = _songRequestPlaylistVideoIds.First();
                    _songRequestPlaylistVideoIds.RemoveAt(0);
                }
                else if (_personalYoutubePlaylistVideoIds.Count > 0)
                {
                    CurrentSongRequestPlaylistItem = _personalYoutubePlaylistVideoIds.First();
                    _personalYoutubePlaylistVideoIds.RemoveAt(0);
                }
                else
                {
                    CurrentSongRequestPlaylistItem = null;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "LibVLCSharpPlayer", "SetNextVideoId()", false);
            }
        }

        private async void PlayMedia()
        {
            try
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
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "LibVLCSharpPlayer", "PlayMedia()", false);
            }
        }

        public async void Play()
        {
            try
            {
                if (_mediaPlayer != null)
                {
                    PlayMedia();
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "LibVLCSharpPlayer", "Play()", false);
            }
        }

        public async void Pause()
        {
            try
            {
                if (_mediaPlayer != null)
                {
                    _mediaPlayer.Pause();
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "LibVLCSharpPlayer", "Pause()", false);
            }
        }

        public async void Skip(int songSkipCount = 0)
        {
            try
            {
                if (_mediaPlayer != null)
                {
                    // skip songs if skip count was specified
                    SkipSongRequestPlaylistVideoIds(ref songSkipCount);
                    SkipPersonalPlaylistVideoIds(ref songSkipCount);

                    SetNextVideoId();
                    PlayMedia();
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "LibVLCSharpPlayer", "Skip()", false);
            }
        }

        public async Task<bool> SetVolume(int volumePercentage)
        {
            try
            {
                if (_mediaPlayer != null && volumePercentage > 0 && volumePercentage <= 100)
                {
                    _mediaPlayer.Volume = volumePercentage;
                    return true;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "LibVLCSharpPlayer", "SetVolume(int)", false);
            }

            return false;
        }

        public async Task<int> GetVolume()
        {
            try
            {
                if (_mediaPlayer != null)
                {
                    return _mediaPlayer.Volume;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "LibVLCSharpPlayer", "GetVolume()", false);
            }

            return 0;
        }

        public void ResetSongRequestQueue()
        {
            _songRequestPlaylistVideoIds = new List<PlaylistItem>();
        }

        public async Task<string> SetAudioOutputDevice(string audioOutputDeviceName)
        {
            try
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
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "LibVLCSharpPlayer", "SetAudioOutputDevice(string)", false);
            }

            return "I cannot find the song request media player";
        }

        public async Task<int> AddSongRequest(PlaylistItem playlistItem)
        {
            try
            {
                if (_songRequestPlaylistVideoIds != null)
                {
                    _songRequestPlaylistVideoIds = _songRequestPlaylistVideoIds.Append(playlistItem).ToList();
                    return _songRequestPlaylistVideoIds.Count;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "LibVLCSharpPlayer", "AddSongRequest(PlaylistItem)", false);
            }

            return 0;
        }

        public VLCState MediaPlayerStatus()
        {
            return _mediaPlayer != null ? _mediaPlayer.State : VLCState.Error;
        }

        public async Task<bool> SetVideoTime(int timeInSec)
        {
            try
            {
                if (_mediaPlayer != null && _mediaPlayer.Time > -1
                    && timeInSec > -1 && timeInSec * 1000 < _mediaPlayer.Length)
                {
                    _mediaPlayer.Time = timeInSec * 1000;
                    return true;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "LibVLCSharpPlayer", "SetVideoTime(int)", false);
            }

            return false;
        }

        public async Task<string> GetVideoTime()
        {
            try
            {
                if (_mediaPlayer != null && _mediaPlayer.Time > -1)
                {
                    TimeSpan currentTimeSpan = new TimeSpan(0, 0, 0, 0, (int)_mediaPlayer.Time);
                    TimeSpan durationTimeSpan = new TimeSpan(0, 0, 0, 0, (int)_mediaPlayer.Length);

                    return $"{_mediaPlayer.Media.State.ToString().ToLower()}" +
                        $" at {ReformatTimeSpan(currentTimeSpan)} of {ReformatTimeSpan(durationTimeSpan)}";
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "LibVLCSharpPlayer", "GetVideoTime()", false);
            }

            return "A YouTube video hasn't been loaded yet";
        }

        public async Task SetPersonalPlaylistShuffle(bool shuffle)
        {
            try
            {
                if (CurrentSongRequestPlaylistItem != null)
                {
                    List<PlaylistItem> personalPlaylist = null;

                    if (!shuffle)
                    {
                        personalPlaylist = await _youTubeClientInstance.GetPlaylistItems(_botConfig.YouTubePersonalPlaylistId);
                        int lastPlayedItemIndex = personalPlaylist.FindIndex(p => p.Id == CurrentSongRequestPlaylistItem.Id);

                        if (lastPlayedItemIndex > -1)
                            personalPlaylist.RemoveRange(0, lastPlayedItemIndex + 1);
                    }
                    else
                    {
                        personalPlaylist = _personalYoutubePlaylistVideoIds.ToList();
                        personalPlaylist.Shuffle();
                    }

                    _personalYoutubePlaylistVideoIds = new List<PlaylistItem>(personalPlaylist);
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "LibVLCSharpPlayer", "SetPersonalPlaylistShuffle(bool)", false);
            }
        }

        public async Task<PlaylistItem> RemoveWrongSong(string username)
        {
            try
            {
                PlaylistItem removedWrongSong = _songRequestPlaylistVideoIds.LastOrDefault(p => p.ContentDetails.Note.Contains(username));

                if (removedWrongSong != null && _songRequestPlaylistVideoIds.Remove(removedWrongSong))
                {
                    return removedWrongSong;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "LibVLCSharpPlayer", "RemoveWrongSong(string)", false);
            }

            return null;
        }

        public void SkipSongRequestPlaylistVideoIds(ref int songSkipCount)
        {
            if (_songRequestPlaylistVideoIds.Count > 0 && songSkipCount > 0)
            {
                if (_songRequestPlaylistVideoIds.Count < songSkipCount)
                {
                    songSkipCount -= _songRequestPlaylistVideoIds.Count + 1; // use "+1" to include currently playing song
                    _songRequestPlaylistVideoIds.Clear();
                }
                else
                {
                    _songRequestPlaylistVideoIds.RemoveRange(0, songSkipCount - 1); // use "-1" to include currently playing song
                    songSkipCount = 0;
                }
            }
        }

        private void SkipPersonalPlaylistVideoIds(ref int songSkipCount)
        {
            if (_personalYoutubePlaylistVideoIds.Count > 0 && songSkipCount > 0)
            {
                if (_personalYoutubePlaylistVideoIds.Count < songSkipCount)
                {
                    _personalYoutubePlaylistVideoIds.Clear();
                }
                else
                {
                    _personalYoutubePlaylistVideoIds.RemoveRange(0, songSkipCount - 1); // use "-1" to include currently playing song
                }
            }
        }

        private string ReformatTimeSpan(TimeSpan ts)
        {
            string response = "";

            // format minutes
            if (ts.Minutes < 1)
                response += $"[00:";
            else if (ts.Minutes > 0 && ts.Minutes < 10)
                response += $"[0{ts.Minutes}:";
            else
                response += $"[{ts.Minutes}:";

            // format seconds
            if (ts.Seconds < 1)
                response += $"00]";
            else if (ts.Seconds > 0 && ts.Seconds < 10)
                response += $"0{ts.Seconds}]";
            else
                response += $"{ts.Seconds}]";

            return response;
        }
    }
}
