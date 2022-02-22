using System;
using System.Configuration;
using System.Threading.Tasks;

using TwitchBotShared.ClientLibraries;
using TwitchBotShared.ClientLibraries.Singletons;
using TwitchBotShared.Config;
using TwitchBotShared.Enums;
using TwitchBotShared.Models;
using TwitchBotShared.Threads;

namespace TwitchBotShared.Commands.Features
{
    /// <summary>
    /// The "Command Subsystem" for the "Twitter" feature
    /// </summary>
    public sealed class TwitterFeature : BaseFeature
    {
        private readonly Configuration _appConfig;
        private readonly DelayedMessageSingleton _delayedMessagesInstance = DelayedMessageSingleton.Instance;
        private readonly TwitterClient _twitterInstance = TwitterClient.Instance;
        private readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        private const string AUTO_TWEET = "!autotweet";
        private const string TWEET = "!tweet";
        private const string LIVE = "!live";
        private const string TWITTER = "!twitter";

        public TwitterFeature(IrcClient irc, TwitchBotConfigurationSection botConfig, Configuration appConfig) : base(irc, botConfig)
        {
            _appConfig = appConfig;
            _rolePermissions.Add(AUTO_TWEET, new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermissions.Add(TWEET, new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermissions.Add(LIVE, new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermissions.Add(TWITTER, new CommandPermission { General = ChatterType.Viewer });
        }

        public override async Task<(bool, DateTime)> ExecCommandAsync(TwitchChatter chatter, string requestedCommand)
        {
            try
            {
                switch (requestedCommand)
                {
                    case AUTO_TWEET:
                        return (true, await SetAutoTweetAsync(chatter));
                    case TWEET:
                        return (true, await TweetAsync(chatter));
                    case LIVE:
                        return (true, await LiveAsync());
                    case TWITTER:
                        return (true, await TwitterLinkAsync());
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "TwitterFeature", "ExecCommandAsync(TwitchChatter, string)", false, requestedCommand, chatter.Message);
            }

            return (false, DateTime.Now);
        }

        /// <summary>
        /// Enables tweets to be sent out from this bot (both auto publish tweets and manual tweets)
        /// </summary>
        /// <param name="chatter"></param>
        private async Task<DateTime> SetAutoTweetAsync(TwitchChatter chatter)
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
                await _errHndlrInstance.LogErrorAsync(ex, "TwitterFeature", "SetAutoTweetAsync(TwitchChatter)", false, AUTO_TWEET);
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Manually send a tweet
        /// </summary>
        /// <param name="chatter"></param>
        private async Task<DateTime> TweetAsync(TwitchChatter chatter)
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
                await _errHndlrInstance.LogErrorAsync(ex, "TwitterFeature", "TweetAsync(TwitchChatter)", false, TWEET);
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Tweet that the stream is live on the broadcaster's behalf
        /// </summary>
        /// <returns></returns>
        private async Task<DateTime> LiveAsync()
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

                    // clear reminder
                    _delayedMessagesInstance.DelayedMessages.RemoveAll(m => m.Message == $"Did you remind Twitter you're \"!live\"? @{_botConfig.Broadcaster}");

                    _irc.SendPublicChatMessage($"{tweetResult} @{_botConfig.Broadcaster}");
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "TwitterFeature", "LiveAsync()", false, LIVE);
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Post the broadcaster's Twitter link if given permission
        /// </summary>
        /// <returns></returns>
        private async Task<DateTime> TwitterLinkAsync()
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
                await _errHndlrInstance.LogErrorAsync(ex, "TwitterFeature", "TwitterLinkAsync()", false, TWITTER);
            }

            return DateTime.Now;
        }
    }
}
