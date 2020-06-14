using System;
using System.Threading.Tasks;

using TwitchBot.Configuration;
using TwitchBot.Libraries;
using TwitchBot.Models;
using TwitchBot.Models.JSON;
using TwitchBot.Services;

using TwitchBotDb.Models;

namespace TwitchBot.Commands.Features
{
    /// <summary>
    /// The "Command Subsystem" for the "Reminder" feature
    /// </summary>
    public sealed class ReminderFeature : BaseFeature
    {
        private readonly TwitchInfoService _twitchInfo;
        private readonly GameDirectoryService _gameDirectory;
        private readonly BossFightSingleton _bossFightSettingsInstance = BossFightSingleton.Instance;
        private readonly BroadcasterSingleton _broadcasterInstance = BroadcasterSingleton.Instance;
        private readonly CustomCommandSingleton _customCommandInstance = CustomCommandSingleton.Instance;
        private readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        public ReminderFeature(IrcClient irc, TwitchBotConfigurationSection botConfig, TwitchInfoService twitchInfo, GameDirectoryService gameDirectory) 
            : base(irc, botConfig)
        {
            _twitchInfo = twitchInfo;
            _gameDirectory = gameDirectory;
            _rolePermission.Add("!refreshreminders", "broadcaster");
            _rolePermission.Add("!refreshbossfight", "broadcaster");
            _rolePermission.Add("!refreshcommands", "broadcaster");
        }

        public override async void ExecCommand(TwitchChatter chatter, string requestedCommand)
        {
            try
            {
                switch (requestedCommand)
                {
                    case "!refreshreminders":
                        await RefreshReminders();
                        break;
                    case "!refreshbossfight":
                        await RefreshBossFight();
                        break;
                    case "!refreshcommands":
                        await RefreshCommands();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "ReminderFeature", "ExecCommand(TwitchChatter, string)", false, requestedCommand, chatter.Message);
            }
        }

        public async Task RefreshReminders()
        {
            try
            {
                await Threads.ChatReminder.RefreshReminders();
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "ReminderFeature", "RefreshReminders()", false, "!refreshreminders");
            }
        }

        public async Task RefreshBossFight()
        {
            try
            {
                // Check if any fighters are queued or fighting
                if (_bossFightSettingsInstance.Fighters.Count > 0)
                {
                    _irc.SendPublicChatMessage($"A boss fight is either queued or in progress @{_botConfig.Broadcaster}");
                    return;
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
        }

        public async Task RefreshCommands()
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
        }

        /* ToDo: Insert new methods here */
    }
}
