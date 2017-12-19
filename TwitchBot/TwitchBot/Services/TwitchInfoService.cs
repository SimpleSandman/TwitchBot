using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using TwitchBot.Configuration;
using TwitchBot.Enums;
using TwitchBot.Libraries;
using TwitchBot.Models;
using TwitchBot.Models.JSON;

namespace TwitchBot.Services
{
    public class TwitchInfoService
    {
        private TwitchBotConfigurationSection _botConfig;

        public TwitchInfoService(TwitchBotConfigurationSection botConfig)
        {
            _botConfig = botConfig;
        }

        public async Task<ChannelJSON> GetBroadcasterChannelById()
        {
            return await TwitchApi.GetBroadcasterChannelById(_botConfig.TwitchClientId);
        }

        public async Task<ChannelJSON> GetUserChannelById(string userId)
        {
            return await TwitchApi.GetUserChannelById(userId, _botConfig.TwitchClientId);
        }

        public async Task<RootStreamJSON> GetBroadcasterStream()
        {
            return await TwitchApi.GetBroadcasterStream(_botConfig.TwitchClientId);
        }

        public async Task<RootStreamJSON> GetUserStream(string userId)
        {
            return await TwitchApi.GetUserStream(userId, _botConfig.TwitchClientId);
        }

        public async Task<RootUserJSON> GetUsersByLoginName(string loginName)
        {
            return await TwitchApi.GetUsersByLoginName(loginName, _botConfig.TwitchClientId);
        }

        public async Task<HttpResponseMessage> CheckFollowerStatus(string username)
        {
            return await TwitchApi.GetFollowerStatus(username, _botConfig.TwitchClientId);
        }

        public async Task<ChatterInfoJSON> GetChatters()
        {
            return await TwitchApi.GetChatters(_botConfig.TwitchClientId);
        }
    }
}
