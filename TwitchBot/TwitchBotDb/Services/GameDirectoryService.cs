using System.Threading.Tasks;

using TwitchBotDb.Models;
using TwitchBotDb.Repositories;


namespace TwitchBotDb.Services
{
    public class GameDirectoryService
    {
        private readonly GameDirectoryRepository _gameDirectoryDb;

        public GameDirectoryService(GameDirectoryRepository gameDirectoryDb)
        {
            _gameDirectoryDb = gameDirectoryDb;
        }

        public async Task<TwitchGameCategory> GetGameIdAsync(string gameTitle)
        {
            gameTitle = gameTitle.TrimEnd();

            if (string.IsNullOrEmpty(gameTitle))
            {
                return null;
            }

            return await _gameDirectoryDb.GetGameIdAsync(gameTitle);
        }
    }
}
