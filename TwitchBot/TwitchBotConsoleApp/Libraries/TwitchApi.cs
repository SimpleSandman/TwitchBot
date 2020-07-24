using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using TwitchBotConsoleApp.Models.JSON;

using TwitchBotUtil.Libraries;

namespace TwitchBotConsoleApp.Libraries
{
    public class TwitchApi
    {
        private static BroadcasterSingleton _broadcasterInstance = BroadcasterSingleton.Instance;

        // Reference: https://dev.twitch.tv/docs/v5/reference/channels/#get-channel-by-id
        public static async Task<ChannelJSON> GetBroadcasterChannelById(string clientId)
        {
            return await ApiTwitchRequest.GetExecuteAsync<ChannelJSON>("https://api.twitch.tv/kraken/channels/" + _broadcasterInstance.TwitchId, clientId);
        }

        // Reference: https://dev.twitch.tv/docs/v5/reference/channels/#get-channel-by-id
        public static async Task<ChannelJSON> GetUserChannelById(string userId, string clientId)
        {
            return await ApiTwitchRequest.GetExecuteAsync<ChannelJSON>("https://api.twitch.tv/kraken/channels/" + userId, clientId);
        }

        // Reference: https://dev.twitch.tv/docs/v5/reference/streams/#get-stream-by-user
        public static async Task<RootStreamJSON> GetBroadcasterStream(string clientId)
        {
            return await ApiTwitchRequest.GetExecuteAsync<RootStreamJSON>("https://api.twitch.tv/kraken/streams/" + _broadcasterInstance.TwitchId, clientId);
        }

        // Reference: https://dev.twitch.tv/docs/v5/reference/streams/#get-stream-by-user
        public static async Task<RootStreamJSON> GetUserStream(string userId, string clientId)
        {
            return await ApiTwitchRequest.GetExecuteAsync<RootStreamJSON>("https://api.twitch.tv/kraken/streams/" + userId, clientId);
        }

        // Reference: https://dev.twitch.tv/docs/v5/reference/users/#get-users
        public static async Task<RootUserJSON> GetUsersByLoginName(string loginName, string clientId)
        {
            return await ApiTwitchRequest.GetExecuteAsync<RootUserJSON>("https://api.twitch.tv/kraken/users?login=" + loginName, clientId);
        }

        // Reference: https://dev.twitch.tv/docs/v5/reference/channels/#get-channel-subscribers
        public static async Task<RootSubscriptionJSON> GetSubscribersByChannel(string clientId, string accessToken)
        {
            string apiUriBaseCall = "https://api.twitch.tv/kraken/channels/" + _broadcasterInstance.TwitchId 
                + "/subscriptions?limit=50&direction=desc"; // get 50 newest subscribers

            return await ApiTwitchRequest.GetWithOAuthExecuteAsync<RootSubscriptionJSON>(apiUriBaseCall, accessToken, clientId);
        }

        // Reference: https://dev.twitch.tv/docs/v5/reference/channels/#get-channel-followers
        public static async Task<RootFollowerJSON> GetFollowersByChannel(string clientId)
        {
            string apiUriBaseCall = "https://api.twitch.tv/kraken/channels/" + _broadcasterInstance.TwitchId
                + "/follows?limit=50&direction=desc"; // get 50 newest followers

            return await ApiTwitchRequest.GetExecuteAsync<RootFollowerJSON>(apiUriBaseCall, clientId);
        }

        // Reference: https://dev.twitch.tv/docs/v5/reference/clips/#get-clip
        public static async Task<ClipJSON> GetClip(string clientId, string slug)
        {
            string apiUriBaseCall = "https://api.twitch.tv/kraken/clips/" + slug;

            return await ApiTwitchRequest.GetExecuteAsync<ClipJSON>(apiUriBaseCall, clientId);
        }

        // Reference: https://dev.twitch.tv/docs/v5/reference/users/#check-user-follows-by-channel
        public static async Task<HttpResponseMessage> GetFollowerStatus(string chatterTwitchId, string clientId)
        {
            string apiUriCall = "https://api.twitch.tv/kraken/users/" + chatterTwitchId + "/follows/channels/" 
                + _broadcasterInstance.TwitchId + "?client_id=" + clientId;

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders
                  .Accept
                  .Add(new MediaTypeWithQualityHeaderValue("application/vnd.twitchtv.v5+json"));

            return await client.GetAsync(apiUriCall);
        }

        // Reference: https://discuss.dev.twitch.tv/t/how-can-i-get-chat-list-in-a-channel-by-api/12225
        public static async Task<HttpResponseMessage> GetChatters(string clientId)
        {
            string apiUriCall = "https://tmi.twitch.tv/group/user/" + _broadcasterInstance.Username 
                + "/chatters?client_id=" + clientId;

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders
                    .Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/vnd.twitchtv.v5+json"));

            return await client.GetAsync(apiUriCall);
        }

        // Reference: https://dev.twitch.tv/docs/v5/reference/channels/#check-channel-subscription-by-user
        public static async Task<HttpResponseMessage> CheckSubscriberStatus(string userTwitchId, string clientId, string accessToken)
        {
            string apiUriCall = "https://api.twitch.tv/kraken/channels/" + _broadcasterInstance.TwitchId
                + "/subscriptions/" + userTwitchId + "?client_id=" + clientId;

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders
                  .Accept
                  .Add(new MediaTypeWithQualityHeaderValue("application/vnd.twitchtv.v5+json"));
            client.DefaultRequestHeaders.Add("Authorization", "OAuth " + accessToken);

            return await client.GetAsync(apiUriCall);
        }
    }
}
