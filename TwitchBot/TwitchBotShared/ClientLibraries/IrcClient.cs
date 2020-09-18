using System;
using System.Net.Security;
using System.Net.Sockets;
using System.IO;
using System.Threading.Tasks;

using TwitchBotShared.ClientLibraries.Singletons;
using TwitchBotShared.Models;

namespace TwitchBotShared.ClientLibraries
{
    public class IrcClient
    {
        private readonly string _username;
        private readonly string _channel;
        private readonly TcpClient _tcpClient;
        private readonly SslStream _sslStream;
        private readonly StreamReader _inputStream;
        private readonly StreamWriter _outputStream;
        private readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        public IrcClient(string username, string password, string channel)
        {
            _username = username;
            _channel = channel;

            _tcpClient = new TcpClient("irc.chat.twitch.tv", 6697);

            _sslStream = new SslStream(_tcpClient.GetStream());
            _sslStream.AuthenticateAsClient("irc.chat.twitch.tv");

            _inputStream = new StreamReader(_sslStream);
            _outputStream = new StreamWriter(_sslStream);

            _outputStream.WriteLine("CAP REQ :twitch.tv/tags"); // Reference: https://dev.twitch.tv/docs/irc/tags/
            _outputStream.WriteLine("CAP REQ :twitch.tv/commands"); // Reference: https://dev.twitch.tv/docs/irc/commands/
            _outputStream.WriteLine("CAP REQ :twitch.tv/membership"); // Reference: https://dev.twitch.tv/docs/irc/membership/
            _outputStream.WriteLine($"PASS {password}");
            _outputStream.WriteLine($"NICK {username}");
            _outputStream.WriteLine($"USER {username} 8 * :{username}");
            _outputStream.WriteLine($"JOIN #{channel}");
            _outputStream.Flush();
        }

        public async void SendIrcMessage(string message)
        {
            try
            {
                _outputStream.WriteLine(message);
                _outputStream.Flush();
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
                SendIrcMessage(":" + _username + "!" + _username + "@" + _username +
                    ".tmi.twitch.tv PRIVMSG #" + _channel + " :" + message);
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
                SendIrcMessage(":" + _username + "!" + _username + "@" + _username +
                    ".tmi.twitch.tv PRIVMSG #" + _channel + " :/delete " + chatter.MessageId);
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
                SendIrcMessage(":" + _username + "!" + _username + "@" + _username +
                    ".tmi.twitch.tv PRIVMSG #" + _channel + " :/timeout " + offender + " " + timeout);
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "IrcClient", "SendChatTimeout(string, int)", false);
            }
        }

        public async Task<string> ReadMessageAsync()
        {
            try
            {
                return _inputStream.ReadLine(); // chat message
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "IrcClient", "ReadMessageAsync()", true);
            }

            return "";
        }
    }
}
