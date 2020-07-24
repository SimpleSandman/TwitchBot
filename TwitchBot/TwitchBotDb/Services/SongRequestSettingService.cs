using System.Threading.Tasks;

using TwitchBotDb.Models;
using TwitchBotDb.Repositories;

namespace TwitchBotDb.Services
{
    public class SongRequestSettingService
    {
        private SongRequestSettingRepository _songRequestSettingDb;

        public SongRequestSettingService(SongRequestSettingRepository songRequestSettingDb)
        {
            _songRequestSettingDb = songRequestSettingDb;
        }

        public async Task<SongRequestSetting> GetSongRequestSetting(int broadcasterId)
        {
            return await _songRequestSettingDb.GetSongRequestSetting(broadcasterId);
        }

        public async Task<SongRequestSetting> CreateSongRequestSetting(string requestPlaylistId, string personalPlaylistId, int broadcasterId)
        {
            if (personalPlaylistId == "")
                personalPlaylistId = null;

            return await _songRequestSettingDb.CreateSongRequestSetting(requestPlaylistId, personalPlaylistId, broadcasterId);
        }

        public async Task UpdateSongRequestSetting(string requestPlaylistId, string personalPlaylistId, int broadcasterId, bool djMode)
        {
            if (personalPlaylistId == "")
                personalPlaylistId = null;

            await _songRequestSettingDb.UpdateSongRequestSetting(requestPlaylistId, personalPlaylistId, broadcasterId, djMode);
        }
    }
}
