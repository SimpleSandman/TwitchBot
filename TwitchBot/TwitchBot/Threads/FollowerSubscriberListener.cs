using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

using TwitchBot.Configuration;
using TwitchBot.Libraries;
using TwitchBot.Models;
using TwitchBot.Models.JSON;
using TwitchBot.Services;

namespace TwitchBot.Threads
{
    public class FollowerSubscriberListener
    {
        private IrcClient _irc;
        private TwitchBotConfigurationSection _botConfig;
        private string _connStr;
        private int _broadcasterId;
        private IEnumerable<Rank> _rankList;
        private Thread _followerListener;
        private TwitchInfoService _twitchInfo;
        private FollowerService _follower;
        private BankService _bank;
        private TwitchChatterList _twitchChatterListInstance = TwitchChatterList.Instance;

        // Empty constructor makes instance of Thread
        public FollowerSubscriberListener(TwitchBotConfigurationSection botConfig, string connStr, TwitchInfoService twitchInfo, FollowerService follower, BankService bank)
        {
            _botConfig = botConfig;
            _connStr = connStr;
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
                CheckFollowersSubscribers().Wait();
                Thread.Sleep(60000); // 1 minute
            }
        }

        private async Task CheckFollowersSubscribers()
        {
            try
            {
                // Wait until chatter lists are available
                while (!_twitchChatterListInstance.AreListsAvailable)
                {
                    Thread.Sleep(500);
                }

                IEnumerable<string> availableChatters = _twitchChatterListInstance.ChattersByName;
                if (availableChatters == null || availableChatters.Count() == 0)
                {
                    return;
                }

                _rankList = _follower.GetRankList(_broadcasterId);

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

                    // check for follower and/or subscriber and add then to their respective lists
                    await CheckFollower(chatter);
                    await CheckSubscriber(chatter);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error inside FollowerSubscriberListener.CheckFollowersSubscribers(): " + ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
            }
        }

        private async Task CheckFollower(string chatter)
        {
            TwitchChatter follower = await GetTwitchFollowerInfo(chatter);

            if (follower == null)
                return;

            // check if follower has experience
            int currentExp = _follower.CurrentExp(chatter, _broadcasterId);
            decimal hoursWatched = 0.0m;

            if (currentExp > -1)
            {
                _follower.UpdateExp(chatter, _broadcasterId, currentExp);

                // check if user has been promoted
                currentExp++;
                Rank capRank = _rankList.FirstOrDefault(r => r.ExpCap == currentExp);
                hoursWatched = _follower.GetHoursWatched(currentExp);

                if (hoursWatched == _botConfig.RegularFollowerHours)
                {
                    Rank currentRank = _follower.GetCurrentRank(_rankList, currentExp);

                    _irc.SendPublicChatMessage($"{currentRank.Name} {chatter} has achieved the salty equlibrium " 
                        + "needed to become a regular soldier in the salt army");
                }
                else if (capRank != null)
                {
                    Rank currentRank = _follower.GetCurrentRank(_rankList, currentExp);

                    _irc.SendPublicChatMessage($"@{chatter} has been promoted to \"{currentRank.Name}\" "
                        + $"with {currentExp}/{currentRank.ExpCap} EXP ({hoursWatched} hours watched)");
                }
            }
            else
            {
                // add new user to the ranks
                _follower.EnlistRecruit(chatter, _broadcasterId);
            }

            // check if follower has a stream currency account
            int funds = _bank.CheckBalance(chatter, _broadcasterId);

            if (funds > -1)
            {
                if (hoursWatched >= _botConfig.RegularFollowerHours)
                {
                    funds += 15;

                    if (!_twitchChatterListInstance.TwitchRegularFollowers.Any(c => c.Username.Equals(chatter)))
                        _twitchChatterListInstance.TwitchRegularFollowers.Add(follower);
                }
                else
                    funds += 10;

                _bank.UpdateFunds(chatter, _broadcasterId, funds);
            }
            else // ToDo: Make currency auto-increment setting
                _bank.CreateAccount(chatter, _broadcasterId, 10);

            // check if user is a new follower
            // if so, give them their sign-on bonus
            TimeSpan followerTimeSpan = DateTime.Now - (DateTime)follower.CreatedAt;

            if (followerTimeSpan.TotalSeconds < 60)
            {
                string welcomeMessage = $"Welcome @{chatter} to the Salt Army! ";

                if (funds > -1)
                {
                    funds += 500;
                    _bank.UpdateFunds(chatter, _broadcasterId, funds);
                    welcomeMessage += $"You now have {funds} {_botConfig.CurrencyType} to gamble!";
                }
                else
                {
                    _bank.CreateAccount(chatter, _broadcasterId, 500);
                    welcomeMessage += $"You now have 500 {_botConfig.CurrencyType} to gamble!";
                }

                _irc.SendPublicChatMessage(welcomeMessage);
            }
        }

