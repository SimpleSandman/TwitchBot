using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TwitchBot.Repositories;

using TwitchBotDb.Models;

namespace TwitchBot.Services
{
    public class InGameUsernameService
    {
        private InGameUsernameRepository _ignDb;

        public InGameUsernameService(InGameUsernameRepository ignDb)
        {
            _ignDb = ignDb;
        }

        public async Task<InGameUsername> GetInGameUsername(TwitchGameCategory game, int broadcasterId)
        {
            int? gameId = game?.Id ?? null;
            return await _ignDb.GetInGameUsername(gameId, broadcasterId);
        }
    }
}
