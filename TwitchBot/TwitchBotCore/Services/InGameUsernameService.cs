using System.Threading.Tasks;

using TwitchBotCore.Repositories;

using TwitchBotDb.Models;

namespace TwitchBotCore.Services
{
    public class InGameUsernameService
    {
        private InGameUsernameRepository _ignDb;

        public InGameUsernameService(InGameUsernameRepository ignDb)
        {
            _ignDb = ignDb;
        }

        public async Task<InGameUsername> GetInGameUsername(int broadcasterId, TwitchGameCategory game = null)
        {
            int? gameId = game?.Id ?? null;
            return await _ignDb.GetInGameUsername(gameId, broadcasterId);
        }

        public async Task UpdateInGameUsername(int id, int broadcasterId, InGameUsername ign)
        {
            await _ignDb.UpdateInGameUsername(id, broadcasterId, ign);
        }

        public async Task CreateInGameUsername(int? gameId, int broadcasterId, string message)
        {
            await _ignDb.CreateInGameUsername(gameId, broadcasterId, message);
        }

        public async Task<InGameUsername> DeleteInGameUsername(int id, int broadcasterId)
        {
            return await _ignDb.DeleteInGameUsername(id, broadcasterId);
        }
    }
}
