using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using TwitchBot.Libraries;

namespace TwitchBot.Threads
{
    /*
    * Class that sends PING to irc server every 5 minutes
    */
    public class PingSender
    {
        private IrcClient _irc;
        static string PING = "PING ";
        private Thread pingSender;

        // Empty constructor makes instance of Thread
        public PingSender(IrcClient irc) 
        {
            _irc = irc;
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
                _irc.sendIrcMessage(PING + "irc.twitch.tv");
                Thread.Sleep(300000); // 5 minutes
            }
        }
    }
}
