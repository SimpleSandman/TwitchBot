using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

using RestSharp;
using RestSharp.Serializers;

namespace TwitchBotDb
{
    public class ApiBotRequest
    {
        public static async Task<T> GetExecuteAsync<T>(string apiUrlCall)
        {
            try
            {
                RestClient client = new RestClient(apiUrlCall);
                RestRequest request = new RestRequest();
                request.AddHeader("Cache-Control", "no-cache");
                request.AddHeader("Content-Type", "application/json");
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
            catch (WebException ex)
            {
                Console.WriteLine(ex.Message);
            }

            return default;
        }

        public static async Task<T> PutExecuteAsync<T>(string apiUrlCall, List<string> updateListString = null)
        {
            // Send HTTP method PUT to base URI in order to change the game
            RestClient client = new RestClient(apiUrlCall);
            RestRequest request = new RestRequest();
            request.AddHeader("Cache-Control", "no-cache");
            request.AddHeader("Content-Type", "application/json");
            request.Method = Method.Put;

            if (updateListString?.Count > 0)
            {
                request.AddJsonBody(updateListString);
            }

            var cancellationToken = new CancellationTokenSource();
            RestResponse response;

            try
            {
                response = await client.ExecuteAsync<T>(request, cancellationToken.Token);
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
                if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.BadRequest)
                {
                    Console.WriteLine("Error 400 detected!!");
                }
                //response = (RestResponse)ex.Response;
                //Console.WriteLine("Error: " + response);
                Console.WriteLine($"Error: {ex.Message}");
            }

            return default;
        }

        public static async Task<T> PutExecuteAsync<T>(string apiUrlCall, T updateObject)
        {
            // Send HTTP method PUT to base URI in order to change the game
            RestClient client = new RestClient(apiUrlCall);
            RestRequest request = new RestRequest();
            request.AddHeader("Cache-Control", "no-cache");
            request.AddHeader("Content-Type", "application/json");
            request.AddStringBody(JsonConvert.SerializeObject(updateObject), ContentType.Json);
            request.Method = Method.Put;

            var cancellationToken = new CancellationTokenSource();
            RestResponse response;

            try
            {
                response = await client.ExecuteAsync<T>(request, cancellationToken.Token);
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
                if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.BadRequest)
                {
                    Console.WriteLine("Error 400 detected!!");
                }
                //response = (RestResponse)ex.Response;
                //Console.WriteLine("Error: " + response);
                Console.WriteLine($"Error: {ex.Message}");
            }

            return default;
        }

        public static async Task<T> PostExecuteAsync<T>(string apiUrlCall, T createObject)
        {
            // Send HTTP method POST to base URI in order to change the game
            RestClient client = new RestClient(apiUrlCall);
            RestRequest request = new RestRequest();
            request.AddHeader("Cache-Control", "no-cache");
            request.AddHeader("Content-Type", "application/json");
            request.AddStringBody(JsonConvert.SerializeObject(createObject), ContentType.Json);
            request.Method = Method.Post;

            var cancellationToken = new CancellationTokenSource();
            RestResponse response;

            try
            {
                response = await client.ExecuteAsync<T>(request, cancellationToken.Token);
                string statResponse = response.StatusCode.ToString();

                if (statResponse.Contains("OK") || statResponse.Contains("NoContent") || statResponse.Contains("Created"))
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
                if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.BadRequest)
                {
                    Console.WriteLine("Error 400 detected!!");
                }
                //response = (RestResponse)ex.Response;
                //Console.WriteLine("Error: " + response);
                Console.WriteLine($"Error: {ex.Message}");
            }

            return default;
        }

        public static async Task<T> PatchExecuteAsync<T>(string apiUrlCall, string path, object value)
        {
            // Send HTTP method PATCH to base URI in order to change the game
            RestClient client = new RestClient(apiUrlCall);
            RestRequest request = new RestRequest();
            request.AddHeader("Cache-Control", "no-cache");
            request.AddHeader("Content-Type", "application/json");
            request.AddStringBody("[{" + $"\"op\": \"replace\", \"path\": \"/{path}\", \"value\": \"{value}\"" + "}]", ContentType.Json);
            request.Method = Method.Patch;

            var cancellationToken = new CancellationTokenSource();
            RestResponse response;

            try
            {
                response = await client.ExecuteAsync<T>(request, cancellationToken.Token);
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
                if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.BadRequest)
                {
                    Console.WriteLine("Error 400 detected!!");
                }
                //response = (RestResponse)ex.Response;
                //Console.WriteLine("Error: " + response);
                Console.WriteLine($"Error: {ex.Message}");
            }

            return default;
        }

        public static async Task<T> DeleteExecuteAsync<T>(string apiUrlCall)
        {
            try
            {
                RestClient client = new RestClient(apiUrlCall);
                RestRequest request = new RestRequest();
                request.AddHeader("Cache-Control", "no-cache");
                request.AddHeader("Content-Type", "application/json");
                request.Method = Method.Delete;

                var cancellationToken = new CancellationTokenSource();

                try
                {
                    RestResponse<T> response = await client.ExecuteAsync<T>(request, cancellationToken.Token);

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

            return default;
        }
    }
}
