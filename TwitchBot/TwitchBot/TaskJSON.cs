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
        public static async Task<ChannelJSON> GetChannel(string broadcasterName)
        {
            using (HttpClient client = new HttpClient())
            {
                string body = await client.GetStringAsync("https://api.twitch.tv/kraken/channels/" + broadcasterName);
                ChannelJSON response = JsonConvert.DeserializeObject<ChannelJSON>(body);
                return response;
            }
        }

        public static async Task<RootStreamJSON> GetStream(string broadcasterName)
        {
            using (HttpClient client = new HttpClient())
            {
                string body = await client.GetStringAsync("https://api.twitch.tv/kraken/streams/" + broadcasterName);
                RootStreamJSON response = JsonConvert.DeserializeObject<RootStreamJSON>(body);
                return response;
            }
        }

        public static async Task<FollowerInfo> GetFollowerInfo(string broadcasterName, int followers)
        {
            using (HttpClient client = new HttpClient())
            {
                string body = await client.GetStringAsync("https://api.twitch.tv/kraken/channels/" + broadcasterName + "/follows?limit=" + followers);
                FollowerInfo response = JsonConvert.DeserializeObject<FollowerInfo>(body);
                return response;
            }
        }

        public static async Task<ChatterInfo> GetChatters(string broadcasterName)
        {
            using (HttpClient client = new HttpClient())
            {
                string body = await client.GetStringAsync("https://tmi.twitch.tv/group/user/" + broadcasterName + "/chatters");
                ChatterInfo response = JsonConvert.DeserializeObject<ChatterInfo>(body);
                return response;
            }
        }
    }
}
