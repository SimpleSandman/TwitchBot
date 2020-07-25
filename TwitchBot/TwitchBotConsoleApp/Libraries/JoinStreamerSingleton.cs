using System.Collections.Generic;
using System.Linq;

using TwitchBotShared.Models;

namespace TwitchBotConsoleApp.Libraries
{
    public class JoinStreamerSingleton
    {
        private static volatile JoinStreamerSingleton _instance;
        private static object _syncRoot = new object();

        private Queue<string> _joinStreamerList = new Queue<string>();

        private JoinStreamerSingleton() { }

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
        public string Invite(TwitchChatter chatter)
        {
            if (_joinStreamerList.Contains(chatter.Username))
            {
                return $"Don't worry @{chatter.DisplayName}. You're on the list to play with " +
                    $"the streamer with your current position at {_joinStreamerList.ToList().IndexOf(chatter.Username) + 1} " +
                    $"of {_joinStreamerList.Count} user(s)";
            }
            else
            {
                _joinStreamerList.Enqueue(chatter.Username);

                return $"Congrats @{chatter.DisplayName}! You're currently in line with your current position at " +
                    $"{_joinStreamerList.ToList().IndexOf(chatter.Username) + 1}";
            }
        }

        public void ResetList()
        {
            _joinStreamerList.Clear();
        }

        public string ListJoin()
        {
            if (_joinStreamerList.Count == 0)
            {
                return $"No one wants to play with the streamer at the moment. Be the first to play with !join";
            }

            // Show list of queued users
            string message = $"List of users waiting to play with the streamer (in order from left to right): < ";

            foreach (string user in _joinStreamerList)
            {
                message += user + " >< ";
            }

            return message;
        }

        public string PopJoin(TwitchChatter chatter)
        {
            if (_joinStreamerList.Count == 0)
            {
                return $"Queue is empty @{chatter.DisplayName}";
            }
            else
            {
                string poppedUser = _joinStreamerList.Dequeue();
                return $"{poppedUser} has been removed from the queue @{chatter.DisplayName}";
            }
        }
    }
}
