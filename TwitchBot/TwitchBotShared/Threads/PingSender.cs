using System.Threading;

using TwitchBotShared.ClientLibraries;

namespace TwitchBotShared.Threads
{
    /// <summary>
    /// Class that sends PING to irc server every 5 minutes
    /// </summary>
    public class PingSender
    {
        private readonly IrcClient _irc;
        private readonly Thread pingSender;

        public PingSender(IrcClient irc) 
        {
            _irc = irc;
            pingSender = new Thread (new ThreadStart(this.Run)); 
        }

        public void Start() 
        {
            pingSender.IsBackground = true;
            pingSender.Start(); 
        }

        /// <summary>
        /// Send PING to irc server every 5 minutes
        /// </summary>
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
