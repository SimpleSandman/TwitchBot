using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using TwitchBotCore.Config;
using TwitchBotCore.Enums;
using TwitchBotCore.Libraries;
using TwitchBotCore.Models;
using TwitchBotCore.Models.JSON;
using TwitchBotCore.Services;

namespace TwitchBotCore.Commands.Features
{
    /// <summary>
    /// The "Command Subsystem" for the "General Bot" feature
    /// </summary>
    public sealed class GeneralFeature : BaseFeature
    {
        private readonly TwitchInfoService _twitchInfo;
        private readonly System.Configuration.Configuration _appConfig;
        private readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        public GeneralFeature(IrcClient irc, TwitchBotConfigurationSection botConfig, TwitchInfoService twitchInfo,
            System.Configuration.Configuration appConfig) : base(irc, botConfig)
        {
            _twitchInfo = twitchInfo;
            _appConfig = appConfig;
            _rolePermission.Add("!settings", new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermission.Add("!exit", new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermission.Add("!streamer", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!so", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!shoutout", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!caster", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!cmds", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!help", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!commands", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!hello", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!hi", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!utctime", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!hosttime", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!uptime", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!setlatency", new CommandPermission { General = ChatterType.Moderator });
            _rolePermission.Add("!latency", new CommandPermission { General = ChatterType.Moderator });
            _rolePermission.Add("!support", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!bot", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!lurk", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!unlurk", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!sub", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!subscribe", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!8ball", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!slap", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!stab", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!shoot", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!throw", new CommandPermission { General = ChatterType.Viewer });
        }

        public override async Task<(bool, DateTime)> ExecCommand(TwitchChatter chatter, string requestedCommand)
        {
            try
            {
                switch (requestedCommand)
                {
                    case "!settings":
                        return (true, await BotSettings());
                    case "!exit":
                        return (true, await ExitBot());
                    case "!streamer":
                    case "!shoutout":
                    case "!caster":
                    case "!so":
                        return (true, await PromoteStreamer(chatter));
                    case "!cmds":
                    case "!help":
                    case "!commands":
                        return (true, await Displays());
                    case "!hello":
                    case "!hi":
                        return (true, await Hello(chatter));
                    case "!utctime":
                        return (true, await UtcTime());
                    case "!hosttime":
                        return (true, await HostTime());
                    case "!uptime":
                        return (true, await Uptime());
                    case "!setlatency":
                    case "!latency":
                        return (true, await SetLatency(chatter));
                    case "!support":
                    case "!bot":
                        return (true, await Support());
                    case "!lurk":
                        return (true, await Lurk(chatter));
                    case "!unlurk":
                        return (true, await Unlurk(chatter));
                    case "!sub":
                    case "!subscribe":
                        return (true, await Subscribe());
                    case "!8ball":
                        return (true, await Magic8Ball(chatter));
                    case "!slap":
                        return (true, await Slap(chatter));
                    case "!stab":
                        return (true, await Stab(chatter));
                    case "!shoot":
                        return (true, await Shoot(chatter));
                    case "!throw":
                        return (true, await Throw(chatter));
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "GeneralFeature", "ExecCommand(TwitchChatter, string)", false, requestedCommand, chatter.Message);
            }

            return (false, DateTime.Now);
        }

        /// <summary>
        /// Display bot settings
        /// </summary>
        private async Task<DateTime> BotSettings()
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

            return DateTime.Now;
        }

        /// <summary>
        /// Stop running the bot
        /// </summary>
        private async Task<DateTime> ExitBot()
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

            return DateTime.Now;
        }

        private async Task<DateTime> Displays()
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

            return DateTime.Now;
        }

        private async Task<DateTime> Hello(TwitchChatter chatter)
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

