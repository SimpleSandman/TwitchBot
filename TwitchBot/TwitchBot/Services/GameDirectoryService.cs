using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchBot.Repositories;

namespace TwitchBot.Services
{
    public class GameDirectoryService
    {
        private GameDirectoryRepository _gameDirectoryDb;

        public GameDirectoryService(GameDirectoryRepository gameDirectoryDb)
        {
            _gameDirectoryDb = gameDirectoryDb;
        }

        public int GetGameId(string gameTitle, out bool hasMultiplayer)
        {
            return _gameDirectoryDb.GetGameId(gameTitle, out hasMultiplayer);
        }
    }
}
