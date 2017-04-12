using TwitchBot.Repositories;

namespace TwitchBot.Services
{
    public class FollowerService
    {
        private FollowerRepository _followerDB;

        public FollowerService(FollowerRepository followerDB)
        {
            _followerDB = followerDB;
        }

        public int CurrExp(string chatter, int broadcasterId)
        {
            return _followerDB.CurrExp(chatter, broadcasterId);
        }

        public void UpdateExp(string chatter, int broadcasterId, int currExp)
        {
            _followerDB.UpdateExp(chatter, broadcasterId, currExp);
        }

        public void EnlistRecruit(string chatter, int broadcasterId)
        {
            _followerDB.EnlistRecruit(chatter, broadcasterId);
        }
    }
}
