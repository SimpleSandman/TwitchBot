using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

using RestSharp;
using RestSharp.Serializers;

namespace TwitchBotShared.ApiLibraries
{
    public class ApiTwitchRequest
    {
        public static async Task<T> GetExecuteAsync<T>(string basicUrl, string accessToken, string clientId)
        {
            try
            {
                RestClient client = new RestClient(basicUrl);
                RestRequest request = new RestRequest();
                request.AddHeader("Cache-Control", "no-cache");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", $"Bearer {accessToken}");
                request.AddHeader("Client-ID", clientId);
                request.Method = Method.Get;

                var cancellationToken = new CancellationTokenSource();

                try
                {
                    RestResponse<T> response = await client.ExecuteAsync<T>(request, cancellationToken.Token);
                    string statResponse = response.StatusCode.ToString();

                    if (statResponse.Contains("OK") || statResponse.Contains("NoContent"))
                    {
                        return JsonConvert.DeserializeObject<T>(response.Content);
                    }
                    else
                    {
                        Console.WriteLine(response.Content);
                    }
                }
                catch (WebException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return default;
        }

        public static async Task<T> PatchExecuteAsync<T>(string basicUrl, string accessToken, string clientId, T updateObject)
        {
            try
            {
                RestClient client = new RestClient(basicUrl);
                RestRequest request = new RestRequest();
                request.AddHeader("Cache-Control", "no-cache");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", $"Bearer {accessToken}");
                request.AddHeader("Client-ID", clientId);
                request.AddStringBody(JsonConvert.SerializeObject(updateObject), ContentType.Json);
                request.Method = Method.Patch;

                var cancellationToken = new CancellationTokenSource();

                try
                {
                    RestResponse<T> response = await client.ExecuteAsync<T>(request, cancellationToken.Token);
                    string statResponse = response.StatusCode.ToString();

                    if (statResponse.Contains("OK") || statResponse.Contains("NoContent"))
                    {
                        return JsonConvert.DeserializeObject<T>(response.Content);
                    }
                    else
                    {
                        Console.WriteLine(response.Content);
                    }
                }
                catch (WebException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return default;
        }
    }
}
