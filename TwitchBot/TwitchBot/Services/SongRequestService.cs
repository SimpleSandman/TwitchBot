using System.Collections.Generic;

using TwitchBot.Models;
using TwitchBot.Repositories;

namespace TwitchBot.Services
{
    public class SongRequestService
    {
        private SongRequestRepository _songRequestDb;

        public SongRequestService(SongRequestRepository songRequestDb)
        {
            _songRequestDb = songRequestDb;
        }

        public List<SongRequestBlacklistItem> GetSongRequestBlackList(int broadcasterId)
        {
            return _songRequestDb.GetSongRequestBlackList(broadcasterId);
        }

        public int AddArtistToBlacklist(string artist, int broadcasterId)
        {
            return _songRequestDb.AddArtistToBlacklist(artist, broadcasterId);
        }

        public int AddSongToBlacklist(string title, string artist, int broadcasterId)
        {
            return _songRequestDb.AddSongToBlacklist(title, artist, broadcasterId);
        }

        public int DeleteArtistFromBlacklist(string artist, int broadcasterId)
        {
            return _songRequestDb.DeleteArtistFromBlacklist(artist, broadcasterId);
        }

        public int DeleteSongFromBlacklist(string title, string artist, int broadcasterId)
        {
            return _songRequestDb.DeleteSongFromBlacklist(title, artist, broadcasterId);
        }

        public int ResetBlacklist(int broadcasterId)
        {
            return _songRequestDb.ResetBlacklist(broadcasterId);
        }
    }
}
