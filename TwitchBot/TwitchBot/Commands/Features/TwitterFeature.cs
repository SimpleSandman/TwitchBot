using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

using TwitchBot.Configuration;
using TwitchBot.Enums;
using TwitchBot.Libraries;
using TwitchBot.Models;
using TwitchBot.Services;

using TwitchBotDb.DTO;

using TwitchBotUtil.Extensions;

namespace TwitchBot.Commands.Features
{
    /// <summary>
    /// The "Command Subsystem" for the "____" feature
    /// </summary>
    public sealed class TwitterFeature : BaseFeature
    {
        private readonly BroadcasterSingleton _broadcasterInstance = BroadcasterSingleton.Instance;
        private readonly TwitchChatterList _twitchChatterListInstance = TwitchChatterList.Instance;
        private readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;
        private readonly System.Configuration.Configuration _appConfig;
        private readonly bool _hasTwitterInfo;

        public TwitterFeature(IrcClient irc, TwitchBotConfigurationSection botConfig, System.Configuration.Configuration appConfig, 
            bool hasTwitterInfo) : base(irc, botConfig)
        {
            _appConfig = appConfig;
            _hasTwitterInfo = hasTwitterInfo;
            _rolePermission.Add("!", "");
            _rolePermission.Add("!", "");
        }

        public override async void ExecCommand(TwitchChatter chatter, string requestedCommand)
        {
            try
            {
                switch (requestedCommand)
                {
                    case "!sendtweet on":
                        EnableTweet();
                        break;
                    case "!sendtweet off":
                        DisableTweet();
                        break;
                    default:
                        if (requestedCommand == "!")
                        {
                            //await OtherCoolThings(chatter);
                            break;
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "TwitterFeature", "ExecCommand(TwitchChatter, string)", false);
            }
        }

        /// <summary>
        /// Enables tweets to be sent out from this bot (both auto publish tweets and manual tweets)
        /// </summary>
        public async void EnableTweet()
        {
            try
            {
                if (!_hasTwitterInfo)
                    _irc.SendPublicChatMessage("You are missing twitter info @" + _botConfig.Broadcaster);
                else
                {
                    _botConfig.EnableTweets = true;
                    _appConfig.AppSettings.Settings.Remove("enableTweets");
                    _appConfig.AppSettings.Settings.Add("enableTweets", "true");
                    _appConfig.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection("TwitchBotConfiguration");

                    Console.WriteLine("Auto publish tweets is set to [" + _botConfig.EnableTweets + "]");
                    _irc.SendPublicChatMessage(_botConfig.Broadcaster + ": Automatic tweets is set to \"" + _botConfig.EnableTweets + "\"");
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdEnableTweet()", false, "!sendtweet on");
            }
        }

        /// <summary>
        /// Disables tweets to be sent out from this bot (both auto publish tweets and manual tweets)
        /// </summary>
        public async void DisableTweet()
        {
            try
            {
                if (!_hasTwitterInfo)
                    _irc.SendPublicChatMessage("You are missing twitter info @" + _botConfig.Broadcaster);
                else
                {
                    _botConfig.EnableTweets = false;
                    _appConfig.AppSettings.Settings.Remove("enableTweets");
                    _appConfig.AppSettings.Settings.Add("enableTweets", "false");
                    _appConfig.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection("TwitchBotConfiguration");

                    Console.WriteLine("Auto publish tweets is set to [" + _botConfig.EnableTweets + "]");
                    _irc.SendPublicChatMessage(_botConfig.Broadcaster + ": Automatic tweets is set to \"" + _botConfig.EnableTweets + "\"");
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdDisableTweet()", false, "!sendtweet off");
            }
        }
    }
}
