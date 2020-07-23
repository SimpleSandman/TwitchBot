using System.Threading.Tasks;

using TwitchBotDb.Models;

using TwitchBotUtil.Libraries;

namespace TwitchBotConsoleApp.Repositories
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
            return await ApiBotRequest.GetExecuteAsync<InGameUsername>(_twitchBotApiLink + $"ingameusernames/get/{broadcasterId}?gameid={gameId}");
        }

        public async Task UpdateInGameUsername(int id, int broadcasterId, InGameUsername ign)
        {
            await ApiBotRequest.PutExecuteAsync(_twitchBotApiLink + $"ingameusernames/update/{broadcasterId}?id={id}", ign);
        }

        public async Task CreateInGameUsername(int? gameId, int broadcasterId, string message)
        {
            InGameUsername ign = new InGameUsername
            {
                GameId = gameId,
                BroadcasterId = broadcasterId,
                Message = message
            };

            await ApiBotRequest.PostExecuteAsync(_twitchBotApiLink + $"ingameusernames/create", ign);
        }

        public async Task<InGameUsername> DeleteInGameUsername(int id, int broadcasterId)
        {
            return await ApiBotRequest.DeleteExecuteAsync<InGameUsername>(_twitchBotApiLink + $"ingameusernames/delete/{broadcasterId}?id={id}");
        }
    }
}
