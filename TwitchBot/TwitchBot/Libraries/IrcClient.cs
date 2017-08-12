using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace TwitchBot.Libraries
{
    // Reference: https://www.youtube.com/watch?v=Ss-OzV9aUZg
    public class IrcClient
    {
        public string username;
        private string channel;

        private TcpClient tcpClient;
        private StreamReader inputStream;
        private StreamWriter outputStream;

        private ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        public IrcClient(string ip, int port, string username, string password, string channel)
        {
            try
            {
                this.username = username;
                this.channel = channel;

                tcpClient = new TcpClient(ip, port);
                inputStream = new StreamReader(tcpClient.GetStream());
                outputStream = new StreamWriter(tcpClient.GetStream());

                outputStream.WriteLine("PASS " + password);
                outputStream.WriteLine("NICK " + username);
                outputStream.WriteLine("USER " + username + " 8 * :" + username);
                outputStream.WriteLine("JOIN #" + channel);
                outputStream.Flush();
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "IrcClient", "IrcClient(string, int, string, string, string)", true);
            }
        }

        public void SendIrcMessage(string message)
        {
            try
            {
                outputStream.WriteLine(message);
                outputStream.Flush();
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "IrcClient", "SendIrcMessage(string)", false);
            }
        }

        public void SendPublicChatMessage(string message)
        {
            try
            {
                SendIrcMessage(":" + username + "!" + username + "@" + username +
                    ".tmi.twitch.tv PRIVMSG #" + channel + " :" + message);
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "IrcClient", "SendPublicChatMessage(string)", false);
            }
        }

        public void SendChatTimeout(string offender, int timeout = 1, string reason = "N/A")
        {
            try
            {
                SendIrcMessage(":" + username + "!" + username + "@" + username +
                    ".tmi.twitch.tv PRIVMSG #" + channel + " :/timeout " + offender + " " + timeout);
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "IrcClient", "SendChatTimeout(string, string)", false);
            }
        }

        public string ReadMessage()
        {
            try
            {
                return inputStream.ReadLine(); // chat message
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "IrcClient", "ReadMessage(string)", true);
            }

            return "";
        }
    }
}
