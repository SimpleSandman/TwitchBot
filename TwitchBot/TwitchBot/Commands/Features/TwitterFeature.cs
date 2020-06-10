using System;
using System.Configuration;
using System.Threading.Tasks;

using TwitchBot.Configuration;
using TwitchBot.Libraries;
using TwitchBot.Models;
using TwitchBot.Threads;

namespace TwitchBot.Commands.Features
{
    /// <summary>
    /// The "Command Subsystem" for the "____" feature
    /// </summary>
    public sealed class TwitterFeature : BaseFeature
    {
        private readonly TwitterClient _twitter = TwitterClient.Instance;
        private readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;
        private readonly System.Configuration.Configuration _appConfig;
        private readonly bool _hasTwitterInfo;

        public TwitterFeature(IrcClient irc, TwitchBotConfigurationSection botConfig, System.Configuration.Configuration appConfig, 
            bool hasTwitterInfo) : base(irc, botConfig)
        {
            _appConfig = appConfig;
            _hasTwitterInfo = hasTwitterInfo;
            _rolePermission.Add("!sendtweet", "broadcaster");
            _rolePermission.Add("!tweet", "broadcaster");
            _rolePermission.Add("!live", "broadcaster");
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
                    case "!tweet":
                        Tweet(chatter);
                        break;
                    case "!live":
                        await Live();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "TwitterFeature", "ExecCommand(TwitchChatter, string)", false, requestedCommand, chatter.Message);
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
                    _irc.SendPublicChatMessage($"You are missing twitter info @{_botConfig.Broadcaster}");
                else
                {
                    _botConfig.EnableTweets = true;
                    _appConfig.AppSettings.Settings.Remove("enableTweets");
                    _appConfig.AppSettings.Settings.Add("enableTweets", "true");
                    _appConfig.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection("TwitchBotConfiguration");

                    _irc.SendPublicChatMessage($"@{_botConfig.Broadcaster} : Automatic tweets is set to \"{_botConfig.EnableTweets}\"");
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "TwitterFeature", "EnableTweet()", false, "!sendtweet on");
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
                    _irc.SendPublicChatMessage($"You are missing twitter info @{_botConfig.Broadcaster}");
                else
                {
                    _botConfig.EnableTweets = false;
                    _appConfig.AppSettings.Settings.Remove("enableTweets");
                    _appConfig.AppSettings.Settings.Add("enableTweets", "false");
                    _appConfig.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection("TwitchBotConfiguration");

                    _irc.SendPublicChatMessage($"@{_botConfig.Broadcaster} : Automatic tweets is set to \"{_botConfig.EnableTweets}\"");
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "TwitterFeature", "DisableTweet()", false, "!sendtweet off");
            }
        }

        /// <summary>
        /// Manually send a tweet
        /// </summary>
        /// <param name="message">Chat message from the user</param>
        public async void Tweet(TwitchChatter chatter)
        {
            try
            {
                if (!_hasTwitterInfo)
                    _irc.SendPublicChatMessage($"You are missing twitter info @{_botConfig.Broadcaster}");
                else
                    _irc.SendPublicChatMessage(_twitter.SendTweet(chatter.Message.Replace("!tweet ", "")));
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "TwitterFeature", "Tweet(TwitchChatter)", false, "!tweet");
            }
        }

        /// <summary>
        /// Tweet when stream is live
        /// </summary>
        /// <returns></returns>
        public async Task Live()
        {
            try
            {
                if (!TwitchStreamStatus.IsLive)
                    _irc.SendPublicChatMessage("This channel is not streaming right now");
                else if (!_botConfig.EnableTweets)
                    _irc.SendPublicChatMessage("Tweets are disabled at the moment");
                else if (string.IsNullOrEmpty(TwitchStreamStatus.CurrentCategory) || string.IsNullOrEmpty(TwitchStreamStatus.CurrentTitle))
                    _irc.SendPublicChatMessage("Unable to pull the Twitch title/category at the moment. Please try again in a few seconds");
                else if (_botConfig.EnableTweets && _hasTwitterInfo)
                {
                    string tweetResult = _twitter.SendTweet($"Live on Twitch playing {TwitchStreamStatus.CurrentCategory} "
                        + $"\"{TwitchStreamStatus.CurrentTitle}\" twitch.tv/{_botConfig.Broadcaster}");

                    _irc.SendPublicChatMessage($"{tweetResult} @{_botConfig.Broadcaster}");
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "TwitterFeature", "Live()", false, "!live");
            }
        }
    }
}
