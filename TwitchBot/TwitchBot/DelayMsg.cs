using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TwitchBot
{
    public class DelayMsg
    {
        private Thread _msgSender;
        private List<Tuple<string, DateTime>> _lstTupSentMsg = new List<Tuple<string, DateTime>>();
        
        public DelayMsg()
        {
            _msgSender = new Thread(new ThreadStart(this.Run));
        }

        public void Start()
        {
            _msgSender.IsBackground = true;
            _msgSender.Start();
        }

        public void Run()
        {
            while (true)
            {
                /* Make sure to send messages at the proper time */
                if (Program._lstTupDelayMsg.Count > 0)
                {
                    foreach (Tuple<string, DateTime> lstTupDelayMsg in Program._lstTupDelayMsg)
                    {
                        if (lstTupDelayMsg.Item2 < DateTime.Now)
                        {
                            Program._irc.sendPublicChatMessage(lstTupDelayMsg.Item1);
                            Console.WriteLine("Message sent: " + lstTupDelayMsg.Item1);
                            _lstTupSentMsg.Add(lstTupDelayMsg); // mark message as sent
                        }
                    }

                    /* Remove sent messages from list of delayed messages */
                    if (_lstTupSentMsg.Count > 0)
                    {
                        foreach (Tuple<string, DateTime> lstTupRmMsg in _lstTupSentMsg)
                        {
                            int intIndex = Program._lstTupDelayMsg.FindIndex(r => r == lstTupRmMsg);
                            Program._lstTupDelayMsg.RemoveAt(intIndex); // remove message from delay message list
                        }

                        _lstTupSentMsg.Clear();
                    }
                }
            }
        }
    }
}
