using System.Threading.Tasks;

using TwitchBotCore.Repositories;

using TwitchBotDb.Models;

namespace TwitchBotCore.Services
{
    public class GameDirectoryService
    {
        private GameDirectoryRepository _gameDirectoryDb;

        public GameDirectoryService(GameDirectoryRepository gameDirectoryDb)
        {
            _gameDirectoryDb = gameDirectoryDb;
        }

        public async Task<TwitchGameCategory> GetGameId(string gameTitle)
        {
            gameTitle = gameTitle.TrimEnd();

            if (string.IsNullOrEmpty(gameTitle))
            {
                return null;
            }

            return await _gameDirectoryDb.GetGameId(gameTitle);
        }
    }
}
