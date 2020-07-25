using System;
using System.Linq;
using System.Threading;

using TwitchBotConsoleApp.Libraries;

using TwitchBotUtil.Models;

namespace TwitchBotConsoleApp.Threads
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

        private async void Run()
        {
            try
            {
                while (true)
                {
                    if (Program.DelayedMessages.Count > 0)
                    {
                        /* Make sure to send messages at the proper time */
                        DelayedMessage delayedMessage = Program.DelayedMessages
                            .FirstOrDefault(m => m.SendDate < DateTime.Now 
                                && (m.ExpirationDateUtc == null || m.ExpirationDateUtc > DateTime.UtcNow));

                        if (delayedMessage != null)
                        {
                            _irc.SendPublicChatMessage(delayedMessage.Message);
                            Console.WriteLine($"Delayed message sent: {delayedMessage.Message}");
                            Program.DelayedMessages.Remove(delayedMessage); // remove sent message from list

                            // re-add message if set as reminder
                            if (delayedMessage.ReminderEveryMin > 0)
                            {
                                Program.DelayedMessages.Add(new DelayedMessage
                                {
                                    ReminderId = delayedMessage.ReminderId,
                                    Message = delayedMessage.Message,
                                    SendDate = delayedMessage.SendDate.AddMinutes((double)delayedMessage.ReminderEveryMin),
                                    ReminderEveryMin = delayedMessage.ReminderEveryMin,
                                    ExpirationDateUtc = delayedMessage.ExpirationDateUtc
                                });
                            }
                        }
                    }

                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "DelayMsg", "Run()", false);
            }
        }
    }
}
