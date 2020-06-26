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
            _rolePermission.Add("!followsince", new List<ChatterType> { ChatterType.Viewer });
            _rolePermission.Add("!rank", new List<ChatterType> { ChatterType.Viewer });
            _rolePermission.Add("!setregularhours", new List<ChatterType> { ChatterType.Broadcaster });
        }

        public override async Task<bool> ExecCommand(TwitchChatter chatter, string requestedCommand)
        {
            try
            {
                switch (requestedCommand)
                {
                    case "!followsince":
                        await FollowSince(chatter);
                        return true;
                    case "!rank":
                        await ViewRank(chatter);
                        return true;
                    case "!setregularhours":
                        SetRegularFollowerHours(chatter);
                        return true;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "FollowerFeature", "ExecCommand(TwitchChatter, string)", false, requestedCommand, chatter.Message);
            }

            return false;
        }

        /// <summary>
        /// Tell the user how long they have been following the broadcaster
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        /// <returns></returns>
        public async Task FollowSince(TwitchChatter chatter)
        {
            try
            {
                if (chatter.Username == _botConfig.Broadcaster.ToLower())
                {
                    _irc.SendPublicChatMessage($"Please don't tell me you're really following yourself...are you {_botConfig.Broadcaster.ToLower()}? WutFace");
                    return;
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
        }

        /// <summary>
        /// Display the follower's stream rank
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        /// <returns></returns>
        public async Task ViewRank(TwitchChatter chatter)
        {
            try
            {
                if (chatter.Username == _botConfig.Broadcaster.ToLower())
                {
                    _irc.SendPublicChatMessage($"Here goes {_botConfig.Broadcaster.ToLower()} flexing his rank...oh wait OpieOP");
                    return;
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
        }

        public async void SetRegularFollowerHours(TwitchChatter chatter)
        {
            try
            {
                bool validInput = int.TryParse(CommandToolbox.ParseChatterCommandParameter(chatter), out int regularHours);
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
                await _errHndlrInstance.LogError(ex, "FollowerFeature", "SetRegularFollowerHours(TwitchChatter)", false, "!setregularhours");
            }
        }
    }
}
