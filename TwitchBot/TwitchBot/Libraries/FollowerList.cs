using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot.Libraries
{
    public class FollowerList
    {
        private static volatile FollowerList _instance;
        private static object _syncRoot = new Object();

        private List<string> _twitchFollowers = new List<string>();

        public List<string> TwitchFollowers
        {
            get { return _twitchFollowers; }
        }

        private FollowerList() { }

        public static FollowerList Instance
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
                            _instance = new FollowerList();
                    }
                }

                return _instance;
            }
        }
    }
}
