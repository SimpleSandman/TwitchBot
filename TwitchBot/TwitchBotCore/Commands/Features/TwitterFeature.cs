using System;
using System.Configuration;
using System.Threading.Tasks;

using TwitchBotConsoleApp.Libraries;
using TwitchBotConsoleApp.Models;
using TwitchBotConsoleApp.Threads;

using TwitchBotUtil.Config;
using TwitchBotUtil.Enums;

namespace TwitchBotConsoleApp.Commands.Features
{
    /// <summary>
    /// The "Command Subsystem" for the "Twitter" feature
    /// </summary>
    public sealed class TwitterFeature : BaseFeature
    {
        private readonly TwitterClient _twitterInstance = TwitterClient.Instance;
        private readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;
        private readonly Configuration _appConfig;

        public TwitterFeature(IrcClient irc, TwitchBotConfigurationSection botConfig, Configuration appConfig) : base(irc, botConfig)
        {
            _appConfig = appConfig;
            _rolePermission.Add("!autotweet", new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermission.Add("!tweet", new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermission.Add("!live", new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermission.Add("!twitter", new CommandPermission { General = ChatterType.Viewer });
        }

        public override async Task<(bool, DateTime)> ExecCommand(TwitchChatter chatter, string requestedCommand)
        {
            try
            {
                switch (requestedCommand)
                {
                    case "!autotweet":
                        return (true, await SetAutoTweet(chatter));
                    case "!tweet":
                        return (true, await Tweet(chatter));
                    case "!live":
                        return (true, await Live());
                    case "!twitter":
                        return (true, await TwitterLink());
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "TwitterFeature", "ExecCommand(TwitchChatter, string)", false, requestedCommand, chatter.Message);
            }

            return (false, DateTime.Now);
        }

        /// <summary>
        /// Enables tweets to be sent out from this bot (both auto publish tweets and manual tweets)
        /// </summary>
        public async Task<DateTime> SetAutoTweet(TwitchChatter chatter)
        {
            try
            {
                if (!_twitterInstance.HasCredentials)
                {
                    _irc.SendPublicChatMessage($"You are missing twitter info @{_botConfig.Broadcaster}");
                }
                else
                {
                    string message = ParseChatterCommandParameter(chatter);
                    bool enableTweets = SetBooleanFromMessage(message);
                    string boolValue = enableTweets ? "true" : "false";

                    _botConfig.EnableTweets = true;
                    SaveAppConfigSettings(boolValue, "enableTweets", _appConfig);

                    _irc.SendPublicChatMessage($"@{_botConfig.Broadcaster} : Automatic tweets is set to \"{_botConfig.EnableTweets}\"");
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "TwitterFeature", "EnableTweet()", false, "!sendtweet on");
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Manually send a tweet
        /// </summary>
        /// <param name="message">Chat message from the user</param>
        public async Task<DateTime> Tweet(TwitchChatter chatter)
        {
            try
            {
                if (!_twitterInstance.HasCredentials)
                    _irc.SendPublicChatMessage($"You are missing twitter info @{_botConfig.Broadcaster}");
                else
                    _irc.SendPublicChatMessage(_twitterInstance.SendTweet(chatter.Message.Replace("!tweet ", "")));
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "TwitterFeature", "Tweet(TwitchChatter)", false, "!tweet");
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Tweet that the stream is live on the broadcaster's behalf
        /// </summary>
        /// <returns></returns>
        public async Task<DateTime> Live()
        {
            try
            {
                if (!TwitchStreamStatus.IsLive)
                    _irc.SendPublicChatMessage("This channel is not streaming right now");
                else if (!_botConfig.EnableTweets)
                    _irc.SendPublicChatMessage("Tweets are disabled at the moment");
                else if (string.IsNullOrEmpty(TwitchStreamStatus.CurrentCategory) || string.IsNullOrEmpty(TwitchStreamStatus.CurrentTitle))
                    _irc.SendPublicChatMessage("Unable to pull the Twitch title/category at the moment. Please try again in a few seconds");
                else if (_twitterInstance.HasCredentials)
                {
                    string tweetResult = _twitterInstance.SendTweet($"Live on Twitch playing {TwitchStreamStatus.CurrentCategory} "
                        + $"\"{TwitchStreamStatus.CurrentTitle}\" twitch.tv/{_botConfig.Broadcaster}");

                    _irc.SendPublicChatMessage($"{tweetResult} @{_botConfig.Broadcaster}");
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "TwitterFeature", "Live()", false, "!live");
            }

            return DateTime.Now;
        }

        public async Task<DateTime> TwitterLink()
        {
            try
            {
                if (!_twitterInstance.HasCredentials)
                    _irc.SendPublicChatMessage($"Twitter username not found @{_botConfig.Broadcaster}");
                else if (string.IsNullOrEmpty(_twitterInstance.ScreenName))
                    _irc.SendPublicChatMessage("I'm sorry. I'm unable to get this broadcaster's Twitter handle/screen name");
                else
                    _irc.SendPublicChatMessage($"Check out this broadcaster's twitter at https://twitter.com/" + _twitterInstance.ScreenName);
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "Gen", "TwitterLink(bool, string)", false, "!twitter");
            }

            return DateTime.Now;
        }
    }
}
