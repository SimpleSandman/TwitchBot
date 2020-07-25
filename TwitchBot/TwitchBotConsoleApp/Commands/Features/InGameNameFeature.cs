using System;
using System.Threading.Tasks;

using TwitchBotConsoleApp.Libraries;

using TwitchBotDb.Models;
using TwitchBotDb.Services;

using TwitchBotUtil.Config;
using TwitchBotUtil.Enums;
using TwitchBotUtil.Models;
using TwitchBotUtil.Models.JSON;

namespace TwitchBotConsoleApp.Commands.Features
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

        public InGameNameFeature(IrcClient irc, TwitchBotConfigurationSection botConfig, TwitchInfoService twitchInfo, 
            GameDirectoryService gameDirectory, InGameUsernameService ign) : base(irc, botConfig)
        {
            _twitchInfo = twitchInfo;
            _gameDirectory = gameDirectory;
            _ign = ign;
            _rolePermission.Add("!setgameign", new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermission.Add("!setgameid", new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermission.Add("!setgamefc", new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermission.Add("!setgenericign", new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermission.Add("!setgenericid", new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermission.Add("!setgenericfc", new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermission.Add("!deleteign", new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermission.Add("!deleteid", new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermission.Add("!deletefc", new CommandPermission { General = ChatterType.Broadcaster });
            _rolePermission.Add("!ign", new CommandPermission { General = ChatterType.Viewer }); // Display the broadcaster's in-game (user) name based on what they're streaming
            _rolePermission.Add("!fc", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!gt", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!allign", new CommandPermission { General = ChatterType.Viewer }); // Display all of the broadcaster's in-game (user) names
            _rolePermission.Add("!allfc", new CommandPermission { General = ChatterType.Viewer });
            _rolePermission.Add("!allgt", new CommandPermission { General = ChatterType.Viewer });
        }

        public override async Task<(bool, DateTime)> ExecCommand(TwitchChatter chatter, string requestedCommand)
        {
            try
            {
                switch (requestedCommand)
                {
                    case "!setgameign":
                    case "!setgameid":
                    case "!setgamefc":
                        return (true, await SetGameIgn(chatter));
                    case "!setgenericign":
                    case "!setgenericid":
                    case "!setgenericfc":
                        return (true, await SetGenericIgn(chatter));
                    case "!deleteign":
                    case "!deleteid":
                    case "!deletefc":
                        return (true, await DeleteIgn(chatter));
                    case "!ign":
                    case "!fc":
                    case "!gt":
                    case "!allign":
                    case "!allfc":
                    case "!allgt":
                        return (true, await InGameUsername(chatter));
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "InGameNameFeature", "ExecCommand(TwitchChatter, string)", false, requestedCommand, chatter.Message);
            }

            return (false, DateTime.Now);
        }

        private async Task<DateTime> SetGameIgn(TwitchChatter chatter)
        {
            try
            {
                string message = chatter.Message;
                string gameIgn = message.Substring(message.IndexOf(" ") + 1);

                // Get current game name
                ChannelJSON json = await _twitchInfo.GetBroadcasterChannelById();
                string gameTitle = json.Game;

                TwitchGameCategory game = await _gameDirectory.GetGameId(gameTitle);
                InGameUsername ign = await _ign.GetInGameUsername(_broadcasterInstance.DatabaseId, game);

                if (game == null)
                {
                    _irc.SendPublicChatMessage("The game isn't in the database. " 
                        + $"Please set this as part of the general game IDs or IGNs using !setgenericign or !setgenericid @{chatter.DisplayName}");
                    return DateTime.Now;
                }

                if (ign == null || (ign != null && ign.GameId == null))
                {
                    await _ign.CreateInGameUsername(game.Id, _broadcasterInstance.DatabaseId, gameIgn);

                    _irc.SendPublicChatMessage($"Yay! You've set your IGN for {gameTitle} to \"{gameIgn}\" @{chatter.DisplayName}");
                }
                else
                {
                    ign.Message = gameIgn;
                    await _ign.UpdateInGameUsername(ign.Id, _broadcasterInstance.DatabaseId, ign);

                    _irc.SendPublicChatMessage($"Yay! You've updated your IGN for {gameTitle} to \"{gameIgn}\" @{chatter.DisplayName}");
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "InGameNameFeature", "SetGameIgn(TwitchChatter)", false, "!setgameign", chatter.Message);
            }

            return DateTime.Now;
        }

        private async Task<DateTime> SetGenericIgn(TwitchChatter chatter)
        {
            try
            {
                string message = chatter.Message;
                string gameIgn = message.Substring(message.IndexOf(" ") + 1);

                // Get current game name
                ChannelJSON json = await _twitchInfo.GetBroadcasterChannelById();
                string gameTitle = json.Game;

                InGameUsername ign = await _ign.GetInGameUsername(_broadcasterInstance.DatabaseId);

                if (ign == null)
                {
                    await _ign.CreateInGameUsername(null, _broadcasterInstance.DatabaseId, gameIgn);

                    _irc.SendPublicChatMessage($"Yay! You've set your generic IGN to \"{gameIgn}\" @{chatter.DisplayName}");
                }
                else
                {
                    ign.Message = gameIgn;
                    await _ign.UpdateInGameUsername(ign.Id, _broadcasterInstance.DatabaseId, ign);

                    _irc.SendPublicChatMessage($"Yay! You've updated your generic IGN to \"{gameIgn}\" @{chatter.DisplayName}");
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "InGameNameFeature", "SetGenericIgn(TwitchChatter)", false, "!setgenericign", chatter.Message);
            }

            return DateTime.Now;
        }

        private async Task<DateTime> DeleteIgn(TwitchChatter chatter)
        {
            try
            {
                // Get current game name
                ChannelJSON json = await _twitchInfo.GetBroadcasterChannelById();
                string gameTitle = json.Game;

                TwitchGameCategory game = await _gameDirectory.GetGameId(gameTitle);
                InGameUsername ign = await _ign.GetInGameUsername(_broadcasterInstance.DatabaseId, game);

                if (game == null)
                {
                    _irc.SendPublicChatMessage($"The game isn't in the database @{chatter.DisplayName}");
                    return DateTime.Now;
                }

                if (ign != null && ign.GameId != null)
                {
                    await _ign.DeleteInGameUsername(ign.Id, _broadcasterInstance.DatabaseId);

                    _irc.SendPublicChatMessage($"Successfully deleted IGN set for the category, \"{game.Title}\" @{chatter.DisplayName}");
                }
                else
                {
                    _irc.SendPublicChatMessage($"Wasn't able to find an IGN to delete for the category, \"{game.Title}\" @{chatter.DisplayName}");
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "InGameNameFeature", "DeleteIgn(TwitchChatter)", false, "!deleteign", chatter.Message);
            }

            return DateTime.Now;
        }

        private async Task<DateTime> InGameUsername(TwitchChatter chatter)
        {
            try
            {
                // Get current game name
                ChannelJSON json = await _twitchInfo.GetBroadcasterChannelById();
                string gameTitle = json.Game;

                TwitchGameCategory game = await _gameDirectory.GetGameId(gameTitle);
                InGameUsername ign = null;

                if (chatter.Message.StartsWith("!all"))
                    ign = await _ign.GetInGameUsername(_broadcasterInstance.DatabaseId); // return generic IGN
                else
                    ign = await _ign.GetInGameUsername(_broadcasterInstance.DatabaseId, game); // return specified IGN (if available)

                if (ign != null && !string.IsNullOrEmpty(ign.Message))
                    _irc.SendPublicChatMessage($"{ign.Message} @{chatter.DisplayName}");
                else
                    _irc.SendPublicChatMessage($"I cannot find your in-game username @{chatter.DisplayName}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "InGameNameFeature", "InGameUsername(TwitchChatter)", false, "!ign");
            }

            return DateTime.Now;
        }
    }
}
