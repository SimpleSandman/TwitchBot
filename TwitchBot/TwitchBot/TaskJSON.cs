using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TwitchBot.Configuration;

namespace TwitchBot
{
    public class TaskJSON
    {
        public static async Task<ChannelJSON> GetChannel(string broadcasterName, string clientID)
        {
            using (HttpClient client = new HttpClient())
            {
                string body = await client.GetStringAsync("https://api.twitch.tv/kraken/channels/" + broadcasterName + "?client_id=" + clientID);
                ChannelJSON response = JsonConvert.DeserializeObject<ChannelJSON>(body);
                return response;
            }
        }

        public static async Task<RootStreamJSON> GetStream(string broadcasterName, string clientID)
        {
            using (HttpClient client = new HttpClient())
            {
                string body = await client.GetStringAsync("https://api.twitch.tv/kraken/streams/" + broadcasterName + "?client_id=" + clientID);
                RootStreamJSON response = JsonConvert.DeserializeObject<RootStreamJSON>(body);
                return response;
            }
        }

        public static async Task<HttpResponseMessage> GetFollowerStatus(string broadcasterName, string clientID, string chatterName)
        {
            string apiUriCall = "https://api.twitch.tv/kraken/users/" + chatterName + "/follows/channels/" + broadcasterName + "?client_id=" + clientID;

            using (HttpClient client = new HttpClient())
            {
                return await client.GetAsync(apiUriCall);
            }
        }

        public static async Task<ChatterInfo> GetChatters(string broadcasterName, string clientID)
        {
            using (HttpClient client = new HttpClient())
            {
                string body = await client.GetStringAsync("https://tmi.twitch.tv/group/user/" + broadcasterName + "/chatters?client_id=" + clientID);
                ChatterInfo response = JsonConvert.DeserializeObject<ChatterInfo>(body);
                return response;
            }
        }
    }
}
