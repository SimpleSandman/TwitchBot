using System.Collections.Generic;
using System.Threading.Tasks;

using TwitchBot.Libraries;

using TwitchBotDb.Models;

namespace TwitchBot.Repositories
{
    public class SongRequestBlacklistRepository
    {
        private readonly string _twitchBotApiLink;

        public SongRequestBlacklistRepository(string twitchBotApiLink)
        {
            _twitchBotApiLink = twitchBotApiLink;
        }

        public async Task<List<SongRequestBlacklist>> GetSongRequestBlackList(int broadcasterId)
        {
            var response = await ApiBotRequest.GetExecuteTaskAsync<List<SongRequestBlacklist>>(_twitchBotApiLink + $"songrequestblacklists/get/{broadcasterId}");

            if (response != null && response.Count > 0)
            {
                return response;
            }

            return new List<SongRequestBlacklist>();
        }

        public async Task<SongRequestBlacklist> AddArtistToBlacklist(string artist, int broadcasterId)
        {
            SongRequestBlacklist blacklistedArtist = new SongRequestBlacklist
            {
                Artist = artist,
                Title = "",
                Broadcaster = broadcasterId
            };

            return await ApiBotRequest.PostExecuteTaskAsync(_twitchBotApiLink + $"songrequestblacklists/create", blacklistedArtist);
        }

        public async Task<SongRequestBlacklist> AddSongToBlacklist(string title, string artist, int broadcasterId)
        {
            SongRequestBlacklist blacklistedSong = new SongRequestBlacklist
            {
                Artist = artist,
                Title = title,
                Broadcaster = broadcasterId
            };

            return await ApiBotRequest.PostExecuteTaskAsync(_twitchBotApiLink + $"songrequestblacklists/create", blacklistedSong);
        }

        public async Task<List<SongRequestBlacklist>> DeleteArtistFromBlacklist(string artist, int broadcasterId)
        {
            return await ApiBotRequest.DeleteExecuteTaskAsync<List<SongRequestBlacklist>>(_twitchBotApiLink + $"songrequestblacklists/delete/{broadcasterId}?artist={artist}");
        }

        public async Task<SongRequestBlacklist> DeleteSongFromBlacklist(string title, string artist, int broadcasterId)
        {
            return await ApiBotRequest.DeleteExecuteTaskAsync<SongRequestBlacklist>(_twitchBotApiLink + $"songrequestblacklists/delete/{broadcasterId}?artist={artist}&title={title}");
        }

        public async Task<List<SongRequestBlacklist>> ResetBlacklist(int broadcasterId)
        {
            return await ApiBotRequest.DeleteExecuteTaskAsync<List<SongRequestBlacklist>>(_twitchBotApiLink + $"songrequestblacklists/delete/{broadcasterId}");
        }
    }
}
