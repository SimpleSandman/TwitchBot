﻿using System;
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
    /// The "Command Subsystem" for the "Party Up" feature
    /// </summary>
    public sealed class PartyUpFeature : BaseFeature
    {
        private readonly TwitchInfoService _twitchInfo;
        private readonly GameDirectoryService _gameDirectory;
        private readonly PartyUpService _partyUp;
        private readonly BroadcasterSingleton _broadcasterInstance = BroadcasterSingleton.Instance;
        private readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        private const string PARTY_UP = "!partyup";
        private const string PARTY_UP_REQUEST_LIST = "!partyuprequestlist";
        private const string PARTY_UP_LIST = "!partyuplist";
        private const string POP_PARTY_UP = "!poppartyup";
        private const string POP_PARTY_UP_REQUEST = "!poppartyuprequest";

        public PartyUpFeature(IrcClient irc, TwitchBotConfigurationSection botConfig, TwitchInfoService twitchInfo,
            GameDirectoryService gameDirectory, PartyUpService partyUp) : base(irc, botConfig)
        {
            _twitchInfo = twitchInfo;
            _gameDirectory = gameDirectory;
            _partyUp = partyUp;
            _rolePermissions.Add(PARTY_UP, new CommandPermission { General = ChatterType.Viewer });
            _rolePermissions.Add(PARTY_UP_REQUEST_LIST, new CommandPermission { General = ChatterType.Viewer });
            _rolePermissions.Add(PARTY_UP_LIST, new CommandPermission { General = ChatterType.Viewer });
            _rolePermissions.Add(POP_PARTY_UP, new CommandPermission { General = ChatterType.VIP });
            _rolePermissions.Add(POP_PARTY_UP_REQUEST, new CommandPermission { General = ChatterType.VIP });
        }

        public override async Task<(bool, DateTime)> ExecCommandAsync(TwitchChatter chatter, string requestedCommand)
        {
            try
            {
                switch (requestedCommand)
                {
                    case PARTY_UP:
                        return (true, await PartyUpAsync(chatter));
                    case PARTY_UP_REQUEST_LIST:
                        return (true, await PartyUpRequestListAsync());
                    case PARTY_UP_LIST:
                        return (true, await PartyUpListAsync());
                    case POP_PARTY_UP:
                    case POP_PARTY_UP_REQUEST:
                        return (true, await PopPartyUpRequestAsync());
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "PartyUpFeature", "ExecCommandAsync(TwitchChatter, string)", false, requestedCommand, chatter.Message);
            }

            return (false, DateTime.Now);
        }

        #region Private Methods
        /// <summary>
        /// Request party member if game and character exists in party up system
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        private async Task<DateTime> PartyUpAsync(TwitchChatter chatter)
        {
            try
            {
                int inputIndex = chatter.Message.IndexOf(" ") + 1;

                // check if user entered something
                if (chatter.Message.Length < inputIndex)
                {
                    _irc.SendPublicChatMessage($"Please enter a party member @{chatter.DisplayName}");
                    return DateTime.Now;
                }

                // get current game info
                ChannelJSON json = await _twitchInfo.GetBroadcasterChannelByIdAsync();
                string gameTitle = json.GameName;
                string partyMemberName = chatter.Message.Substring(inputIndex);
                TwitchGameCategory game = await _gameDirectory.GetGameIdAsync(gameTitle);

                // attempt to add requested party member into the queue
                if (string.IsNullOrEmpty(gameTitle))
                {
                    _irc.SendPublicChatMessage("I cannot see the name of the game. It's currently set to either NULL or EMPTY. "
                        + "Please have the chat verify that the game has been set for this stream or if this is intentional. "
                        + $"If the error persists, please have @{_botConfig.Broadcaster.ToLower()} retype the game in their Twitch Live Dashboard. "
                        + "If this error shows up again and your chat can see the game set for the stream, please contact my master with !support in this chat");
                    return DateTime.Now;
                }
                else if (game == null || game.Id == 0)
                {
                    _irc.SendPublicChatMessage("This game is not part of the \"Party Up\" system");
                    return DateTime.Now;
                }

                PartyUp partyMember = await _partyUp.GetPartyMemberAsync(partyMemberName, game.Id, _broadcasterInstance.DatabaseId);

                if (partyMember == null)
                {
                    _irc.SendPublicChatMessage($"I couldn't find the requested party member \"{partyMemberName}\" @{chatter.DisplayName}. "
                        + "Please check with the broadcaster for possible spelling errors");
                    return DateTime.Now;
                }

                if (await _partyUp.HasUserAlreadyRequestedAsync(chatter.DisplayName, game.Id, _broadcasterInstance.DatabaseId))
                {
                    _irc.SendPublicChatMessage($"You have already requested a party member. "
                        + $"Please wait until your request has been completed @{chatter.DisplayName}");
                    return DateTime.Now;
                }

                await _partyUp.AddRequestedPartyMemberAsync(chatter.DisplayName, partyMember.Id);

                _irc.SendPublicChatMessage($"@{chatter.DisplayName}: {partyMemberName} has been added to the party queue");

            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "PartyUpFeature", "PartyUpAsync(TwitchChatter)", false, PARTY_UP, chatter.Message);
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Check what other user's have requested
        /// </summary>
        private async Task<DateTime> PartyUpRequestListAsync()
        {
            try
            {
                // get current game info
                ChannelJSON json = await _twitchInfo.GetBroadcasterChannelByIdAsync();
                string gameTitle = json.GameName;
                TwitchGameCategory game = await _gameDirectory.GetGameIdAsync(gameTitle);

                if (string.IsNullOrEmpty(gameTitle))
                {
                    _irc.SendPublicChatMessage("I cannot see the name of the game. It's currently set to either NULL or EMPTY. "
                        + "Please have the chat verify that the game has been set for this stream or if this is intentional. "
                        + $"If the error persists, please have @{_botConfig.Broadcaster.ToLower()} retype the game in their Twitch Live Dashboard. "
                        + "If this error shows up again and your chat can see the game set for the stream, please contact my master with !support in this chat");
                }
                else if (game == null || game.Id == 0)
                    _irc.SendPublicChatMessage("This game is currently not a part of the \"Party Up\" system");
                else
                    _irc.SendPublicChatMessage(await _partyUp.GetRequestListAsync(game.Id, _broadcasterInstance.DatabaseId));
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "PartyUpFeature", "PartyUpRequestListAsync()", false, PARTY_UP_REQUEST_LIST);
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Check what party members are available (if game is part of the party up system)
        /// </summary>
        private async Task<DateTime> PartyUpListAsync()
        {
            try
            {
                // get current game info
                ChannelJSON json = await _twitchInfo.GetBroadcasterChannelByIdAsync();
                string gameTitle = json.GameName;
                TwitchGameCategory game = await _gameDirectory.GetGameIdAsync(gameTitle);

                if (string.IsNullOrEmpty(gameTitle))
                {
                    _irc.SendPublicChatMessage("I cannot see the name of the game. It's currently set to either NULL or EMPTY. "
                        + "Please have the chat verify that the game has been set for this stream or if this is intentional. "
                        + $"If the error persists, please have @{_botConfig.Broadcaster.ToLower()} retype the game in their Twitch Live Dashboard. "
                        + "If this error shows up again and your chat can see the game set for the stream, please contact my master with !support in this chat");
                    return DateTime.Now;
                }
                else if (game == null || game.Id == 0)
                    _irc.SendPublicChatMessage("This game is currently not a part of the \"Party Up\" system");
                else
                    _irc.SendPublicChatMessage(await _partyUp.GetPartyListAsync(game.Id, _broadcasterInstance.DatabaseId));
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "PartyUpFeature", "PartyUpListAsync()", false, PARTY_UP_LIST);
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Removes first party memeber in queue of party up requests
        /// </summary>
        private async Task<DateTime> PopPartyUpRequestAsync()
        {
            try
            {
                // get current game info
                ChannelJSON json = await _twitchInfo.GetBroadcasterChannelByIdAsync();
                string gameTitle = json.GameName;
                TwitchGameCategory game = await _gameDirectory.GetGameIdAsync(gameTitle);

                if (string.IsNullOrEmpty(gameTitle))
                {
                    _irc.SendPublicChatMessage("I cannot see the name of the game. It's currently set to either NULL or EMPTY. "
                        + "Please have the chat verify that the game has been set for this stream or if this is intentional. "
                        + $"If the error persists, please have @{_botConfig.Broadcaster.ToLower()} retype the game in their Twitch Live Dashboard. "
                        + "If this error shows up again and your chat can see the game set for the stream, please contact my master with !support in this chat");
                }
                else if (game?.Id > 0)
                    _irc.SendPublicChatMessage(await _partyUp.PopRequestedPartyMemberAsync(game.Id, _broadcasterInstance.DatabaseId));
                else
                    _irc.SendPublicChatMessage("This game is not part of the \"Party Up\" system");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "PartyUpFeature", "PopPartyUpRequestAsync()", false, POP_PARTY_UP_REQUEST);
            }

            return DateTime.Now;
        }
        #endregion
    }
}
