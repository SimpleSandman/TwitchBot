using System;
using System.Configuration;
using System.Threading.Tasks;

using Google.Apis.YouTube.v3.Data;

using LibVLCSharp.Shared;

using TwitchBotShared.ClientLibraries;
using TwitchBotShared.ClientLibraries.Singletons;
using TwitchBotShared.Config;
using TwitchBotShared.Enums;
using TwitchBotShared.Models;
using TwitchBotShared.Threads;

namespace TwitchBotShared.Commands.Features
{
    /// <summary>
    /// The "Command Subsystem" for the "LibVLCSharp Player" feature
    /// </summary>
    public sealed class LibVLCSharpPlayerFeature : BaseFeature
    {
        private readonly LibVLCSharpPlayer _libVLCSharpPlayer;
        private readonly Configuration _appConfig;
        private readonly YoutubeClient _youTubeClientInstance = YoutubeClient.Instance;
        private readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        #region Private Constant Variables
        private const string SR_START = "!srstart";
        private const string SR_STOP = "!srstop";
        private const string SR_PAUSE = "!srpause";
        private const string SR_AOD = "!sraod";
        private const string SR_SHUFFLE = "!srshuffle";
        private const string SR_PLAY = "!srplay";
        private const string SR_VOLUME = "!srvolume";
        private const string SR_SKIP = "!srskip";
        private const string SR_TIME = "!srtime";
        #endregion

