using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

using RestSharp;

using TwitchBot.Models.JSON;

namespace TwitchBot.Libraries
{
    public class TwitchApi
    {
        private static Broadcaster _broadcasterInstance = Broadcaster.Instance;

        public static async Task<ChannelJSON> GetBroadcasterChannelById(string clientId)
        {
            return await GetRequestExecuteTaskAsync<ChannelJSON>("https://api.twitch.tv/kraken/channels/" + _broadcasterInstance.TwitchId, clientId);
        }

        public static async Task<ChannelJSON> GetUserChannelById(string userId, string clientId)
        {
            return await GetRequestExecuteTaskAsync<ChannelJSON>("https://api.twitch.tv/kraken/channels/" + userId, clientId);
        }

        public static async Task<RootStreamJSON> GetBroadcasterStream(string clientId)
        {
            return await GetRequestExecuteTaskAsync<RootStreamJSON>("https://api.twitch.tv/kraken/streams/" + _broadcasterInstance.TwitchId, clientId);
        }

        public static async Task<RootStreamJSON> GetUserStream(string userId, string clientId)
        {
            return await GetRequestExecuteTaskAsync<RootStreamJSON>("https://api.twitch.tv/kraken/streams/" + userId, clientId);
        }

        public static async Task<RootUserJSON> GetUsersByLoginName(string loginName, string clientId)
        {
            return await GetRequestExecuteTaskAsync<RootUserJSON>("https://api.twitch.tv/kraken/users?login=" + loginName, clientId);
        }

        public static async Task<RootSubscriptionJSON> GetSubscribersByChannel(string clientId, string accessToken)
        {
            string apiUriBaseCall = "https://api.twitch.tv/kraken/channels/" + _broadcasterInstance.TwitchId 
                + "/subscriptions?limit=50&direction=desc"; // get 50 newest subscribers

            return await GetRequestWithOAuthExecuteTaskAsync<RootSubscriptionJSON>(apiUriBaseCall, accessToken, clientId);
        }

        public static async Task<RootFollowerJSON> GetFollowersByChannel(string clientId)
        {
            string apiUriBaseCall = "https://api.twitch.tv/kraken/channels/" + _broadcasterInstance.TwitchId
                + "/follows?limit=50&direction=desc"; // get 50 newest followers

            return await GetRequestExecuteTaskAsync<RootFollowerJSON>(apiUriBaseCall, clientId);
        }

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

        private static async Task<T> GetRequestExecuteTaskAsync<T>(string basicUrl, string clientId)
        {
            try
            {
                RestClient client = new RestClient(basicUrl);
                RestRequest request = new RestRequest(Method.GET);
                request.AddHeader("Cache-Control", "no-cache");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Accept", "application/vnd.twitchtv.v5+json");
                request.AddHeader("Client-ID", clientId);

                var cancellationToken = new CancellationTokenSource();

                try
                {
                    IRestResponse<T> response = await client.ExecuteTaskAsync<T>(request, cancellationToken.Token);

                    return JsonConvert.DeserializeObject<T>(response.Content);
                }
                catch (WebException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            catch (WebException ex)
            {
                Console.WriteLine(ex.Message);
            }

            return default(T);
        }

        private static async Task<T> GetRequestWithOAuthExecuteTaskAsync<T>(string basicUrl, string accessToken, string clientId)
        {
            try
            {
                RestClient client = new RestClient(basicUrl);
                RestRequest request = new RestRequest(Method.GET);
                request.AddHeader("Cache-Control", "no-cache");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", "OAuth " + accessToken);
                request.AddHeader("Accept", "application/vnd.twitchtv.v5+json");
                request.AddHeader("Client-ID", clientId);

                var cancellationToken = new CancellationTokenSource();

                try
                {
                    IRestResponse<T> response = await client.ExecuteTaskAsync<T>(request, cancellationToken.Token);

                    return JsonConvert.DeserializeObject<T>(response.Content);
                }
                catch (WebException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            catch (WebException ex)
            {
                Console.WriteLine(ex.Message);
            }

            return default(T);
        }
    }
}
