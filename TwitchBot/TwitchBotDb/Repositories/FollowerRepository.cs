using System.Collections.Generic;
using System.Threading.Tasks;

using TwitchBotDb.Models;

namespace TwitchBotDb.Repositories
{
    public class FollowerRepository
    {
        private readonly string _twitchBotApiLink;

        public FollowerRepository(string twitchBotApiLink)
        {
            _twitchBotApiLink = twitchBotApiLink;
        }

        public async Task<int> CurrentExp(string chatter, int broadcasterId)
        {
            RankFollower follower = await ApiBotRequest.GetExecuteAsync<RankFollower>(_twitchBotApiLink + $"rankfollowers/get/{broadcasterId}?username={chatter}");

            return follower.Experience;
        }

        public async Task UpdateExp(string chatter, int broadcasterId, int exp)
        {
            await ApiBotRequest.PutExecuteAsync<RankFollower>(_twitchBotApiLink + $"rankfollowers/updateexp/{broadcasterId}?username={chatter}&exp={exp}");
        }

        public async Task EnlistRecruit(string chatter, int broadcasterId)
        {
            RankFollower freshRecruit = new RankFollower
            {
                Username = chatter,
                Experience = 0,
                BroadcasterId = broadcasterId
            };

            await ApiBotRequest.PostExecuteAsync(_twitchBotApiLink + $"rankfollowers/create", freshRecruit);
        }

        public async Task<List<Rank>> GetRankList(int broadcasterId)
        {
            return await ApiBotRequest.GetExecuteAsync<List<Rank>>(_twitchBotApiLink + $"ranks/get/{broadcasterId}");
        }

        public async Task<IEnumerable<Rank>> CreateDefaultRanks(int broadcasterId)
        {
            List<Rank> rank = new List<Rank>
            {
                new Rank
                {
                    BroadcasterId = broadcasterId
                }
            };

            return await ApiBotRequest.PostExecuteAsync(_twitchBotApiLink + $"ranks/createdefault", rank);
        }

        public async Task<IEnumerable<RankFollower>> GetFollowersLeaderboard(int broadcasterId)
        {
            return await ApiBotRequest.GetExecuteAsync<List<RankFollower>>(_twitchBotApiLink + $"rankfollowers/getleaderboard/{broadcasterId}?topnumber=3");
        }
    }
}
