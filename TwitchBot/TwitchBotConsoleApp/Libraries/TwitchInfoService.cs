using System.Net.Http;
using System.Threading.Tasks;

using TwitchBotConsoleApp.Libraries;

using TwitchBotUtil.Config;
using TwitchBotUtil.Models.JSON;

namespace TwitchBotDb.Services
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

        public async Task<RootSubscriptionJSON> GetSubscribersByChannel()
        {
            return await TwitchApi.GetSubscribersByChannel(_botConfig.TwitchClientId, _botConfig.TwitchAccessToken);
        }

        public async Task<RootFollowerJSON> GetFollowersByChannel()
        {
            return await TwitchApi.GetFollowersByChannel(_botConfig.TwitchClientId);
        }

        public async Task<ClipJSON> GetClip(string slug)
        {
            return await TwitchApi.GetClip(_botConfig.TwitchClientId, slug);
        }

        public async Task<HttpResponseMessage> CheckFollowerStatus(string chatterTwitchId)
        {
            return await TwitchApi.GetFollowerStatus(chatterTwitchId, _botConfig.TwitchClientId);
        }

        public async Task<HttpResponseMessage> GetChatters()
        {
            return await TwitchApi.GetChatters(_botConfig.TwitchClientId);
        }

        public async Task<HttpResponseMessage> CheckSubscriberStatus(string userId)
        {
            return await TwitchApi.CheckSubscriberStatus(userId, _botConfig.TwitchClientId, _botConfig.TwitchAccessToken);
        }
    }
}
