using System;
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
        private static readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        // Old reference: https://dev.twitch.tv/docs/v5/reference/channels/#get-channel-by-id
        // New reference: https://dev.twitch.tv/docs/api/reference#get-channel-information
        public static async Task<RootChannelJSON> GetBroadcasterChannelByIdAsync(string clientId, string accessToken)
        {
            try
            {
                return await ApiTwitchRequest.GetExecuteAsync<RootChannelJSON>($"{BASE_API_LINK}/channels?broadcaster_id={_broadcasterInstance.TwitchId}", accessToken, clientId);
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "TwitchApi", "GetBroadcasterChannelByIdAsync(string, string)", false);
            }

            return new RootChannelJSON();
        }

        // Old reference: https://dev.twitch.tv/docs/v5/reference/channels/#get-channel-by-id
        // New reference: https://dev.twitch.tv/docs/api/reference#get-channel-information
        public static async Task<RootChannelJSON> GetUserChannelByIdAsync(string userId, string clientId, string accessToken)
        {
            try
            {
                return await ApiTwitchRequest.GetExecuteAsync<RootChannelJSON>($"{BASE_API_LINK}/channels?broadcaster_id={userId}", accessToken, clientId);
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "TwitchApi", "GetUserChannelByIdAsync(string, string, string)", false);
            }

            return new RootChannelJSON();
        }

        // Reference: https://dev.twitch.tv/docs/api/reference#get-users
        public static async Task<RootUserJSON> GetBroadcasterUserByIdAsync(string clientId, string accessToken)
        {
            try
            {
                return await ApiTwitchRequest.GetExecuteAsync<RootUserJSON>($"{BASE_API_LINK}/users?id={_broadcasterInstance.TwitchId}", accessToken, clientId);
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "TwitchApi", "GetBroadcasterUserByIdAsync(string, string)", false);
            }

            return new RootUserJSON();
        }

        // Old reference: https://dev.twitch.tv/docs/v5/reference/users/#get-users
        // New reference: https://dev.twitch.tv/docs/api/reference#get-users
        public static async Task<RootUserJSON> GetUserByLoginNameAsync(string loginName, string clientId, string accessToken)
        {
            try
            {
                return await ApiTwitchRequest.GetExecuteAsync<RootUserJSON>($"{BASE_API_LINK}/users?login={loginName}", accessToken, clientId);
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "TwitchApi", "GetUserByLoginNameAsync(string, string)", false);
            }

            return new RootUserJSON();
        }

        // Old reference: https://dev.twitch.tv/docs/v5/reference/streams/#get-stream-by-user
        // New reference: https://dev.twitch.tv/docs/api/reference#get-streams
        public static async Task<RootStreamJSON> GetBroadcasterStreamAsync(string clientId, string accessToken)
        {
            try
            {
                return await ApiTwitchRequest.GetExecuteAsync<RootStreamJSON>($"{BASE_API_LINK}/streams?user_id={_broadcasterInstance.TwitchId}", accessToken, clientId);
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "TwitchApi", "GetBroadcasterStreamAsync(string, string)", false);
            }

            return new RootStreamJSON();
        }

        // Old reference: https://dev.twitch.tv/docs/v5/reference/streams/#get-stream-by-user
        // New reference: https://dev.twitch.tv/docs/api/reference#get-streams
        public static async Task<RootStreamJSON> GetUserStreamAsync(string userId, string clientId, string accessToken)
        {
            try
            {
                return await ApiTwitchRequest.GetExecuteAsync<RootStreamJSON>($"{BASE_API_LINK}/streams?user_id={userId}", accessToken, clientId);
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "TwitchApi", "GetUserStreamAsync(string, string, string)", false);
            }
            
            return new RootStreamJSON();
        }

        // Old reference: https://dev.twitch.tv/docs/v5/reference/channels/#get-channel-subscribers
        // New reference: https://dev.twitch.tv/docs/api/reference#get-broadcaster-subscriptions
        public static async Task<RootSubscriptionJSON> GetSubscribersByChannelAsync(string clientId, string accessToken)
        {
            try
            {
                string apiUriBaseCall = $"{BASE_API_LINK}/subscriptions?broadcaster_id={_broadcasterInstance.TwitchId}"
                    + "&first=50"; // get 50 newest subscribers

                return await ApiTwitchRequest.GetExecuteAsync<RootSubscriptionJSON>(apiUriBaseCall, accessToken, clientId);
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "TwitchApi", "GetSubscribersByChannelAsync(string, string)", false);
            }
            
            return new RootSubscriptionJSON();
        }

        // Old reference: https://dev.twitch.tv/docs/v5/reference/channels/#get-channel-followers
        // New reference: https://dev.twitch.tv/docs/api/reference#get-users-follows
        public static async Task<RootFollowerJSON> GetFollowersByChannelAsync(string clientId, string accessToken)
        {
            try
            {
                string apiUriBaseCall = $"{BASE_API_LINK}/users/follows?to_id={_broadcasterInstance.TwitchId}"
                + "&first=50"; // get 50 newest followers

                return await ApiTwitchRequest.GetExecuteAsync<RootFollowerJSON>(apiUriBaseCall, accessToken, clientId);
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "TwitchApi", "GetFollowersByChannelAsync(string, string)", false);
            }

            return new RootFollowerJSON();
        }

        // Old reference: https://dev.twitch.tv/docs/v5/reference/clips/#get-clip
        // New reference: https://dev.twitch.tv/docs/api/reference#get-clips
        public static async Task<RootClipJSON> GetClipAsync(string clientId, string slug, string accessToken)
        {
            try
            {
                string apiUriBaseCall = $"{BASE_API_LINK}/clips?id={slug}";

                return await ApiTwitchRequest.GetExecuteAsync<RootClipJSON>(apiUriBaseCall, accessToken, clientId);
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "TwitchApi", "GetClipAsync(string, string, string)", false);
            }

            return new RootClipJSON();
        }

        // Old reference: https://dev.twitch.tv/docs/v5/reference/videos/#get-video
        // New reference: https://dev.twitch.tv/docs/api/reference#get-videos
        public static async Task<RootVideoJSON> GetVideoAsync(string clientId, string videoId, string accessToken)
        {
            try
            {
                string apiUriBaseCall = $"{BASE_API_LINK}/videos?id={videoId}";

                return await ApiTwitchRequest.GetExecuteAsync<RootVideoJSON>(apiUriBaseCall, accessToken, clientId);
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "TwitchApi", "GetVideoAsync(string, string, string)", false);
            }
            
            return new RootVideoJSON();
        }

        // Old reference: https://dev.twitch.tv/docs/v5/reference/users/#check-user-follows-by-channel
        // New reference: https://dev.twitch.tv/docs/api/reference#get-users-follows
        public static async Task<RootFollowerJSON> GetFollowerStatusAsync(string chatterTwitchId, string clientId, string accessToken)
        {
            try
            {
                string apiUriCall = $"{BASE_API_LINK}/users/follows?to_id={_broadcasterInstance.TwitchId}&from_id={chatterTwitchId}";

                return await ApiTwitchRequest.GetExecuteAsync<RootFollowerJSON>(apiUriCall, accessToken, clientId);
            }
            catch (Exception ex) 
            {
                await _errHndlrInstance.LogErrorAsync(ex, "TwitchApi", "GetFollowerStatusAsync(string, string, string)", false);
            }

            return new RootFollowerJSON();
        }

        // Reference: https://discuss.dev.twitch.tv/t/how-can-i-get-chat-list-in-a-channel-by-api/12225
        public static async Task<ChatterInfoJSON> GetChattersAsync(string clientId, string accessToken)
        {
            try
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
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "TwitchApi", "GetChattersAsync(string, string)", false);
            }
                
            return new ChatterInfoJSON();
        }

        // Old reference: https://dev.twitch.tv/docs/v5/reference/channels/#check-channel-subscription-by-user
        // New reference: https://dev.twitch.tv/docs/api/reference#get-broadcaster-subscriptions
        public static async Task<RootSubscriptionJSON> CheckSubscriberStatusAsync(string userTwitchId, string clientId, string accessToken)
        {
            try
            {
                string apiUriCall = $"{BASE_API_LINK}/subscriptions?broadcaster_id={_broadcasterInstance.TwitchId}&user_id={userTwitchId}";

                return await ApiTwitchRequest.GetExecuteAsync<RootSubscriptionJSON>(apiUriCall, accessToken, clientId);
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "TwitchApi", "CheckSubscriberStatusAsync(string, string, string)", false);
            }

            return new RootSubscriptionJSON();
        }

        // Reference: https://dev.twitch.tv/docs/api/reference#modify-channel-information
        public static async Task<ChannelUpdateJSON> UpdateChannelInfoAsync(ChannelUpdateJSON updateObject, string clientId, string accessToken)
        {
            try
            {
                string apiUriCall = $"{BASE_API_LINK}/channels?broadcaster_id={_broadcasterInstance.TwitchId}";

                return await ApiTwitchRequest.PatchExecuteAsync(apiUriCall, accessToken, clientId, updateObject);
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "TwitchApi", "UpdateChannelInfoAsync(ChannelUpdateJSON, string, string)", false);
            }
            
            return new ChannelUpdateJSON();
        }

        // Reference: https://dev.twitch.tv/docs/api/reference#get-games
        public static async Task<RootGameJSON> GetGameInfoAsync(string name, string clientId, string accessToken)
        {
            try
            {
                string apiUriCall = $"{BASE_API_LINK}/games?name={name}";

                return await ApiTwitchRequest.GetExecuteAsync<RootGameJSON>(apiUriCall, accessToken, clientId);
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "TwitchApi", "GetGameInfoAsync(ChannelUpdateJSON, string, string)", false);
            }

            return new RootGameJSON();
        }
    }
}
