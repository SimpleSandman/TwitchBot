using System;
using System.Configuration;
using System.Globalization;
using System.Threading.Tasks;

using TwitchBotShared.ClientLibraries;
using TwitchBotShared.ClientLibraries.Singletons;
using TwitchBotShared.Config;
using TwitchBotShared.Enums;
using TwitchBotShared.Models;
using TwitchBotShared.Models.JSON;

namespace TwitchBotShared.Commands.Features
{
    /// <summary>
    /// The "Command Subsystem" for the "General Bot" feature
    /// </summary>
    public sealed class GeneralFeature : BaseFeature
    {
        private readonly TwitchInfoService _twitchInfo;
        private readonly Configuration _appConfig;
        private readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        #region Private Constant Variables
        private const string SETTINGS = "!settings";
        private const string EXIT = "!exit";
        private const string STREAMER = "!streamer";
        private const string SO = "!so";
        private const string SHOUTOUT = "!shoutout";
        private const string CASTER = "!caster";
        private const string CMDS = "!cmds";
        private const string HELP = "!help";
        private const string COMMANDS = "!commands";
        private const string HELLO = "!hello";
        private const string HI = "!hi";
        private const string UTC_TIME = "!utctime";
        private const string HOST_TIME = "!hosttime";
        private const string UPTIME = "!uptime";
        private const string SET_LATENCY = "!setlatency";
        private const string LATENCY = "!latency";
        private const string SUPPORT = "!support";
        private const string BOT = "!bot";
        private const string LURK = "!lurk";
        private const string UNLURK = "!unlurk";
        private const string SUB = "!sub";
        private const string SUBSCRIBE = "!subscribe";
        private const string EIGHT_BALL = "!8ball";
        private const string SLAP = "!slap";
        private const string STAB = "!stab";
        private const string SHOOT = "!shoot";
        private const string THROW = "!throw";
        #endregion

