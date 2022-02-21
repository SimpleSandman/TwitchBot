using System.Threading.Tasks;

using TwitchBotDb.Models;
using TwitchBotDb.Repositories;


namespace TwitchBotDb.Services
{
    public class InGameUsernameService
    {
        private readonly InGameUsernameRepository _ignDb;

        public InGameUsernameService(InGameUsernameRepository ignDb)
        {
            _ignDb = ignDb;
        }

        public async Task<InGameUsername> GetInGameUsernameAsync(int broadcasterId, TwitchGameCategory game = null)
        {
            int? gameId = game?.Id ?? null;
            return await _ignDb.GetInGameUsernameAsync(gameId, broadcasterId);
        }

        public async Task UpdateInGameUsernameAsync(int id, int broadcasterId, InGameUsername ign)
        {
            await _ignDb.UpdateInGameUsernameAsync(id, broadcasterId, ign);
        }

        public async Task CreateInGameUsernameAsync(int? gameId, int broadcasterId, string message)
        {
            await _ignDb.CreateInGameUsernameAsync(gameId, broadcasterId, message);
        }

        public async Task<InGameUsername> DeleteInGameUsernameAsync(int id, int broadcasterId)
        {
            return await _ignDb.DeleteInGameUsernameAsync(id, broadcasterId);
        }
    }
}