            return DateTime.Now;
        }

        private async Task<DateTime> UtcTime()
        {
            try
            {
                _irc.SendPublicChatMessage($"UTC Time: {DateTime.UtcNow}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "GeneralFeature", "UtcTime()", false, "!utctime");
            }

            return DateTime.Now;
        }

        private async Task<DateTime> HostTime()
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
                await _errHndlrInstance.LogError(ex, "GeneralFeature", "HostTime()", false, "!hosttime");
            }

            return DateTime.Now;
        }

        private async Task<DateTime> Uptime()
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

            return DateTime.Now;
        }

        /// <summary>
        /// Set delay for messages based on the latency of the stream
        /// </summary>
        /// <param name="chatter"></param>
        private async Task<DateTime> SetLatency(TwitchChatter chatter)
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
                await _errHndlrInstance.LogError(ex, "GeneralFeature", "SetLatency(TwitchChatter)", false, "!setlatency");
            }

            return DateTime.Now;
        }

        private async Task<DateTime> Support()
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
                await _errHndlrInstance.LogError(ex, "Gen", "Support()", false, "!support");
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Tell the streamer the user is lurking
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        /// <returns></returns>
        private async Task<DateTime> Lurk(TwitchChatter chatter)
        {
            try
            {
                _irc.SendPublicChatMessage($"Okay {chatter.DisplayName}! @{_botConfig.Broadcaster} will be waiting for you TPFufun");
                return DateTime.Now.AddMinutes(5);
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "Gen", "Lurk(TwitchChatter)", false, "!lurk");
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Tell the streamer the user is back from lurking
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        /// <returns></returns>
        private async Task<DateTime> Unlurk(TwitchChatter chatter)
        {
            try
            {
                _irc.SendPublicChatMessage($"Welcome back {chatter.DisplayName}! KonCha I'll let @{_botConfig.Broadcaster} know you're here!");
                return DateTime.Now.AddMinutes(5);
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "Gen", "Unlurk(TwitchChatter)", false, "!unlurk");
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Display the broadcaster's subscriber link (if they're an Affiliate/Partner)
        /// </summary>
        /// <returns></returns>
        private async Task<DateTime> Subscribe()
        {
            try
            {
                // Get broadcaster type and check if they can have subscribers
                ChannelJSON json = await _twitchInfo.GetBroadcasterChannelById();
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
                await _errHndlrInstance.LogError(ex, "Gen", "Subscribe()", false, "!sub");
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Ask any question and the Magic 8 Ball will give a fortune
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        private async Task<DateTime> Magic8Ball(TwitchChatter chatter)
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
                await _errHndlrInstance.LogError(ex, "Gen", "Magic8Ball(TwitchChatter)", false, "!8ball");
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Slaps a user and rates its effectiveness
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        private async Task<DateTime> Slap(TwitchChatter chatter)
        {
            try
            {
                string recipient = chatter.Message.Substring(chatter.Message.IndexOf("@") + 1);
                ReactionCommand(_irc, chatter.DisplayName, recipient, "Stop smacking yourself", "slaps", Effectiveness());
                return DateTime.Now.AddSeconds(20);
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "Gen", "Slap(TwitchChatter)", false, "!slap", chatter.Message);
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Stabs a user and rates its effectiveness
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        private async Task<DateTime> Stab(TwitchChatter chatter)
        {
            try
            {
                string recipient = chatter.Message.Substring(chatter.Message.IndexOf("@") + 1);
                ReactionCommand(_irc, chatter.DisplayName, recipient, "Stop stabbing yourself! You'll bleed out", "stabs", Effectiveness());
                return DateTime.Now.AddSeconds(20);
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "Gen", "Stab(TwitchChatter)", false, "!stab", chatter.Message);
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Shoots a viewer's random body part
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        private async Task<DateTime> Shoot(TwitchChatter chatter)
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
                await _errHndlrInstance.LogError(ex, "Gen", "Shoot(TwitchChatter)", false, "!shoot", chatter.Message);
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Throws an item at a viewer and rates its effectiveness against the victim
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        private async Task<DateTime> Throw(TwitchChatter chatter)
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
                await _errHndlrInstance.LogError(ex, "Gen", "Throw(TwitchChatter)", false, "!throw", chatter.Message);
            }

            return DateTime.Now;
        }

        private async Task<DateTime> PromoteStreamer(TwitchChatter chatter)
        {
            try
            {
                string streamerUsername = ParseChatterMessageUsername(chatter);

                RootUserJSON userInfo = await _twitchInfo.GetUsersByLoginName(streamerUsername);
                if (userInfo.Users == null || userInfo.Users.Count == 0)
                {
                    _irc.SendPublicChatMessage($"Cannot find the requested user @{chatter.DisplayName}");
                    return DateTime.Now;
                }

                string userId = userInfo.Users.First().Id;
                string promotionMessage = $"Hey everyone! Check out {streamerUsername}'s channel at https://www.twitch.tv/"
                    + $"{streamerUsername} and slam that follow button!";

                RootStreamJSON userStreamInfo = await _twitchInfo.GetUserStream(userId);

                if (userStreamInfo.Stream == null)
                {
                    ChannelJSON channelInfo = await _twitchInfo.GetUserChannelById(userId);

                    if (!string.IsNullOrEmpty(channelInfo.Game))
                        promotionMessage += $" They were last seen playing \"{channelInfo.Game}\"";
                }
                else
                {
                    if (!string.IsNullOrEmpty(userStreamInfo.Stream.Game))
                        promotionMessage += $" Right now, they're playing \"{userStreamInfo.Stream.Game}\"";
                }

                _irc.SendPublicChatMessage(promotionMessage);
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "Vip", "PromoteStreamer(TwitchChatter)", false, "!streamer");
            }

            return DateTime.Now;
        }
    }
}
