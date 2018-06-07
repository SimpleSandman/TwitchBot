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
    public static class ApiRequest
    {
        public static async Task<T> GetBotExecuteTaskAsync<T>(string apiUrlCall)
        {
            try
            {
                RestClient client = new RestClient(apiUrlCall);
                RestRequest request = new RestRequest(Method.GET);
                request.AddHeader("Cache-Control", "no-cache");
                request.AddHeader("Content-Type", "application/json");

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

        public static async Task PutBotExecuteTaskAsync<T>(string apiUrlCall, T updateObject)
        {
            // Send HTTP method PUT to base URI in order to change the game
            RestClient client = new RestClient(apiUrlCall);
            RestRequest request = new RestRequest(Method.PUT);
            request.AddHeader("Cache-Control", "no-cache");
            request.AddHeader("Content-Type", "application/json");

            var cancellationToken = new CancellationTokenSource();
            IRestResponse response = null;

            try
            {
                response = await client.ExecuteTaskAsync<T>(request, cancellationToken.Token);
                string statResponse = response.StatusCode.ToString();

                if (statResponse.Contains("OK"))
                {
                    
                }
                else
                {
                    Console.WriteLine(response.Content);
                }
            }
            catch (WebException ex)
            {
                if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.BadRequest)
                {
                    Console.WriteLine("Error 400 detected!!");
                }
                response = (IRestResponse)ex.Response;
                Console.WriteLine("Error: " + response);
            }
        }

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
