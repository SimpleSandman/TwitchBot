using System;
using System.Configuration;
using System.Threading.Tasks;

using Google.Apis.YouTube.v3.Data;

using LibVLCSharp.Shared;

using TwitchBot.Configuration;
using TwitchBot.Libraries;
using TwitchBot.Models;
using TwitchBot.Threads;

namespace TwitchBot.Commands.Features
{
    /// <summary>
    /// The "Command Subsystem" for the "____" feature
    /// </summary>
    public sealed class LibVLCSharpPlayerFeature : BaseFeature
    {
        private readonly LibVLCSharpPlayer _libVLCSharpPlayer;
        private readonly System.Configuration.Configuration _appConfig;
        private readonly YoutubeClient _youTubeClientInstance = YoutubeClient.Instance;
        private readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        public LibVLCSharpPlayerFeature(IrcClient irc, TwitchBotConfigurationSection botConfig, System.Configuration.Configuration appConfig) : base(irc, botConfig)
        {
            _appConfig = appConfig;
            _rolePermission.Add("!srstart", "broadcaster");
            _rolePermission.Add("!srstop", "broadcaster");
            _rolePermission.Add("!srpause", "broadcaster");
            _rolePermission.Add("!sraod", "broadcaster");
            _rolePermission.Add("!srshuffle", "broadcaster");
            _rolePermission.Add("!srplay", "broadcaster");
        }

        public override async void ExecCommand(TwitchChatter chatter, string requestedCommand)
        {
            try
            {
                switch (requestedCommand)
                {
                    case "!srstart":
                        await Start();
                        break;
                    case "!srstop":
                        Stop();
                        break;
                    case "!srpause":
                        await Pause();
                        break;
                    case "!sraod":
                        SetAudioOutputDevice(chatter);
                        break;
                    case "!srshuffle on":
                        PersonalPlaylistShuffle(true);
                        break;
                    case "!srshuffle off":
                        PersonalPlaylistShuffle(false);
                        break;
                    case "!srplay":
                        await Play();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "LibVLCSharpPlayerFeature", "ExecCommand(TwitchChatter, string)", false, requestedCommand, chatter.Message);
            }
        }

        public async Task Start()
        {
            try
            {
                await _libVLCSharpPlayer.Start();
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "LibVLCSharpPlayerFeature", "Start()", false, "!srstart");
            }
        }

        public async void Stop()
        {
            try
            {
                _libVLCSharpPlayer.Stop();
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "LibVLCSharpPlayerFeature", "Stop()", false, "!srstop");
            }
        }

        public async Task Pause()
        {
            try
            {
                if (_libVLCSharpPlayer.MediaPlayerStatus() == VLCState.Paused && _libVLCSharpPlayer.MediaPlayerStatus() == VLCState.Stopped)
                {
                    _irc.SendPublicChatMessage($"This media player is already {await _libVLCSharpPlayer.GetVideoTime()} @{_botConfig.Broadcaster}");
                    return;
                }

                _libVLCSharpPlayer.Pause();
                _irc.SendPublicChatMessage($"Paused current song request @{_botConfig.Broadcaster}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "LibVLCSharpPlayerFeature", "Pause()", false, "!srpause");
            }
        }

        public async void SetAudioOutputDevice(TwitchChatter chatter)
        {
            try
            {
                string audioOutputDevice = chatter.Message.Substring(chatter.Message.IndexOf(" ") + 1);

                _irc.SendPublicChatMessage($"{_libVLCSharpPlayer.SetAudioOutputDevice(audioOutputDevice)} @{_botConfig.Broadcaster}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "LibVLCSharpPlayerFeature", "SetAudioOutputDevice(string)", false, "!sraod");
            }
        }

        public async void PersonalPlaylistShuffle(bool shuffle)
        {
            try
            {
                if (_botConfig.EnablePersonalPlaylistShuffle == shuffle)
                {
                    if (shuffle)
                        _irc.SendPublicChatMessage($"Your personal playlist has already been shuffled @{_botConfig.Broadcaster}");
                    else
                        _irc.SendPublicChatMessage($"Your personal playlist is already in order @{_botConfig.Broadcaster}");
                }
                else
                {
                    await _libVLCSharpPlayer.SetPersonalPlaylistShuffle(shuffle);

                    _botConfig.EnablePersonalPlaylistShuffle = shuffle;
                    _appConfig.AppSettings.Settings.Remove("enablePersonalPlaylistShuffle");

                    if (shuffle)
                    {
                        _appConfig.AppSettings.Settings.Add("enablePersonalPlaylistShuffle", "true");

                        _irc.SendPublicChatMessage("Your personal playlist queue has been shuffled. "
                            + $"Don't worry, I didn't touch your actual YouTube playlist @{_botConfig.Broadcaster} ;)");
                    }
                    else
                    {
                        _appConfig.AppSettings.Settings.Add("enablePersonalPlaylistShuffle", "false");

                        _irc.SendPublicChatMessage("Your personal playlist queue has been reset to its proper order continuing on from this video. "
                            + $"Don't worry, I didn't touch your actual YouTube playlist @{_botConfig.Broadcaster} ;)");
                    }

                    _appConfig.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection("TwitchBotConfiguration");
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "LibVLCSharpPlayerFeature", "PersonalPlaylistShuffle(bool)", false, "!srshuffle");
            }
        }

        public async Task Play()
        {
            try
            {
                if (_libVLCSharpPlayer.MediaPlayerStatus() == VLCState.Playing)
                {
                    _irc.SendPublicChatMessage($"This media player is already {await _libVLCSharpPlayer.GetVideoTime()} @{_botConfig.Broadcaster}");
                    return;
                }

                await _libVLCSharpPlayer.Play();

                PlaylistItem playlistItem = _libVLCSharpPlayer.CurrentSongRequestPlaylistItem;

                string songRequest = _youTubeClientInstance.ShowPlayingSongRequest(playlistItem);

                if (!string.IsNullOrEmpty(songRequest))
                    _irc.SendPublicChatMessage($"@{_botConfig.Broadcaster} <-- Now playing: {songRequest} Currently {await _libVLCSharpPlayer.GetVideoTime()}");
                else
                    _irc.SendPublicChatMessage($"Unable to display the current song @{_botConfig.Broadcaster}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "LibVLCSharpPlayerFeature", "Play()", false, "!srplay");
            }
        }

        /* ToDo: Insert new methods here */
    }
}
