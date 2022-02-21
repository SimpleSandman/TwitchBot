using System.Threading.Tasks;

using TwitchBotDb.Models;
using TwitchBotDb.Repositories;

namespace TwitchBotDb.Services
{
    public class SongRequestSettingService
    {
        private readonly SongRequestSettingRepository _songRequestSettingDb;

        public SongRequestSettingService(SongRequestSettingRepository songRequestSettingDb)
        {
            _songRequestSettingDb = songRequestSettingDb;
        }

        public async Task<SongRequestSetting> GetSongRequestSettingAsync(int broadcasterId)
        {
            return await _songRequestSettingDb.GetSongRequestSettingAsync(broadcasterId);
        }

        public async Task<SongRequestSetting> CreateSongRequestSettingAsync(string requestPlaylistId, string personalPlaylistId, int broadcasterId)
        {
            if (personalPlaylistId == "")
                personalPlaylistId = null;

            return await _songRequestSettingDb.CreateSongRequestSettingAsync(requestPlaylistId, personalPlaylistId, broadcasterId);
        }

        public async Task UpdateSongRequestSettingAsync(string requestPlaylistId, string personalPlaylistId, int broadcasterId, bool djMode)
        {
            if (personalPlaylistId == "")
                personalPlaylistId = null;

            await _songRequestSettingDb.UpdateSongRequestSettingAsync(requestPlaylistId, personalPlaylistId, broadcasterId, djMode);
        }
    }
}
