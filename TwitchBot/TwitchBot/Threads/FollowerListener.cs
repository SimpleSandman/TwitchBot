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
    public class FollowerListener
    {
        private IrcClient _irc;
        private TwitchBotConfigurationSection _botConfig;
        private string _connStr;
        private int _broadcasterId;
        private Thread _followerListener;
        private TwitchInfoService _twitchInfo;
        private FollowerService _follower;
        private BankService _bank;
        private FollowerList _followerListInstance = FollowerList.Instance;
        private TwitchChatterList _twitchChatterListInstance = TwitchChatterList.Instance;

        // Empty constructor makes instance of Thread
        public FollowerListener(TwitchBotConfigurationSection botConfig, string connStr, TwitchInfoService twitchInfo, FollowerService follower, BankService bank)
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
                CheckFollowers().Wait();
                Thread.Sleep(60000); // 1 minute
            }
        }

        private async Task CheckFollowers()
        {
            try
            {
                // Wait until chatter lists are available
                while (!_twitchChatterListInstance.ListsAvailable)
                {
                    Thread.Sleep(1000);
                }

                IEnumerable<string> availableChatters = _twitchChatterListInstance.ChattersByName;
                if (availableChatters == null || availableChatters.Count() == 0)
                {
                    return;
                }

                IEnumerable<Rank> rankList = _follower.GetRankList(_broadcasterId);

                // Check for existing or new followers
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
                    RootUserJSON rootUserJSON = await _twitchInfo.GetUsersByLoginName(chatter);

                    using (HttpResponseMessage message = await _twitchInfo.CheckFollowerStatus(rootUserJSON.Users.First().Id))
                    {
                        // check if chatter is a follower
                        if (!message.IsSuccessStatusCode)
                        {
                            // check if user was a follower but isn't anymore
                            if (_followerListInstance.TwitchFollowers.Any(c => c.Equals(chatter)))
                                _followerListInstance.TwitchFollowers.Remove(chatter);

                            continue;
                        }

                        // add follower to global instance
                        if (!_followerListInstance.TwitchFollowers.Any(c => c.Equals(chatter)))
                            _followerListInstance.TwitchFollowers.Add(chatter);

                        // check if follower has experience
                        int currentExp = _follower.CurrentExp(chatter, _broadcasterId);

                        if (currentExp > -1)
                        {
                            _follower.UpdateExp(chatter, _broadcasterId, currentExp);

                            // check if user has been promoted
                            currentExp++; // ToDo: Update current users' rank exp via multiplication by 5 in DB
                            Rank capRank = rankList.FirstOrDefault(r => r.ExpCap == currentExp);

                            if (capRank != null)
                            {
                                Rank currentRank = _follower.GetCurrentRank(rankList, currentExp);
                                decimal hoursWatched = _follower.GetHoursWatched(currentExp);

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
                            funds += 10; // deposit 10 stream currency for each iteration
                            _bank.UpdateFunds(chatter, _broadcasterId, funds);
                        }
                        else // ToDo: Make currency auto-increment setting
                            _bank.CreateAccount(chatter, _broadcasterId, 10);

                        string body = await message.Content.ReadAsStringAsync();
                        FollowingSinceJSON response = JsonConvert.DeserializeObject<FollowingSinceJSON>(body);
                        DateTime startedFollowing = Convert.ToDateTime(response.CreatedAt);
                        TimeSpan followerTimeSpan = DateTime.Now - startedFollowing;

                        // check if user is a new follower
                        // if so, give them their sign-on bonus
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
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error inside FollowerListener Run(): " + ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
            }
        }
    }
}
