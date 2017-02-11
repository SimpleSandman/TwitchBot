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
    }
}
