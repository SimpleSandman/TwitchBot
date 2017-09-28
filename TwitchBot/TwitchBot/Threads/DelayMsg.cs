using System;
using System.Linq;
using System.Threading;

using TwitchBot.Libraries;
using TwitchBot.Models;

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
                    if (Program.DelayedMessages.Count > 0)
                    {
                        /* Make sure to send messages at the proper time */
                        DelayedMessage firstMsg = Program.DelayedMessages.OrderBy(d => d.SendDate).First();
                        if (firstMsg.SendDate < DateTime.Now)
                        {
                            _irc.SendPublicChatMessage(firstMsg.Message);
                            Console.WriteLine($"Delayed message sent: {firstMsg.Message}");
                            Program.DelayedMessages.Remove(firstMsg); // remove sent message from list
                        }
                    }

                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "DelayMsg", "Run()", false);
            }
        }
    }
}