        #region Constructor
        public GeneralFeature(IrcClient irc, TwitchBotConfigurationSection botConfig, TwitchInfoService twitchInfo,
            Configuration appConfig) : base(irc, botConfig)
        {
            _twitchInfo = twitchInfo;
            _appConfig = appConfig;
            _rolePermissions.Add(SETTINGS, new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermissions.Add(EXIT, new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermissions.Add(STREAMER, new CommandPermission { General = ChatterType.Viewer });
            _rolePermissions.Add(SO, new CommandPermission { General = ChatterType.VIP });
            _rolePermissions.Add(SHOUTOUT, new CommandPermission { General = ChatterType.Viewer });
            _rolePermissions.Add(CASTER, new CommandPermission { General = ChatterType.Viewer });
            _rolePermissions.Add(CMDS, new CommandPermission { General = ChatterType.Viewer });
            _rolePermissions.Add(HELP, new CommandPermission { General = ChatterType.Viewer });
            _rolePermissions.Add(COMMANDS, new CommandPermission { General = ChatterType.Viewer });
            _rolePermissions.Add(HELLO, new CommandPermission { General = ChatterType.Viewer });
            _rolePermissions.Add(HI, new CommandPermission { General = ChatterType.Viewer });
            _rolePermissions.Add(UTC_TIME, new CommandPermission { General = ChatterType.Viewer });
            _rolePermissions.Add(HOST_TIME, new CommandPermission { General = ChatterType.Viewer });
            _rolePermissions.Add(UPTIME, new CommandPermission { General = ChatterType.Viewer });
            _rolePermissions.Add(SET_LATENCY, new CommandPermission { General = ChatterType.Moderator });
            _rolePermissions.Add(LATENCY, new CommandPermission { General = ChatterType.Moderator });
            _rolePermissions.Add(SUPPORT, new CommandPermission { General = ChatterType.Viewer });
            _rolePermissions.Add(BOT, new CommandPermission { General = ChatterType.Viewer });
            _rolePermissions.Add(LURK, new CommandPermission { General = ChatterType.Viewer });
            _rolePermissions.Add(UNLURK, new CommandPermission { General = ChatterType.Viewer });
            _rolePermissions.Add(SUB, new CommandPermission { General = ChatterType.Viewer });
            _rolePermissions.Add(SUBSCRIBE, new CommandPermission { General = ChatterType.Viewer });
            _rolePermissions.Add(EIGHT_BALL, new CommandPermission { General = ChatterType.Viewer });
            _rolePermissions.Add(SLAP, new CommandPermission { General = ChatterType.Viewer });
            _rolePermissions.Add(STAB, new CommandPermission { General = ChatterType.Viewer });
            _rolePermissions.Add(SHOOT, new CommandPermission { General = ChatterType.Viewer });
            _rolePermissions.Add(THROW, new CommandPermission { General = ChatterType.Viewer });
        }
        #endregion

        public override async Task<(bool, DateTime)> ExecCommandAsync(TwitchChatter chatter, string requestedCommand)
        {
            try
            {
                switch (requestedCommand)
                {
                    case SETTINGS:
                        return (true, await BotSettingsAsync());
                    case EXIT:
                        return (true, await ExitBotAsync());
                    case STREAMER:
                    case SHOUTOUT:
                    case CASTER:
                    case SO:
                        return (true, await PromoteStreamerAsync(chatter));
                    case CMDS:
                    case HELP:
                    case COMMANDS:
                        return (true, await DisplayCommandsAsync());
                    case HELLO:
                    case HI:
                        return (true, await HelloAsync(chatter));
                    case UTC_TIME:
                        return (true, await UtcTimeAsync());
                    case HOST_TIME:
                        return (true, await HostTimeAsync());
                    case UPTIME:
                        return (true, await UptimeAsync());
                    case SET_LATENCY:
                    case LATENCY:
                        return (true, await SetLatencyAsync(chatter));
                    case SUPPORT:
                    case BOT:
                        return (true, await SupportAsync());
                    case LURK:
                        return (true, await LurkAsync(chatter));
                    case UNLURK:
                        return (true, await UnlurkAsync(chatter));
                    case SUB:
                    case SUBSCRIBE:
                        return (true, await SubscribeAsync());
                    case EIGHT_BALL:
                        return (true, await Magic8BallAsync(chatter));
                    case SLAP:
                        return (true, await SlapAsync(chatter));
                    case STAB:
                        return (true, await StabAsync(chatter));
                    case SHOOT:
                        return (true, await ShootAsync(chatter));
                    case THROW:
                        return (true, await ThrowAsync(chatter));
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "GeneralFeature", "ExecCommandAsync(TwitchChatter, string)", false, requestedCommand, chatter.Message);
            }

            return (false, DateTime.Now);
        }

        #region Private Methods
        /// <summary>
        /// Display bot settings
        /// </summary>
        private async Task<DateTime> BotSettingsAsync()
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
                await _errHndlrInstance.LogErrorAsync(ex, "GeneralFeature", "BotSettingsAsync()", false, SETTINGS);
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Stop running the bot
        /// </summary>
        private async Task<DateTime> ExitBotAsync()
        {
            try
            {
                _irc.SendPublicChatMessage("Bye! Have a beautiful time!");
                Environment.Exit(0); // exit program
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "GeneralFeature", "ExitBotAsync()", false, EXIT);
            }

            return DateTime.Now;
        }

        private async Task<DateTime> DisplayCommandsAsync()
        {
            try
            {
                _irc.SendPublicChatMessage($"---> {HELLO}, {SLAP} @[username], {STAB} @[username], {THROW} [item] @[username], {SHOOT} @[username], "
                    + "!sr [youtube link/search], !ytsl, !partyup [party member name], !gamble [money], !join, "
                    + $"!quote, {EIGHT_BALL} [question], !{_botConfig.CurrencyType.ToLower()} (check stream currency) <---"
                    + " Link to full list of commands: http://bit.ly/2bXLlEe");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "GeneralFeature", "DisplayCommandsAsync()", false, CMDS);
            }

            return DateTime.Now;
        }

        private async Task<DateTime> HelloAsync(TwitchChatter chatter)
        {
            try
            {
                _irc.SendPublicChatMessage($"Hey @{chatter.DisplayName}! Thanks for talking to me :) "
                    + $"I'll let @{_botConfig.Broadcaster.ToLower()} know you're here!");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "GeneralFeature", "HelloAsync(TwitchChatter)", false, HELLO);
            }

