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

        public async Task<List<SongRequestIgnore>> GetSongRequestIgnore(int broadcasterId)
        {
            return await _songRequestDb.GetSongRequestIgnore(broadcasterId);
        }

        public async Task<SongRequestIgnore> IgnoreArtist(string artist, int broadcasterId)
        {
            return await _songRequestDb.IgnoreArtist(artist, broadcasterId);
        }

        public async Task<SongRequestIgnore> IgnoreSong(string title, string artist, int broadcasterId)
        {
            return await _songRequestDb.IgnoreSong(title, artist, broadcasterId);
        }

        public async Task<List<SongRequestIgnore>> AllowArtist(string artist, int broadcasterId)
        {
            return await _songRequestDb.AllowArtist(artist, broadcasterId);
        }

        public async Task<SongRequestIgnore> AllowSong(string title, string artist, int broadcasterId)
        {
            return await _songRequestDb.AllowSong(title, artist, broadcasterId);
        }

        public async Task<List<SongRequestIgnore>> ResetIgnoreList(int broadcasterId)
        {
            return await _songRequestDb.ResetIgnoreList(broadcasterId);
        }
    }
}
