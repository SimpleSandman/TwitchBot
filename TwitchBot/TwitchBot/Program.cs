using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Threading;
using SpotifyAPI.Local;
using SpotifyAPI.Local.Enums;
using SpotifyAPI.Local.Models;
using System.Data.SqlClient;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using Newtonsoft.Json;
using System.IO;
using RestSharp;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using Tweetinvi;
using Tweetinvi.Core;
using Tweetinvi.Core.Credentials;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace TwitchBot
{
    class Program
    {
        public static SpotifyControl _spotify;
        public static IrcClient _irc;
        public static Moderator _mod;
        public static Timeout _timeout;
        public static string _strBroadcasterName = "";
        public static int _intBroadcasterID = 0; // associated with db
        public static int _intStreamLatency = 12; // (in seconds)
        public static string _strBroadcasterGame = "";
        public static string _strBotName = "";
        public static string _strCurrencyType = "coins";
        public static string _connStr = ""; // connection string
        public static int _intFollowers = 0;
        public static string _strDiscordLink = "Link unavailable at the moment"; // provide discord server link if available
        public static bool _isAutoPublishTweet = false; // set to auto publish tweets (disabled by default)
        public static bool _isAutoDisplaySong = false; // set to auto song status (disabled by default)
        public static List<Tuple<string, DateTime>> _lstTupDelayMsg = new List<Tuple<string, DateTime>>(); // used to handle delayed msgs
        public static CmdBrdCstr _cmdBrdCstr = new CmdBrdCstr();
        public static CmdMod _cmdMod = new CmdMod();
        public static CmdGen _cmdGen = new CmdGen();

        static void Main(string[] args)
        {       
            // Twitch variables
            string twitchOAuth = "";
            string twitchClientID = "";
            string twitchAccessToken = "";  // used for channel editing

            // Twitter variables
            bool hasTwitterInfo = true;
            string twitterConsumerKey = "";
            string twitterConsumerSecret = "";
            string twitterAccessToken = "";
            string twitterAccessSecret = "";

            bool isSongRequestAvail = false;  // check song request status (disabled by default)

            /* Connect to database or exit program on connection error */
            try
            {
                // Grab connection string (production or test)
                if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.conn))
                {
                    _connStr = Properties.Settings.Default.conn; // production database only
                    Console.WriteLine("Connecting to database...");
                }
                else if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.connTest))
                {
                    _connStr = Properties.Settings.Default.connTest; // test database only
                    Console.WriteLine("<<<< WARNING: Connecting to local database (testing only) >>>>");
                }
                else
                {
                    Console.WriteLine("Internal Error: Connection string to database not provided!");
                    Console.WriteLine("Shutting down now...");
                    Thread.Sleep(3500);
                    Environment.Exit(1);
                }

                // Check if server is connected
                if (!IsServerConnected(_connStr))
                {
                    // clear sensitive data
                    _connStr = null;

                    Console.WriteLine("Datebase connection failed. Please try again");
                    Console.WriteLine();
                    Console.WriteLine("-- Common technical issues: --");
                    Console.WriteLine("1: Check if firewall settings has your client IP address.");
                    Console.WriteLine("2: Double check the connection string under 'Properties' inside 'Settings'");
                    Console.WriteLine();
                    Console.WriteLine("Shutting down now...");
                    Thread.Sleep(5000);
                    Environment.Exit(1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Message: " + ex.Message);
                Console.WriteLine("Local troubleshooting needed by author of this bot");
                Console.WriteLine();
                Console.WriteLine("Shutting down now...");
                Thread.Sleep(3000);
                Environment.Exit(1);
            }

            /* Try to grab the info needed for the bot to connect to the channel */
            try
            {
                Console.WriteLine("Database connection successful!");
                Console.WriteLine();
                Console.WriteLine("Checking if user settings has all necessary info...");

                // Grab settings
                _strBotName = Properties.Settings.Default.botName;
                _strBroadcasterName = Properties.Settings.Default.broadcaster;
                twitchOAuth = Properties.Settings.Default.twitchOAuth;
                twitchClientID = Properties.Settings.Default.twitchClientID;
                twitchAccessToken = Properties.Settings.Default.twitchAccessToken;
                twitterConsumerKey = Properties.Settings.Default.twitterConsumerKey;
                twitterConsumerSecret = Properties.Settings.Default.twitterConsumerSecret;
                twitterAccessToken = Properties.Settings.Default.twitterAccessToken;
                twitterAccessSecret = Properties.Settings.Default.twitterAccessSecret;
                _strDiscordLink = Properties.Settings.Default.discordLink;
                _strCurrencyType = Properties.Settings.Default.currencyType;
                _isAutoDisplaySong = Properties.Settings.Default.enableDisplaySong;
                _isAutoPublishTweet = Properties.Settings.Default.enableTweet;
                _intStreamLatency = Properties.Settings.Default.streamLatency;

                // Check if program has client ID (developer needs to provide this inside the settings)
                if (string.IsNullOrWhiteSpace(twitchClientID))
                {
                    Console.WriteLine("Error: MISSING Twitch Client ID");
                    Console.WriteLine("Please contact the author of this bot to re-release this application with the client ID");
                    Console.WriteLine();
                    Console.WriteLine("Shutting down now...");
                    Thread.Sleep(3000);
                    Environment.Exit(1);
                }

                // Check if user has the minimum info in order to run the bot
                // Tell user to input essential info
                while (string.IsNullOrWhiteSpace(_strBotName)
                    && string.IsNullOrWhiteSpace(_strBroadcasterName)
                    && string.IsNullOrWhiteSpace(twitchOAuth)
                    && string.IsNullOrWhiteSpace(twitchAccessToken))
                {
                    Console.WriteLine("You are missing essential info");
                    if (string.IsNullOrWhiteSpace(_strBotName))
                    {
                        Console.WriteLine("Enter your bot's username: ");
                        Properties.Settings.Default.botName = Console.ReadLine();
                        _strBotName = Properties.Settings.Default.botName;
                    }

                    if (string.IsNullOrWhiteSpace(_strBroadcasterName))
                    {
                        Console.WriteLine("Enter your Twitch username: ");
                        Properties.Settings.Default.broadcaster = Console.ReadLine();
                        _strBroadcasterName = Properties.Settings.Default.broadcaster;
                    }

                    if (string.IsNullOrWhiteSpace(twitchOAuth))
                    {
                        Console.WriteLine("Enter your Twitch OAuth: ");
                        Properties.Settings.Default.twitchOAuth = Console.ReadLine();
                        twitchOAuth = Properties.Settings.Default.twitchOAuth;
                    }

                    if (string.IsNullOrWhiteSpace(twitchAccessToken))
                    {
                        Console.WriteLine("Enter your Twitch Access Token: ");
                        Properties.Settings.Default.twitchAccessToken = Console.ReadLine();
                        twitchAccessToken = Properties.Settings.Default.twitchAccessToken;
                    }

                    Properties.Settings.Default.Save();
                    Console.WriteLine("Saved Settings!");
                    Console.WriteLine();
                }

                // Option to edit settings before running it
                Console.WriteLine("Do you want to edit your essential bot settings (y/n or yes/no)?");
                string strResponse = Console.ReadLine().ToLower();

                // Check if user inserted a valid option
                while (!(strResponse.Equals("y") || strResponse.Equals("yes") 
                    || strResponse.Equals("n") || strResponse.Equals("no")))
                {
                    Console.WriteLine("Please insert a valid option (y/n or yes/no)");
                    strResponse = Console.ReadLine().ToLower();
                }

                // Change some settings
                if (strResponse.Equals("y") || strResponse.Equals("yes"))
                {
                    int intOption = 0;
                    Console.WriteLine();

                    /* Loop until user is finished making changes */
                    do
                    {
                        // Display bot settings
                        Console.WriteLine("---> Here are your essential bot settings <---");
                        Console.WriteLine("1. Bot's username: " + Properties.Settings.Default.botName);
                        Console.WriteLine("2. Your main Twitch username: " + Properties.Settings.Default.broadcaster);
                        Console.WriteLine("3. Twitch OAuth: " + Properties.Settings.Default.twitchOAuth);
                        Console.WriteLine("4. Twitch Access Token: " + Properties.Settings.Default.twitchAccessToken);

                        Console.WriteLine();
                        Console.WriteLine("From the options 1-4 (or 0 to exit editing), which option do you want to edit?");

                        // Edit an option
                        if (int.TryParse(Console.ReadLine(), out intOption) && intOption < 5 && intOption >= 0)
                        {
                            Console.WriteLine();
                            switch (intOption)
                            {
                                case 1:
                                    Console.WriteLine("Enter your bot's new username: ");
                                    Properties.Settings.Default.botName = Console.ReadLine();
                                    _strBotName = Properties.Settings.Default.botName;
                                    break;
                                case 2:
                                    Console.WriteLine("Enter your new Twitch username: ");
                                    Properties.Settings.Default.broadcaster = Console.ReadLine();
                                    _strBroadcasterName = Properties.Settings.Default.broadcaster;
                                    break;
                                case 3:
                                    Console.WriteLine("Enter your new Twitch OAuth (include 'oauth:' along the 30 character phrase): ");
                                    Properties.Settings.Default.twitchOAuth = Console.ReadLine();
                                    twitchOAuth = Properties.Settings.Default.twitchOAuth;
                                    break;
                                case 4:
                                    Console.WriteLine("Enter your new Twitch access token: ");
                                    Properties.Settings.Default.twitchAccessToken = Console.ReadLine();
                                    twitchAccessToken = Properties.Settings.Default.twitchAccessToken;
                                    break;
                            }

                            Properties.Settings.Default.Save();
                            Console.WriteLine("Saved Settings!");
                            Console.WriteLine();
                        }
                        else
                        {
                            Console.WriteLine("Please write a valid option between 1-4 (or 0 to exit editing)");
                            Console.WriteLine();
                        }
                    } while (intOption != 0);

                    Console.WriteLine("Finished with editing settings");
                }
                else // No need to change settings
                    Console.WriteLine("Essential settings confirmed!");

                Console.WriteLine();

                // Extra settings menu
                Console.WriteLine("Do you want to edit your extra bot settings [twitter/discord/currency] (y/n or yes/no)?");
                strResponse = Console.ReadLine().ToLower();

                // Check if user inserted a valid option
                while (!(strResponse.Equals("y") || strResponse.Equals("yes")
                    || strResponse.Equals("n") || strResponse.Equals("no")))
                {
                    Console.WriteLine("Please insert a valid option (y/n or yes/no)");
                    strResponse = Console.ReadLine().ToLower();
                }

                // Change some settings
                if (strResponse.Equals("y") || strResponse.Equals("yes"))
                {
                    int intOption = 0;
                    Console.WriteLine();

                    /* Loop until user is finished making changes */
                    do
                    {
                        // Display bot settings
                        Console.WriteLine("---> Here are your extra bot settings <---");
                        Console.WriteLine("1. Twitter consumer key: " + Properties.Settings.Default.twitterConsumerKey);
                        Console.WriteLine("2. Twitter consumer secret: " + Properties.Settings.Default.twitterConsumerSecret);
                        Console.WriteLine("3. Twitter access token: " + Properties.Settings.Default.twitterAccessToken);
                        Console.WriteLine("4. Twitter access secret: " + Properties.Settings.Default.twitterAccessSecret);
                        Console.WriteLine("5. Discord link: " + Properties.Settings.Default.discordLink);
                        Console.WriteLine("6. Currency type: " + Properties.Settings.Default.currencyType);
                        Console.WriteLine("7. Enable Auto Tweets: " + Properties.Settings.Default.enableTweet);
                        Console.WriteLine("8. Enable Auto Display Songs: " + Properties.Settings.Default.enableDisplaySong);
                        Console.WriteLine("9. Stream Latency: " + Properties.Settings.Default.streamLatency);

                        Console.WriteLine();
                        Console.WriteLine("From the options 1-9 (or 0 to exit editing), which option do you want to edit?");

                        // Edit an option
                        string strOption = Console.ReadLine();
                        if (int.TryParse(strOption, out intOption) && intOption < 10 && intOption >= 0)
                        {
                            Console.WriteLine();
                            switch (intOption)
                            {
                                case 1:
                                    Console.WriteLine("Enter your new Twitter consumer key: ");
                                    Properties.Settings.Default.twitterConsumerKey = Console.ReadLine();
                                    twitterConsumerKey = Properties.Settings.Default.twitterConsumerKey;
                                    break;
                                case 2:
                                    Console.WriteLine("Enter your new Twitter consumer secret: ");
                                    Properties.Settings.Default.twitterConsumerSecret = Console.ReadLine();
                                    twitterConsumerSecret = Properties.Settings.Default.twitterConsumerSecret;
                                    break;
                                case 3:
                                    Console.WriteLine("Enter your new Twitter access token: ");
                                    Properties.Settings.Default.twitterAccessToken = Console.ReadLine();
                                    twitterAccessToken = Properties.Settings.Default.twitterAccessToken;
                                    break;
                                case 4:
                                    Console.WriteLine("Enter your new Twitter access secret: ");
                                    Properties.Settings.Default.twitterAccessSecret = Console.ReadLine();
                                    twitterAccessSecret = Properties.Settings.Default.twitterAccessSecret;
                                    break;
                                case 5:
                                    Console.WriteLine("Enter your new Discord link: ");
                                    Properties.Settings.Default.discordLink = Console.ReadLine();
                                    _strDiscordLink = Properties.Settings.Default.discordLink;
                                    break;
                                case 6:
                                    Console.WriteLine("Enter your new currency type: ");
                                    Properties.Settings.Default.currencyType = Console.ReadLine();
                                    _strCurrencyType = Properties.Settings.Default.currencyType;
                                    break;
                                case 7:
                                    Console.WriteLine("Want to enable (true) or disable (false) auto tweets: ");
                                    Properties.Settings.Default.enableTweet = Convert.ToBoolean(Console.ReadLine());
                                    _isAutoPublishTweet = Properties.Settings.Default.enableTweet;
                                    break;
                                case 8:
                                    Console.WriteLine("Want to enable (true) or disable (false) display songs from Spotify: ");
                                    Properties.Settings.Default.enableDisplaySong = Convert.ToBoolean(Console.ReadLine());
                                    _isAutoDisplaySong = Properties.Settings.Default.enableDisplaySong;
                                    break;
                                case 9:
                                    Console.WriteLine("Enter your new stream latency (in seconds): ");
                                    Properties.Settings.Default.streamLatency = int.Parse(Console.ReadLine());
                                    _intStreamLatency = Properties.Settings.Default.streamLatency;
                                    break;
                            }

                            Properties.Settings.Default.Save();
                            Console.WriteLine("Saved Settings!");
                            Console.WriteLine();
                        }
                        else
                        {
                            Console.WriteLine("Please write a valid option between 1-9 (or 0 to exit editing)");
                            Console.WriteLine();
                        }
                    } while (intOption != 0);

                    Console.WriteLine("Finished with editing settings");
                }
                else // No need to change settings
                    Console.WriteLine("Extra settings confirmed!");

                Console.WriteLine();

                // Get broadcaster ID so the user can only see their data from the db
                _intBroadcasterID = getBroadcasterID(_strBroadcasterName.ToLower());

                // Add broadcaster as new user to database
                if (_intBroadcasterID == 0)
                {
                    string query = "INSERT INTO tblBroadcasters (username) VALUES (@username)";

                    using (SqlConnection conn = new SqlConnection(_connStr))
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = _strBroadcasterName;

                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }

                    _intBroadcasterID = getBroadcasterID(_strBroadcasterName.ToLower());
                }

                // Try looking for the broadcaster's ID again
                if (_intBroadcasterID == 0)
                {
                    Console.WriteLine("Cannot find a broadcaster ID for you. " 
                        + "Please contact the author with a detailed description of the issue");
                    Thread.Sleep(3000);
                    Environment.Exit(1);
                }

                /* Connect to local Spotify client */
                _spotify = new SpotifyControl();
                _spotify.Connect();

                Console.WriteLine();
                Console.WriteLine("Time to get to work!");
                Console.WriteLine();

                /* Make sure usernames are set to lowercase for the rest of the application */
                _strBotName = _strBotName.ToLower();
                _strBroadcasterName = _strBroadcasterName.ToLower();

                // Password from www.twitchapps.com/tmi/
                // include the "oauth:" portion
                // Use chat bot's oauth
                /* main server: irc.twitch.tv, 6667 */
                _irc = new IrcClient("irc.twitch.tv", 6667, _strBotName, twitchOAuth, _strBroadcasterName);

                // Update channel info
                _intFollowers = TaskJSON.GetChannel().Result.followers;
                _strBroadcasterGame = TaskJSON.GetChannel().Result.game;

                /* Make new thread to get messages */
                Thread thdIrcClient = new Thread(() => TwitchBotApp.GetChatBox(isSongRequestAvail, twitchAccessToken, hasTwitterInfo));
                thdIrcClient.Start();

                /* Whisper broadcaster bot settings */
                Console.WriteLine("---> Extra Bot Settings <---");
                Console.WriteLine("Discord link: " + _strDiscordLink);
                Console.WriteLine("Currency type: " + _strCurrencyType);
                Console.WriteLine("Enable Auto Tweets: " + _isAutoPublishTweet);
                Console.WriteLine("Enable Auto Display Songs: " + _isAutoDisplaySong);
                Console.WriteLine("Stream latency: " + _intStreamLatency + " second(s)");
                Console.WriteLine();

                /* Start listening for delayed messages */
                DelayMsg delayMsg = new DelayMsg();
                delayMsg.Start();

                /* Get list of mods */
                _mod = new Moderator();
                setListMods();

                /* Get list of timed out users */
                _timeout = new Timeout();
                setListTimeouts();

                /* Ping to twitch server to prevent auto-disconnect */
                PingSender ping = new PingSender();
                ping.Start();

                /* Remind viewers of bot's existance */
                PresenceReminder preRmd = new PresenceReminder();
                preRmd.Start();

                /* Authenticate to Twitter if possible */
                if (hasTwitterInfo)
                {
                    Auth.ApplicationCredentials = new TwitterCredentials(
                        twitterConsumerKey, twitterConsumerSecret,
                        twitterAccessToken, twitterAccessSecret
                    );
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "Program", "Main(string[])", true);
            }
        }

        

        /// <summary>
        /// Test that the server is connected
        /// </summary>
        /// <param name="connectionString">The connection string</param>
        /// <returns>true if the connection is opened</returns>
        private static bool IsServerConnected(string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    return true;
                }
                catch (SqlException)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Find the Nth index of a character
        /// </summary>
        /// <param name="s"></param>
        /// <param name="findChar"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static int GetNthIndex(string s, char findChar, int n)
        {
            int count = 0;
            
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == findChar)
                {
                    count++;
                    if (count == n)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        public static void SendTweet(string pendingMessage, string command)
        {
            // Check if there are at least two quotation marks before sending message using LINQ
            string resultMessage = "";
            if (command.Count(c => c == '"') < 2)
            {
                resultMessage = "Please use at least two quotation marks (\") before sending a tweet. " +
                    "Quotations are used to find the start and end of a message wanting to be sent";
                Console.WriteLine(resultMessage);
                _irc.sendPublicChatMessage(resultMessage);
                return;
            }

            // Get message from quotation parameter
            string tweetMessage = string.Empty;
            int length = (pendingMessage.LastIndexOf('"') - pendingMessage.IndexOf('"')) - 1;
            if (length == -1) // if no quotations were found
                length = pendingMessage.Length;
            int startIndex = pendingMessage.IndexOf('"') + 1;
            tweetMessage = pendingMessage.Substring(startIndex, length);

            // Check if message length is at or under 140 characters
            var basicTweet = new object();
            
            if (tweetMessage.Length <= 140)
            {
                basicTweet = Tweet.PublishTweet(tweetMessage);
                resultMessage = "Tweet successfully published!";
                Console.WriteLine(resultMessage);
                _irc.sendPublicChatMessage(resultMessage);
            }
            else
            {
                int overCharLimit = tweetMessage.Length - 140;
                resultMessage = "The message you attempted to tweet had " + overCharLimit +
                    " characters more than the 140 character limit. Please shorten your message and try again";
                Console.WriteLine(resultMessage);
                _irc.sendPublicChatMessage(resultMessage);
            }
        }

        public static void LogError(Exception ex, string strClass, string strMethod, bool hasToExit, string strCmd = "N/A", string strUserMsg = "N/A")
        {
            Console.WriteLine("Error: " + ex.Message);

            // if username not available grab default user to show local error after db connection
            if (_intBroadcasterID == 0)
                _intBroadcasterID = getBroadcasterID("n/a");

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
                cmd.Parameters.Add("@class", SqlDbType.VarChar, 100).Value = strClass;
                cmd.Parameters.Add("@method", SqlDbType.VarChar, 100).Value = strMethod;
                cmd.Parameters.Add("@msg", SqlDbType.VarChar, 4000).Value = ex.Message;
                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;
                cmd.Parameters.Add("@command", SqlDbType.VarChar, 100).Value = strCmd;
                cmd.Parameters.Add("@userMsg", SqlDbType.VarChar, 500).Value = strUserMsg;

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }

            string strPublicErrMsg = "I ran into an unexpected internal error! "
                + "@" + _strBroadcasterName + " please look into the error log when you have time";

            if (hasToExit)
                strPublicErrMsg += " I am leaving as well. Have a great time with this stream everyone :)";

            if (_irc != null)
                _irc.sendPublicChatMessage(strPublicErrMsg);

            if (hasToExit)
            {
                Console.WriteLine();
                Console.WriteLine("Shutting down now...");
                Thread.Sleep(3000);
                Environment.Exit(1);
            }
        }

        private static string chatterValid(string strOrigUser, string strRecipient, string strSearchCriteria = "")
        {
            // Check if the requested user is this bot
            if (strRecipient.Equals(_strBotName))
                return "mod";

            // Grab list of chatters (viewers, mods, etc.)
            Chatters chatters = TaskJSON.GetChatters().Result.chatters;

            // check moderators
            if (strSearchCriteria.Equals("") || strSearchCriteria.Equals("mod"))
            {
                foreach (string moderator in chatters.moderators)
                {
                    if (strRecipient.ToLower().Equals(moderator))
                        return "mod";
                }
            }

            // check viewers
            if (strSearchCriteria.Equals("") || strSearchCriteria.Equals("viewer"))
            {
                foreach (string viewer in chatters.viewers)
                {
                    if (strRecipient.ToLower().Equals(viewer))
                        return "viewer";
                }
            }

            // check staff
            if (strSearchCriteria.Equals("") || strSearchCriteria.Equals("staff"))
            {
                foreach (string staffMember in chatters.staff)
                {
                    if (strRecipient.ToLower().Equals(staffMember))
                        return "staff";
                }
            }

            // check admins
            if (strSearchCriteria.Equals("") || strSearchCriteria.Equals("admin"))
            {
                foreach (string admin in chatters.admins)
                {
                    if (strRecipient.ToLower().Equals(admin))
                        return "admin";
                }
            }

            // check global moderators
            if (strSearchCriteria.Equals("") || strSearchCriteria.Equals("gmod"))
            {
                foreach (string globalMod in chatters.global_mods)
                {
                    if (strRecipient.ToLower().Equals(globalMod))
                        return "gmod";
                }
            }

            // finished searching with no results
            _irc.sendPublicChatMessage("@" + strOrigUser + ": I cannot find the user you wanted to interact with. Perhaps the user left us?");
            return "";
        }

        public static bool reactionCmd(string message, string strOrigUser, string strRecipient, string strMsgToSelf, string strAction, string strAddlMsg = "")
        {
            string strRoleType = chatterValid(strOrigUser, strRecipient);

            // check if user currently watching the channel
            if (!string.IsNullOrEmpty(strRoleType))
            {
                if (strOrigUser.Equals(strRecipient))
                    _irc.sendPublicChatMessage(strMsgToSelf + " @" + strOrigUser);
                else
                    _irc.sendPublicChatMessage(strOrigUser + " " + strAction + " @" + strRecipient + " " + strAddlMsg);

                return true;
            }
            else
                return false;
        }
        
        public static int currencyBalance(string username)
        {
            int intBalance = -1;

            // check if user already has a bank account
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblBank WHERE broadcaster = @broadcaster", conn))
                {
                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (username.Equals(reader["username"].ToString()))
                                {
                                    intBalance = int.Parse(reader["wallet"].ToString());
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return intBalance;
        }

        public static void updateWallet(string strWalletOwner, int intNewWalletBalance)
        {
            string query = "UPDATE dbo.tblBank SET wallet = @wallet WHERE (username = @username AND broadcaster = @broadcaster)";

            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add("@wallet", SqlDbType.Int).Value = intNewWalletBalance;
                cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = strWalletOwner;
                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private static void setListMods()
        {
            try
            {
                List<string> lstMod = new List<string>();

                using (SqlConnection conn = new SqlConnection(_connStr))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblModerators WHERE broadcaster = @broadcaster", conn))
                    {
                        cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    lstMod.Add(reader["username"].ToString());
                                }
                            }
                        }
                    }
                }

                _mod.setLstMod(lstMod);
            }
            catch (Exception ex)
            {   
                Console.WriteLine(ex.Message);
                LogError(ex, "Program", "setListMods()", true);
            }
        }

        private static void setListTimeouts()
        {
            try
            {
                string query = "DELETE FROM tblTimeout WHERE broadcaster = @broadcaster AND timeout < GETDATE()";

                // Create connection and command
                using (SqlConnection conn = new SqlConnection(_connStr))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }

                Dictionary<string, DateTime> dicTimeout = new Dictionary<string, DateTime>();

                using (SqlConnection conn = new SqlConnection(_connStr))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblTimeout WHERE broadcaster = @broadcaster", conn))
                    {
                        cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    dicTimeout.Add(reader["username"].ToString(), Convert.ToDateTime(reader["timeout"]));
                                }
                            }
                        }
                    }
                }

                _timeout.setLstTimeout(dicTimeout);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                LogError(ex, "Program", "setListTimeouts()", true);
            }
        }

        public static bool isUserTimedout(string strUserName)
        {
            if (_timeout.getLstTimeout().ContainsKey(strUserName))
            {
                string timeout = _timeout.getTimoutFromUser(strUserName, _intBroadcasterID, _connStr);

                if (timeout.Equals("0 seconds"))
                    _irc.sendPublicChatMessage("You are now allowed to talk to me again @" + strUserName
                        + ". Please try the requested command once more");
                else
                    _irc.sendPublicChatMessage("I am not allowed to talk to you for " + timeout);

                return true;
            }

            return false;
        }

        private static int getBroadcasterID(string strBroadcaster)
        {
            int intBroadcasterID = 0;

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
                                    intBroadcasterID = int.Parse(reader["id"].ToString());
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return intBroadcasterID;
        }

        public static string Effectiveness()
        {
            Random rnd = new Random(DateTime.Now.Millisecond);
            int intEffectiveLvl = rnd.Next(3); // between 0 and 2
            string strEffectiveness = "";

            if (intEffectiveLvl == 0)
                strEffectiveness = "It's super effective!";
            else if (intEffectiveLvl == 1)
                strEffectiveness = "It wasn't very effective";
            else
                strEffectiveness = "It had no effect";

            return strEffectiveness;
        }

    }
}
