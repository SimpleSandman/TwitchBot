using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TwitchBotDb.Models;

using TwitchBotUtil.Libraries;

namespace TwitchBot.Repositories
{
    public class InGameUsernameRepository
    {
        private readonly string _twitchBotApiLink;

        public InGameUsernameRepository(string twitchBotApiLink)
        {
            _twitchBotApiLink = twitchBotApiLink;
        }

        public async Task<InGameUsername> GetInGameUsername(int? gameId, int broadcasterId)
        {
            return await ApiBotRequest.GetExecuteTaskAsync<InGameUsername>(_twitchBotApiLink + $"ingameusernames/get/{broadcasterId}?gameid={gameId}");
        }
    }
}
