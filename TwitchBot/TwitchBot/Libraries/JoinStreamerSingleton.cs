using System;
using System.Collections.Generic;

using TwitchBot.Models;

namespace TwitchBot.Libraries
{
    public class JoinStreamerSingleton
    {
        private static volatile JoinStreamerSingleton _instance;
        private static object _syncRoot = new object();

        private List<string> _joinStreamerList = new List<string>();

        private JoinStreamerSingleton()
        { }

        public static JoinStreamerSingleton Instance
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
                            _instance = new JoinStreamerSingleton();
                    }
                }

                return _instance;
            }
        }

        /// <summary>
        /// Put a cooldown for a user on a command
        /// </summary>
        /// <param name="chatter"></param>
        /// <param name="cooldown"></param>
        public void AddUser(TwitchChatter chatter, DateTime cooldown)
        {

        }
    }
}
