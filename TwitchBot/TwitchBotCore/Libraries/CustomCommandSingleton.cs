using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TwitchBotDb.Models;

using TwitchBotUtil.Libraries;

namespace TwitchBotConsoleApp.Libraries
{
    public class CustomCommandSingleton
    {
        private static volatile CustomCommandSingleton _instance;
        private static readonly object _syncRoot = new object();

        private List<CustomCommand> _customCommands = new List<CustomCommand>();

        private CustomCommandSingleton() { }

        public static CustomCommandSingleton Instance
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
                            _instance = new CustomCommandSingleton();
                    }
                }

                return _instance;
            }
        }

        public async Task LoadCustomCommands(string twitchBotApiLink, int broadcasterId)
        {
            _customCommands = await ApiBotRequest.GetExecuteAsync<List<CustomCommand>>(twitchBotApiLink + $"customcommands/get/{broadcasterId}");
        }

        public IEnumerable<CustomCommand> GetSoundCommands()
        {
            return _customCommands.FindAll(s => s.IsSound).OrderBy(o => o.Name);
        }

        public CustomCommand FindCustomCommand(string commandName, int? gameId = null)
        {
            return _customCommands.SingleOrDefault(c => c.Name == commandName && (c.GameId == gameId || c.GameId == null));
        }

        public async Task AddCustomCommand(string twitchBotApiLink, CustomCommand customCommand)
        {
            await ApiBotRequest.PostExecuteAsync(twitchBotApiLink + $"customcommands/create", customCommand);

            _customCommands.Add(customCommand);
        }

        public async Task DeleteCustomCommand(string twitchBotApiLink, int broadcasterId, string username)
        {
            CustomCommand customCommand = await ApiBotRequest.DeleteExecuteAsync<CustomCommand>(twitchBotApiLink + $"customcommands/delete/{broadcasterId}?name={username}");

            _customCommands.Remove(customCommand);
        }
    }
}
