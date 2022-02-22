using System;
using System.Threading.Tasks;

using TwitchBotDb.Models;
using TwitchBotDb.Services;

using TwitchBotShared.ClientLibraries;
using TwitchBotShared.ClientLibraries.Singletons;
using TwitchBotShared.Config;
using TwitchBotShared.Enums;
using TwitchBotShared.Models;
using TwitchBotShared.Models.JSON;


namespace TwitchBotShared.Commands.Features
{
    /// <summary>
    /// The "Command Subsystem" for the "Refresh" feature
    /// </summary>
    public sealed class RefreshFeature : BaseFeature
    {
        private readonly TwitchInfoService _twitchInfo;
        private readonly GameDirectoryService _gameDirectory;
        private readonly BossFightSingleton _bossFightSettingsInstance = BossFightSingleton.Instance;
        private readonly BroadcasterSingleton _broadcasterInstance = BroadcasterSingleton.Instance;
        private readonly CustomCommandSingleton _customCommandInstance = CustomCommandSingleton.Instance;
        private readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        private const string REFRESH_REMINDERS = "!refreshreminders";
        private const string REFRESH_BOSS_FIGHT = "!refreshbossfight";
        private const string REFRESH_COMMANDS = "!refreshcommands";

        public RefreshFeature(IrcClient irc, TwitchBotConfigurationSection botConfig, TwitchInfoService twitchInfo, GameDirectoryService gameDirectory) 
            : base(irc, botConfig)
        {
            _twitchInfo = twitchInfo;
            _gameDirectory = gameDirectory;
            _rolePermissions.Add(REFRESH_REMINDERS, new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermissions.Add(REFRESH_BOSS_FIGHT, new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermissions.Add(REFRESH_COMMANDS, new CommandPermission { General = ChatterType.Broadcaster });
        }

        public override async Task<(bool, DateTime)> ExecCommandAsync(TwitchChatter chatter, string requestedCommand)
        {
            try
            {
                switch (requestedCommand)
                {
                    case REFRESH_REMINDERS:
                        return (true, await RefreshRemindersAsync());
                    case REFRESH_BOSS_FIGHT:
                        return (true, await RefreshBossFightAsync());
                    case REFRESH_COMMANDS:
                        return (true, await RefreshCommandsAsync());
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "RefreshFeature", "ExecCommandAsync(TwitchChatter, string)", false, requestedCommand, chatter.Message);
            }

            return (false, DateTime.Now);
        }

        #region Private Methods
        private async Task<DateTime> RefreshRemindersAsync()
        {
            try
            {
                await Threads.ChatReminder.RefreshRemindersAsync();
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "RefreshFeature", "RefreshRemindersAsync()", false, REFRESH_REMINDERS);
            }

            return DateTime.Now;
        }

        private async Task<DateTime> RefreshBossFightAsync()
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
                ChannelJSON json = await _twitchInfo.GetBroadcasterChannelByIdAsync();
                string gameTitle = json.GameName;

                // Grab game id in order to find party member
                TwitchGameCategory game = await _gameDirectory.GetGameIdAsync(gameTitle);

                // During refresh, make sure no fighters can join
                _bossFightSettingsInstance.RefreshBossFight = true;
                await _bossFightSettingsInstance.LoadSettings(_broadcasterInstance.DatabaseId, game?.Id, _botConfig.TwitchBotApiLink);
                _bossFightSettingsInstance.RefreshBossFight = false;

                _irc.SendPublicChatMessage($"Boss fight settings refreshed @{_botConfig.Broadcaster}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "RefreshFeature", "RefreshBossFightAsync()", false, REFRESH_BOSS_FIGHT);
            }

            return DateTime.Now;
        }

        private async Task<DateTime> RefreshCommandsAsync()
        {
            try
            {
                await _customCommandInstance.LoadCustomCommands(_botConfig.TwitchBotApiLink, _broadcasterInstance.DatabaseId);

                _irc.SendPublicChatMessage($"Your commands have been refreshed @{_botConfig.Broadcaster}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "RefreshFeature", "RefreshCommandsAsync()", false, REFRESH_COMMANDS);
            }

            return DateTime.Now;
        }
        #endregion
    }
}
