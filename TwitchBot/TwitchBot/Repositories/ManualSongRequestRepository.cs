using System.Collections.Generic;
using System.Threading.Tasks;

using TwitchBotDb.Models;

using TwitchBotUtil.Libraries;

namespace TwitchBot.Repositories
{
    public class ManualSongRequestRepository
    {
        private readonly string _twitchBotApiLink;

        public ManualSongRequestRepository(string twitchBotApiLink)
        {
            _twitchBotApiLink = twitchBotApiLink;
        }

        public async Task<SongRequest> AddSongRequest(string songRequestName, string username, int broadcasterId)
        {
            SongRequest songRequest = new SongRequest
            {
                Name = songRequestName,
                Username = username,
                BroadcasterId = broadcasterId
            };

            return await ApiBotRequest.PostExecuteTaskAsync(_twitchBotApiLink + $"songrequests/create", songRequest);
        }

        public async Task<List<SongRequest>> ListSongRequests(int broadcasterId)
        {
            return await ApiBotRequest.GetExecuteTaskAsync<List<SongRequest>>(_twitchBotApiLink + $"songrequests/get/{broadcasterId}");
        }

        public async Task<SongRequest> PopSongRequest(int broadcasterId)
        {
            return await ApiBotRequest.DeleteExecuteTaskAsync<SongRequest>(_twitchBotApiLink + $"songrequests/delete/{broadcasterId}?popone=true");
        }

        public async Task<List<SongRequest>> ResetSongRequests(int broadcasterId)
        {
            return await ApiBotRequest.DeleteExecuteTaskAsync<List<SongRequest>>(_twitchBotApiLink + $"songrequests/delete/{broadcasterId}");
        }
    }
}
