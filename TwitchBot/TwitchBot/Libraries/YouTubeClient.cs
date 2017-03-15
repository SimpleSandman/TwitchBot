using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

using Newtonsoft.Json;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

using TwitchBot.Models.JSON;
using TwitchBot.Configuration;

namespace TwitchBot.Libraries
{
    public class YouTubeClient
    {
        private TwitchBotConfigurationSection _botConfig;
        private System.Configuration.Configuration _appConfig;
        private ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        public YouTubeClient(TwitchBotConfigurationSection botConfig, System.Configuration.Configuration appConfig)
        {
            _botConfig = botConfig;
            _appConfig = appConfig;
        }

        /// <summary>
        /// Get access and refresh tokens from user's account
        /// </summary>
        public void RetrieveTokens()
        {
            try
            {
                RestClient client = new RestClient("https://accounts.google.com/o/oauth2/token");
                RestRequest request = new RestRequest(Method.POST);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("content-type", "application/x-www-form-urlencoded");
                request.AddHeader("host", "accounts.google.com");
                request.AddParameter(
                    "application/x-www-form-urlencoded",
                    $"code={_botConfig.YouTubeCode}"
                        + $"&client_id={_botConfig.YouTubeClientId}" // ToDo: Store away from user config (debugging only)
                        + $"&client_secret={_botConfig.YouTubeClientSecret}" // ToDo: Store away from user config (debugging only)
                        + "&redirect_uri=http://localhost"
                        + "&grant_type=authorization_code", 
                    ParameterType.RequestBody);

                IRestResponse response = null;
                try
                {
                    response = client.Execute(request);
                    string statResponse = response.StatusCode.ToString();
                    if (statResponse.Contains("OK"))
                    {
                        YouTubeAuthJSON auth = JsonConvert.DeserializeObject<YouTubeAuthJSON>(response.Content);
                        _botConfig.YouTubeAccessToken = auth.access_token;
                        _botConfig.YouTubeRefreshToken = auth.refresh_token;
                        _appConfig.Save();

                        Console.WriteLine("YouTube account permission granted");
                    }
                    else
                        Console.WriteLine(response.ErrorMessage);
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
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "YouTube", "Connect(string)", false);
            }
        }
    }
}
