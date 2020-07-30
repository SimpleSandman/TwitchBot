using System.Threading.Tasks;

using TwitchBotDb.Models;

namespace TwitchBotDb.Repositories
{
    public class InGameUsernameRepository
    {
        private readonly string _twitchBotApiLink;

        public InGameUsernameRepository(string twitchBotApiLink)
        {
            _twitchBotApiLink = twitchBotApiLink;
        }

        public async Task<InGameUsername> GetInGameUsernameAsync(int? gameId, int broadcasterId)
        {
            return await ApiBotRequest.GetExecuteAsync<InGameUsername>(_twitchBotApiLink + $"ingameusernames/get/{broadcasterId}?gameid={gameId}");
        }

        public async Task UpdateInGameUsernameAsync(int id, int broadcasterId, InGameUsername ign)
        {
            await ApiBotRequest.PutExecuteAsync(_twitchBotApiLink + $"ingameusernames/update/{broadcasterId}?id={id}", ign);
        }

        public async Task CreateInGameUsernameAsync(int? gameId, int broadcasterId, string message)
        {
            InGameUsername ign = new InGameUsername
            {
                GameId = gameId,
                BroadcasterId = broadcasterId,
                Message = message
            };

            await ApiBotRequest.PostExecuteAsync(_twitchBotApiLink + $"ingameusernames/create", ign);
        }

        public async Task<InGameUsername> DeleteInGameUsernameAsync(int id, int broadcasterId)
        {
            return await ApiBotRequest.DeleteExecuteAsync<InGameUsername>(_twitchBotApiLink + $"ingameusernames/delete/{broadcasterId}?id={id}");
        }
    }
}
