﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

using TwitchBotDb.Models;
using TwitchBotDb.Services;

using TwitchBotShared.ClientLibraries;
using TwitchBotShared.Config;
using TwitchBotShared.Extensions;
using TwitchBotShared.Models;
using TwitchBotShared.Models.JSON;

namespace TwitchBotShared.Threads
{
    public class FollowerSubscriberListener
    {
        private IrcClient _irc;
        private TwitchBotConfigurationSection _botConfig;
        private int _broadcasterId;
        private IEnumerable<Rank> _rankList;
        private Thread _followerListener;
        private TwitchInfoService _twitchInfo;
        private FollowerService _follower;
        private BankService _bank;
        private TwitchChatterList _twitchChatterListInstance = TwitchChatterList.Instance;

        // Empty constructor makes instance of Thread
        public FollowerSubscriberListener(TwitchBotConfigurationSection botConfig, TwitchInfoService twitchInfo, 
            FollowerService follower, BankService bank)
        {
            _botConfig = botConfig;
            _followerListener = new Thread(new ThreadStart(this.Run));
            _twitchInfo = twitchInfo;
            _follower = follower;
            _bank = bank;
        }

        // Starts the thread
        public void Start(IrcClient irc, int broadcasterId)
        {
            _irc = irc;
            _broadcasterId = broadcasterId;

            _followerListener.IsBackground = true;
            _followerListener.Start();
        }

        /// <summary>
        /// Check if follower is watching. If so, give following viewer experience every iteration
        /// </summary>
        private void Run()
        {
            while (true)
            {
                CheckNewFollowersSubscribersAsync().Wait();
                CheckChatterFollowersSubscribersAsync().Wait();
                Thread.Sleep(60000); // 1 minute
            }
        }

