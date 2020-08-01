using System.Net.Http;
using System.Threading.Tasks;

using TwitchBotShared.Config;
using TwitchBotShared.Models.JSON;

namespace TwitchBotShared.ClientLibraries
{
    public class TwitchInfoService
    {
        private TwitchBotConfigurationSection _botConfig;

        public TwitchInfoService(TwitchBotConfigurationSection botConfig)
        {
            _botConfig = botConfig;
        }

        public async Task<ChannelJSON> GetBroadcasterChannelByIdAsync()
        {
            return await TwitchApi.GetBroadcasterChannelByIdAsync(_botConfig.TwitchClientId);
        }

        public async Task<ChannelJSON> GetUserChannelByIdAsync(string userId)
        {
            return await TwitchApi.GetUserChannelByIdAsync(userId, _botConfig.TwitchClientId);
        }

        public async Task<RootStreamJSON> GetBroadcasterStreamAsync()
        {
            return await TwitchApi.GetBroadcasterStreamAsync(_botConfig.TwitchClientId);
        }

        public async Task<RootStreamJSON> GetUserStreamAsync(string userId)
        {
            return await TwitchApi.GetUserStreamAsync(userId, _botConfig.TwitchClientId);
        }

        public async Task<RootUserJSON> GetUsersByLoginNameAsync(string loginName)
        {
            return await TwitchApi.GetUsersByLoginNameAsync(loginName, _botConfig.TwitchClientId);
        }

        public async Task<RootSubscriptionJSON> GetSubscribersByChannelAsync()
        {
            return await TwitchApi.GetSubscribersByChannelAsync(_botConfig.TwitchClientId, _botConfig.TwitchAccessToken);
        }

        public async Task<RootFollowerJSON> GetFollowersByChannelAsync()
        {
            return await TwitchApi.GetFollowersByChannelAsync(_botConfig.TwitchClientId);
        }

        public async Task<ClipJSON> GetClipAsync(string slug)
        {
            return await TwitchApi.GetClipAsync(_botConfig.TwitchClientId, slug);
        }

        public async Task<VideoJSON> GetVideoAsync(string videoId)
        {
            return await TwitchApi.GetVideoAsync(_botConfig.TwitchClientId, videoId);
        }

        public async Task<HttpResponseMessage> CheckFollowerStatusAsync(string chatterTwitchId)
        {
            return await TwitchApi.GetFollowerStatusAsync(chatterTwitchId, _botConfig.TwitchClientId);
        }

        public async Task<HttpResponseMessage> GetChattersAsync()
        {
            return await TwitchApi.GetChattersAsync(_botConfig.TwitchClientId);
        }

        public async Task<HttpResponseMessage> CheckSubscriberStatusAsync(string userId)
        {
            return await TwitchApi.CheckSubscriberStatusAsync(userId, _botConfig.TwitchClientId, _botConfig.TwitchAccessToken);
        }
    }
}
