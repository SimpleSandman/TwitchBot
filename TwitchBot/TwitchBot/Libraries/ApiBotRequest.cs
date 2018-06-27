using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

using RestSharp;

namespace TwitchBot.Libraries
{
    public class ApiBotRequest
    {
        public static async Task<T> GetExecuteTaskAsync<T>(string apiUrlCall)
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

        public static async Task<T> PutExecuteTaskAsync<T>(string apiUrlCall, List<string> updateListString = null)
        {
            // Send HTTP method PUT to base URI in order to change the game
            RestClient client = new RestClient(apiUrlCall);
            RestRequest request = new RestRequest(Method.PUT);
            request.AddHeader("Cache-Control", "no-cache");
            request.AddHeader("Content-Type", "application/json");

            if (updateListString?.Count > 0)
            {
                request.AddParameter(new Parameter
                {
                    ContentType = "application/json",
                    Name = "JSONPAYLOAD",
                    Type = ParameterType.RequestBody,
                    Value = JsonConvert.SerializeObject(updateListString)
                });
            }

            var cancellationToken = new CancellationTokenSource();
            IRestResponse response = null;

            try
            {
                response = await client.ExecuteTaskAsync<T>(request, cancellationToken.Token);
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

            return default(T);
        }

        public static async Task<T> PutExecuteTaskAsync<T>(string apiUrlCall, T updateObject)
        {
            // Send HTTP method PUT to base URI in order to change the game
            RestClient client = new RestClient(apiUrlCall);
            RestRequest request = new RestRequest(Method.PUT);
            request.AddHeader("Cache-Control", "no-cache");
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter(new Parameter
            {
                ContentType = "application/json",
                Name = "JSONPAYLOAD",
                Type = ParameterType.RequestBody,
                Value = JsonConvert.SerializeObject(updateObject)
            });

            var cancellationToken = new CancellationTokenSource();
            IRestResponse response = null;

            try
            {
                response = await client.ExecuteTaskAsync<T>(request, cancellationToken.Token);
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

            return default(T);
        }

        public static async Task<T> PostExecuteTaskAsync<T>(string apiUrlCall, T createObject)
        {
            // Send HTTP method PUT to base URI in order to change the game
            RestClient client = new RestClient(apiUrlCall);
            RestRequest request = new RestRequest(Method.POST);
            request.AddHeader("Cache-Control", "no-cache");
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter(new Parameter
            {
                ContentType = "application/json",
                Name = "JSONPAYLOAD",
                Type = ParameterType.RequestBody,
                Value = JsonConvert.SerializeObject(createObject)
            });

            var cancellationToken = new CancellationTokenSource();
            IRestResponse response = null;

            try
            {
                response = await client.ExecuteTaskAsync<T>(request, cancellationToken.Token);
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

            return default(T);
        }

        public static async Task<T> PatchExecuteTaskAsync<T>(string apiUrlCall, string path, object value)
        {
            // Send HTTP method PUT to base URI in order to change the game
            RestClient client = new RestClient(apiUrlCall);
            RestRequest request = new RestRequest(Method.PATCH);
            request.AddHeader("Cache-Control", "no-cache");
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter(new Parameter
            {
                ContentType = "application/json",
                Name = "JSONPAYLOAD",
                Type = ParameterType.RequestBody,
                Value = "[{" + $"\"op\": \"replace\", \"path\": \"/{path}\", \"value\": \"{value}\"" + "}]"
            });

            var cancellationToken = new CancellationTokenSource();
            IRestResponse response = null;

            try
            {
                response = await client.ExecuteTaskAsync<T>(request, cancellationToken.Token);
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

            return default(T);
        }

        public static async Task<T> DeleteExecuteTaskAsync<T>(string apiUrlCall)
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
