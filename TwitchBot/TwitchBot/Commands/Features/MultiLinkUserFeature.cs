using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TwitchBot.Configuration;
using TwitchBot.Enums;
using TwitchBot.Libraries;
using TwitchBot.Models;
using TwitchBot.Services;

using TwitchBotDb.DTO;

using TwitchBotUtil.Extensions;

namespace TwitchBot.Commands.Features
{
    /// <summary>
    /// The "Command Subsystem" for the "____" feature
    /// </summary>
    public sealed class MultiLinkUserFeature : BaseFeature
    {
        private readonly BroadcasterSingleton _broadcasterInstance = BroadcasterSingleton.Instance;
        private readonly TwitchChatterList _twitchChatterListInstance = TwitchChatterList.Instance;
        private readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        public MultiLinkUserFeature(IrcClient irc, TwitchBotConfigurationSection botConfig) : base(irc, botConfig)
        {
            _rolePermission.Add("!", new List<ChatterType> { ChatterType.Viewer });
        }

        public override async Task<(bool, DateTime)> ExecCommand(TwitchChatter chatter, string requestedCommand)
        {
            try
            {
                switch (requestedCommand)
                {
                    case "!":
                        //return (true, await SomethingCool(chatter));
                    default:
                        if (requestedCommand == "!")
                        {
                            //return (true, await OtherCoolThings(chatter));
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "TemplateFeature", "ExecCommand(TwitchChatter, string)", false, requestedCommand, chatter.Message);
            }

            return (false, DateTime.Now);
        }

        /// <summary>
        /// Displays MultiStream link so multiple streamers can be watched at once
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        /// <param name="multiStreamUsers">List of broadcasters that are a part of the link</param>
        public async void CmdMultiStreamLink(TwitchChatter chatter, List<string> multiStreamUsers)
        {
            try
            {
                if (multiStreamUsers.Count == 0)
                    _irc.SendPublicChatMessage($"MultiStream link is not set up @{chatter.DisplayName}");
                else
                {
                    string multiStreamLink = "https://multitwitch.live/" + _botConfig.Broadcaster.ToLower();

                    foreach (string multiStreamUser in multiStreamUsers)
                        multiStreamLink += $"/{multiStreamUser}";

                    _irc.SendPublicChatMessage($"Check out these awesome streamers at the same time! (Use desktop for best results) {multiStreamLink}");
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdGen", "CmdMultiStreamLink(TwitchChatter, List<string>)", false, "!msl");
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
                int userLimit = 7;

                // Hard-coded limit to 8 users (including broadcaster) 
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
        public async Task<List<string>> CmdResetMultiStreamLink(TwitchChatter chatter)
        {
            try
            {
                string resultMsg = "MultiStream link has been reset. " +
                    $"Please reconfigure the link if you are planning on using it in the near future @{chatter.DisplayName}";

                if (chatter.Username == _botConfig.Broadcaster.ToLower())
                    _irc.SendPublicChatMessage(resultMsg);
                else
                    _irc.SendPublicChatMessage($"{resultMsg} @{_botConfig.Broadcaster.ToLower()}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "CmdVip", "CmdResetMultiStream(TwitchChatter)", false, "!resetmsl");
            }

            return new List<string>();
        }
    }
}
