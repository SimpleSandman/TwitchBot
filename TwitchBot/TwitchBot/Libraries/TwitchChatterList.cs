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

        public bool ListsAvailable { get; set; } = false;

        public List<TwitchChatterType> ChattersByType
        {
            get { return _chattersByType; }
        }

        public List<string> ChattersByName
        {
            get { return _chattersByName; }
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
