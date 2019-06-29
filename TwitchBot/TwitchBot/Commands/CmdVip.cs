using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TwitchBot.Configuration;
using TwitchBot.Libraries;
using TwitchBot.Services;
using TwitchBot.Models;
using TwitchBot.Models.JSON;

using TwitchBotDb.Models;

using TwitchBotUtil.Extensions;

namespace TwitchBot.Commands
{
    public class CmdVip
    {
        private IrcClient _irc;
        private TimeoutCmd _timeout;
        private System.Configuration.Configuration _appConfig;
        private TwitchBotConfigurationSection _botConfig;
        private int _broadcasterId;
        private BankService _bank;
        private TwitchInfoService _twitchInfo;
        private ManualSongRequestService _manualSongRequest;
        private QuoteService _quote;
        private PartyUpService _partyUp;
        private GameDirectoryService _gameDirectory;
        private ErrorHandler _errHndlrInstance = ErrorHandler.Instance;
        private TwitterClient _twitter = TwitterClient.Instance;
        private BroadcasterSingleton _broadcasterInstance = BroadcasterSingleton.Instance;
        private TwitchChatterList _twitchChatterListInstance = TwitchChatterList.Instance;

        public CmdVip(IrcClient irc, TimeoutCmd timeout, TwitchBotConfigurationSection botConfig, int broadcasterId, 
            System.Configuration.Configuration appConfig, BankService bank, TwitchInfoService twitchInfo, ManualSongRequestService manualSongRequest,
            QuoteService quote, PartyUpService partyUp, GameDirectoryService gameDirectory)
        {
            _irc = irc;
            _timeout = timeout;
            _botConfig = botConfig;
            _broadcasterId = broadcasterId;
            _appConfig = appConfig;
            _bank = bank;
            _twitchInfo = twitchInfo;
            _manualSongRequest = manualSongRequest;
            _quote = quote;
            _partyUp = partyUp;
            _gameDirectory = gameDirectory;
        }

        /// <summary>
        /// Removes the first song in the queue of song requests
        /// </summary>
        public async Task CmdPopManualSr()
        {
            try
            {
                SongRequest removedSong = await _manualSongRequest.PopSongRequest(_broadcasterId);

                if (removedSong != null)
                    _irc.SendPublicChatMessage($"The first song in the queue, \"{removedSong.Name}\" ({removedSong.Username}), has been removed");
                else
                    _irc.SendPublicChatMessage("There are no songs that can be removed from the song request list");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdVip", "CmdPopManualSr()", false, "!poprsr");
            }
        }

        /// <summary>
        /// Removes first party memeber in queue of party up requests
        /// </summary>
        public async Task CmdPopPartyUpRequest()
        {
            try
            {
                // get current game info
                ChannelJSON json = await _twitchInfo.GetBroadcasterChannelById();
                string gameTitle = json.Game;
                TwitchGameCategory game = await _gameDirectory.GetGameId(gameTitle);

                if (string.IsNullOrEmpty(gameTitle))
                {
                    _irc.SendPublicChatMessage("I cannot see the name of the game. It's currently set to either NULL or EMPTY. "
                        + "Please have the chat verify that the game has been set for this stream. "
                        + $"If the error persists, please have @{_botConfig.Broadcaster.ToLower()} retype the game in their Twitch Live Dashboard. "
                        + "If this error shows up again and your chat can see the game set for the stream, please contact my master with !support in this chat");
                }
                else if (game?.Id > 0)
                    _irc.SendPublicChatMessage(await _partyUp.PopRequestedPartyMember(game.Id, _broadcasterId));
                else
                    _irc.SendPublicChatMessage("This game is not part of the \"Party Up\" system");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdVip", "CmdPopPartyUpRequest()", false, "!poppartyuprequest");
            }
        }

        /// <summary>
        /// Add a mod/broadcaster quote
        /// </summary>
        /// <param name="chatter"></param>
        public async Task CmdAddQuote(TwitchChatter chatter)
        {
            try
            {
                string quote = chatter.Message.Substring(chatter.Message.IndexOf(" ") + 1);

                await _quote.AddQuote(quote, chatter.DisplayName, _broadcasterId);

                _irc.SendPublicChatMessage($"Quote has been created @{chatter.DisplayName}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdVip", "CmdAddQuote(TwitchChatter)", false, "!addquote");
            }
        }