        /// <summary>
        /// Check broadcaster's info via API for newest followers or subscribers
        /// </summary>
        /// <returns></returns>
        private async Task CheckNewFollowersSubscribersAsync()
        {
            try
            {
                // Get broadcaster type and check if they can have subscribers
                ChannelJSON channelJson = await _twitchInfo.GetBroadcasterChannelByIdAsync();
                string broadcasterType = channelJson.BroadcasterType;

                if (broadcasterType == "partner" || broadcasterType == "affiliate")
                {
                    /* Check for new subscribers */
                    RootSubscriptionJSON rootSubscriptionJson = await _twitchInfo.GetSubscribersByChannelAsync();
                    IEnumerable<string> freshSubscribers = rootSubscriptionJson.Subscriptions
                        ?.Where(u => Convert.ToDateTime(u.CreatedAt).ToLocalTime() > DateTime.Now.AddSeconds(-60))
                        .Select(u => u.User.Name);

                    if (freshSubscribers?.Count() > 0)
                    {
                        string subscriberUsername = "";
                        string subscriberRoleplayName = "";

                        if (freshSubscribers.Count() == 1)
                        {
                            subscriberUsername = $"@{freshSubscribers.First()}";
                            subscriberRoleplayName = "a NaCl agent";
                        }
                        else if (freshSubscribers.Count() > 1)
                        {
                            subscriberRoleplayName = "NaCl agents";

                            foreach (string subscriber in freshSubscribers)
                            {
                                subscriberUsername += $"{subscriber}, ";
                            }

                            subscriberUsername = subscriberUsername.ReplaceLastOccurrence(", ", ""); // replace trailing ","
                            subscriberUsername = subscriberUsername.ReplaceLastOccurrence(", ", " & "); // replace last ","
                        }

                        string welcomeMessage = $"Congrats {subscriberUsername} on becoming @{_botConfig.Broadcaster} 's {subscriberRoleplayName}!";

                        // Break up welcome message if it's too big
                        if (welcomeMessage.Count() > 500)
                        {
                            string[] separators = new string[] { ", ", " & " };
                            List<string> subscribers = subscriberUsername.Split(separators, StringSplitOptions.RemoveEmptyEntries).ToList();

                            while (subscribers.Count > 0)
                            {
                                int popCount = 14;

                                if (subscribers.Count < popCount)
                                {
                                    popCount = subscribers.Count;
                                }

                                subscriberUsername = string.Join(", ", subscribers.Take(popCount));
                                subscriberUsername = subscriberUsername.ReplaceLastOccurrence(", ", ""); // replace trailing ","
                                subscriberUsername = subscriberUsername.ReplaceLastOccurrence(", ", " & "); // replace last ","

                                subscribers.RemoveRange(0, popCount);

                                _irc.SendPublicChatMessage($"Congrats {subscriberUsername} on becoming @{_botConfig.Broadcaster} 's {subscriberRoleplayName}!");
                                await Task.Delay(750);
                            }
                        }
                        else
                        {
                            _irc.SendPublicChatMessage(welcomeMessage);
                        }
                    }
                }

                /* Check for new followers */
                RootFollowerJSON rootFollowerJson = await _twitchInfo.GetFollowersByChannelAsync();
                IEnumerable<string> freshFollowers = rootFollowerJson.Followers
                    ?.Where(u => Convert.ToDateTime(u.CreatedAt).ToLocalTime() > DateTime.Now.AddSeconds(-60))
                    .Select(u => u.User.Name);

                if (freshFollowers?.Count() > 0)
                {
                    string followerUsername = "";

                    if (freshFollowers.Count() == 1)
                    {
                        followerUsername = $"@{freshFollowers.First()}";
                    }
                    else if (freshFollowers.Count() > 1)
                    {
                        foreach (string follower in freshFollowers)
                        {
                            followerUsername += $"{follower}, ";
                        }

                        followerUsername = followerUsername.ReplaceLastOccurrence(", ", ""); // replace trailing ","
                        followerUsername = followerUsername.ReplaceLastOccurrence(", ", " & "); // replacing joining ","
                    }

                    _irc.SendPublicChatMessage($"Welcome {followerUsername} to the Salt Army! " 
                        + $"With @{_botConfig.Broadcaster} we will pillage the seven salty seas of Twitch together!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error inside FollowerSubscriberListener.CheckNewFollowersSubscribers(): " + ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
            }
        }

        /// <summary>
        /// Check the chatter list for any followers or subscribers
        /// </summary>
        /// <returns></returns>
        private async Task CheckChatterFollowersSubscribersAsync()
        {
            try
            {
                DateTime timeToGetOut = DateTime.Now.AddSeconds(3);

                // Wait until chatter lists are available
                while (!_twitchChatterListInstance.AreListsAvailable && DateTime.Now < timeToGetOut)
                {
                    Thread.Sleep(500);
                }

                IEnumerable<string> availableChatters = _twitchChatterListInstance.ChattersByName;
                if (availableChatters == null || availableChatters.Count() == 0)
                {
                    return;
                }

                _rankList = await _follower.GetRankListAsync(_broadcasterId);

                if (_rankList == null)
                {
                    _rankList = await _follower.CreateDefaultRanksAsync(_broadcasterId);
                }

                // Check for existing or new followers/subscribers
                for (int i = 0; i < availableChatters.Count(); i++)
                {
                    string chatter = availableChatters.ElementAt(i);

                    // skip bot and broadcaster
                    if (string.Equals(chatter, _botConfig.BotName, StringComparison.CurrentCultureIgnoreCase)
                        || string.Equals(chatter, _botConfig.Broadcaster, StringComparison.CurrentCultureIgnoreCase))
                    {
                        continue;
                    }

                    // get chatter info
                    RootUserJSON rootUserJSON = await _twitchInfo.GetUsersByLoginNameAsync(chatter);
                    string userTwitchId = rootUserJSON.Users.FirstOrDefault()?.Id;

                    // skip chatter if Twitch ID is missing
                    if (string.IsNullOrEmpty(userTwitchId))
                    {
                        continue;
                    }

                    // check for follower and/or subscriber and add then to their respective lists
                    await CheckFollowerAsync(chatter, userTwitchId);
                    await CheckSubscriberAsync(chatter, userTwitchId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inside FollowerSubscriberListener.CheckChatterFollowersSubscribers(): {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
            }
        }

        private async Task CheckFollowerAsync(string chatter, string userTwitchId)
        {
            try
            {
                TwitchChatter follower = await GetTwitchFollowerInfoAsync(chatter, userTwitchId);

                if (follower == null)
                    return;

                /* Manage follower experience */
                int currentExp = await _follower.CurrentExpAsync(chatter, _broadcasterId);

                if (TwitchStreamStatus.IsLive)
                {
                    if (currentExp > -1)
                    {
                        await _follower.UpdateExpAsync(chatter, _broadcasterId, ++currentExp);
                    }
                    else
                    {
                        // add new user to the ranks
                        await _follower.EnlistRecruitAsync(chatter, _broadcasterId);
                    }
                }

                // check if follower has a stream currency account
                int setIncrementFunds = 10; // default to normal follower amount

                if (_follower.IsRegularFollower(currentExp, _botConfig.RegularFollowerHours))
                {
                    setIncrementFunds = 15;

                    if (!_twitchChatterListInstance.TwitchRegularFollowers.Any(c => c.Username == chatter))
                        _twitchChatterListInstance.TwitchRegularFollowers.Add(follower);
                }

                /* Manage follower streaming currency */
                if (TwitchStreamStatus.IsLive)
                {
                    int funds = await _bank.CheckBalanceAsync(chatter, _broadcasterId);

                    if (funds > -1)
                    {
                        funds += setIncrementFunds;
                        await _bank.UpdateFundsAsync(chatter, _broadcasterId, funds);
                    }
                    else // ToDo: Make currency auto-increment setting
                    {
                        await _bank.CreateAccountAsync(chatter, _broadcasterId, setIncrementFunds);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inside FollowerSubscriberListener.CheckFollower(string, string): {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
            }
        }

        private async Task<TwitchChatter> GetTwitchFollowerInfoAsync(string chatter, string userTwitchId)
        {
            TwitchChatter follower = null;

            try
            {
                using (HttpResponseMessage message = await _twitchInfo.CheckFollowerStatusAsync(userTwitchId))
                {
                    // check if chatter is a follower
                    if (!message.IsSuccessStatusCode)
                    {
                        // check if user was a follower but isn't anymore
                        if (_twitchChatterListInstance.TwitchFollowers.Any(c => c.Username == chatter))
                        {
                            _twitchChatterListInstance.TwitchFollowers.RemoveAll(c => c.Username == chatter);
                            _twitchChatterListInstance.TwitchRegularFollowers.RemoveAll(c => c.Username == chatter);
                        }

                        return null;
                    }

                    string body = await message.Content.ReadAsStringAsync();
                    FollowerJSON response = JsonConvert.DeserializeObject<FollowerJSON>(body);
                    DateTime startedFollowing = Convert.ToDateTime(response.CreatedAt);

                    follower = new TwitchChatter { Username = chatter, CreatedAt = startedFollowing, TwitchId = userTwitchId };

                    if (!_twitchChatterListInstance.TwitchFollowers.Any(c => c.Username == chatter))
                        _twitchChatterListInstance.TwitchFollowers.Add(follower);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inside FollowerSubscriberListener.GetTwitchFollowerInfo(string, string): {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
            }

            return follower;
        }

        private async Task CheckSubscriberAsync(string chatter, string userTwitchId)
        {
            try
            {
                await GetTwitchSubscriberInfoAsync(chatter, userTwitchId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inside FollowerSubscriberListener.CheckSubscriber(string, string): {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
            }
        }

        private async Task<TwitchChatter> GetTwitchSubscriberInfoAsync(string chatter, string userTwitchId)
        {
            TwitchChatter subscriber = null;

            try
            {
                using (HttpResponseMessage message = await _twitchInfo.CheckSubscriberStatusAsync(userTwitchId))
                {
                    // check if chatter is a subscriber
                    if (!message.IsSuccessStatusCode)
                    {
                        // check if user was a subscriber but isn't anymore
                        if (_twitchChatterListInstance.TwitchSubscribers.Any(c => c.Username == chatter))
                            _twitchChatterListInstance.TwitchSubscribers.RemoveAll(c => c.Username == chatter);

                        return null;
                    }

                    string body = await message.Content.ReadAsStringAsync();
                    SubscriptionJSON response = JsonConvert.DeserializeObject<SubscriptionJSON>(body);
                    DateTime startedSubscribing = Convert.ToDateTime(response.CreatedAt);

                    subscriber = new TwitchChatter { Username = chatter, CreatedAt = startedSubscribing, TwitchId = userTwitchId };

                    // add subscriber to global instance
                    if (!_twitchChatterListInstance.TwitchSubscribers.Any(c => c.Username == chatter))
                    {
                        _twitchChatterListInstance.TwitchSubscribers.Add(subscriber);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inside FollowerSubscriberListener.GetTwitchSubscriberInfo(string, string): {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
            }

            return subscriber;
        }
    }
}
