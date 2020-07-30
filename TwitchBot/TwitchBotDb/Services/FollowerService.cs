using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TwitchBotDb.Repositories;

using TwitchBotDb.Models;

namespace TwitchBotDb.Services
{
    public class FollowerService
    {
        private FollowerRepository _followerDb;

        public FollowerService(FollowerRepository followerDb)
        {
            _followerDb = followerDb;
        }

        public async Task<int> CurrentExpAsync(string chatter, int broadcasterId)
        {
            return await _followerDb.CurrentExpAsync(chatter, broadcasterId);
        }

        public async Task UpdateExpAsync(string chatter, int broadcasterId, int exp)
        {
            await _followerDb.UpdateExpAsync(chatter, broadcasterId, exp);
        }

        public async Task EnlistRecruitAsync(string chatter, int broadcasterId)
        {
            await _followerDb.EnlistRecruitAsync(chatter, broadcasterId);
        }

        public async Task<IEnumerable<Rank>> GetRankListAsync(int broadcasterId)
        {
            return await _followerDb.GetRankListAsync(broadcasterId);
        }

        public async Task<IEnumerable<Rank>> CreateDefaultRanksAsync(int broadcasterId)
        {
            return await _followerDb.CreateDefaultRanksAsync(broadcasterId);
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

        public bool IsRegularFollower(int currentExp, int regularFollowerHours)
        {
            return GetHoursWatched(currentExp) >= regularFollowerHours;
        }

        public async Task<IEnumerable<RankFollower>> GetFollowersLeaderboardAsync(int broadcasterId)
        {
            return await _followerDb.GetFollowersLeaderboardAsync(broadcasterId);
        }
    }
}
