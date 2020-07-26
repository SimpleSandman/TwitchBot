using System;
using System.Collections.Generic;

using TwitchBotShared.Models;

namespace TwitchBotShared.ClientLibraries.Singletons
{
    public class DelayedMessagesSingleton
    {
        /* Singleton Instance */
        private static volatile DelayedMessagesSingleton _instance;
        private static object _syncRoot = new Object();

        public List<DelayedMessage> DelayedMessages { get; set; } = new List<DelayedMessage>();

        private DelayedMessagesSingleton() { }

        public static DelayedMessagesSingleton Instance
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
                            _instance = new DelayedMessagesSingleton();
                    }
                }

                return _instance;
            }
        }
    }
}
