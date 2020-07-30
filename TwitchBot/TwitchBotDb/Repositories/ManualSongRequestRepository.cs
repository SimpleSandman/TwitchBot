using System.Collections.Generic;
using System.Threading.Tasks;

using TwitchBotDb.Models;

namespace TwitchBotDb.Repositories
{
    public class ManualSongRequestRepository
    {
        private readonly string _twitchBotApiLink;

        public ManualSongRequestRepository(string twitchBotApiLink)
        {
            _twitchBotApiLink = twitchBotApiLink;
        }

        public async Task<SongRequest> AddSongRequestAsync(string songRequestName, string username, int broadcasterId)
        {
            SongRequest songRequest = new SongRequest
            {
                Name = songRequestName,
                Username = username,
                BroadcasterId = broadcasterId
            };

            return await ApiBotRequest.PostExecuteAsync(_twitchBotApiLink + $"songrequests/create", songRequest);
        }

        public async Task<List<SongRequest>> ListSongRequestsAsync(int broadcasterId)
        {
            return await ApiBotRequest.GetExecuteAsync<List<SongRequest>>(_twitchBotApiLink + $"songrequests/get/{broadcasterId}");
        }

        public async Task<SongRequest> PopSongRequestAsync(int broadcasterId)
        {
            return await ApiBotRequest.DeleteExecuteAsync<SongRequest>(_twitchBotApiLink + $"songrequests/delete/{broadcasterId}?popone=true");
        }

        public async Task<List<SongRequest>> ResetSongRequestsAsync(int broadcasterId)
        {
            return await ApiBotRequest.DeleteExecuteAsync<List<SongRequest>>(_twitchBotApiLink + $"songrequests/delete/{broadcasterId}");
        }
    }
}