        private async Task CheckSubscriber(string chatter)
        {
            TwitchChatter subscriber = await GetTwitchSubscriberInfo(chatter);

            if (subscriber == null)
                return;

            TimeSpan subscriberTimeSpan = DateTime.Now - Convert.ToDateTime(subscriber.CreatedAt).ToLocalTime();

            // check if user is a new subscriber
            if (subscriberTimeSpan.TotalSeconds < 60)
            {
                string welcomeMessage = $"Thank you so much on becoming a secret NaCl agent @{chatter} . "
                    + $"Your dedication will allow @{_botConfig.Broadcaster} to continue commanding the Salt Army!";

                _irc.SendPublicChatMessage(welcomeMessage);
            }
        }

        private async Task<TwitchChatter> GetTwitchFollowerInfo(string chatter)
        {
            TwitchChatter follower = null;

            // get chatter info
            RootUserJSON rootUserJSON = await _twitchInfo.GetUsersByLoginName(chatter);
            string userTwitchId = rootUserJSON.Users.First().Id;

            using (HttpResponseMessage message = await _twitchInfo.CheckFollowerStatus(userTwitchId))
            {
                // check if chatter is a follower
                if (!message.IsSuccessStatusCode)
                {
                    // check if user was a follower but isn't anymore
                    if (_twitchChatterListInstance.TwitchFollowers.Any(c => c.Username.Equals(chatter)))
                    {
                        _twitchChatterListInstance.TwitchFollowers.RemoveAll(c => c.Username.Equals(chatter));
                        _twitchChatterListInstance.TwitchRegularFollowers.RemoveAll(c => c.Username.Equals(chatter));
                    }

                    return null;
                }

                string body = await message.Content.ReadAsStringAsync();
                FollowingSinceJSON response = JsonConvert.DeserializeObject<FollowingSinceJSON>(body);
                DateTime startedFollowing = Convert.ToDateTime(response.CreatedAt);

                follower = new TwitchChatter { Username = chatter, CreatedAt = startedFollowing };

                if (!_twitchChatterListInstance.TwitchFollowers.Any(c => c.Username.Equals(chatter)))
                    _twitchChatterListInstance.TwitchFollowers.Add(follower);
            }

            return follower;
        }

        private async Task<TwitchChatter> GetTwitchSubscriberInfo(string chatter)
        {
            TwitchChatter subscriber = null;

            // get chatter info
            RootUserJSON rootUserJSON = await _twitchInfo.GetUsersByLoginName(chatter);
            string userTwitchId = rootUserJSON.Users.First().Id;

            using (HttpResponseMessage message = await _twitchInfo.CheckSubscriberStatus(userTwitchId))
            {
                // check if chatter is a subscriber
                if (!message.IsSuccessStatusCode)
                {
                    // check if user was a subscriber but isn't anymore
                    if (_twitchChatterListInstance.TwitchSubscribers.Any(c => c.Equals(chatter)))
                        _twitchChatterListInstance.TwitchSubscribers.RemoveAll(c => c.Username.Equals(chatter));

                    return null;
                }

                string body = await message.Content.ReadAsStringAsync();
                SubscribedUserJSON response = JsonConvert.DeserializeObject<SubscribedUserJSON>(body);
                DateTime startedSubscribing = Convert.ToDateTime(response.CreatedAt);

                subscriber = new TwitchChatter { Username = chatter, CreatedAt = startedSubscribing };

                // add subscriber to global instance
                if (!_twitchChatterListInstance.TwitchSubscribers.Any(c => c.Equals(chatter)))
                {
                    _twitchChatterListInstance.TwitchSubscribers.Add(subscriber);
                }
            }

            return subscriber;
        }
    }
}
