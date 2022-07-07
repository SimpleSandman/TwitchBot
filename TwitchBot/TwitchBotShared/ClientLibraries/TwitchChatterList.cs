using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TwitchBotShared.ClientLibraries.Singletons;
using TwitchBotShared.Enums;
using TwitchBotShared.Models;

namespace TwitchBotShared.ClientLibraries
{
    public class TwitchChatterList
    {
        private static volatile TwitchChatterList _instance;
        private static object _syncRoot = new object();
        private static readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        public bool AreListsAvailable { get; set; } = false;

        public List<TwitchChatterType> ChattersByType { get; } = new List<TwitchChatterType>();

        public List<string> ChattersByName { get; } = new List<string>();

        public List<TwitchChatter> TwitchFollowers { get; } = new List<TwitchChatter>();

        public List<TwitchChatter> TwitchSubscribers { get; } = new List<TwitchChatter>();

        public List<TwitchChatter> TwitchRegularFollowers { get; } = new List<TwitchChatter>();

        private TwitchChatterList() { }

        public static TwitchChatterList Instance
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
                            _instance = new TwitchChatterList();
                    }
                }

                return _instance;
            }
        }

        public async Task<ChatterType> GetUserChatterTypeAsync(string username)
        {
            try
            {
                DateTime timeToGetOut = DateTime.Now.AddSeconds(3);

                // wait until lists are available
                while (!AreListsAvailable && DateTime.Now < timeToGetOut)
                {

                }

                foreach (TwitchChatterType chatterType in ChattersByType.OrderByDescending(t => t.ChatterType))
                {
                    if (chatterType.TwitchChatters.Any(u => u.Username == username))
                        return chatterType.ChatterType;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogErrorAsync(ex, "TwitchChatterList", "GetUserChatterTypeAsync(string)", false);
            }

            return ChatterType.DoesNotExist;
        }
    }
}
