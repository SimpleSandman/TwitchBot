using System.Threading.Tasks;

using TwitchBotDb.Models;

using TwitchBotUtil.Libraries;

namespace TwitchBotCore.Repositories
{
    public class SongRequestSettingRepository
    {
        private readonly string _twitchBotApiLink;

        public SongRequestSettingRepository(string twitchBotApiLink)
        {
            _twitchBotApiLink = twitchBotApiLink;
        }

        public async Task<SongRequestSetting> GetSongRequestSetting(int broadcasterId)
        {
            var response = await ApiBotRequest.GetExecuteAsync<SongRequestSetting>(_twitchBotApiLink + $"songrequestsettings/get/{broadcasterId}");

            if (response != null)
            {
                return response;
            }

            return new SongRequestSetting();
        }

        public async Task<SongRequestSetting> CreateSongRequestSetting(string requestPlaylistId, string personalPlaylistId, int broadcasterId)
        {
            SongRequestSetting setting = new SongRequestSetting
            {
                RequestPlaylistId = requestPlaylistId,
                PersonalPlaylistId = personalPlaylistId,
                BroadcasterId = broadcasterId,
                DjMode = false
            };

            return await ApiBotRequest.PostExecuteAsync(_twitchBotApiLink + "songrequestsettings/create", setting);
        }

        public async Task UpdateSongRequestSetting(string requestPlaylistId, string personalPlaylistId, int broadcasterId, bool djMode)
        {
            SongRequestSetting updatedSettings = new SongRequestSetting
            {
                RequestPlaylistId = requestPlaylistId,
                PersonalPlaylistId = personalPlaylistId,
                BroadcasterId = broadcasterId,
                DjMode = djMode
            };

            await ApiBotRequest.PutExecuteAsync(_twitchBotApiLink + $"songrequestsettings/update/{broadcasterId}", updatedSettings);
        }
    }
}
