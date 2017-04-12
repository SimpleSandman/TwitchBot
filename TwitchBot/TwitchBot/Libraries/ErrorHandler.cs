using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;

using TwitchBot.Configuration;

namespace TwitchBot.Libraries
{
    // Using Singleton design pattern
    public sealed class ErrorHandler
    {
        private static ErrorHandler _instance;

        private static int _broadcasterId;
        private static string _connStr;
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
        public static void Configure(int broadcasterId, string connStr, IrcClient irc, TwitchBotConfigurationSection botConfig)
        {
            _broadcasterId = broadcasterId;
            _connStr = connStr;
            _irc = irc;
            _botConfig = botConfig;
        }

        public void LogError(Exception ex, string className, string methodName, bool hasToExit, string botCmd = "N/A", string userMsg = "N/A")
        {
            Console.WriteLine("Error: " + ex.Message);

            try
            {
                /* If username not available, grab default user to show local error after db connection */
                if (_broadcasterId == 0)
                {
                    string strBroadcaster = "n/a";
                    using (SqlConnection conn = new SqlConnection(_connStr))
                    {
                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblBroadcasters WHERE username = @username", conn))
                        {
                            cmd.Parameters.AddWithValue("@username", strBroadcaster);
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        if (strBroadcaster.Equals(reader["username"].ToString().ToLower()))
                                        {
                                            _broadcasterId = int.Parse(reader["id"].ToString());
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
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

                /* Add song request to database */
                string query = "INSERT INTO tblErrorLog (errorTime, errorLine, errorClass, errorMethod, errorMsg, broadcaster, command, userMsg) "
                    + "VALUES (@time, @lineNum, @class, @method, @msg, @broadcaster, @command, @userMsg)";

                // Create connection and command
                using (SqlConnection conn = new SqlConnection(_connStr))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@time", SqlDbType.DateTime).Value = DateTime.UtcNow;
                    cmd.Parameters.Add("@lineNum", SqlDbType.Int).Value = lineNumber;
                    cmd.Parameters.Add("@class", SqlDbType.VarChar, 100).Value = className;
                    cmd.Parameters.Add("@method", SqlDbType.VarChar, 100).Value = methodName;
                    cmd.Parameters.Add("@msg", SqlDbType.VarChar, 4000).Value = ex.Message;
                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _broadcasterId;
                    cmd.Parameters.Add("@command", SqlDbType.VarChar, 100).Value = botCmd;
                    cmd.Parameters.Add("@userMsg", SqlDbType.VarChar, 500).Value = userMsg;

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                string publicErrMsg = "I ran into an unexpected internal error! "
                    + "@" + _botConfig.Broadcaster + " please look into the error log when you have time";

                if (hasToExit)
                    publicErrMsg += " I am leaving as well. Have a great time with this stream everyone :)";

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
