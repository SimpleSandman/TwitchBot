using System;
using System.Globalization;
using System.Threading.Tasks;

using TwitchBot.Configuration;
using TwitchBot.Libraries;
using TwitchBot.Models;
using TwitchBot.Models.JSON;
using TwitchBot.Services;

namespace TwitchBot.Commands.Features
{
    /// <summary>
    /// The "Command Subsystem" for the "General Bot" feature
    /// </summary>
    public sealed class GeneralFeature : BaseFeature
    {
        private readonly TwitchInfoService _twitchInfo;
        private readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        public GeneralFeature(IrcClient irc, TwitchBotConfigurationSection botConfig, TwitchInfoService twitchInfo) : base(irc, botConfig)
        {
            _twitchInfo = twitchInfo;
            _rolePermission.Add("!settings", "broadcaster");
            _rolePermission.Add("!exit", "broadcaster");
        }

        public override async void ExecCommand(TwitchChatter chatter, string requestedCommand)
        {
            try
            {
                switch (requestedCommand)
                {
                    case "!settings":
                        BotSettings();
                        break;
                    case "!exit":
                        ExitBot();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "GeneralFeature", "ExecCommand(TwitchChatter, string)", false, requestedCommand, chatter.Message);
            }
        }

        /// <summary>
        /// Display bot settings
        /// </summary>
        public async void BotSettings()
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
                await _errHndlrInstance.LogError(ex, "GeneralFeature", "BotSettings()", false, "!settings");
            }
        }

        /// <summary>
        /// Stop running the bot
        /// </summary>
        public async void ExitBot()
        {
            try
            {
                _irc.SendPublicChatMessage("Bye! Have a beautiful time!");
                Environment.Exit(0); // exit program
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "GeneralFeature", "ExitBot()", false, "!exit");
            }
        }

        public async void Displays()
        {
            try
            {
                _irc.SendPublicChatMessage("---> !hello, !slap @[username], !stab @[username], !throw [item] @[username], !shoot @[username], "
                    + "!sr [youtube link/search], !ytsl, !partyup [party member name], !gamble [money], !join, "
                    + $"!quote, !8ball [question], !{_botConfig.CurrencyType.ToLower()} (check stream currency) <---"
                    + " Link to full list of commands: http://bit.ly/2bXLlEe");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "GeneralFeature", "Displays()", false, "!cmds");
            }
        }

        public async void Hello(TwitchChatter chatter)
        {
            try
            {
                _irc.SendPublicChatMessage($"Hey @{chatter.DisplayName}! Thanks for talking to me :) "
                    + $"I'll let @{_botConfig.Broadcaster.ToLower()} know you're here!");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "GeneralFeature", "Hello(TwitchChatter)", false, "!hello");
            }
        }

        public async void UtcTime()
        {
            try
            {
                _irc.SendPublicChatMessage($"UTC Time: {DateTime.UtcNow}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "GeneralFeature", "UtcTime()", false, "!utctime");
            }
        }

        public async void HostTime()
        {
            try
            {
                string response = $"{_botConfig.Broadcaster}'s Current Time: {DateTime.Now} ";

                if (DateTime.Now.IsDaylightSavingTime())
                    response += $"({TimeZone.CurrentTimeZone.DaylightName})";
                else
                    response += $"({TimeZone.CurrentTimeZone.StandardName})";

                _irc.SendPublicChatMessage(response);
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "GeneralFeature", "HostTime()", false, "!hosttime");
            }
        }

        public async Task Uptime()
        {
            try
            {
                RootStreamJSON streamJson = await _twitchInfo.GetBroadcasterStream();

                // Check if the channel is live
                if (streamJson.Stream != null)
                {
                    string duration = streamJson.Stream.CreatedAt;
                    TimeSpan ts = DateTime.UtcNow - DateTime.Parse(duration, new DateTimeFormatInfo(), DateTimeStyles.AdjustToUniversal);
                    string resultDuration = string.Format("{0:h\\:mm\\:ss}", ts);
                    _irc.SendPublicChatMessage($"This channel's current uptime (length of current stream) is {resultDuration}");
                }
                else
                    _irc.SendPublicChatMessage("This channel is not streaming right now");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "GeneralFeature", "Uptime()", false, "!uptime");
            }
        }
    }
}