        /// <summary>
        /// Add user(s) to a MultiStream link so viewers can watch multiple streamers at the same time
        /// </summary>
        /// <param name="chatter"></param>
        /// <param name="multiStreamUsers">List of users that have already been added to the link</param>
        public async Task<List<string>> CmdAddMultiStreamUser(TwitchChatter chatter, List<string> multiStreamUsers)
        {
            try
            {
                int userLimit = 3;

                // Hard-coded limit to 4 users (including broadcaster) 
                // because of possible video bandwidth issues for users...for now
                if (multiStreamUsers.Count >= userLimit)
                    _irc.SendPublicChatMessage($"Max limit of users set for the MultiStream link! Please reset the link @{chatter.DisplayName}");
                else if (chatter.Message.IndexOf("@") == -1)
                    _irc.SendPublicChatMessage($"Please use the \"@\" to define new user(s) to add @{chatter.DisplayName}");
                else if (chatter.Message.Contains(_botConfig.Broadcaster, StringComparison.CurrentCultureIgnoreCase)
                            || chatter.Message.Contains(_botConfig.BotName, StringComparison.CurrentCultureIgnoreCase))
                {
                    _irc.SendPublicChatMessage($"I cannot add the broadcaster or myself to the MultiStream link @{chatter.DisplayName}");
                }
                else
                {
                    List<int> indexNewUsers = chatter.Message.AllIndexesOf("@");

                    if (multiStreamUsers.Count + indexNewUsers.Count > userLimit)
                        _irc.SendPublicChatMessage("Too many users are being added to the MultiStream link " + 
                            $"< Number of users already added: \"{multiStreamUsers.Count}\" >" + 
                            $"< User limit (without broadcaster): \"{userLimit}\" > @{chatter.DisplayName}");
                    else
                    {
                        string setMultiStreamUsers = "";
                        string verbUsage = "has ";

                        if (indexNewUsers.Count == 1)
                        {
                            string newUser = chatter.Message.Substring(indexNewUsers[0] + 1);

                            if (!multiStreamUsers.Contains(newUser.ToLower()))
                            {
                                multiStreamUsers.Add(newUser.ToLower());
                                setMultiStreamUsers = $"@{newUser.ToLower()} ";
                            }
                            else
                            {
                                setMultiStreamUsers = $"{newUser} ";
                                verbUsage = "has already ";
                            }
                        }
                        else
                        {
                            for (int i = 0; i < indexNewUsers.Count; i++)
                            {
                                int indexNewUser = indexNewUsers[i] + 1;
                                string setMultiStreamUser = "";

                                if (i + 1 < indexNewUsers.Count)
                                    setMultiStreamUser = chatter.Message.Substring(indexNewUser, indexNewUsers[i + 1] - indexNewUser - 1).ToLower();
                                else
                                    setMultiStreamUser = chatter.Message.Substring(indexNewUser).ToLower();

                                if (!multiStreamUsers.Contains(setMultiStreamUser))
                                    multiStreamUsers.Add(setMultiStreamUser.ToLower());
                            }

                            foreach (string multiStreamUser in multiStreamUsers)
                                setMultiStreamUsers += $"@{multiStreamUser} ";

                            verbUsage = "have ";
                        }

                        string resultMsg = $"{setMultiStreamUsers} {verbUsage} been set up for the MultiStream link @{chatter.DisplayName}";

                        if (chatter.Username.ToLower() == _botConfig.Broadcaster.ToLower())
                            _irc.SendPublicChatMessage(resultMsg);
                        else
                            _irc.SendPublicChatMessage($"{resultMsg} @{_botConfig.Broadcaster.ToLower()}");
                    }
                }

                return multiStreamUsers;
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdVip", "CmdAddMultiStreamUser(TwitchChatter, ref List<string>)", false, "!addmsl", chatter.Message);
            }

            return new List<string>();
        }

        /// <summary>
        /// Reset the MultiStream link to allow the link to be reconfigured
        /// </summary>
        /// <param name="chatter"></param>
        /// <param name="multiStreamUsers">List of users that have already been added to the link</param>
        public async Task<List<string>> CmdResetMultiStreamLink(TwitchChatter chatter, List<string> multiStreamUsers)
        {
            try
            {
                multiStreamUsers = new List<string>();

                string resultMsg = "MultiStream link has been reset. " + 
                    $"Please reconfigure the link if you are planning on using it in the near future @{chatter.DisplayName}";

                if (chatter.Username == _botConfig.Broadcaster.ToLower())
                    _irc.SendPublicChatMessage(resultMsg);
                else
                    _irc.SendPublicChatMessage($"{resultMsg} @{_botConfig.Broadcaster.ToLower()}");

                return multiStreamUsers;
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdVip", "CmdResetMultiStream(string, List<string>)", false, "!resetmsl");
            }

            return new List<string>();
        }

        public async Task<Queue<string>> CmdPopJoin(TwitchChatter chatter, Queue<string> gameQueueUsers)
        {
            try
            {
                if (gameQueueUsers.Count == 0)
                    _irc.SendPublicChatMessage($"Queue is empty @{chatter.DisplayName}");
                else
                {
                    string poppedUser = gameQueueUsers.Dequeue();
                    _irc.SendPublicChatMessage($"{poppedUser} has been removed from the queue @{chatter.DisplayName}");
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdVip", "CmdPopJoin(TwitchChatter, Queue<string>)", false, "!popjoin");
            }

            return gameQueueUsers;
        }

        public async Task CmdPromoteStreamer(TwitchChatter chatter)
        {
            try
            {
                string streamerUsername = chatter.Message.Substring(chatter.Message.IndexOf("@") + 1).ToLower();

                RootUserJSON userInfo = await _twitchInfo.GetUsersByLoginName(streamerUsername);
                if (userInfo.Users.Count == 0)
                {
                    _irc.SendPublicChatMessage($"Cannot find the requested user @{chatter.DisplayName}");
                    return;
                }

                string userId = userInfo.Users.First().Id;
                string promotionMessage = $"Hey everyone! Check out {streamerUsername}'s channel at https://www.twitch.tv/" 
                    + $"{streamerUsername} and slam that follow button!";

                RootStreamJSON userStreamInfo = await _twitchInfo.GetUserStream(userId);

                if (userStreamInfo.Stream == null)
                {
                    ChannelJSON channelInfo = await _twitchInfo.GetUserChannelById(userId);

                    if (!string.IsNullOrEmpty(channelInfo.Game))
                        promotionMessage += $" They were last seen playing \"{channelInfo.Game}\"";
                }
                else
                {
                    if (!string.IsNullOrEmpty(userStreamInfo.Stream.Game))
                        promotionMessage += $" Right now, they're playing \"{userStreamInfo.Stream.Game}\"";
                }

                _irc.SendPublicChatMessage(promotionMessage);
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdVip", "CmdPromoteStreamer(TwitchChatter)", false, "!streamer");
            }
        }
    }
}
