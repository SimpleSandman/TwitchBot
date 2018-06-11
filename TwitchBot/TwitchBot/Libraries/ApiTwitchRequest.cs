using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

using RestSharp;

namespace TwitchBot.Libraries
{
    public class ApiTwitchRequest
    {
        public static async Task<T> GetTwitchExecuteTaskAsync<T>(string basicUrl, string clientId)
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

        public static async Task<T> GetTwitchWithOAuthExecuteTaskAsync<T>(string basicUrl, string accessToken, string clientId)
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
