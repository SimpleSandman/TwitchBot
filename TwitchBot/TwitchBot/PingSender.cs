using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TwitchBot
{
    /*
    * Class that sends PING to irc server every 5 minutes
    */
    class PingSender
    {
        static string PING = "PING ";
        private Thread pingSender;

        // Empty constructor makes instance of Thread
        public PingSender() 
        {
            pingSender = new Thread (new ThreadStart (this.Run) ); 
        }

        // Starts the thread
        public void Start() 
        {
            pingSender.IsBackground = true;
            pingSender.Start(); 
        }

        // Send PING to irc server every 5 minutes
        public void Run()
        {
            while (true)
            {
                Program.irc.sendIrcMessage(PING + "irc.twitch.tv");
                Thread.Sleep(300000); // 5 minutes
            }
        }
    }
}
