using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using Newtonsoft.Json;
using RestSharp;

using TwitchBot.Models.JSON;

namespace TwitchBot.Libraries
{
    public class TwitchApi
    {
        private static Broadcaster _broadcasterInstance = Broadcaster.Instance;

        public static async Task<ChannelJSON> GetChannelById(string clientId)
        {
            try
            {
                RestClient client = new RestClient("https://api.twitch.tv/kraken/channels/" + _broadcasterInstance.TwitchId);
                RestRequest request = new RestRequest(Method.GET);
                request.AddHeader("Cache-Control", "no-cache");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Accept", "application/vnd.twitchtv.v5+json");
                request.AddHeader("Client-ID", clientId);

                var taskCompletionSource = new TaskCompletionSource<string>();

                try
                {
                    client.ExecuteAsync<ChannelJSON>(request, response =>
                    {
                        taskCompletionSource.SetResult(response.Content);
                    });

                    return JsonConvert.DeserializeObject<ChannelJSON>(await taskCompletionSource.Task);
                }
                catch (WebException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        public static async Task<RootStreamJSON> GetStream(string clientId)
        {
            try
            {
                RestClient client = new RestClient("https://api.twitch.tv/kraken/streams/" + _broadcasterInstance.TwitchId);
                RestRequest request = new RestRequest(Method.GET);
                request.AddHeader("Cache-Control", "no-cache");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Accept", "application/vnd.twitchtv.v5+json");
                request.AddHeader("Client-ID", clientId);

                var taskCompletionSource = new TaskCompletionSource<string>();

                try
                {
                    client.ExecuteAsync<RootStreamJSON>(request, response =>
                    {
                        taskCompletionSource.SetResult(response.Content);
                    });

                    return JsonConvert.DeserializeObject<RootStreamJSON>(await taskCompletionSource.Task);
                }
                catch (WebException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        public static async Task<HttpResponseMessage> GetFollowerStatus(string chatterTwitchId, string clientId)
        {
            string apiUriCall = "https://api.twitch.tv/kraken/users/" + chatterTwitchId + "/follows/channels/" + _broadcasterInstance.TwitchId + "?client_id=" + clientId;
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders
                  .Accept
                  .Add(new MediaTypeWithQualityHeaderValue("application/vnd.twitchtv.v5+json"));

            return await client.GetAsync(apiUriCall);
        }

        public static async Task<RootUserJSON> GetUsersByLoginName(string loginName, string clientId)
        {
            try
            {
                RestClient client = new RestClient("https://api.twitch.tv/kraken/users?login=" + loginName);
                RestRequest request = new RestRequest(Method.GET);
                request.AddHeader("Cache-Control", "no-cache");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Accept", "application/vnd.twitchtv.v5+json");
                request.AddHeader("Client-ID", clientId);

                var taskCompletionSource = new TaskCompletionSource<string>();

                try
                {
                    client.ExecuteAsync<RootUserJSON>(request, response =>
                    {
                        taskCompletionSource.SetResult(response.Content);
                    });

                    return JsonConvert.DeserializeObject<RootUserJSON>(await taskCompletionSource.Task);
                }
                catch (WebException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        public static async Task<ChatterInfoJSON> GetChatters(string clientId)
        {
            try
            {
                string body = await Program.HttpClient.GetStringAsync("https://tmi.twitch.tv/group/user/" + _broadcasterInstance.Username + "/chatters?client_id=" + clientId);
                ChatterInfoJSON response = JsonConvert.DeserializeObject<ChatterInfoJSON>(body);
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new ChatterInfoJSON
                {
                    Chatters = new Chatters
                    {
                        Viewers = new List<string>(),
                        Moderators = new List<string>(),
                        Admins = new List<string>(),
                        GlobalMods = new List<string>(),
                        Staff = new List<string>()
                    },
                    ChatterCount = 0
                };
            }
        }
    }
}
