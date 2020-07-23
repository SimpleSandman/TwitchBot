using System;
using System.Collections.Generic;

using TwitchBotConsoleApp.Models;

using TwitchBotUtil.Extensions;

namespace TwitchBotConsoleApp.Libraries
{
    public class MultiLinkUserSingleton
    {
        private static volatile MultiLinkUserSingleton _instance;
        private static object _syncRoot = new object();

        private List<string> _multiLinkUsers = new List<string>();

        private MultiLinkUserSingleton() { }

        public static MultiLinkUserSingleton Instance
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
                            _instance = new MultiLinkUserSingleton();
                    }
                }

                return _instance;
            }
        }

        /// <summary>
        /// Add user(s) to a MultiStream link so viewers can watch multiple streamers at the same time
        /// </summary>
        /// <param name="chatter"></param>
        /// <param name="broadcasterName"></param>
        /// <param name="botName"></param>
        public string AddUser(TwitchChatter chatter, string broadcasterName, string botName)
        {
            int userLimit = 7;

            // Hard-coded limit to 8 users (including broadcaster) 
            // because of possible video bandwidth issues for users...for now
            if (_multiLinkUsers.Count >= userLimit)
            {
                return $"Max limit of users set for the MultiStream link! Please reset the link @{chatter.DisplayName}";
            }
            else if (chatter.Message.IndexOf("@") == -1)
            {
                return $"Please use the \"@\" to define new user(s) to add @{chatter.DisplayName}";
            }
            else if (chatter.Message.Contains(broadcasterName, StringComparison.CurrentCultureIgnoreCase)
                || chatter.Message.Contains(botName, StringComparison.CurrentCultureIgnoreCase))
            {
                return $"I cannot add the broadcaster or myself to the MultiStream link @{chatter.DisplayName}";
            }
            else
            {
                List<int> indexNewUsers = chatter.Message.AllIndexesOf("@");

                if (_multiLinkUsers.Count + indexNewUsers.Count > userLimit)
                {
                    return "Too many users are being added to the MultiStream link " +
                        $"< Number of users already added: \"{_multiLinkUsers.Count}\" >" +
                        $"< User limit (without broadcaster): \"{userLimit}\" > @{chatter.DisplayName}";
                }
                else
                {
                    string setMultiStreamUsers = "";
                    string verbUsage = "has ";

                    if (indexNewUsers.Count == 1)
                    {
                        string newUser = chatter.Message.Substring(indexNewUsers[0] + 1);

                        if (!_multiLinkUsers.Contains(newUser.ToLower()))
                        {
                            _multiLinkUsers.Add(newUser.ToLower());
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

                            if (!_multiLinkUsers.Contains(setMultiStreamUser))
                            {
                                _multiLinkUsers.Add(setMultiStreamUser.ToLower());
                            }
                        }

                        foreach (string multiStreamUser in _multiLinkUsers)
                        {
                            setMultiStreamUsers += $"@{multiStreamUser} ";
                        }

                        verbUsage = "have ";
                    }

                    string resultMsg = $"{setMultiStreamUsers} {verbUsage} been set up for the MultiStream link @{chatter.DisplayName}";

                    if (chatter.Username.ToLower() == broadcasterName.ToLower())
                        return resultMsg;
                    else
                        return $"{resultMsg} @{broadcasterName.ToLower()}";
                }
            }
        }

        public void ResetMultiLink()
        {
            _multiLinkUsers = new List<string>();
        }

        public string ShowLink(TwitchChatter chatter, string broadcasterName)
        {
            if (_multiLinkUsers.Count == 0)
            {
                return $"MultiStream link is not set up @{chatter.DisplayName}";
            }
            else
            {
                string multiStreamLink = $"https://multitwitch.live/" + broadcasterName;

                foreach (string multiStreamUser in _multiLinkUsers)
                {
                    multiStreamLink += $"/{multiStreamUser}";
                }

                return $"Check out these awesome streamers at the same time! (Use desktop for best results) {multiStreamLink}";
            }
        }
    }
}