        #region Constructors
        public LibVLCSharpPlayerFeature(IrcClient irc, TwitchBotConfigurationSection botConfig, Configuration appConfig,
            LibVLCSharpPlayer libVLCSharpPlayer) : base(irc, botConfig)
        {
            _libVLCSharpPlayer = libVLCSharpPlayer;
            _appConfig = appConfig;
            _rolePermissions.Add(SR_START, new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermissions.Add(SR_STOP, new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermissions.Add(SR_PAUSE, new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermissions.Add(SR_AOD, new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermissions.Add(SR_SHUFFLE, new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermissions.Add(SR_PLAY, new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermissions.Add(SR_VOLUME, new CommandPermission { General = ChatterType.Viewer, Elevated = ChatterType.Moderator });
            _rolePermissions.Add(SR_SKIP, new CommandPermission { General = ChatterType.Moderator });
            _rolePermissions.Add(SR_TIME, new CommandPermission { General = ChatterType.Viewer, Elevated = ChatterType.Moderator });
        }
        #endregion

        public override async Task<(bool, DateTime)> ExecCommandAsync(TwitchChatter chatter, string requestedCommand)
        {
            try
            {
                switch (requestedCommand)
                {
                    case SR_START:
                        return (true, await StartAsync());
                    case SR_STOP:
                        return (true, await StopAsync());
                    case SR_PAUSE:
                        return (true, await PauseAsync());
                    case SR_AOD:
                        return (true, await SetAudioOutputDeviceAsync(chatter));
                    case SR_SHUFFLE:
                        return (true, await PersonalPlaylistShuffleAsync(chatter));
                    case SR_PLAY:
                        return (true, await PlayAsync());
                    case SR_VOLUME:
                        if (chatter.Message.StartsWith($"{SR_VOLUME} ") && HasPermission(SR_VOLUME, DetermineChatterPermissions(chatter), _rolePermissions, true))
                        {
                            return (true, await SetVolumeAsync(chatter));
                        }
                        else if (chatter.Message == SR_VOLUME)
                        {
                            return (true, await ShowVolumeAsync(chatter));
                        }
                        break;
                    case SR_SKIP:
                        return (true, await SkipAsync(chatter));
                    case SR_TIME:
                        if (chatter.Message.StartsWith($"{SR_TIME} ") && HasPermission(SR_TIME, DetermineChatterPermissions(chatter), _rolePermissions, true))
                        {
                            return (true, await SetTimeAsync(chatter));
                        }
                        else if (chatter.Message == SR_TIME)
                        {
                            return (true, await ShowTimeAsync(chatter));
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "LibVLCSharpPlayerFeature", "ExecCommandAsync(TwitchChatter, string)", false, requestedCommand, chatter.Message);
            }

            return (false, DateTime.Now);
        }

        #region Private Methods
        private async Task<DateTime> StartAsync()
        {
            try
            {
                await _libVLCSharpPlayer.StartAsync();
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "LibVLCSharpPlayerFeature", "StartAsync()", false, SR_START);
            }

            return DateTime.Now;
        }

        private async Task<DateTime> StopAsync()
        {
            try
            {
                _libVLCSharpPlayer.Stop();
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "LibVLCSharpPlayerFeature", "StopAsync()", false, SR_STOP);
            }

            return DateTime.Now;
        }

        private async Task<DateTime> PauseAsync()
        {
            try
            {
                if (_libVLCSharpPlayer.MediaPlayerStatus() == VLCState.Paused && _libVLCSharpPlayer.MediaPlayerStatus() == VLCState.Stopped)
                {
                    _irc.SendPublicChatMessage($"This media player is already {await _libVLCSharpPlayer.GetVideoTimeAsync()} @{_botConfig.Broadcaster}");
                    return DateTime.Now;
                }

                _libVLCSharpPlayer.Pause();
                _irc.SendPublicChatMessage($"Paused current song request @{_botConfig.Broadcaster}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "LibVLCSharpPlayerFeature", "PauseAsync()", false, SR_PAUSE);
            }

            return DateTime.Now;
        }

        private async Task<DateTime> SetAudioOutputDeviceAsync(TwitchChatter chatter)
        {
            try
            {
                string audioOutputDevice = chatter.Message.Substring(chatter.Message.IndexOf(" ") + 1);

                _irc.SendPublicChatMessage($"{await _libVLCSharpPlayer.SetAudioOutputDeviceAsync(audioOutputDevice)} @{_botConfig.Broadcaster}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "LibVLCSharpPlayerFeature", "SetAudioOutputDeviceAsync(TwitchChatter)", false, SR_AOD);
            }

            return DateTime.Now;
        }

        private async Task<DateTime> PersonalPlaylistShuffleAsync(TwitchChatter chatter)
        {
            try
            {
                string message = chatter.Message.Substring(chatter.Message.IndexOf(" ") + 1);
                bool shuffle = SetBooleanFromMessage(message);

                if (_botConfig.EnablePersonalPlaylistShuffle == shuffle)
                {
                    if (shuffle)
                        _irc.SendPublicChatMessage($"Your personal playlist has already been shuffled @{_botConfig.Broadcaster}");
                    else
                        _irc.SendPublicChatMessage($"Your personal playlist is already in order @{_botConfig.Broadcaster}");
                }
                else
                {
                    await _libVLCSharpPlayer.SetPersonalPlaylistShuffleAsync(shuffle);

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
                await _errHndlrInstance.LogErrorAsync(ex, "LibVLCSharpPlayerFeature", "PersonalPlaylistShuffleAsync(TwitchChatter)", false, SR_SHUFFLE);
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Play the video
        /// </summary>
        /// <returns></returns>
        private async Task<DateTime> PlayAsync()
        {
            try
            {
                if (_libVLCSharpPlayer.MediaPlayerStatus() == VLCState.Playing)
                {
                    _irc.SendPublicChatMessage($"This media player is already {await _libVLCSharpPlayer.GetVideoTimeAsync()} @{_botConfig.Broadcaster}");
                    return DateTime.Now;
                }

                await _libVLCSharpPlayer.PlayAsync();

                PlaylistItem playlistItem = _libVLCSharpPlayer.CurrentSongRequestPlaylistItem;

                string songRequest = _youTubeClientInstance.ShowPlayingSongRequest(playlistItem);

                if (!string.IsNullOrEmpty(songRequest))
                    _irc.SendPublicChatMessage($"@{_botConfig.Broadcaster} <-- Now playing: {songRequest} Currently {await _libVLCSharpPlayer.GetVideoTimeAsync()}");
                else
                    _irc.SendPublicChatMessage($"Unable to display the current song @{_botConfig.Broadcaster}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "LibVLCSharpPlayerFeature", "PlayAsync()", false, SR_PLAY);
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Set the volume for the player
        /// </summary>
        /// <param name="chatter"></param>
        /// <returns></returns>
        private async Task<DateTime> SetVolumeAsync(TwitchChatter chatter)
        {
            try
            {
                bool validMessage = int.TryParse(chatter.Message.AsSpan(chatter.Message.IndexOf(" ") + 1), out int volumePercentage);

                if (validMessage)
                {
                    if (await _libVLCSharpPlayer.SetVolumeAsync(volumePercentage))
                        _irc.SendPublicChatMessage($"Song request volume set to {volumePercentage}% @{chatter.DisplayName}");
                    else if (volumePercentage < 1 || volumePercentage > 100)
                        _irc.SendPublicChatMessage($"Please use a value for the song request volume from 1-100 @{chatter.DisplayName}");
                }
                else
                {
                    _irc.SendPublicChatMessage($"Requested volume not valid. Please set the song request volume from 1-100 @{chatter.DisplayName}");
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "LibVLCSharpPlayerFeature", "SetVolumeAsync(TwitchChatter)", false, SR_VOLUME, chatter.Message);
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Skip to the next video in the queue
        /// </summary>
        /// <param name="chatter"></param>
        /// <returns></returns>
        private async Task<DateTime> SkipAsync(TwitchChatter chatter)
        {
            try
            {
                bool validMessage = int.TryParse(chatter.Message.AsSpan(chatter.Message.IndexOf(" ") + 1), out int songSkipCount);

                if (!validMessage)
                    await _libVLCSharpPlayer.SkipAsync();
                else
                    await _libVLCSharpPlayer.SkipAsync(songSkipCount);

                string songRequest = _youTubeClientInstance.ShowPlayingSongRequest(_libVLCSharpPlayer.CurrentSongRequestPlaylistItem);

                if (!string.IsNullOrEmpty(songRequest))
                    _irc.SendPublicChatMessage($"@{chatter.DisplayName} <-- Now playing: {songRequest}");
                else
                    _irc.SendPublicChatMessage($"Unable to display the current song @{chatter.DisplayName}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "LibVLCSharpPlayerFeature", "SkipAsync(TwitchChatter)", false, SR_SKIP, chatter.Message);
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Seek to a specific time in the video
        /// </summary>
        /// <param name="chatter"></param>
        /// <returns></returns>
        private async Task<DateTime> SetTimeAsync(TwitchChatter chatter)
        {
            try
            {
                bool validMessage = int.TryParse(chatter.Message.AsSpan(chatter.Message.IndexOf(" ") + 1), out int seekVideoTime);

                if (validMessage && await _libVLCSharpPlayer.SetVideoTimeAsync(seekVideoTime))
                    _irc.SendPublicChatMessage($"Video seek time set to {seekVideoTime} second(s) @{chatter.DisplayName}");
                else
                    _irc.SendPublicChatMessage($"Time not valid. Please set the time (in seconds) between 0 and the length of the video @{chatter.DisplayName}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "LibVLCSharpPlayerFeature", "SetTimeAsync(TwitchChatter)", false, SR_TIME, chatter.Message);
            }

            return DateTime.Now;
        }

        private async Task<DateTime> ShowVolumeAsync(TwitchChatter chatter)
        {
            try
            {
                _irc.SendPublicChatMessage($"Song request volume is currently at {await _libVLCSharpPlayer.GetVolumeAsync()}% @{chatter.DisplayName}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "LibVLCSharpPlayerFeature", "ShowVolumeAsync(TwitchChatter)", false, SR_VOLUME);
            }

            return DateTime.Now;
        }

        private async Task<DateTime> ShowTimeAsync(TwitchChatter chatter)
        {
            try
            {
                _irc.SendPublicChatMessage($"Currently {await _libVLCSharpPlayer.GetVideoTimeAsync()} @{chatter.DisplayName}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "LibVLCSharpPlayerFeature", "ShowTimeAsync(TwitchChatter)", false, SR_TIME);
            }

            return DateTime.Now;
        }
        #endregion
    }
}
