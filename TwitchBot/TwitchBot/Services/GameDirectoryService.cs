using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TwitchBot.Repositories;

using TwitchBotDb.Models;

namespace TwitchBot.Services
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
            return await _gameDirectoryDb.GetGameId(gameTitle);
        }
    }
}
