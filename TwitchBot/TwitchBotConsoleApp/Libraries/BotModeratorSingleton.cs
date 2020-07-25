using System.Collections.Generic;
using System.Threading.Tasks;

using TwitchBotDb.Models;

using TwitchBotShared.Libraries;

namespace TwitchBotConsoleApp.Libraries
{
    public class BotModeratorSingleton
    {
        private static volatile BotModeratorSingleton _instance;
        private static object _syncRoot = new object();

        private List<BotModerator> _botModerators = new List<BotModerator>();

        private BotModeratorSingleton() { }

        public static BotModeratorSingleton Instance
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
                            _instance = new BotModeratorSingleton();
                    }
                }

                return _instance;
            }
        }

        public async Task LoadExistingModerators(string twitchBotApiLink, int broadcasterId)
        {
            _botModerators = await ApiBotRequest.GetExecuteAsync<List<BotModerator>>(twitchBotApiLink + $"botmoderators/get/{broadcasterId}");
        }

        public bool IsBotModerator(string twitchId)
        {
            return _botModerators.Exists(m => m.TwitchId.ToString() == twitchId);
        }

        public async Task AddModerator(string twitchBotApiLink, BotModerator botModerator)
        {
            await ApiBotRequest.PostExecuteAsync(twitchBotApiLink + $"botmoderators/create", botModerator);

            _botModerators.Add(botModerator);
        }

        public async Task DeleteModerator(string twitchBotApiLink, int broadcasterId, string username)
        {
            BotModerator botModerator = await ApiBotRequest.DeleteExecuteAsync<BotModerator>(twitchBotApiLink + $"botmoderators/delete/{broadcasterId}?username={username}");

            _botModerators.Remove(botModerator);
        }
    }
}
