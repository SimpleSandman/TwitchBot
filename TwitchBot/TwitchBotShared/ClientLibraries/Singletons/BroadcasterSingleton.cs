using System;
using System.Threading.Tasks;

using TwitchBotDb;
using TwitchBotDb.Models;

namespace TwitchBotShared.ClientLibraries.Singletons
{
    public class BroadcasterSingleton
    {
        public string Username { get; set; }
        public int DatabaseId { get; set; }
        public string TwitchId { get; set; }

        private static volatile BroadcasterSingleton _instance;
        private static object _syncRoot = new Object();

        private BroadcasterSingleton() { }

        public static BroadcasterSingleton Instance
        {
            get
            {
                // first check
                if (_instance == null)
                {
                    lock (_syncRoot)
                    {
                        // second check
                        if (_instance == null)
                            _instance = new BroadcasterSingleton();
                    }
                }

                return _instance;
            }
        }

        public async Task FindBroadcaster(string twitchId, string twitchBotApiLink, string username = "")
        {
            Broadcaster broadcaster = null;

            if (!string.IsNullOrEmpty(username))
                broadcaster = await ApiBotRequest.GetExecuteAsync<Broadcaster>(twitchBotApiLink + $"broadcasters/get/{twitchId}?username={username}");
            else
                broadcaster = await ApiBotRequest.GetExecuteAsync<Broadcaster>(twitchBotApiLink + $"broadcasters/get/{twitchId}");

            if (broadcaster != null)
            {
                Username = broadcaster.Username;
                TwitchId = twitchId;
                DatabaseId = broadcaster.Id;
            }
        }

        public async Task AddBroadcaster(string twitchBotApiLink)
        {
            Broadcaster freshBroadcaster = new Broadcaster
            {
                Username = Username,
                TwitchId = int.Parse(TwitchId)
            };

            await ApiBotRequest.PostExecuteAsync(twitchBotApiLink + $"broadcasters/create", freshBroadcaster);
        }

        public async Task UpdateBroadcaster(string twitchBotApiLink)
        {
            Broadcaster updatedBroadcaster = new Broadcaster
            {
                Username = Username,
                TwitchId = int.Parse(TwitchId)
            };

            await ApiBotRequest.PutExecuteAsync(twitchBotApiLink + $"broadcasters/update/{TwitchId}", updatedBroadcaster);
        }
    }
}
