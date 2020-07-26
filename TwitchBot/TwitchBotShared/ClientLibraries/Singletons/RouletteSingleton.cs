using System;
using System.Collections.Generic;

using TwitchBotShared.Models;

namespace TwitchBotShared.ClientLibraries.Singletons
{
    public class RouletteSingleton
    {
        /* Singleton Instance */
        private static volatile RouletteSingleton _instance;
        private static object _syncRoot = new Object();

        public List<RouletteUser> RouletteUsers { get; set; } = new List<RouletteUser>();

        private RouletteSingleton() { }

        public static RouletteSingleton Instance
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
                            _instance = new RouletteSingleton();
                    }
                }

                return _instance;
            }
        }
    }
}
