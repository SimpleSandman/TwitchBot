using System;
using System.IO;

using Newtonsoft.Json;

namespace TwitchBotDb.Temp
{
    public class CefSharpCache
    {
        public string Url { get; set; }
        public string LastRequestPlaylistVideoId { get; set; }
        public string LastPersonalPlaylistVideoId { get; set; }
        public string RequestPlaylistId { get; set; }

        /// <summary>
        /// Load CefSharp cache info from JSON file stored in local app data
        /// </summary>
        public static CefSharpCache Load()
        {
            string filepath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TwitchBot");
            string filename = "CefSharpCache.json";

            CefSharpCache cefSharpCache = new CefSharpCache();

            using (StreamReader file = File.OpenText($"{filepath}\\{filename}"))
            {
                JsonSerializer serializer = new JsonSerializer();
                cefSharpCache = (CefSharpCache)serializer.Deserialize(file, typeof(CefSharpCache));
            }

            return cefSharpCache;
        }

        /// <summary>
        /// Save CefSharp cache info into JSON file stored in local app data
        /// </summary>
        public static void Save(CefSharpCache cefSharpCache)
        {
            string filepath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TwitchBot");
            string filename = "CefSharpCache.json";

            Directory.CreateDirectory(filepath);

            using (StreamWriter file = File.CreateText($"{filepath}\\{filename}"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, cefSharpCache);
            }
        }
    }
}