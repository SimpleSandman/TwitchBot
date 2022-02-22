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
    /// The "Command Subsystem" for the "In Game Name" feature
    /// </summary>
    public sealed class InGameNameFeature : BaseFeature
    {
        private readonly TwitchInfoService _twitchInfo;
        private readonly GameDirectoryService _gameDirectory;
        private readonly InGameUsernameService _ign;
        private readonly BroadcasterSingleton _broadcasterInstance = BroadcasterSingleton.Instance;
        private readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        #region Private Constant Variables
        private const string SET_GAME_IGN = "!setgameign";
        private const string SET_GAME_ID = "!setgameid";
        private const string SET_GAME_FC = "!setgamefc";
        private const string SET_GENERIC_IGN = "!setgenericign";
        private const string SET_GENERIC_ID = "!setgenericid";
        private const string SET_GENERIC_FC = "!setgenericfc";
        private const string DELETE_IGN = "!deleteign";
        private const string DELETE_ID = "!deleteid";
        private const string DELETE_FC = "!deletefc";
        private const string IGN = "!ign";
        private const string FC = "!fc";
        private const string GT = "!gt";
        private const string ALL_IGN = "!allign";
        private const string ALL_FC = "!allfc";
        private const string ALL_GT = "!allgt";
        #endregion

        #region Constructor
        public InGameNameFeature(IrcClient irc, TwitchBotConfigurationSection botConfig, TwitchInfoService twitchInfo, 
            GameDirectoryService gameDirectory, InGameUsernameService ign) : base(irc, botConfig)
        {
            _twitchInfo = twitchInfo;
            _gameDirectory = gameDirectory;
            _ign = ign;
            _rolePermissions.Add(SET_GAME_IGN, new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermissions.Add(SET_GAME_ID, new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermissions.Add(SET_GAME_FC, new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermissions.Add(SET_GENERIC_IGN, new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermissions.Add(SET_GENERIC_ID, new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermissions.Add(SET_GENERIC_FC, new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermissions.Add(DELETE_IGN, new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermissions.Add(DELETE_ID, new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermissions.Add(DELETE_FC, new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermissions.Add(IGN, new CommandPermission { General = ChatterType.Viewer }); // Display the broadcaster's in-game (user) name based on what they're streaming
            _rolePermissions.Add(FC, new CommandPermission { General = ChatterType.Viewer });
            _rolePermissions.Add(GT, new CommandPermission { General = ChatterType.Viewer });
            _rolePermissions.Add(ALL_IGN, new CommandPermission { General = ChatterType.Viewer }); // Display all of the broadcaster's in-game (user) names
            _rolePermissions.Add(ALL_FC, new CommandPermission { General = ChatterType.Viewer });
            _rolePermissions.Add(ALL_GT, new CommandPermission { General = ChatterType.Viewer });
        }
        #endregion

        public override async Task<(bool, DateTime)> ExecCommandAsync(TwitchChatter chatter, string requestedCommand)
        {
            try
            {
                switch (requestedCommand)
                {
                    case SET_GAME_IGN:
                    case SET_GAME_ID:
                    case SET_GAME_FC:
                        return (true, await SetGameIgnAsync(chatter));
                    case SET_GENERIC_IGN:
                    case SET_GENERIC_ID:
                    case SET_GENERIC_FC:
                        return (true, await SetGenericIgnAsync(chatter));
                    case DELETE_IGN:
                    case DELETE_ID:
                    case DELETE_FC:
                        return (true, await DeleteIgnAsync(chatter));
                    case IGN:
                    case FC:
                    case GT:
                    case ALL_IGN:
                    case ALL_FC:
                    case ALL_GT:
                        return (true, await InGameUsernameAsync(chatter));
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "InGameNameFeature", "ExecCommandAsync(TwitchChatter, string)", false, requestedCommand, chatter.Message);
            }

            return (false, DateTime.Now);
        }

        #region Private Methods
        private async Task<DateTime> SetGameIgnAsync(TwitchChatter chatter)
        {
            try
            {
                string message = chatter.Message;
                string gameIgn = message.Substring(message.IndexOf(" ") + 1);

                // Get current game name
                ChannelJSON json = await _twitchInfo.GetBroadcasterChannelByIdAsync();
                string gameTitle = json.GameName;

                TwitchGameCategory game = await _gameDirectory.GetGameIdAsync(gameTitle);
                InGameUsername ign = await _ign.GetInGameUsernameAsync(_broadcasterInstance.DatabaseId, game);

                if (game == null)
                {
                    _irc.SendPublicChatMessage("The game isn't in the database. " 
                        + $"Please set this as part of the general game IDs or IGNs using !setgenericign or !setgenericid @{chatter.DisplayName}");
                    return DateTime.Now;
                }

                if (ign == null || (ign != null && ign.GameId == null))
                {
                    await _ign.CreateInGameUsernameAsync(game.Id, _broadcasterInstance.DatabaseId, gameIgn);

                    _irc.SendPublicChatMessage($"Yay! You've set your IGN for {gameTitle} to \"{gameIgn}\" @{chatter.DisplayName}");
                }
                else
                {
                    ign.Message = gameIgn;
                    await _ign.UpdateInGameUsernameAsync(ign.Id, _broadcasterInstance.DatabaseId, ign);

                    _irc.SendPublicChatMessage($"Yay! You've updated your IGN for {gameTitle} to \"{gameIgn}\" @{chatter.DisplayName}");
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "InGameNameFeature", "SetGameIgnAsync(TwitchChatter)", false, SET_GAME_IGN, chatter.Message);
            }

            return DateTime.Now;
        }

        private async Task<DateTime> SetGenericIgnAsync(TwitchChatter chatter)
        {
            try
            {
                string message = chatter.Message;
                string gameIgn = message.Substring(message.IndexOf(" ") + 1);

                // Get current game name
                ChannelJSON json = await _twitchInfo.GetBroadcasterChannelByIdAsync();
                string gameTitle = json.GameName;

                InGameUsername ign = await _ign.GetInGameUsernameAsync(_broadcasterInstance.DatabaseId);

                if (ign == null)
                {
                    await _ign.CreateInGameUsernameAsync(null, _broadcasterInstance.DatabaseId, gameIgn);

                    _irc.SendPublicChatMessage($"Yay! You've set your generic IGN to \"{gameIgn}\" @{chatter.DisplayName}");
                }
                else
                {
                    ign.Message = gameIgn;
                    await _ign.UpdateInGameUsernameAsync(ign.Id, _broadcasterInstance.DatabaseId, ign);

                    _irc.SendPublicChatMessage($"Yay! You've updated your generic IGN to \"{gameIgn}\" @{chatter.DisplayName}");
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "InGameNameFeature", "SetGenericIgnAsync(TwitchChatter)", false, SET_GENERIC_IGN, chatter.Message);
            }

            return DateTime.Now;
        }

        private async Task<DateTime> DeleteIgnAsync(TwitchChatter chatter)
        {
            try
            {
                // Get current game name
                ChannelJSON json = await _twitchInfo.GetBroadcasterChannelByIdAsync();
                string gameTitle = json.GameName;

                TwitchGameCategory game = await _gameDirectory.GetGameIdAsync(gameTitle);
                InGameUsername ign = await _ign.GetInGameUsernameAsync(_broadcasterInstance.DatabaseId, game);

                if (game == null)
                {
                    _irc.SendPublicChatMessage($"The game isn't in the database @{chatter.DisplayName}");
                    return DateTime.Now;
                }

                if (ign != null && ign.GameId != null)
                {
                    await _ign.DeleteInGameUsernameAsync(ign.Id, _broadcasterInstance.DatabaseId);

                    _irc.SendPublicChatMessage($"Successfully deleted IGN set for the category, \"{game.Title}\" @{chatter.DisplayName}");
                }
                else
                {
                    _irc.SendPublicChatMessage($"Wasn't able to find an IGN to delete for the category, \"{game.Title}\" @{chatter.DisplayName}");
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "InGameNameFeature", "DeleteIgnAsync(TwitchChatter)", false, DELETE_IGN, chatter.Message);
            }

            return DateTime.Now;
        }

        private async Task<DateTime> InGameUsernameAsync(TwitchChatter chatter)
        {
            try
            {
                // Get current game name
                ChannelJSON json = await _twitchInfo.GetBroadcasterChannelByIdAsync();
                string gameTitle = json.GameName;

                TwitchGameCategory game = await _gameDirectory.GetGameIdAsync(gameTitle);
                InGameUsername ign = null;

                if (chatter.Message.StartsWith("!all"))
                    ign = await _ign.GetInGameUsernameAsync(_broadcasterInstance.DatabaseId); // return generic IGN
                else
                    ign = await _ign.GetInGameUsernameAsync(_broadcasterInstance.DatabaseId, game); // return specified IGN (if available)

                if (ign != null && !string.IsNullOrEmpty(ign.Message))
                    _irc.SendPublicChatMessage($"{ign.Message} @{chatter.DisplayName}");
                else
                    _irc.SendPublicChatMessage($"I cannot find your in-game username @{chatter.DisplayName}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "InGameNameFeature", "InGameUsernameAsync(TwitchChatter)", false, IGN);
            }

            return DateTime.Now;
        }
        #endregion
    }
}
