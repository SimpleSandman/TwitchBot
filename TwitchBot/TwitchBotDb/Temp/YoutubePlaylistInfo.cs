using System;
using System.IO;

using Newtonsoft.Json;

namespace TwitchBotDb.Temp
{
    public class YoutubePlaylistInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }

        /// <summary>
        /// Load playlist info from JSON file stored in local app data
        /// </summary>
        public static YoutubePlaylistInfo Load()
        {
            string filepath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TwitchBot");
            string filename = "YoutubePlaylistInfo.json";

            YoutubePlaylistInfo youtubePlaylistInfo = new YoutubePlaylistInfo();

            if (File.Exists($"{filepath}\\{filename}"))
            {
                using (StreamReader file = File.OpenText($"{filepath}\\{filename}"))
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
        public static void Save(string playlistId, string playlistName, string youTubeClientId, string youTubeClientSecret)
        {
            YoutubePlaylistInfo youtubePlaylistInfo = new YoutubePlaylistInfo
            {
                Id = playlistId,
                Name = playlistName,
                ClientId = youTubeClientId,
                ClientSecret = youTubeClientSecret
            };

            string filepath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TwitchBot");
            string filename = "YoutubePlaylistInfo.json";

            Directory.CreateDirectory(filepath);

            using (StreamWriter file = File.CreateText($"{filepath}\\{filename}"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, youtubePlaylistInfo);
            }
        }
    }
}
