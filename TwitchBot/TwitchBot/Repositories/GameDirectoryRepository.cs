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
            return await ApiBotRequest.GetExecuteTaskAsync<TwitchGameCategory>(_twitchBotApiLink + $"twitchgamecategories/get/{gameTitle}");
        }
    }
}
