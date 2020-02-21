using System.Threading;

using TwitchBot.Libraries;

namespace TwitchBot.Threads
{
    /*
    * Class that sends PING to irc server every 5 minutes
    */
    public class PingSender
    {
        private readonly IrcClient _irc;
        private readonly Thread pingSender;

        // Empty constructor makes instance of Thread
        public PingSender(IrcClient irc) 
        {
            _irc = irc;
            pingSender = new Thread (new ThreadStart(this.Run)); 
        }

        // Starts the thread
        public void Start() 
        {
            pingSender.IsBackground = true;
            pingSender.Start(); 
        }

        // Send PING to irc server every 5 minutes
        private void Run()
        {
            while (true)
            {
                _irc.SendIrcMessage("PING irc.twitch.tv");
                Thread.Sleep(300000); // 5 minutes
            }
        }
    }
}
