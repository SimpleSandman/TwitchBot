using System;
using System.Collections.Generic;

using TwitchBotShared.Models;

namespace TwitchBotShared.ClientLibraries.Singletons
{
    public class DelayedMessageSingleton
    {
        /* Singleton Instance */
        private static volatile DelayedMessageSingleton _instance;
        private static object _syncRoot = new Object();

        public List<DelayedMessage> DelayedMessages { get; set; } = new List<DelayedMessage>();

        private DelayedMessageSingleton() { }

        public static DelayedMessageSingleton Instance
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
                            _instance = new DelayedMessageSingleton();
                    }
                }

                return _instance;
            }
        }
    }
}
