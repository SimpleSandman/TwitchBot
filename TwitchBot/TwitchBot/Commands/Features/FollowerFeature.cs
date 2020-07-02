using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Newtonsoft.Json;

using TwitchBot.Configuration;
using TwitchBot.Enums;
using TwitchBot.Libraries;
using TwitchBot.Models;
using TwitchBot.Models.JSON;
using TwitchBot.Services;

using TwitchBotDb.Models;

using TwitchBotUtil.Extensions;

namespace TwitchBot.Commands.Features
{
    /// <summary>
    /// The "Command Subsystem" for the "Follower" feature
    /// </summary>
    public sealed class FollowerFeature : BaseFeature
    {
        private readonly TwitchInfoService _twitchInfo;
        private readonly FollowerService _follower;
        private readonly System.Configuration.Configuration _appConfig;
        private readonly BroadcasterSingleton _broadcasterInstance = BroadcasterSingleton.Instance;
        private readonly TwitchChatterList _twitchChatterListInstance = TwitchChatterList.Instance;
        private readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        public FollowerFeature(IrcClient irc, TwitchBotConfigurationSection botConfig, TwitchInfoService twitchInfo, FollowerService follower,
            System.Configuration.Configuration appConfig) : base(irc, botConfig)
        {
            _follower = follower;
            _twitchInfo = twitchInfo;
            _appConfig = appConfig;
            _rolePermission.Add("!followsince", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!rank", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!ranktop3", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!setregularhours", new CommandPermission { General = ChatterType.Broadcaster });
        }

        public override async Task<(bool, DateTime)> ExecCommand(TwitchChatter chatter, string requestedCommand)
        {
            try
            {
                switch (requestedCommand)
                {
                    case "!followsince":
                        return (true, await FollowSince(chatter));
                    case "!rank":
                        return (true, await ViewRank(chatter));
                    case "!setregularhours":
                        return (true, await SetRegularFollowerHours(chatter));
                    case "!ranktop3":
                        return (true, await LeaderboardRank(chatter));
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "FollowerFeature", "ExecCommand(TwitchChatter, string)", false, requestedCommand, chatter.Message);
            }

            return (false, DateTime.Now);
        }

        /// <summary>
        /// Tell the user how long they have been following the broadcaster
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        /// <returns></returns>
        private async Task<DateTime> FollowSince(TwitchChatter chatter)
        {
            try
            {
                if (chatter.Username == _botConfig.Broadcaster.ToLower())
                {
                    _irc.SendPublicChatMessage($"Please don't tell me you're really following yourself...are you {_botConfig.Broadcaster.ToLower()}? WutFace");
                    return DateTime.Now;
                }

                chatter.CreatedAt = _twitchChatterListInstance.TwitchFollowers.FirstOrDefault(c => c.Username == chatter.Username).CreatedAt;

                if (chatter.CreatedAt == null)
                {
                    // get chatter info manually
                    RootUserJSON rootUserJSON = await _twitchInfo.GetUsersByLoginName(chatter.Username);

                    using (HttpResponseMessage message = await _twitchInfo.CheckFollowerStatus(rootUserJSON.Users.First().Id))
                    {
                        string body = await message.Content.ReadAsStringAsync();
                        FollowerJSON response = JsonConvert.DeserializeObject<FollowerJSON>(body);

                        if (!string.IsNullOrEmpty(response.CreatedAt))
                        {
                            chatter.CreatedAt = Convert.ToDateTime(response.CreatedAt);
                        }
                    }
                }

                // mainly used if chatter was originally null
                if (chatter.CreatedAt != null)
                {
                    DateTime startedFollowing = Convert.ToDateTime(chatter.CreatedAt);
                    _irc.SendPublicChatMessage($"@{chatter.DisplayName} has been following since {startedFollowing.ToLongDateString()}");
                }
                else
                {
                    _irc.SendPublicChatMessage($"{chatter.DisplayName} is not following {_botConfig.Broadcaster.ToLower()}");
                }

            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "FollowerFeature", "FollowSince(TwitchChatter)", false, "!followsince");
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Display the follower's stream rank
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        /// <returns></returns>
        private async Task<DateTime> ViewRank(TwitchChatter chatter)
        {
            try
            {
                if (chatter.Username == _botConfig.Broadcaster.ToLower())
                {
                    _irc.SendPublicChatMessage($"Here goes {_botConfig.Broadcaster.ToLower()} flexing his rank...oh wait OpieOP");
                    return DateTime.Now;
                }

                DateTime? createdAt = _twitchChatterListInstance.TwitchFollowers.FirstOrDefault(c => c.Username == chatter.Username)?.CreatedAt ?? null;

                if (createdAt == null)
                {
                    using (HttpResponseMessage message = await _twitchInfo.CheckFollowerStatus(chatter.TwitchId))
                    {
                        string body = await message.Content.ReadAsStringAsync();
                        FollowerJSON response = JsonConvert.DeserializeObject<FollowerJSON>(body);

                        if (!string.IsNullOrEmpty(response.CreatedAt))
                        {
                            createdAt = Convert.ToDateTime(response.CreatedAt);
                        }
                    }
                }

                if (createdAt != null)
                {
                    int currExp = await _follower.CurrentExp(chatter.Username, _broadcasterInstance.DatabaseId);

                    // Grab the follower's associated rank
                    if (currExp > -1)
                    {
                        IEnumerable<Rank> rankList = await _follower.GetRankList(_broadcasterInstance.DatabaseId);
                        Rank currFollowerRank = _follower.GetCurrentRank(rankList, currExp);
                        decimal hoursWatched = _follower.GetHoursWatched(currExp);

                        _irc.SendPublicChatMessage($"@{chatter.DisplayName}: \"{currFollowerRank.Name}\" "
                            + $"{currExp}/{currFollowerRank.ExpCap} EXP ({hoursWatched} hours watched)");
                    }
                    else
                    {
                        await _follower.EnlistRecruit(chatter.Username, _broadcasterInstance.DatabaseId);

                        _irc.SendPublicChatMessage($"Welcome to the army @{chatter.DisplayName}. View your new rank using !rank");
                    }
                }
                else
                {
                    _irc.SendPublicChatMessage($"{chatter.DisplayName} is not following {_botConfig.Broadcaster.ToLower()}");
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "FollowerFeature", "ViewRank(TwitchChatter)", false, "!rank");
            }

            return DateTime.Now;
        }

        private async Task<DateTime> SetRegularFollowerHours(TwitchChatter chatter)
        {
            try
            {
                bool validInput = int.TryParse(ParseChatterCommandParameter(chatter), out int regularHours);
                if (!validInput)
                {
                    _irc.SendPublicChatMessage($"I can't process the time you've entered. " +
                        $"Please insert positive hours @{_botConfig.Broadcaster}");
                    return DateTime.Now;
                }
                else if (regularHours < 1)
                {
                    _irc.SendPublicChatMessage($"Please insert positive hours @{_botConfig.Broadcaster}");
                    return DateTime.Now;
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
                await _errHndlrInstance.LogError(ex, "FollowerFeature", "SetRegularFollowerHours(TwitchChatter)", false, "!setregularhours");
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Display the top 3 highest ranking members (if available)
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        private async Task<DateTime> LeaderboardRank(TwitchChatter chatter)
        {
            try
            {
                IEnumerable<RankFollower> highestRankedFollowers = await _follower.GetFollowersLeaderboard(_botConfig.Broadcaster, _broadcasterInstance.DatabaseId, _botConfig.BotName);

                if (highestRankedFollowers.Count() == 0)
                {
                    _irc.SendPublicChatMessage($"There's no one in your ranks. Start recruiting today! @{chatter.DisplayName}");
                    return DateTime.Now;
                }

                IEnumerable<Rank> rankList = await _follower.GetRankList(_broadcasterInstance.DatabaseId);

                string resultMsg = "";
                foreach (RankFollower follower in highestRankedFollowers)
                {
                    Rank currFollowerRank = _follower.GetCurrentRank(rankList, follower.Experience);
                    decimal hoursWatched = _follower.GetHoursWatched(follower.Experience);

                    resultMsg += $"\"{currFollowerRank.Name} {follower.Username}\" with {hoursWatched} hour(s), ";
                }

                resultMsg = resultMsg.Remove(resultMsg.Length - 2); // remove extra ","

                // improve list grammar
                if (highestRankedFollowers.Count() == 2)
                    resultMsg = resultMsg.ReplaceLastOccurrence(", ", " and ");
                else if (highestRankedFollowers.Count() > 2)
                    resultMsg = resultMsg.ReplaceLastOccurrence(", ", ", and ");

                if (highestRankedFollowers.Count() == 1)
                    _irc.SendPublicChatMessage($"This leader's highest ranking member is {resultMsg}");
                else
                    _irc.SendPublicChatMessage($"This leader's highest ranking members are: {resultMsg}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "FollowerFeature", "LeaderboardRank(TwitchChatter)", false, "!ranktop3");
            }

            return DateTime.Now;
        }
    }
}
