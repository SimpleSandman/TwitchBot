using System;
using System.Threading.Tasks;

using TwitchBotConsoleApp.Config;
using TwitchBotConsoleApp.Enums;
using TwitchBotConsoleApp.Libraries;
using TwitchBotConsoleApp.Models;
using TwitchBotConsoleApp.Models.JSON;
using TwitchBotConsoleApp.Services;

using TwitchBotDb.Models;

namespace TwitchBotConsoleApp.Commands.Features
{
    /// <summary>
    /// The "Command Subsystem" for the "Reminder" feature
    /// </summary>
    public sealed class RefreshFeature : BaseFeature
    {
        private readonly TwitchInfoService _twitchInfo;
        private readonly GameDirectoryService _gameDirectory;
        private readonly BossFightSingleton _bossFightSettingsInstance = BossFightSingleton.Instance;
        private readonly BroadcasterSingleton _broadcasterInstance = BroadcasterSingleton.Instance;
        private readonly CustomCommandSingleton _customCommandInstance = CustomCommandSingleton.Instance;
        private readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        public RefreshFeature(IrcClient irc, TwitchBotConfigurationSection botConfig, TwitchInfoService twitchInfo, GameDirectoryService gameDirectory) 
            : base(irc, botConfig)
        {
            _twitchInfo = twitchInfo;
            _gameDirectory = gameDirectory;
            _rolePermission.Add("!refreshreminders", new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermission.Add("!refreshbossfight", new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermission.Add("!refreshcommands", new CommandPermission { General = ChatterType.Broadcaster });
        }

        public override async Task<(bool, DateTime)> ExecCommand(TwitchChatter chatter, string requestedCommand)
        {
            try
            {
                switch (requestedCommand)
                {
                    case "!refreshreminders":
                        return (true, await RefreshReminders());
                    case "!refreshbossfight":
                        return (true, await RefreshBossFight());
                    case "!refreshcommands":
                        return (true, await RefreshCommands());
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "ReminderFeature", "ExecCommand(TwitchChatter, string)", false, requestedCommand, chatter.Message);
            }

            return (false, DateTime.Now);
        }

        public async Task<DateTime> RefreshReminders()
        {
            try
            {
                await Threads.ChatReminder.RefreshReminders();
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "ReminderFeature", "RefreshReminders()", false, "!refreshreminders");
            }

            return DateTime.Now;
        }

        public async Task<DateTime> RefreshBossFight()
        {
            try
            {
                // Check if any fighters are queued or fighting
                if (_bossFightSettingsInstance.Fighters.Count > 0)
                {
                    _irc.SendPublicChatMessage($"A boss fight is either queued or in progress @{_botConfig.Broadcaster}");
                    return DateTime.Now;
                }

                // Get current game name
                ChannelJSON json = await _twitchInfo.GetBroadcasterChannelById();
                string gameTitle = json.Game;

                // Grab game id in order to find party member
                TwitchGameCategory game = await _gameDirectory.GetGameId(gameTitle);

                // During refresh, make sure no fighters can join
                _bossFightSettingsInstance.RefreshBossFight = true;
                await _bossFightSettingsInstance.LoadSettings(_broadcasterInstance.DatabaseId, game?.Id, _botConfig.TwitchBotApiLink);
                _bossFightSettingsInstance.RefreshBossFight = false;

                _irc.SendPublicChatMessage($"Boss fight settings refreshed @{_botConfig.Broadcaster}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "ReminderFeature", "RefreshBossFight()", false, "!refreshbossfight");
            }

            return DateTime.Now;
        }

        public async Task<DateTime> RefreshCommands()
        {
            try
            {
                await _customCommandInstance.LoadCustomCommands(_botConfig.TwitchBotApiLink, _broadcasterInstance.DatabaseId);

                _irc.SendPublicChatMessage($"Your commands have been refreshed @{_botConfig.Broadcaster}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "ReminderFeature", "RefreshCommands()", false, "!refreshcommands");
            }

            return DateTime.Now;
        }
    }
}
