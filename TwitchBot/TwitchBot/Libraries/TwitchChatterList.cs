using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TwitchBot.Models;

namespace TwitchBot.Libraries
{
    public class TwitchChatterList
    {
        private static volatile TwitchChatterList _instance;
        private static object _syncRoot = new Object();

        private List<TwitchChatterType> _chattersByType = new List<TwitchChatterType>();
        private List<string> _chattersByName = new List<string>();

        private List<TwitchChatter> _twitchFollowers = new List<TwitchChatter>();
        private List<TwitchChatter> _twitchRegularFollowers = new List<TwitchChatter>();
        private List<TwitchChatter> _twitchSubscribers = new List<TwitchChatter>();

        public bool AreListsAvailable { get; set; } = false;

        public List<TwitchChatterType> ChattersByType
        {
            get { return _chattersByType; }
        }

        public List<string> ChattersByName
        {
            get { return _chattersByName; }
        }

        public List<TwitchChatter> TwitchFollowers
        {
            get { return _twitchFollowers; }
        }

        public List<TwitchChatter> TwitchSubscribers
        {
            get { return _twitchSubscribers; }
        }

        public List<TwitchChatter> TwitchRegularFollowers
        {
            get { return _twitchRegularFollowers; }
        }

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
    }
}
