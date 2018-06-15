using System.Collections.Generic;
using System.Threading.Tasks;

using TwitchBot.Libraries;
using TwitchBotDb.Models;

namespace TwitchBot.Repositories
{
    public class FollowerRepository
    {
        private readonly string _connStr;
        private readonly string _twitchBotApiLink;

        public FollowerRepository(string connStr, string twitchBotApiLink)
        {
            _connStr = connStr;
            _twitchBotApiLink = twitchBotApiLink;
        }

        public async Task<int> CurrentExp(string chatter, int broadcasterId)
        {
            RankFollowers follower = await ApiBotRequest.GetExecuteTaskAsync<RankFollowers>(_twitchBotApiLink + $"rankfollowers/get/{broadcasterId}?username={chatter}");

            return follower.Exp;
        }

        public async Task UpdateExp(string chatter, int broadcasterId, int exp)
        {
            await ApiBotRequest.PutExecuteTaskAsync<RankFollowers>(_twitchBotApiLink + $"rankfollowers/updateexp/{broadcasterId}?username={chatter}&exp={exp}");
        }

        public async Task EnlistRecruit(string chatter, int broadcasterId)
        {
            RankFollowers freshRecruit = new RankFollowers
            {
                Username = chatter,
                Exp = 0,
                Broadcaster = broadcasterId
            };

            await ApiBotRequest.PostExecuteTaskAsync(_twitchBotApiLink + $"rankfollowers/create", freshRecruit);
        }

        public async Task<List<Rank>> GetRankList(int broadcasterId)
        {
            return await ApiBotRequest.GetExecuteTaskAsync<List<Rank>>(_twitchBotApiLink + $"ranks/get/{broadcasterId}");
        }

        public async Task<IEnumerable<RankFollowers>> GetFollowersLeaderboard(string broadcasterName, int broadcasterId, string botName)
        {
            return await ApiBotRequest.GetExecuteTaskAsync<List<RankFollowers>>(_twitchBotApiLink + $"rankfollowers/getleaderboard/{broadcasterId}?topnumber=3");
        }
    }
}
