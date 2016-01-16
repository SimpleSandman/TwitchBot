using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TwitchBot
{
    class PresenceReminder
    {
        private Thread presenceReminder;

        // Empty constructor makes instance of Thread
        public PresenceReminder() 
        {
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
                Program.irc.sendPublicChatMessage("Just as a reminder. Big brother is watching! deIlluminati " 
                    + "Type !commands to see a link to the list of this bot's commands");
                Thread.Sleep(900000); // 15 minutes
            }
        }
    }
}
