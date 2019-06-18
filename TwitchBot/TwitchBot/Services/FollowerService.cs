using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TwitchBot.Repositories;

using TwitchBotDb.Models;

namespace TwitchBot.Services
{
    public class FollowerService
    {
        private FollowerRepository _followerDb;

        public FollowerService(FollowerRepository followerDb)
        {
            _followerDb = followerDb;
        }

        public async Task<int> CurrentExp(string chatter, int broadcasterId)
        {
            return await _followerDb.CurrentExp(chatter, broadcasterId);
        }

        public async Task UpdateExp(string chatter, int broadcasterId, int exp)
        {
            await _followerDb.UpdateExp(chatter, broadcasterId, exp);
        }

        public async Task EnlistRecruit(string chatter, int broadcasterId)
        {
            await _followerDb.EnlistRecruit(chatter, broadcasterId);
        }

        public async Task<IEnumerable<Rank>> GetRankList(int broadcasterId)
        {
            return await _followerDb.GetRankList(broadcasterId);
        }

        public async Task<IEnumerable<Rank>> CreateDefaultRanks(int broadcasterId)
        {
            return await _followerDb.CreateDefaultRanks(broadcasterId);
        }

        public Rank GetCurrentRank(IEnumerable<Rank> rankList, int currentExp)
        {
            Rank currentRank = new Rank();

            // find the user's current rank by experience cap
            foreach (Rank rank in rankList.OrderBy(r => r.ExpCap))
            {
                // search until current experience < experience cap
                if (currentExp >= rank.ExpCap)
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

        public decimal GetHoursWatched(int currentExp)
        {
            return Math.Round(Convert.ToDecimal(currentExp) / (decimal)60.0, 2);
        }

        public async Task<IEnumerable<RankFollower>> GetFollowersLeaderboard(string broadcasterName, int broadcasterId, string botName)
        {
            return await _followerDb.GetFollowersLeaderboard(broadcasterName, broadcasterId, botName);
        }
    }
}
