using System.Threading.Tasks;

using TwitchBotShared.ApiLibraries;
using TwitchBotShared.ClientLibraries.Singletons;
using TwitchBotShared.Models.JSON;

namespace TwitchBotShared.ClientLibraries
{
    public class TwitchApi
    {
        private const string BASE_API_LINK = "https://api.twitch.tv/helix";
        private static readonly BroadcasterSingleton _broadcasterInstance = BroadcasterSingleton.Instance;

        // Old reference: https://dev.twitch.tv/docs/v5/reference/channels/#get-channel-by-id
        // New reference: https://dev.twitch.tv/docs/api/reference#get-channel-information
        public static async Task<RootChannelJSON> GetBroadcasterChannelByIdAsync(string clientId, string accessToken)
        {
            return await ApiTwitchRequest.GetExecuteAsync<RootChannelJSON>($"{BASE_API_LINK}/channels?broadcaster_id={_broadcasterInstance.TwitchId}", accessToken, clientId);
        }

        // Old reference: https://dev.twitch.tv/docs/v5/reference/channels/#get-channel-by-id
        // New reference: https://dev.twitch.tv/docs/api/reference#get-channel-information
        public static async Task<RootChannelJSON> GetUserChannelByIdAsync(string userId, string clientId, string accessToken)
        {
            return await ApiTwitchRequest.GetExecuteAsync<RootChannelJSON>($"{BASE_API_LINK}/channels?broadcaster_id={userId}", accessToken, clientId);
        }

        // Reference: https://dev.twitch.tv/docs/api/reference#get-users
        public static async Task<RootUserJSON> GetBroadcasterUserByIdAsync(string clientId, string accessToken)
        {
            return await ApiTwitchRequest.GetExecuteAsync<RootUserJSON>($"{BASE_API_LINK}/users?id={_broadcasterInstance.TwitchId}", accessToken, clientId);
        }

        // Old reference: https://dev.twitch.tv/docs/v5/reference/users/#get-users
        // New reference: https://dev.twitch.tv/docs/api/reference#get-users
        public static async Task<RootUserJSON> GetUserByLoginNameAsync(string loginName, string clientId, string accessToken)
        {
            return await ApiTwitchRequest.GetExecuteAsync<RootUserJSON>($"{BASE_API_LINK}/users?login={loginName}", accessToken, clientId);
        }

        // Old reference: https://dev.twitch.tv/docs/v5/reference/streams/#get-stream-by-user
        // New reference: https://dev.twitch.tv/docs/api/reference#get-streams
        public static async Task<RootStreamJSON> GetBroadcasterStreamAsync(string clientId, string accessToken)
        {
            return await ApiTwitchRequest.GetExecuteAsync<RootStreamJSON>($"{BASE_API_LINK}/streams?user_id={_broadcasterInstance.TwitchId}", accessToken, clientId);
        }

        // Old reference: https://dev.twitch.tv/docs/v5/reference/streams/#get-stream-by-user
        // New reference: https://dev.twitch.tv/docs/api/reference#get-streams
        public static async Task<RootStreamJSON> GetUserStreamAsync(string userId, string clientId, string accessToken)
        {
            return await ApiTwitchRequest.GetExecuteAsync<RootStreamJSON>($"{BASE_API_LINK}/streams?user_id={userId}", accessToken, clientId);
        }

        // Old reference: https://dev.twitch.tv/docs/v5/reference/channels/#get-channel-subscribers
        // New reference: https://dev.twitch.tv/docs/api/reference#get-broadcaster-subscriptions
        public static async Task<RootSubscriptionJSON> GetSubscribersByChannelAsync(string clientId, string accessToken)
        {
            string apiUriBaseCall = $"{BASE_API_LINK}/subscriptions?broadcaster_id={_broadcasterInstance.TwitchId}"
                + "&first=50"; // get 50 newest subscribers

            return await ApiTwitchRequest.GetExecuteAsync<RootSubscriptionJSON>(apiUriBaseCall, accessToken, clientId);
        }

        // Old reference: https://dev.twitch.tv/docs/v5/reference/channels/#get-channel-followers
        // New reference: https://dev.twitch.tv/docs/api/reference#get-users-follows
        public static async Task<RootFollowerJSON> GetFollowersByChannelAsync(string clientId, string accessToken)
        {
            string apiUriBaseCall = $"{BASE_API_LINK}/users/follows?to_id={_broadcasterInstance.TwitchId}"
                + "&first=50"; // get 50 newest followers

            return await ApiTwitchRequest.GetExecuteAsync<RootFollowerJSON>(apiUriBaseCall, accessToken, clientId);
        }

