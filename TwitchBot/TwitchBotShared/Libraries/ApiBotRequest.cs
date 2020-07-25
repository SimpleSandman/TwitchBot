using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

using RestSharp;

namespace TwitchBotShared.Libraries
{
    public class ApiBotRequest
    {
        public static async Task<T> GetExecuteAsync<T>(string apiUrlCall)
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
                    IRestResponse<T> response = await client.ExecuteAsync<T>(request, cancellationToken.Token);
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
            RestRequest request = new RestRequest(Method.PUT);
            request.AddHeader("Cache-Control", "no-cache");
            request.AddHeader("Content-Type", "application/json");

            if (updateListString?.Count > 0)
            {
                request.AddParameter("application/json", JsonConvert.SerializeObject(updateListString), ParameterType.RequestBody);
            }

            var cancellationToken = new CancellationTokenSource();
            IRestResponse response;

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
                response = (IRestResponse)ex.Response;
                Console.WriteLine("Error: " + response);
            }

            return default;
        }

        public static async Task<T> PutExecuteAsync<T>(string apiUrlCall, T updateObject)
        {
            // Send HTTP method PUT to base URI in order to change the game
            RestClient client = new RestClient(apiUrlCall);
            RestRequest request = new RestRequest(Method.PUT);
            request.AddHeader("Cache-Control", "no-cache");
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", JsonConvert.SerializeObject(updateObject), ParameterType.RequestBody);

            var cancellationToken = new CancellationTokenSource();
            IRestResponse response;

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
                response = (IRestResponse)ex.Response;
                Console.WriteLine("Error: " + response);
            }

            return default;
        }

        public static async Task<T> PostExecuteAsync<T>(string apiUrlCall, T createObject)
        {
            // Send HTTP method POST to base URI in order to change the game
            RestClient client = new RestClient(apiUrlCall);
            RestRequest request = new RestRequest(Method.POST);
            request.AddHeader("Cache-Control", "no-cache");
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", JsonConvert.SerializeObject(createObject), ParameterType.RequestBody);

            var cancellationToken = new CancellationTokenSource();
            IRestResponse response;

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
                response = (IRestResponse)ex.Response;
                Console.WriteLine("Error: " + response);
            }

            return default;
        }

        public static async Task<T> PatchExecuteAsync<T>(string apiUrlCall, string path, object value)
        {
            // Send HTTP method PATCH to base URI in order to change the game
            RestClient client = new RestClient(apiUrlCall);
            RestRequest request = new RestRequest(Method.PATCH);
            request.AddHeader("Cache-Control", "no-cache");
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", "[{" + $"\"op\": \"replace\", \"path\": \"/{path}\", \"value\": \"{value}\"" + "}]", ParameterType.RequestBody);

            var cancellationToken = new CancellationTokenSource();
            IRestResponse response;

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
                response = (IRestResponse)ex.Response;
                Console.WriteLine("Error: " + response);
            }

            return default;
        }

        public static async Task<T> DeleteExecuteAsync<T>(string apiUrlCall)
        {
            try
            {
                RestClient client = new RestClient(apiUrlCall);
                RestRequest request = new RestRequest(Method.DELETE);
                request.AddHeader("Cache-Control", "no-cache");
                request.AddHeader("Content-Type", "application/json");

                var cancellationToken = new CancellationTokenSource();

                try
                {
                    IRestResponse<T> response = await client.ExecuteAsync<T>(request, cancellationToken.Token);

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
