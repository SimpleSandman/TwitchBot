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
            try
            {
                while (true)
                {
                    /* Make sure to send messages at the proper time */
                    if (Program._lstTupDelayMsg.Count > 0)
                    {
                        /* Send the first element from the list of delayed messages */
                        Tuple<string, DateTime> tupFirstMsg = Program._lstTupDelayMsg.First();
                        if (tupFirstMsg.Item2 < DateTime.Now)
                        {
                            Program._irc.sendPublicChatMessage(tupFirstMsg.Item1);
                            Console.WriteLine("Delayed message sent: " + tupFirstMsg.Item1);
                            Program._lstTupDelayMsg.Remove(tupFirstMsg); // remove sent message from list
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "DelayMsg", "Run()", false);
            }
        }
    }
}
