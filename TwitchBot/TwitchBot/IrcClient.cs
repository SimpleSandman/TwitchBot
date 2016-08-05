using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace TwitchBot
{
    // Reference: https://www.youtube.com/watch?v=Ss-OzV9aUZg
    class IrcClient
    {
        public string userName;
        private string channel;

        private TcpClient tcpClient;
        private StreamReader inputStream;
        private StreamWriter outputStream;

        public IrcClient(string ip, int port, string userName, string password, string channel)
        {
            try
            {
                this.userName = userName;
                this.channel = channel;

                tcpClient = new TcpClient(ip, port);
                inputStream = new StreamReader(tcpClient.GetStream());
                outputStream = new StreamWriter(tcpClient.GetStream());

                outputStream.WriteLine("PASS " + password);
                outputStream.WriteLine("NICK " + userName);
                outputStream.WriteLine("USER " + userName + " 8 * :" + userName);
                outputStream.WriteLine("JOIN #" + channel);
                outputStream.Flush();
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "IrcClient", "IrcClient(string, int, string, string, string)", true);
            }
        }

        public void sendIrcMessage(string message)
        {
            try
            {
                outputStream.WriteLine(message);
                outputStream.Flush();
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "IrcClient", "sendIrcMessage", false);
            }
        }

        public void sendPublicChatMessage(string message)
        {
            try
            {
                sendIrcMessage(":" + userName + "!" + userName + "@" + userName +
                ".tmi.twitch.tv PRIVMSG #" + channel + " :" + message);
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "IrcClient", "sendPublicMessage", false);
            }
        }

        public string readMessage()
        {
            string message = inputStream.ReadLine();
            return message;
        }
    }
}
