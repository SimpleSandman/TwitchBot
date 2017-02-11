using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using TwitchBot.Libraries;

namespace TwitchBot.Threads
{
    public class DelayMsg
    {
        private Thread _msgSender;
        private IrcClient _irc;

        private ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        public DelayMsg(IrcClient irc)
        {
            _irc = irc;
            _msgSender = new Thread(new ThreadStart(this.Run));
        }

        public void Start()
        {
            _msgSender.IsBackground = true;
            _msgSender.Start();
        }

        public void Run()
        {
            try
            {
                while (true)
                {
                    /* Make sure to send messages at the proper time */
                    if (Program.LstTupDelayMsg.Count > 0)
                    {
                        /* Send the first element from the list of delayed messages */
                        Tuple<string, DateTime> tupFirstMsg = Program.LstTupDelayMsg.First();
                        if (tupFirstMsg.Item2 < DateTime.Now)
                        {
                            _irc.sendPublicChatMessage(tupFirstMsg.Item1);
                            Console.WriteLine("Delayed message sent: " + tupFirstMsg.Item1);
                            Program.LstTupDelayMsg.Remove(tupFirstMsg); // remove sent message from list
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "DelayMsg", "Run()", false);
            }
        }
    }
}
