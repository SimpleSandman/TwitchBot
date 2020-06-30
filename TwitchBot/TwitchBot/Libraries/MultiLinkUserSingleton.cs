using System;
using System.Collections.Generic;

using TwitchBot.Models;

namespace TwitchBot.Libraries
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
        /// Put a cooldown for a user on a command
        /// </summary>
        /// <param name="chatter"></param>
        public void AddUser(TwitchChatter chatter)
        {
            
        }

        public void ResetMultiLink()
        {
            _multiLinkUsers = new List<string>();
        }
    }
}
