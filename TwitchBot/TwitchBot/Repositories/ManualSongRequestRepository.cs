using System.Collections.Generic;
using System.Threading.Tasks;

using TwitchBot.Libraries;

using TwitchBotDb.Models;

namespace TwitchBot.Repositories
{
    public class ManualSongRequestRepository
    {
        private readonly string _connStr;
        private readonly string _twitchBotApiLink;

        public ManualSongRequestRepository(string connStr, string twitchBotApiLink)
        {
            _connStr = connStr;
            _twitchBotApiLink = twitchBotApiLink;
        }

        public async Task<SongRequests> AddSongRequest(string songRequestName, string username, int broadcasterId)
        {
            SongRequests songRequest = new SongRequests
            {
                Requests = songRequestName,
                Chatter = username,
                Broadcaster = broadcasterId
            };

            return await ApiBotRequest.PostExecuteTaskAsync(_twitchBotApiLink + $"songrequests/create", songRequest);
        }

        public async Task<List<SongRequests>> ListSongRequests(int broadcasterId)
        {
            return await ApiBotRequest.GetExecuteTaskAsync<List<SongRequests>>(_twitchBotApiLink + $"songrequests/get/{broadcasterId}");
        }

        public async Task<SongRequests> PopSongRequest(int broadcasterId)
        {
            return await ApiBotRequest.DeleteExecuteTaskAsync<SongRequests>(_twitchBotApiLink + $"songrequests/delete/{broadcasterId}?popone=true");
        }

        public async Task<List<SongRequests>> ResetSongRequests(int broadcasterId)
        {
            return await ApiBotRequest.DeleteExecuteTaskAsync<List<SongRequests>>(_twitchBotApiLink + $"songrequests/delete/{broadcasterId}");
        }
    }
}
