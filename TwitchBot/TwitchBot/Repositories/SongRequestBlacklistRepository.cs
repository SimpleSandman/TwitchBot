using System.Collections.Generic;
using System.Threading.Tasks;

using TwitchBotDb.Models;

using TwitchBotUtil.Libraries;

namespace TwitchBot.Repositories
{
    public class SongRequestBlacklistRepository
    {
        private readonly string _twitchBotApiLink;

        public SongRequestBlacklistRepository(string twitchBotApiLink)
        {
            _twitchBotApiLink = twitchBotApiLink;
        }

        public async Task<List<SongRequestIgnore>> GetSongRequestIgnore(int broadcasterId)
        {
            var response = await ApiBotRequest.GetExecuteTaskAsync<List<SongRequestIgnore>>(_twitchBotApiLink + $"songrequestignores/get/{broadcasterId}");

            if (response != null && response.Count > 0)
            {
                return response;
            }

            return new List<SongRequestIgnore>();
        }

        public async Task<SongRequestIgnore> IgnoreArtist(string artist, int broadcasterId)
        {
            SongRequestIgnore ignoreArtist = new SongRequestIgnore
            {
                Artist = artist,
                Title = "",
                BroadcasterId = broadcasterId
            };

            return await ApiBotRequest.PostExecuteTaskAsync(_twitchBotApiLink + $"songrequestignores/create", ignoreArtist);
        }

        public async Task<SongRequestIgnore> IgnoreSong(string title, string artist, int broadcasterId)
        {
            SongRequestIgnore ignoreSong = new SongRequestIgnore
            {
                Artist = artist,
                Title = title,
                BroadcasterId = broadcasterId
            };

            return await ApiBotRequest.PostExecuteTaskAsync(_twitchBotApiLink + $"songrequestignores/create", ignoreSong);
        }

        public async Task<List<SongRequestIgnore>> AllowArtist(string artist, int broadcasterId)
        {
            return await ApiBotRequest.DeleteExecuteTaskAsync<List<SongRequestIgnore>>(_twitchBotApiLink + $"songrequestignores/delete/{broadcasterId}?artist={artist}");
        }

        public async Task<SongRequestIgnore> AllowSong(string title, string artist, int broadcasterId)
        {
            return await ApiBotRequest.DeleteExecuteTaskAsync<SongRequestIgnore>(_twitchBotApiLink + $"songrequestignores/delete/{broadcasterId}?artist={artist}&title={title}");
        }

        public async Task<List<SongRequestIgnore>> ResetIgnoreList(int broadcasterId)
        {
            return await ApiBotRequest.DeleteExecuteTaskAsync<List<SongRequestIgnore>>(_twitchBotApiLink + $"songrequestignores/delete/{broadcasterId}");
        }
    }
}
