using System;
using System.Linq;
using System.Threading;

using TwitchBotShared.ClientLibraries.Singletons;
using TwitchBotShared.ClientLibraries;
using TwitchBotShared.Models;

namespace TwitchBotShared.Threads
{
    public class DelayMessage
    {
        private readonly Thread _msgSender;
        private readonly IrcClient _irc;
        private readonly DelayedMessageSingleton _delayedMessagesInstance = DelayedMessageSingleton.Instance;
        private readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        public DelayMessage(IrcClient irc)
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
                    if (_delayedMessagesInstance.DelayedMessages.Count > 0)
                    {
                        /* Make sure to send messages at the proper time */
                        DelayedMessage delayedMessage = _delayedMessagesInstance.DelayedMessages
                            .FirstOrDefault(m => m.SendDate < DateTime.Now 
                                && (m.ExpirationDateUtc == null || m.ExpirationDateUtc > DateTime.UtcNow));

                        if (delayedMessage != null)
                        {
                            _irc.SendPublicChatMessage(delayedMessage.Message);
                            Console.WriteLine($"Delayed message sent: {delayedMessage.Message}");
                            _delayedMessagesInstance.DelayedMessages.Remove(delayedMessage); // remove sent message from list

                            // re-add message if set as reminder
                            if (delayedMessage.ReminderEveryMin > 0)
                            {
                                _delayedMessagesInstance.DelayedMessages.Add(new DelayedMessage
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
                await _errHndlrInstance.LogErrorAsync(ex, "DelayMsg", "Run()", false);
            }
        }
    }
}
