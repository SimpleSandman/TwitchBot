using System.Threading.Tasks;

using TwitchBotDb.Models;

using TwitchBotUtil.Libraries;

namespace TwitchBot.Repositories
{
    public class GameDirectoryRepository
    {
        private readonly string _twitchBotApiLink;

        public GameDirectoryRepository(string twitchBotApiLink)
        {
            _twitchBotApiLink = twitchBotApiLink;
        }

        public async Task<TwitchGameCategory> GetGameId(string gameTitle)
        {
            return await ApiBotRequest.GetExecuteAsync<TwitchGameCategory>(_twitchBotApiLink + $"twitchgamecategories/get?title={gameTitle}");
        }
    }
}
