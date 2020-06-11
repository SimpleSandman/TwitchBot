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
            _rolePermission.Add("!setgameign", "broadcaster");
            _rolePermission.Add("!setgameid", "broadcaster");
            _rolePermission.Add("!setgenericign", "broadcaster");
            _rolePermission.Add("!setgenericid", "broadcaster");
            _rolePermission.Add("!deleteign", "broadcaster");
            _rolePermission.Add("!ign", ""); // Display the broadcaster's in-game (user) name based on what they're streaming
            _rolePermission.Add("!fc", "");
            _rolePermission.Add("!gt", "");
            _rolePermission.Add("!allign", ""); // Display all of the broadcaster's in-game (user) names
            _rolePermission.Add("!allfc", "");
            _rolePermission.Add("!allgt", "");
        }

        public override async void ExecCommand(TwitchChatter chatter, string requestedCommand)
        {
            try
            {
                switch (requestedCommand)
                {
                    case "!setgameign":
                    case "!setgameid":
                        await SetGameIgn(chatter);
                        break;
                    case "!setgenericign":
                    case "setgenericid":
                        await SetGenericIgn(chatter);
                        break;
                    case "!deleteign":
                        await DeleteIgn();
                        break;
                    case "!ign":
                    case "!fc":
                    case "!gt":
                    case "!allign":
                    case "!allfc":
                    case "!allgt":
                        await InGameUsername(chatter);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "InGameNameFeature", "ExecCommand(TwitchChatter, string)", false, requestedCommand, chatter.Message);
            }
        }

        public async Task SetGameIgn(TwitchChatter chatter)
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

                if (ign == null || (ign != null && ign.GameId == null))
                {
                    await _ign.CreateInGameUsername(game.Id, _broadcasterInstance.DatabaseId, gameIgn);

                    _irc.SendPublicChatMessage($"Yay! You've set your IGN for {gameTitle} to \"{gameIgn}\"");
                }
                else
                {
                    ign.Message = gameIgn;
                    await _ign.UpdateInGameUsername(ign.Id, _broadcasterInstance.DatabaseId, ign);

                    _irc.SendPublicChatMessage($"Yay! You've updated your IGN for {gameTitle} to \"{gameIgn}\"");
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "InGameNameFeature", "SetGameIgn(string)", false, "!setgameign");
            }
        }

        public async Task SetGenericIgn(TwitchChatter chatter)
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

                    _irc.SendPublicChatMessage($"Yay! You've set your generic IGN to \"{gameIgn}\"");
                }
                else
                {
                    ign.Message = gameIgn;
                    await _ign.UpdateInGameUsername(ign.Id, _broadcasterInstance.DatabaseId, ign);

                    _irc.SendPublicChatMessage($"Yay! You've updated your generic IGN to \"{gameIgn}\"");
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "InGameNameFeature", "SetGenericIgn(string)", false, "!setgenericign");
            }
        }

        public async Task DeleteIgn()
        {
            try
            {
                // Get current game name
                ChannelJSON json = await _twitchInfo.GetBroadcasterChannelById();
                string gameTitle = json.Game;

                TwitchGameCategory game = await _gameDirectory.GetGameId(gameTitle);
                InGameUsername ign = await _ign.GetInGameUsername(_broadcasterInstance.DatabaseId, game);

                if (ign != null && ign.GameId != null)
                {
                    await _ign.DeleteInGameUsername(ign.Id, _broadcasterInstance.DatabaseId);

                    _irc.SendPublicChatMessage($"Successfully deleted IGN set for the category \"{game.Title}\"");
                }
                else
                {
                    _irc.SendPublicChatMessage($"Wasn't able to find an IGN to delete for the category \"{game.Title}\"");
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "InGameNameFeature", "DeleteIgn()", false, "!deleteign");
            }
        }

        public async Task InGameUsername(TwitchChatter chatter)
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
                    _irc.SendPublicChatMessage(ign.Message);
                else
                    _irc.SendPublicChatMessage($"I cannot find your in-game username @{chatter.DisplayName}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "InGameNameFeature", "InGameUsername(TwitchChatter)", false, "!ign");
            }
        }
    }
}
