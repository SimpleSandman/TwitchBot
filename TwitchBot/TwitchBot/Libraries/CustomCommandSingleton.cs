using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TwitchBotDb.Models;

using TwitchBotUtil.Libraries;

namespace TwitchBot.Libraries
{
    public class CustomCommandSingleton
    {
        private static volatile CustomCommandSingleton _instance;
        private static object _syncRoot = new object();

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
            _customCommands = await ApiBotRequest.GetExecuteTaskAsync<List<CustomCommand>>(twitchBotApiLink + $"customcommands/get/{broadcasterId}");
        }

        public CustomCommand FindCustomCommand(string commandName)
        {
            return _customCommands.SingleOrDefault(c => c.Name == commandName);
        }

        public async Task AddCustomCommand(string twitchBotApiLink, CustomCommand customCommand)
        {
            await ApiBotRequest.PostExecuteTaskAsync(twitchBotApiLink + $"customcommands/create", customCommand);

            _customCommands.Add(customCommand);
        }

        public async Task DeleteCustomCommand(string twitchBotApiLink, int broadcasterId, string username)
        {
            CustomCommand customCommand = await ApiBotRequest.DeleteExecuteTaskAsync<CustomCommand>(twitchBotApiLink + $"customcommands/delete/{broadcasterId}?name={username}");

            _customCommands.Remove(customCommand);
        }
    }
}
