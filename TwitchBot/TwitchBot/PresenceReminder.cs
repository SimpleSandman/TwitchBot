using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TwitchBot
{
    class PresenceReminder
    {
        private IrcClient _irc;
        private Thread presenceReminder;

        // Empty constructor makes instance of Thread
        public PresenceReminder(IrcClient irc) 
        {
            _irc = irc;
            presenceReminder = new Thread (new ThreadStart (this.Run) ); 
        }

        // Starts the thread
        public void Start() 
        {
            presenceReminder.IsBackground = true;
            presenceReminder.Start(); 
        }

        // Remind viewers of chat bot's presence
        public void Run()
        {
            while (true)
            {
                _irc.sendPublicChatMessage("Just as a reminder. Big brother is watching! deIlluminati " 
                    + "Type !cmds to see the list of this bot's commands");
                Thread.Sleep(1200000); // 20 minutes
            }
        }
    }
}
