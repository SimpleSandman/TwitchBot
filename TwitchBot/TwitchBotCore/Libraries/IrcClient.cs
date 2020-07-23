using System;
using System.Net.Sockets;
using System.IO;
using System.Threading.Tasks;

using TwitchBotConsoleApp.Models;

namespace TwitchBotConsoleApp.Libraries
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

        public IrcClient(string username, string password, string channel)
        {
            this.username = username;
            this.channel = channel;

            tcpClient = new TcpClient("irc.chat.twitch.tv", 6667);
            inputStream = new StreamReader(tcpClient.GetStream());
            outputStream = new StreamWriter(tcpClient.GetStream());

            outputStream.WriteLine("CAP REQ :twitch.tv/tags"); // Reference: https://dev.twitch.tv/docs/irc/tags/
            outputStream.WriteLine("CAP REQ :twitch.tv/commands"); // Reference: https://dev.twitch.tv/docs/irc/commands/
            outputStream.WriteLine("CAP REQ :twitch.tv/membership"); // Reference: https://dev.twitch.tv/docs/irc/membership/
            outputStream.WriteLine("PASS " + password);
            outputStream.WriteLine("NICK " + username);
            outputStream.WriteLine("USER " + username + " 8 * :" + username);
            outputStream.WriteLine("JOIN #" + channel);
            outputStream.Flush();
        }

        public async void SendIrcMessage(string message)
        {
            try
            {
                outputStream.WriteLine(message);
                outputStream.Flush();
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "IrcClient", "SendIrcMessage(string)", false);
            }
        }

        public async void SendPublicChatMessage(string message)
        {
            try
            {
                SendIrcMessage(":" + username + "!" + username + "@" + username +
                    ".tmi.twitch.tv PRIVMSG #" + channel + " :" + message);
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "IrcClient", "SendPublicChatMessage(string)", false);
            }
        }

        public async void ClearMessage(TwitchChatter chatter)
        {
            try
            {
                SendIrcMessage(":" + username + "!" + username + "@" + username +
                    ".tmi.twitch.tv PRIVMSG #" + channel + " :/delete " + chatter.MessageId);
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "IrcClient", "ClearMessage(TwitchChatter)", false);
            }
        }

        public async void SendChatTimeout(string offender, int timeout = 1)
        {
            try
            {
                SendIrcMessage(":" + username + "!" + username + "@" + username +
                    ".tmi.twitch.tv PRIVMSG #" + channel + " :/timeout " + offender + " " + timeout);
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "IrcClient", "SendChatTimeout(string, int)", false);
            }
        }

        public async Task<string> ReadMessage()
        {
            try
            {
                return inputStream.ReadLine(); // chat message
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "IrcClient", "ReadMessage()", true);
            }

            return "";
        }
    }
}
