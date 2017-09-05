using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
                    SendDelayMsg().Wait();
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "DelayMsg", "Run()", false);
            }
        }

        public async Task SendDelayMsg()
        {
            try
            {
                /* Make sure to send messages at the proper time */
                if (Program.DelayedMessages.Count == 0)
                {
                    return;
                }
                
                /* Send the first element from the list of delayed messages */
                DelayedMessage firstMsg = Program.DelayedMessages.First();
                if (firstMsg.SendDate < DateTime.Now)
                {
                    _irc.SendPublicChatMessage(firstMsg.Message);
                    Console.WriteLine($"Delayed message sent: {firstMsg.Message}");
                    Program.DelayedMessages.Remove(firstMsg); // remove sent message from list
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "DelayMsg", "SendDelayMsg()", false);
            }
        }
    }
}
