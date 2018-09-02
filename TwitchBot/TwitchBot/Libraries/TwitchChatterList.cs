using System;
using System.Collections.Generic;
using System.Linq;

using TwitchBot.Enums;
using TwitchBot.Models;

namespace TwitchBot.Libraries
{
    public class TwitchChatterList
    {
        private static volatile TwitchChatterList _instance;
        private static object _syncRoot = new Object();

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

        public ChatterType GetUserChatterType(string username)
        {
            // wait until lists are available
            while (!AreListsAvailable)
            {

            }

            foreach (TwitchChatterType chatterType in ChattersByType.OrderByDescending(t => t.ChatterType))
            {
                if (chatterType.TwitchChatters.Any(u => u.Username.Equals(username)))
                    return chatterType.ChatterType;
            }

            return ChatterType.DoesNotExist;
        }
    }
}
