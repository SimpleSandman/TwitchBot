using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using TwitchBot.Configuration;
using TwitchBot.Services;

namespace TwitchBot.Threads
{
    public class FollowerListener
    {
        private TwitchBotConfigurationSection _botConfig;
        private string _connStr;
        private int _broadcasterId;
        private Thread _followerListener;
        private TwitchInfoService _twitchInfo;
        private FollowerService _follower;
        private BankService _bank;

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
        public void Start(int broadcasterId)
        {
            _broadcasterId = broadcasterId;
            _followerListener.IsBackground = true;
            _followerListener.Start();
        }

        /// <summary>
        /// Check if follower is watching. If so, give following viewer experience every iteration
        /// </summary>
        public void Run()
        {
            while (true)
            {
                CheckFollowers().Wait();
                Thread.Sleep(300000); // 5 minutes
            }
        }

        public async Task CheckFollowers()
        {
            try
            {
                // Grab user's chatter info (viewers, mods, etc.)
                List<List<string>> lstAvailChatterType = await _twitchInfo.GetChatterListByType();
                if (lstAvailChatterType.Count == 0)
                {
                    return;
                }

                // Check for existing or new followers
                for (int i = 0; i < lstAvailChatterType.Count; i++)
                {
                    foreach (string chatter in lstAvailChatterType[i])
                    {
                        // skip bot and broadcaster
                        if (string.Equals(chatter, _botConfig.BotName, StringComparison.CurrentCultureIgnoreCase)
                            || string.Equals(chatter, _botConfig.Broadcaster, StringComparison.CurrentCultureIgnoreCase))
                        {
                            continue;
                        }

                        using (HttpResponseMessage message = await _twitchInfo.CheckFollowerStatus(chatter))
                        {
                            // check if chatter is a follower
                            if (!message.IsSuccessStatusCode)
                            {
                                continue;
                            }

                            // check if follower has experience
                            int intCurrExp = _follower.CurrExp(chatter, _broadcasterId);

                            if (intCurrExp > -1)
                                _follower.UpdateExp(chatter, _broadcasterId, intCurrExp);
                            else
                                _follower.EnlistRecruit(chatter, _broadcasterId);

                            // check if follower has a stream currency account
                            int intFunds = _bank.CheckBalance(chatter, _broadcasterId);

                            if (intFunds > -1)
                            {
                                intFunds += 10; // deposit 10 stream currency for each iteration
                                _bank.UpdateFunds(chatter, _broadcasterId, intFunds);
                            }
                            else // ToDo: Make currency auto-increment setting
                                _bank.CreateAccount(chatter, _broadcasterId, 10);
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
