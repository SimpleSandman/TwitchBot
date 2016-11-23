using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot
{
    public class TaskJSON
    {
        public static async Task<ChannelJSON> GetChannel()
        {
            using (HttpClient client = new HttpClient())
            {
                string body = await client.GetStringAsync("https://api.twitch.tv/kraken/channels/" + Program._strBroadcasterName + "?client_id=" + Properties.Settings.Default.twitchClientID);
                ChannelJSON response = JsonConvert.DeserializeObject<ChannelJSON>(body);
                return response;
            }
        }

        public static async Task<RootStreamJSON> GetStream()
        {
            using (HttpClient client = new HttpClient())
            {
                string body = await client.GetStringAsync("https://api.twitch.tv/kraken/streams/" + Program._strBroadcasterName + "?client_id=" + Properties.Settings.Default.twitchClientID);
                RootStreamJSON response = JsonConvert.DeserializeObject<RootStreamJSON>(body);
                return response;
            }
        }

        public static async Task<FollowerInfo> GetFollowerInfo()
        {
            using (HttpClient client = new HttpClient())
            {
                string body = await client.GetStringAsync("https://api.twitch.tv/kraken/channels/" + Program._strBroadcasterName + "/follows?limit=" + Program._intFollowers + "&client_id=" + Properties.Settings.Default.twitchClientID);
                FollowerInfo response = JsonConvert.DeserializeObject<FollowerInfo>(body);
                return response;
            }
        }

        public static async Task<ChatterInfo> GetChatters()
        {
            using (HttpClient client = new HttpClient())
            {
                string body = await client.GetStringAsync("https://tmi.twitch.tv/group/user/" + Program._strBroadcasterName + "/chatters");
                ChatterInfo response = JsonConvert.DeserializeObject<ChatterInfo>(body);
                return response;
            }
        }
    }
}