            return DateTime.Now;
        }

        private async Task<DateTime> UtcTimeAsync()
        {
            try
            {
                _irc.SendPublicChatMessage($"UTC Time: {DateTime.UtcNow}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "GeneralFeature", "UtcTimeAsync()", false, UTC_TIME);
            }

            return DateTime.Now;
        }

        private async Task<DateTime> HostTimeAsync()
        {
            try
            {
                string response = $"{_botConfig.Broadcaster}'s Current Time: {DateTime.Now} ";

                if (DateTime.Now.IsDaylightSavingTime())
                    response += $"({TimeZoneInfo.Local.DaylightName})";
                else
                    response += $"({TimeZoneInfo.Local.StandardName})";

                _irc.SendPublicChatMessage(response);
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "GeneralFeature", "HostTimeAsync()", false, HOST_TIME);
            }

            return DateTime.Now;
        }

        private async Task<DateTime> UptimeAsync()
        {
            try
            {
                StreamJSON streamJson = await _twitchInfo.GetBroadcasterStreamAsync();

                // Check if the channel is live
                if (streamJson != null)
                {
                    string duration = streamJson.StartedAt;
                    TimeSpan ts = DateTime.UtcNow - DateTime.Parse(duration, new DateTimeFormatInfo(), DateTimeStyles.AdjustToUniversal);
                    string resultDuration = string.Format("{0:h\\:mm\\:ss}", ts);
                    _irc.SendPublicChatMessage($"This channel's current uptime (length of current stream) is {resultDuration}");
                }
                else
                    _irc.SendPublicChatMessage("This channel is not streaming right now");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "GeneralFeature", "UptimeAsync()", false, UPTIME);
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Set delay for messages based on the latency of the stream
        /// </summary>
        /// <param name="chatter"></param>
        private async Task<DateTime> SetLatencyAsync(TwitchChatter chatter)
        {
            try
            {
                int latency = -1;
                bool isValidInput = int.TryParse(ParseChatterCommandParameter(chatter), out latency);

                if (!isValidInput || latency < 0)
                    _irc.SendPublicChatMessage("Please insert a valid positive alloted amount of time (in seconds)");
                else
                {
                    _botConfig.StreamLatency = latency;
                    SaveAppConfigSettings(latency.ToString(), "streamLatency", _appConfig);

                    _irc.SendPublicChatMessage($"Bot settings for stream latency set to {_botConfig.StreamLatency} second(s) @{chatter.DisplayName}");
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "GeneralFeature", "SetLatencyAsync(TwitchChatter)", false, SET_LATENCY);
            }

            return DateTime.Now;
        }

        private async Task<DateTime> SupportAsync()
        {
            try
            {
                _irc.SendPublicChatMessage("@Simple_Sandman is the source of all of my powers PowerUpL Jebaited PowerUpR "
                    + "Please check out his Twitch at https://twitch.tv/simple_sandman "
                    + "If you need any support, send him a direct message at his Twitter https://twitter.com/Simple_Sandman "
                    + "Also, if you want to help me with power leveling, check out the Github https://github.com/SimpleSandman/TwitchBot");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "GeneralFeature", "SupportAsync()", false, SUPPORT);
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Tell the streamer the user is lurking
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        /// <returns></returns>
        private async Task<DateTime> LurkAsync(TwitchChatter chatter)
        {
            try
            {
                _irc.SendPublicChatMessage($"Okay {chatter.DisplayName}! @{_botConfig.Broadcaster} will be waiting for you TPFufun");
                return DateTime.Now.AddMinutes(5);
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "GeneralFeature", "LurkAsync(TwitchChatter)", false, LURK);
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Tell the streamer the user is back from lurking
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        /// <returns></returns>
        private async Task<DateTime> UnlurkAsync(TwitchChatter chatter)
        {
            try
            {
                _irc.SendPublicChatMessage($"Welcome back {chatter.DisplayName}! KonCha I'll let @{_botConfig.Broadcaster} know you're here!");
                return DateTime.Now.AddMinutes(5);
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "GeneralFeature", "UnlurkAsync(TwitchChatter)", false, UNLURK);
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Display the broadcaster's subscriber link (if they're an Affiliate/Partner)
        /// </summary>
        /// <returns></returns>
        private async Task<DateTime> SubscribeAsync()
        {
            try
            {
                // Get broadcaster type and check if they can have subscribers
                UserJSON json = await _twitchInfo.GetUserByLoginNameAsync(_botConfig.Broadcaster);
                string broadcasterType = json.BroadcasterType;

                if (broadcasterType == "partner" || broadcasterType == "affiliate")
                {
                    _irc.SendPublicChatMessage("Subscribe here! https://www.twitch.tv/subs/" + _botConfig.Broadcaster);
                }
                else
                {
                    _irc.SendPublicChatMessage($"{_botConfig.Broadcaster} is not a Twitch Affiliate/Partner. "
                        + "Please stick around and make their dream not a meme BlessRNG");
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "GeneralFeature", "SubscribeAsync()", false, SUB);
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Ask any question and the Magic 8 Ball will give a fortune
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        private async Task<DateTime> Magic8BallAsync(TwitchChatter chatter)
        {
            try
            {
                Random rnd = new Random(DateTime.Now.Millisecond);
                int answerId = rnd.Next(20); // between 0 and 19

                string[] possibleAnswers = new string[20]
                {
                    $"It is certain @{chatter.DisplayName}",
                    $"It is decidedly so @{chatter.DisplayName}",
                    $"Without a doubt @{chatter.DisplayName}",
                    $"Yes definitely @{chatter.DisplayName}",
                    $"You may rely on it @{chatter.DisplayName}",
                    $"As I see it, yes @{chatter.DisplayName}",
                    $"Most likely @{chatter.DisplayName}",
                    $"Outlook good @{chatter.DisplayName}",
                    $"Yes @{chatter.DisplayName}",
                    $"Signs point to yes @{chatter.DisplayName}",
                    $"Reply hazy try again @{chatter.DisplayName}",
                    $"Ask again later @{chatter.DisplayName}",
                    $"Better not tell you now @{chatter.DisplayName}",
                    $"Cannot predict now @{chatter.DisplayName}",
                    $"Concentrate and ask again @{chatter.DisplayName}",
                    $"Don't count on it @{chatter.DisplayName}",
                    $"My reply is no @{chatter.DisplayName}",
                    $"My sources say no @{chatter.DisplayName}",
                    $"Outlook not so good @{chatter.DisplayName}",
                    $"Very doubtful @{chatter.DisplayName}"
                };

                _irc.SendPublicChatMessage(possibleAnswers[answerId]);
                return DateTime.Now.AddSeconds(20);
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "GeneralFeature", "Magic8BallAsync(TwitchChatter)", false, EIGHT_BALL);
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Slaps a user and rates its effectiveness
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        private async Task<DateTime> SlapAsync(TwitchChatter chatter)
        {
            try
            {
                string recipient = chatter.Message.Substring(chatter.Message.IndexOf("@") + 1);
                ReactionCommand(_irc, chatter.DisplayName, recipient, "Stop smacking yourself", "slaps", Effectiveness());
                return DateTime.Now.AddSeconds(20);
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "GeneralFeature", "SlapAsync(TwitchChatter)", false, SLAP, chatter.Message);
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Stabs a user and rates its effectiveness
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        private async Task<DateTime> StabAsync(TwitchChatter chatter)
        {
            try
            {
                string recipient = chatter.Message.Substring(chatter.Message.IndexOf("@") + 1);
                ReactionCommand(_irc, chatter.DisplayName, recipient, "Stop stabbing yourself! You'll bleed out", "stabs", Effectiveness());
                return DateTime.Now.AddSeconds(20);
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "GeneralFeature", "StabAsync(TwitchChatter)", false, STAB, chatter.Message);
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Shoots a viewer's random body part
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        private async Task<DateTime> ShootAsync(TwitchChatter chatter)
        {
            try
            {
                string bodyPart = "'s ";
                string recipient = chatter.Message.Substring(chatter.Message.IndexOf("@") + 1);
                Random rnd = new Random(DateTime.Now.Millisecond);
                int bodyPartId = rnd.Next(8); // between 0 and 7

                switch (bodyPartId)
                {
                    case 0:
                        bodyPart += "head";
                        break;
                    case 1:
                        bodyPart += "left leg";
                        break;
                    case 2:
                        bodyPart += "right leg";
                        break;
                    case 3:
                        bodyPart += "left arm";
                        break;
                    case 4:
                        bodyPart += "right arm";
                        break;
                    case 5:
                        bodyPart += "stomach";
                        break;
                    case 6:
                        bodyPart += "neck";
                        break;
                    default: // found largest random value
                        bodyPart += " but missed";
                        break;
                }

                if (bodyPart == " but missed")
                {
                    _irc.SendPublicChatMessage($"Ha! You missed @{chatter.DisplayName}");
                }
                else
                {
                    // bot makes a special response if shot at
                    if (recipient == _botConfig.BotName.ToLower())
                    {
                        _irc.SendPublicChatMessage($"You think shooting me in the {bodyPart.Replace("'s ", "")} would hurt me? I am a bot!");
                    }
                    else // viewer is the target
                    {
                        ReactionCommand(_irc, chatter.DisplayName, recipient, $"You just shot your own {bodyPart.Replace("'s ", "")}", "shoots", bodyPart);
                        return DateTime.Now.AddSeconds(20);
                    }
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "GeneralFeature", "ShootAsync(TwitchChatter)", false, SHOOT, chatter.Message);
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Throws an item at a viewer and rates its effectiveness against the victim
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        private async Task<DateTime> ThrowAsync(TwitchChatter chatter)
        {
            try
            {
                int indexAction = chatter.Message.IndexOf(" ");

                if (chatter.Message.StartsWith("!throw @"))
                    _irc.SendPublicChatMessage($"Please throw an item to a user @{chatter.DisplayName}");
                else
                {
                    string recipient = chatter.Message.Substring(chatter.Message.IndexOf("@") + 1);
                    string item = chatter.Message.Substring(indexAction, chatter.Message.IndexOf("@") - indexAction - 1);

                    ReactionCommand(_irc, chatter.DisplayName, recipient, $"Stop throwing {item} at yourself", $"throws {item} at", $". {Effectiveness()}");
                    return DateTime.Now.AddSeconds(20);
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "GeneralFeature", "ThrowAsync(TwitchChatter)", false, THROW, chatter.Message);
            }

            return DateTime.Now;
        }

        private async Task<DateTime> PromoteStreamerAsync(TwitchChatter chatter)
        {
            try
            {
                string streamerUsername = ParseChatterMessageUsername(chatter);
                if (string.IsNullOrEmpty(streamerUsername))
                {
                    _irc.SendPublicChatMessage($"You didn't suggest a streamer to promote @{chatter.DisplayName}");
                    return DateTime.Now;
                }

                UserJSON userInfo = await _twitchInfo.GetUserByLoginNameAsync(streamerUsername);
                if (userInfo == null)
                {
                    _irc.SendPublicChatMessage($"Cannot find the requested user @{chatter.DisplayName}");
                    return DateTime.Now;
                }

                string userId = userInfo.Id;
                string promotionMessage = $"Hey everyone! Check out {streamerUsername}'s channel at https://www.twitch.tv/"
                    + $"{streamerUsername} and slam that follow button!";

                StreamJSON userStreamInfo = await _twitchInfo.GetUserStreamAsync(userId);

                if (userStreamInfo == null)
                {
                    ChannelJSON channelInfo = await _twitchInfo.GetUserChannelByIdAsync(userId);

                    if (!string.IsNullOrEmpty(channelInfo.GameName))
                        promotionMessage += $" They were last seen playing \"{channelInfo.GameName}\"";
                }
                else
                {
                    if (!string.IsNullOrEmpty(userStreamInfo.GameName))
                    {
                        promotionMessage += $" Right now, they're playing \"{userStreamInfo.GameName}\"";
                    }
                }

                _irc.SendPublicChatMessage(promotionMessage);
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "GeneralFeature", "PromoteStreamerAsync(TwitchChatter)", false, STREAMER);
            }

            return DateTime.Now;
        }
        #endregion
    }
}
