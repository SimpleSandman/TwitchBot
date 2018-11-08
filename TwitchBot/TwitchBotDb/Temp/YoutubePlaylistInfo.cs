using System;
using System.IO;

using Newtonsoft.Json;

namespace TwitchBotDb.Temp
{
    public class YoutubePlaylistInfo
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public int BroadcasterId { get; set; }
        public string TwitchBotApiLink { get; set; }

        private static readonly string _filepath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TwitchBot");
        private static readonly string _filename = "YoutubePlaylistInfo.json";

        /// <summary>
        /// Load playlist info from JSON file stored in local app data
        /// </summary>
        public static YoutubePlaylistInfo Load()
        {
            YoutubePlaylistInfo youtubePlaylistInfo = new YoutubePlaylistInfo();

            if (File.Exists($"{_filepath}\\{_filename}"))
            {
                using (StreamReader file = File.OpenText($"{_filepath}\\{_filename}"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    youtubePlaylistInfo = (YoutubePlaylistInfo)serializer.Deserialize(file, typeof(YoutubePlaylistInfo));
                }
            }

            return youtubePlaylistInfo;
        }

        /// <summary>
        /// Save playlist info into JSON file stored in local app data
        /// </summary>
        public static void Save(string youTubeClientId, string youTubeClientSecret, string twitchBotApiLink, int broadcasterId)
        {
            YoutubePlaylistInfo youtubePlaylistInfo = new YoutubePlaylistInfo
            {
                ClientId = youTubeClientId,
                ClientSecret = youTubeClientSecret,
                BroadcasterId = broadcasterId,
                TwitchBotApiLink = twitchBotApiLink
            };

            Directory.CreateDirectory(_filepath);

            using (StreamWriter file = File.CreateText($"{_filepath}\\{_filename}"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, youtubePlaylistInfo);
            }
        }
    }
}
