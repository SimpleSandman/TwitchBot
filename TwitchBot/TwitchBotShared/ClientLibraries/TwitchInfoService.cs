using System.Linq;
using System.Threading.Tasks;

using TwitchBotShared.Config;
using TwitchBotShared.Models.JSON;

namespace TwitchBotShared.ClientLibraries
{
    public class TwitchInfoService
    {
        private readonly TwitchBotConfigurationSection _botConfig;

        public TwitchInfoService(TwitchBotConfigurationSection botConfig)
        {
            _botConfig = botConfig;
        }

        public async Task<ChannelJSON> GetBroadcasterChannelByIdAsync()
        {
            RootChannelJSON result = await TwitchApi.GetBroadcasterChannelByIdAsync(_botConfig.TwitchClientId, _botConfig.TwitchAccessToken);
            return result.Channels.FirstOrDefault();
        }

        public async Task<ChannelJSON> GetUserChannelByIdAsync(string userId)
        {
            RootChannelJSON result = await TwitchApi.GetUserChannelByIdAsync(userId, _botConfig.TwitchClientId, _botConfig.TwitchAccessToken);
            return result.Channels.FirstOrDefault();
        }

        public async Task<UserJSON> GetBroadcasterUserByIdAsync()
        {
            RootUserJSON result = await TwitchApi.GetBroadcasterUserByIdAsync(_botConfig.TwitchClientId, _botConfig.TwitchAccessToken);
            return result.Users.FirstOrDefault();
        }

        public async Task<UserJSON> GetUserByLoginNameAsync(string loginName)
        {
            RootUserJSON result = await TwitchApi.GetUserByLoginNameAsync(loginName, _botConfig.TwitchClientId, _botConfig.TwitchAccessToken);
            return result.Users.FirstOrDefault();
        }

        public async Task<StreamJSON> GetBroadcasterStreamAsync()
        {
            RootStreamJSON result = await TwitchApi.GetBroadcasterStreamAsync(_botConfig.TwitchClientId, _botConfig.TwitchAccessToken);
            return result.Stream.FirstOrDefault();
        }

        public async Task<StreamJSON> GetUserStreamAsync(string userId)
        {
            RootStreamJSON result = await TwitchApi.GetUserStreamAsync(userId, _botConfig.TwitchClientId, _botConfig.TwitchAccessToken);
            return result.Stream.FirstOrDefault();
        }

        public async Task<RootSubscriptionJSON> GetSubscribersByChannelAsync()
        {
            return await TwitchApi.GetSubscribersByChannelAsync(_botConfig.TwitchClientId, _botConfig.TwitchAccessToken);
        }

        public async Task<RootFollowerJSON> GetFollowersByChannelAsync()
        {
            return await TwitchApi.GetFollowersByChannelAsync(_botConfig.TwitchClientId, _botConfig.TwitchAccessToken);
        }

        public async Task<ClipJSON> GetClipAsync(string slug)
        {
            RootClipJSON result = await TwitchApi.GetClipAsync(_botConfig.TwitchClientId, slug, _botConfig.TwitchAccessToken);
            return result.Clips.FirstOrDefault();
        }

        public async Task<VideoJSON> GetVideoAsync(string videoId)
        {
            RootVideoJSON result = await TwitchApi.GetVideoAsync(_botConfig.TwitchClientId, videoId, _botConfig.TwitchAccessToken);
            return result.Videos.FirstOrDefault();
        }

        public async Task<FollowerJSON> CheckFollowerStatusAsync(string chatterTwitchId)
        {
            RootFollowerJSON result = await TwitchApi.GetFollowerStatusAsync(chatterTwitchId, _botConfig.TwitchClientId, _botConfig.TwitchAccessToken);
            return result.Followers.FirstOrDefault();
        }

        public async Task<ChatterInfoJSON> GetChattersAsync()
        {
            return await TwitchApi.GetChattersAsync(_botConfig.TwitchClientId, _botConfig.TwitchAccessToken);
        }

        public async Task<SubscriptionJSON> CheckSubscriberStatusAsync(string userId)
        {
            RootSubscriptionJSON result = await TwitchApi.CheckSubscriberStatusAsync(userId, _botConfig.TwitchClientId, _botConfig.TwitchAccessToken);
            return result.Subscriptions.FirstOrDefault();
        }

        public async Task<ChannelUpdateJSON> UpdateChannelInfoAsync(ChannelUpdateJSON updateObject)
        {
            return await TwitchApi.UpdateChannelInfoAsync(updateObject, _botConfig.TwitchClientId, _botConfig.TwitchAccessToken);
        }

        public async Task<RootGameJSON> GetGameInfoAsync(string name)
        {
            return await TwitchApi.GetGameInfoAsync(name, _botConfig.TwitchClientId, _botConfig.TwitchAccessToken);
        }
    }
}
