using System;
using System.Configuration;

using TwitchBot.Configuration;
using TwitchBot.Libraries;

namespace TwitchBot.Commands
{
    public class CmdBrdCstr
    {
        private IrcClient _irc;
        private System.Configuration.Configuration _appConfig;
        private TwitchBotConfigurationSection _botConfig;
        private ErrorHandler _errHndlrInstance = ErrorHandler.Instance;   

        public CmdBrdCstr(IrcClient irc, TwitchBotConfigurationSection botConfig, System.Configuration.Configuration appConfig)
        {
            _irc = irc;
            _botConfig = botConfig;
            _appConfig = appConfig;
        }

        /// <summary>
        /// Display bot settings
        /// </summary>
        public async void CmdBotSettings()
        {
            try
            {
                _irc.SendPublicChatMessage($"Auto tweets set to \"{_botConfig.EnableTweets}\" "
                    + $">< Auto display songs set to \"{_botConfig.EnableDisplaySong}\" "
                    + $">< Currency set to \"{_botConfig.CurrencyType}\" "
                    + $">< Stream Latency set to \"{_botConfig.StreamLatency} second(s)\" "
                    + $">< Regular follower hours set to \"{_botConfig.RegularFollowerHours}\"");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdBotSettings()", false, "!settings");
            }
        }

        /// <summary>
        /// Stop running the bot
        /// </summary>
        public async void CmdExitBot()
        {
            try
            {
                _irc.SendPublicChatMessage("Bye! Have a beautiful time!");
                Environment.Exit(0); // exit program
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdExitBot()", false, "!exit");
            }
        }

        

        public async void CmdSetRegularFollowerHours(string message)
        {
            try
            {
                bool validInput = int.TryParse(message.Substring(17), out int regularHours);
                if (!validInput)
                {
                    _irc.SendPublicChatMessage($"I can't process the time you've entered. " + 
                        $"Please insert positive hours @{_botConfig.Broadcaster}");
                    return;
                }
                else if (regularHours < 1)
                {
                    _irc.SendPublicChatMessage($"Please insert positive hours @{_botConfig.Broadcaster}");
                    return;
                }

                _botConfig.RegularFollowerHours = regularHours;
                _appConfig.AppSettings.Settings.Remove("regularFollowerHours");
                _appConfig.AppSettings.Settings.Add("regularFollowerHours", regularHours.ToString());
                _appConfig.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("TwitchBotConfiguration");

                Console.WriteLine($"Regular followers are set to {_botConfig.RegularFollowerHours}");
                _irc.SendPublicChatMessage($"{_botConfig.Broadcaster} : Regular followers now need {_botConfig.RegularFollowerHours} hours");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdSetRegularFollowerHours(string)", false, "!setregularhours");
            }
        }
    }
}
