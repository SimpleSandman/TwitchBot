using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using TwitchBotDb.Models;

namespace TwitchBot.Libraries
{
    public sealed class ModeratorSingleton
    {
        private static volatile ModeratorSingleton _instance;
        private static object _syncRoot = new Object();

        public List<string> Moderators { get; } = new List<string>();

        private ModeratorSingleton() { }

        public static ModeratorSingleton Instance
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
                            _instance = new ModeratorSingleton();
                    }
                }

                return _instance;
            }
        }

        public async Task GetModerators(int broadcasterId, string twitchBotApiLink)
        {
            try
            {
                List<Moderators> moderators = await ApiBotRequest.GetExecuteTaskAsync<List<Moderators>>(twitchBotApiLink + $"moderators/get/{broadcasterId}");

                foreach (var moderator in moderators)
                {
                    Moderators.Add(moderator.Username);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task<string> AddModerator(string recipient, int broadcasterId, string twitchBotApiLink)
        {
            Moderators freshModerator = new Moderators { Username = recipient, Broadcaster = broadcasterId };

            Moderators addedModerator = await ApiBotRequest.PostExecuteTaskAsync(twitchBotApiLink + $"moderators/create", freshModerator);
            string name = addedModerator.Username;

            Moderators.Add(name);
            return name;
        }

        public async Task<string> DeleteModerator(string recipient, int broadcasterId, string twitchBotApiLink)
        {
            Moderators removedModerator = await ApiBotRequest.DeleteExecuteTaskAsync<Moderators>(twitchBotApiLink + $"moderators/delete/{broadcasterId}?username={recipient}");
            if (removedModerator == null) return "";

            string name = removedModerator.Username;

            Moderators.Remove(name);
            return name;
        }
    }
}
