using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

using TwitchBot.Configuration;
using TwitchBot.Libraries;
using TwitchBot.Services;
using TwitchBot.Models;

using TwitchBotDb.Models;

using TwitchBotUtil.Extensions;

namespace TwitchBot.Commands
{
    public class CmdMod
    {
        private IrcClient _irc;
        private TimeoutCmd _timeout;
        private System.Configuration.Configuration _appConfig;
        private TwitchBotConfigurationSection _botConfig;
        private ManualSongRequestService _manualSongRequest;
        private ErrorHandler _errHndlrInstance = ErrorHandler.Instance;
        private BroadcasterSingleton _broadcasterInstance = BroadcasterSingleton.Instance;
        

        public CmdMod(IrcClient irc, TimeoutCmd timeout, TwitchBotConfigurationSection botConfig, System.Configuration.Configuration appConfig, 
            ManualSongRequestService manualSongRequest)
        {
            _irc = irc;
            _timeout = timeout;
            _botConfig = botConfig;
            _appConfig = appConfig;
            _manualSongRequest = manualSongRequest;
        }

        /// <summary>
        /// Resets the song request queue
        /// </summary>
        public async Task CmdResetManualSr()
        {
            try
            {
                List<SongRequest> removedSong = await _manualSongRequest.ResetSongRequests(_broadcasterInstance.DatabaseId);

                if (removedSong != null && removedSong.Count > 0)
                    _irc.SendPublicChatMessage($"The song request queue has been reset @{_botConfig.Broadcaster}");
                else
                    _irc.SendPublicChatMessage($"Song requests are empty @{_botConfig.Broadcaster}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdMod", "CmdResetManualSr()", false, "!resetrsr");
            }
        }

