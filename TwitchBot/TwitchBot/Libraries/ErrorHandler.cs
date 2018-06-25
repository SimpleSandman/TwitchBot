using System;
using System.Threading;
using System.Threading.Tasks;

using TwitchBot.Configuration;

using TwitchBotDb.Models;

namespace TwitchBot.Libraries
{
    // Using Singleton design pattern
    public sealed class ErrorHandler
    {
        private static ErrorHandler _instance;

        private static int _broadcasterId;
        private static IrcClient _irc;
        private static TwitchBotConfigurationSection _botConfig;

        static ErrorHandler() { _instance = new ErrorHandler(); }

        public static ErrorHandler Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// Used first chance that error logging can be possible
        /// </summary>
        public static void Configure(int broadcasterId, IrcClient irc, TwitchBotConfigurationSection botConfig)
        {
            _broadcasterId = broadcasterId;
            _irc = irc;
            _botConfig = botConfig;
        }

        public async Task LogError(Exception ex, string className, string methodName, bool hasToExit, string botCmd = "N/A", string userMsg = "N/A")
        {
            Console.WriteLine("Error: " + ex.Message);

            try
            {
                /* If username not available, grab default user to show local error after db connection */
                if (_broadcasterId == 0)
                {
                    Broadcasters broadcaster = await ApiBotRequest.GetExecuteTaskAsync<Broadcasters>(_botConfig.TwitchBotApiLink + $"broadcasters/get/-1");
                    _broadcasterId = broadcaster.Id;
                }

                /* Get line number from error message */
                int lineNumber = 0;
                const string lineSearch = ":line ";
                int index = ex.StackTrace.LastIndexOf(lineSearch);

                if (index != -1)
                {
                    string lineNumberText = ex.StackTrace.Substring(index + lineSearch.Length);
                    if (!int.TryParse(lineNumberText, out lineNumber))
                    {
                        lineNumber = -1; // couldn't parse line number
                    }
                }

                ErrorLog error = new ErrorLog
                {
                    ErrorTime = DateTime.UtcNow,
                    ErrorLine = lineNumber,
                    ErrorClass = className,
                    ErrorMethod = methodName,
                    ErrorMsg = ex.Message,
                    Broadcaster = _broadcasterId,
                    Command = botCmd,
                    UserMsg = userMsg
                };

                await ApiBotRequest.PostExecuteTaskAsync(_botConfig.TwitchBotApiLink + $"errorlogs/create", error);

                string publicErrMsg = "I ran into an unexpected internal error! "
                    + "@" + _botConfig.Broadcaster + " please look into the error log when you have time";

                if (hasToExit)
                    publicErrMsg += " I am leaving as well. Have a great time with this stream everyone KonCha";

                if (_irc != null)
                    _irc.SendPublicChatMessage(publicErrMsg);

                if (hasToExit)
                {
                    Console.WriteLine();
                    Console.WriteLine("Shutting down now...");
                    Thread.Sleep(3000);
                    Environment.Exit(1);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine();
                Console.WriteLine("Logging error found: " + e.Message);
                Console.WriteLine("Please inform author of this error!");
                Thread.Sleep(5000);
            }
        }
    }
}
