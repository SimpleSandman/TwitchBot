using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public int CurrExp(string chatter, int intBroadcasterID)
        {
            return _followerDB.CurrExp(chatter, intBroadcasterID);
        }

        public void UpdateExp(string strChatter, int intBroadcasterID, int intCurrExp)
        {
            _followerDB.UpdateExp(strChatter, intBroadcasterID, intCurrExp);
        }

        public void EnlistRecruit(string strChatter, int intBroadcasterID)
        {
            _followerDB.EnlistRecruit(strChatter, intBroadcasterID);
        }
    }
}
