using System.Collections.Generic;
using System.Threading.Tasks;

using TwitchBotDb.Models;
using TwitchBotDb.Repositories;


namespace TwitchBotDb.Services
{
    public class SongRequestBlacklistService
    {
        private readonly SongRequestBlacklistRepository _songRequestDb;

        public SongRequestBlacklistService(SongRequestBlacklistRepository songRequestDb)
        {
            _songRequestDb = songRequestDb;
        }

        public async Task<List<SongRequestIgnore>> GetSongRequestIgnoreAsync(int broadcasterId)
        {
            return await _songRequestDb.GetSongRequestIgnoreAsync(broadcasterId);
        }

        public async Task<SongRequestIgnore> IgnoreArtistAsync(string artist, int broadcasterId)
        {
            return await _songRequestDb.IgnoreArtistAsync(artist, broadcasterId);
        }

        public async Task<SongRequestIgnore> IgnoreSongAsync(string title, string artist, int broadcasterId)
        {
            return await _songRequestDb.IgnoreSongAsync(title, artist, broadcasterId);
        }

        public async Task<List<SongRequestIgnore>> AllowArtistAsync(string artist, int broadcasterId)
        {
            return await _songRequestDb.AllowArtistAsync(artist, broadcasterId);
        }

        public async Task<SongRequestIgnore> AllowSongAsync(string title, string artist, int broadcasterId)
        {
            return await _songRequestDb.AllowSongAsync(title, artist, broadcasterId);
        }

        public async Task<List<SongRequestIgnore>> ResetIgnoreListAsync(int broadcasterId)
        {
            return await _songRequestDb.ResetIgnoreListAsync(broadcasterId);
        }
    }
}