        // Old reference: https://dev.twitch.tv/docs/v5/reference/clips/#get-clip
        // New reference: https://dev.twitch.tv/docs/api/reference#get-clips
        public static async Task<RootClipJSON> GetClipAsync(string clientId, string slug, string accessToken)
        {
            string apiUriBaseCall = $"{BASE_API_LINK}/clips?id={slug}";

            return await ApiTwitchRequest.GetExecuteAsync<RootClipJSON>(apiUriBaseCall, accessToken, clientId);
        }

        // Old reference: https://dev.twitch.tv/docs/v5/reference/videos/#get-video
        // New reference: https://dev.twitch.tv/docs/api/reference#get-videos
        public static async Task<RootVideoJSON> GetVideoAsync(string clientId, string videoId, string accessToken)
        {
            string apiUriBaseCall = $"{BASE_API_LINK}/videos?id={videoId}";

            return await ApiTwitchRequest.GetExecuteAsync<RootVideoJSON>(apiUriBaseCall, accessToken, clientId);
        }

        // Old reference: https://dev.twitch.tv/docs/v5/reference/users/#check-user-follows-by-channel
        // New reference: https://dev.twitch.tv/docs/api/reference#get-users-follows
        public static async Task<RootFollowerJSON> GetFollowerStatusAsync(string chatterTwitchId, string clientId, string accessToken)
        {
            string apiUriCall = $"{BASE_API_LINK}/users/follows?to_id={_broadcasterInstance.TwitchId}&from_id={chatterTwitchId}";

            return await ApiTwitchRequest.GetExecuteAsync<RootFollowerJSON>(apiUriCall, accessToken, clientId);
        }

        // Reference: https://discuss.dev.twitch.tv/t/how-can-i-get-chat-list-in-a-channel-by-api/12225
        public static async Task<ChatterInfoJSON> GetChattersAsync(string clientId, string accessToken)
        {
            //string apiUriCall = "https://tmi.twitch.tv/group/user/" + _broadcasterInstance.Username
            //    + "/chatters?client_id=" + clientId; 
            string apiUriCall = $"https://tmi.twitch.tv/group/user/" + _broadcasterInstance.Username + "/chatters";

            //HttpClient client = new HttpClient();
            //client.DefaultRequestHeaders
            //        .Accept
            //        .Add(new MediaTypeWithQualityHeaderValue("application/vnd.twitchtv.v5+json"));

            //return await client.GetAsync(apiUriCall);

            return await ApiTwitchRequest.GetExecuteAsync<ChatterInfoJSON>(apiUriCall, accessToken, clientId);
        }

        // Old reference: https://dev.twitch.tv/docs/v5/reference/channels/#check-channel-subscription-by-user
        // New reference: https://dev.twitch.tv/docs/api/reference#check-user-subscription
        public static async Task<RootSubscriptionJSON> CheckSubscriberStatusAsync(string userTwitchId, string clientId, string accessToken)
        {
            string apiUriCall = $"{BASE_API_LINK}/subscriptions/user?broadcaster_id={_broadcasterInstance.TwitchId}&user_id={userTwitchId}";

            return await ApiTwitchRequest.GetExecuteAsync<RootSubscriptionJSON>(apiUriCall, accessToken, clientId);
        }

        // Reference: https://dev.twitch.tv/docs/api/reference#modify-channel-information
        public static async Task<ChannelUpdateJSON> UpdateChannelInfoAsync(ChannelUpdateJSON updateObject, string clientId, string accessToken)
        {
            string apiUriCall = $"{BASE_API_LINK}/channels?broadcaster_id={_broadcasterInstance.TwitchId}";

            return await ApiTwitchRequest.PatchExecuteAsync(apiUriCall, accessToken, clientId, updateObject);
        }

        // Reference: https://dev.twitch.tv/docs/api/reference#get-games
        public static async Task<RootGameJSON> GetGameInfoAsync(string name, string clientId, string accessToken)
        {
            string apiUriCall = $"{BASE_API_LINK}/games?name={name}";

            return await ApiTwitchRequest.GetExecuteAsync<RootGameJSON>(apiUriCall, accessToken, clientId);
        }
    }
}
