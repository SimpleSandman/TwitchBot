using System.Collections.Generic;
using System.Threading.Tasks;

using TwitchBot.Repositories;

using TwitchBotDb.Models;

namespace TwitchBot.Services
{
    public class SongRequestBlacklistService
    {
        private SongRequestBlacklistRepository _songRequestDb;

        public SongRequestBlacklistService(SongRequestBlacklistRepository songRequestDb)
        {
            _songRequestDb = songRequestDb;
        }

        public async Task<List<SongRequestBlacklist>> GetSongRequestBlackList(int broadcasterId)
        {
            return await _songRequestDb.GetSongRequestBlackList(broadcasterId);
        }

        public async Task<SongRequestBlacklist> AddArtistToBlacklist(string artist, int broadcasterId)
        {
            return await _songRequestDb.AddArtistToBlacklist(artist, broadcasterId);
        }

        public async Task<SongRequestBlacklist> AddSongToBlacklist(string title, string artist, int broadcasterId)
        {
            return await _songRequestDb.AddSongToBlacklist(title, artist, broadcasterId);
        }

        public async Task<List<SongRequestBlacklist>> DeleteArtistFromBlacklist(string artist, int broadcasterId)
        {
            return await _songRequestDb.DeleteArtistFromBlacklist(artist, broadcasterId);
        }

        public async Task<SongRequestBlacklist> DeleteSongFromBlacklist(string title, string artist, int broadcasterId)
        {
            return await _songRequestDb.DeleteSongFromBlacklist(title, artist, broadcasterId);
        }

        public async Task<List<SongRequestBlacklist>> ResetBlacklist(int broadcasterId)
        {
            return await _songRequestDb.ResetBlacklist(broadcasterId);
        }
    }
}
