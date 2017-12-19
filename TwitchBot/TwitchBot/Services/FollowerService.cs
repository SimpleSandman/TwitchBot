using System;
using System.Collections.Generic;
using System.Linq;

using TwitchBot.Models;
using TwitchBot.Repositories;

namespace TwitchBot.Services
{
    public class FollowerService
    {
        private FollowerRepository _followerDb;

        public FollowerService(FollowerRepository followerDb)
        {
            _followerDb = followerDb;
        }

        public int CurrentExp(string chatter, int broadcasterId)
        {
            return _followerDb.CurrentExp(chatter, broadcasterId);
        }

        public void UpdateExp(string chatter, int broadcasterId, int currExp)
        {
            _followerDb.UpdateExp(chatter, broadcasterId, currExp);
        }

        public void EnlistRecruit(string chatter, int broadcasterId)
        {
            _followerDb.EnlistRecruit(chatter, broadcasterId);
        }

        public List<Rank> GetRankList(int broadcasterId)
        {
            return _followerDb.GetRankList(broadcasterId);
        }

        public Rank GetCurrentRank(List<Rank> rankList, int currExp)
        {
            Rank currentRank = new Rank();

            // find the user's current rank by experience cap
            foreach (Rank rank in rankList.OrderBy(r => r.ExpCap))
            {
                // search until current experience < experience cap
                if (currExp >= rank.ExpCap)
                {
                    continue;
                }
                else
                {
                    currentRank.Name = rank.Name;
                    currentRank.ExpCap = rank.ExpCap;
                    break;
                }
            }

            return currentRank;
        }

        public decimal GetHoursWatched(int currExp)
        {
            return Math.Round(Convert.ToDecimal(currExp) / (decimal)60.0, 2);
        }

        public List<Follower> GetFollowersLeaderboard(string broadcasterName, int broadcasterId, string botName)
        {
            return _followerDb.GetFollowersLeaderboard(broadcasterName, broadcasterId, botName);
        }
    }
}