        /// <summary>
        /// Bot-specific timeout on a user for a set amount of time
        /// </summary>
        /// <param name="chatter"></param>
        public async Task CmdAddTimeout(TwitchChatter chatter)
        {
            try
            {
                if (chatter.Message.ToLower().Contains(_botConfig.Broadcaster.ToLower()))
                    _irc.SendPublicChatMessage($"I cannot betray @{_botConfig.Broadcaster} by not allowing him to communicate with me @{chatter.DisplayName}");
                else if (chatter.Message.ToLower().Contains(_botConfig.BotName.ToLower()))
                    _irc.SendPublicChatMessage($"You can't time me out @{chatter.DisplayName} PowerUpL Jebaited PowerUpR");
                else
                {
                    int recipientIndexAction = chatter.Message.IndexOf("@");
                    int cooldownAmountIndex = chatter.Message.IndexOf(" ");
                    string recipient = chatter.Message.Substring(recipientIndexAction + 1).ToLower();

                    double seconds = -1.0;
                    bool isValidTimeout = false;
                    bool isPermanentTimeout = false;

                    // if cooldown is valid, create the timeout
                    if (cooldownAmountIndex > 0 && chatter.Message.GetNthCharIndex(' ', 2) > 0)
                    {
                        isValidTimeout = double.TryParse(chatter.Message.Substring(cooldownAmountIndex + 1, recipientIndexAction - cooldownAmountIndex - 2), out seconds);
                    }
                    else if (recipient.Length > 0)
                    {
                        isValidTimeout = true;
                        isPermanentTimeout = true;
                    }

                    if (!isValidTimeout || (isValidTimeout && !isPermanentTimeout && seconds < 15.00))
                    {
                        _irc.SendPublicChatMessage($"The timeout amount wasn't accepted. Please try again with at least 15 seconds @{chatter.DisplayName}");
                        return;
                    }

                    DateTime timeoutExpiration = await _timeout.AddTimeout(recipient, _broadcasterInstance.DatabaseId, _botConfig.TwitchBotApiLink, seconds);

                    // create the output
                    string response = $"I'm told not to talk to you until ";

                    if (timeoutExpiration != DateTime.MaxValue)
                    {
                        response += $"{timeoutExpiration.ToLocalTime()} ";

                        if (timeoutExpiration.ToLocalTime().IsDaylightSavingTime())
                            response += $"({TimeZone.CurrentTimeZone.DaylightName})";
                        else
                            response += $"({TimeZone.CurrentTimeZone.StandardName})";
                    }
                    else
                    {
                        response += "THE END OF TIME! DarkMode";
                    }

                    _irc.SendPublicChatMessage($"{response} @{recipient}");
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdMod", "CmdAddTimeout(TwitchChatter)", false, "!addtimeout");
            }
        }

        /// <summary>
        /// Remove bot-specific timeout on a user for a set amount of time
        /// </summary>
        /// <param name="chatter"></param>
        public async Task CmdDeleteTimeout(TwitchChatter chatter)
        {
            try
            {
                string recipient = chatter.Message.Substring(chatter.Message.IndexOf("@") + 1).ToLower();

                recipient = await _timeout.DeleteUserTimeout(recipient, _broadcasterInstance.DatabaseId, _botConfig.TwitchBotApiLink);

                if (!string.IsNullOrEmpty(recipient))
                    _irc.SendPublicChatMessage($"{recipient} can now interact with me again because of @{chatter.DisplayName} @{_botConfig.Broadcaster}");
                else
                    _irc.SendPublicChatMessage($"Cannot find the user you wish to timeout @{chatter.DisplayName}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdMod", "CmdDelTimeout(TwitchChatter)", false, "!deltimeout");
            }
        }

        /// <summary>
        /// Set delay for messages based on the latency of the stream
        /// </summary>
        /// <param name="chatter"></param>
        public async void CmdSetLatency(TwitchChatter chatter)
        {
            try
            {
                int latency = -1;
                bool isValidInput = int.TryParse(chatter.Message.Substring(chatter.Message.IndexOf(" ")), out latency);

                if (!isValidInput || latency < 0)
                    _irc.SendPublicChatMessage("Please insert a valid positive alloted amount of time (in seconds)");
                else
                {
                    _botConfig.StreamLatency = latency;
                    _appConfig.AppSettings.Settings.Remove("streamLatency");
                    _appConfig.AppSettings.Settings.Add("streamLatency", latency.ToString());
                    _appConfig.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection("TwitchBotConfiguration");

                    _irc.SendPublicChatMessage($"Bot settings for stream latency set to {_botConfig.StreamLatency} second(s) @{chatter.DisplayName}");
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdMod", "CmdSetLatency(TwitchChatter)", false, "!setlatency");
            }
        }

        /// <summary>
        /// Tell the stream the specified moderator will be AFK
        /// </summary>
        /// <param name="chatter"></param>
        public async void CmdModAfk(TwitchChatter chatter)
        {
            try
            {
                _irc.SendPublicChatMessage($"@{chatter.DisplayName} is going AFK @{_botConfig.Broadcaster}! SwiftRage");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdMod", "CmdModAfk(string)", false, "!modafk");
            }
        }

        /// <summary>
        /// Tell the stream the specified moderator is back
        /// </summary>
        /// <param name="chatter"></param>
        public async void CmdModBack(TwitchChatter chatter)
        {
            try
            {
                _irc.SendPublicChatMessage($"@{chatter.DisplayName} is back @{_botConfig.Broadcaster}! BlessRNG");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdMod", "CmdModBack(string)", false, "!modback");
            }
        }

        public async Task<Queue<string>> CmdResetJoin(TwitchChatter chatter, Queue<string> gameQueueUsers)
        {
            try
            {
                if (gameQueueUsers.Count != 0)
                    gameQueueUsers.Clear();

                _irc.SendPublicChatMessage($"Queue is empty @{chatter.DisplayName}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdMod", "CmdResetJoin(TwitchChatter, Queue<string>)", false, "!resetjoin");
            }

            return gameQueueUsers;
        }
    }
}
