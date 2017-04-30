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
    }
}
