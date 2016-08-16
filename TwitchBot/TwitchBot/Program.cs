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

namespace TwitchBot
{
    class Program
    {
        public static SpotifyLocalAPI _spotify;
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
        public static TimeZone _localZone = TimeZone.CurrentTimeZone;
        public static int _intFollowers = 0;
        public static string _strDiscordLink = "Link unavailable at the moment"; // provide discord server link if available
        public static bool _isAutoPublishTweet = false; // set to auto publish tweets (disabled by default)
        public static bool _isAutoDisplaySong = false; // set to auto song status (disabled by default)
        public static List<Tuple<string, DateTime>> _lstTupDelayMsg = new List<Tuple<string, DateTime>>(); // used to handle delayed msgs

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
                // Grab connection string
                //_connStr = ConfigurationManager.ConnectionStrings["TwitchBot.Properties.Settings.conn"].ConnectionString; // production only
                _connStr = ConfigurationManager.ConnectionStrings["TwitchBot.Properties.Settings.connTest"].ConnectionString; // debugging only

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
                    Thread.Sleep(3000);
                    Environment.Exit(1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Message: " + ex.Message);
                Console.WriteLine("**** Local troubleshooting needed by author of this bot ****");
                Thread.Sleep(3000);
                Environment.Exit(1);
            }

            /* Try to grab the info needed for the bot to connect to the channel */
            try
            {
                Console.WriteLine("Database connection successful!");
                Console.WriteLine();
                Console.WriteLine("Checking if user settings has all necessary info...");

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
                    Thread.Sleep(3000);
                    Environment.Exit(1);
                }

                // Check if user has the minimum info in order to run the bot
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
                Console.WriteLine("Time to get to work!");
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

                /* Connect to local spotify client */
                _spotify = new SpotifyLocalAPI();
                SpotifyControl spotifyCtrl = new SpotifyControl();
                _spotify.OnPlayStateChange += spotifyCtrl.spotify_OnPlayStateChange;
                _spotify.OnTrackChange += spotifyCtrl.spotify_OnTrackChange;

                /* Make sure usernames are set to lowercase for the rest of the application */
                _strBotName = _strBotName.ToLower();
                _strBroadcasterName = _strBroadcasterName.ToLower();

                // Password from www.twitchapps.com/tmi/
                // include the "oauth:" portion
                // Use chat bot's oauth
                /* main server: irc.twitch.tv, 6667 */
                _irc = new IrcClient("irc.twitch.tv", 6667, _strBotName, twitchOAuth, _strBroadcasterName);

                // Update channel info
                _intFollowers = GetChannel().Result.followers;
                _strBroadcasterGame = GetChannel().Result.game;

                /* Make new thread to get messages */
                Thread thdIrcClient = new Thread(() => GetChatBox(spotifyCtrl, isSongRequestAvail, twitchAccessToken, hasTwitterInfo));
                thdIrcClient.Start();

                spotifyCtrl.Connect(); // attempt to connect to local Spotify client

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
        /// Monitor chat box for commands
        /// </summary>
        /// <param name="spotifyCtrl"></param>
        /// <param name="isSongRequestAvail"></param>
        /// <param name="twitchAccessToken"></param>
        /// <param name="hasTwitterInfo"></param>
        private static void GetChatBox(SpotifyControl spotifyCtrl, bool isSongRequestAvail, string twitchAccessToken, bool hasTwitterInfo)
        {
            try
            {
                /* Master loop */
                while (true)
                {
                    // Read any message inside the chat room
                    string message = _irc.readMessage();
                    Console.WriteLine(message);

                    if (!string.IsNullOrEmpty(message))
                    {
                        /* 
                        * Get user name and message from chat 
                        * and check if user has access to certain functions
                        */
                        if (message.Contains("PRIVMSG"))
                        {
                            // Modify message to only show user and message
                            int intIndexParseSign = message.IndexOf('!');
                            StringBuilder strBdrMessage = new StringBuilder(message);
                            string strUserName = message.Substring(1, intIndexParseSign - 1);

                            intIndexParseSign = message.IndexOf(" :");
                            strBdrMessage.Remove(0, intIndexParseSign + 2); // remove unnecessary info before and including the parse symbol
                            message = strBdrMessage.ToString();

                            /* 
                             * Broadcaster commands 
                             */
                            if (strUserName.Equals(_strBroadcasterName))
                            {
                                if (message.Equals("!botsettings"))
                                {
                                    try
                                    {
                                        _irc.sendPublicChatMessage("Auto tweets set to \"" + _isAutoPublishTweet + "\" "
                                            + "|| Auto display songs set to \"" + _isAutoDisplaySong + "\" " 
                                            + "|| Currency set to \"" + _strCurrencyType + "\"");
                                    }
                                    catch (Exception ex)
                                    {
                                        LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!botsettings");
                                    }
                                }

                                if (message.Equals("!exitbot"))
                                {
                                    try
                                    {
                                        _irc.sendPublicChatMessage("Bye! Have a beautiful time!");
                                        Environment.Exit(0); // exit program
                                    }
                                    catch (Exception ex)
                                    {
                                        LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!exitbot");
                                    }
                                }

                                if (message.Equals("!spotifyconnect"))
                                    spotifyCtrl.Connect(); // manually connect to spotify

                                if (message.Equals("!spotifyplay"))
                                    spotifyCtrl.playBtn_Click();

                                if (message.Equals("!spotifypause"))
                                    spotifyCtrl.pauseBtn_Click();

                                if (message.Equals("!spotifyprev"))
                                    spotifyCtrl.prevBtn_Click();

                                if (message.Equals("!spotifynext"))
                                    spotifyCtrl.skipBtn_Click();

                                if (message.Equals("!enabletweet"))
                                {
                                    try
                                    {
                                        if (!hasTwitterInfo)
                                            _irc.sendPublicChatMessage("You are missing twitter info @" + _strBroadcasterName);
                                        else
                                        {
                                            _isAutoPublishTweet = true;
                                            Properties.Settings.Default.enableTweet = _isAutoPublishTweet;
                                            Properties.Settings.Default.Save();

                                            Console.WriteLine("Auto publish tweets is set to [" + _isAutoPublishTweet + "]");
                                            _irc.sendPublicChatMessage(_strBroadcasterName + ": Automatic tweets is set to \"" + _isAutoPublishTweet + "\"");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!enabletweet");
                                    }
                                }

                                if (message.Equals("!disabletweet"))
                                {
                                    try
                                    {
                                        if (!hasTwitterInfo)
                                            _irc.sendPublicChatMessage("You are missing twitter info @" + _strBroadcasterName);
                                        else
                                        {
                                            _isAutoPublishTweet = false;
                                            Properties.Settings.Default.enableTweet = _isAutoPublishTweet;
                                            Properties.Settings.Default.Save();

                                            Console.WriteLine("Auto publish tweets is set to [" + _isAutoPublishTweet + "]");
                                            _irc.sendPublicChatMessage(_strBroadcasterName + ": Automatic tweets is set to \"" + _isAutoPublishTweet + "\"");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!disabletweet");
                                    }
                                }

                                if (message.Equals("!srmode on"))
                                {
                                    try
                                    {
                                        isSongRequestAvail = true;
                                        _irc.sendPublicChatMessage("Song requests enabled");
                                    }
                                    catch (Exception ex)
                                    {
                                        LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!srmode on");
                                    }
                                }

                                if (message.Equals("!srmode off"))
                                {
                                    try
                                    {
                                        isSongRequestAvail = false;
                                        _irc.sendPublicChatMessage("Song requests disabled");
                                    }
                                    catch (Exception ex)
                                    {
                                        LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!srmode off");
                                    }
                                }

                                if (message.StartsWith("!updatetitle"))
                                {
                                    try
                                    {
                                        // Get title from command parameter
                                        string title = string.Empty;
                                        int lengthParam1 = (GetNthIndex(message, '"', 2) - message.IndexOf('"')) - 1;
                                        int startIndexParam1 = message.IndexOf('"') + 1;
                                        title = message.Substring(startIndexParam1, lengthParam1);

                                        // Send HTTP method PUT to base URI in order to change the title
                                        var client = new RestClient("https://api.twitch.tv/kraken/channels/" + _strBroadcasterName);
                                        var request = new RestRequest(Method.PUT);
                                        request.AddHeader("cache-control", "no-cache");
                                        request.AddHeader("content-type", "application/json");
                                        request.AddHeader("authorization", "OAuth " + twitchAccessToken);
                                        request.AddHeader("accept", "application/vnd.twitchtv.v3+json");
                                        request.AddParameter("application/json", "{\"channel\":{\"status\":\"" + title + "\"}}",
                                            ParameterType.RequestBody);

                                        IRestResponse response = null;
                                        try
                                        {
                                            response = client.Execute(request);
                                            string statResponse = response.StatusCode.ToString();
                                            if (statResponse.Contains("OK"))
                                            {
                                                _irc.sendPublicChatMessage("Twitch channel title updated to \"" + title +
                                                    "\" || Refresh your browser ([CTRL] + [F5]) or twitch app in order to see the change");
                                            }
                                            else
                                                Console.WriteLine(response.ErrorMessage);
                                        }
                                        catch (WebException ex)
                                        {
                                            if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.BadRequest)
                                            {
                                                Console.WriteLine("Error 400 detected!!");
                                            }
                                            response = (IRestResponse)ex.Response;
                                            Console.WriteLine("Error: " + response);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!updatetitle");
                                    }
                                }

                                if (message.StartsWith("!updategame"))
                                {
                                    try
                                    {
                                        // Get title from command parameter
                                        string game = string.Empty;
                                        int lengthParam1 = (message.LastIndexOf('"') - message.IndexOf('"')) - 1;
                                        int startIndexParam1 = message.IndexOf('"') + 1;
                                        game = message.Substring(startIndexParam1, lengthParam1);

                                        // Send HTTP method PUT to base URI in order to change the game
                                        var client = new RestClient("https://api.twitch.tv/kraken/channels/" + _strBroadcasterName);
                                        var request = new RestRequest(Method.PUT);
                                        request.AddHeader("cache-control", "no-cache");
                                        request.AddHeader("content-type", "application/json");
                                        request.AddHeader("authorization", "OAuth " + twitchAccessToken);
                                        request.AddHeader("accept", "application/vnd.twitchtv.v3+json");
                                        request.AddParameter("application/json", "{\"channel\":{\"game\":\"" + game + "\"}}",
                                            ParameterType.RequestBody);

                                        IRestResponse response = null;
                                        try
                                        {
                                            response = client.Execute(request);
                                            string statResponse = response.StatusCode.ToString();
                                            if (statResponse.Contains("OK"))
                                            {
                                                _irc.sendPublicChatMessage("Twitch channel game status updated to \"" + game +
                                                    "\" || Restart your connection to the stream or twitch app in order to see the change");
                                                if (_isAutoPublishTweet && hasTwitterInfo)
                                                {
                                                    SendTweet("Watch me stream " + game + " on Twitch" + Environment.NewLine
                                                        + "http://goo.gl/SNyDFD" + Environment.NewLine
                                                        + "#twitch #gaming #streaming", message);
                                                }
                                            }
                                            else
                                                Console.WriteLine(response.ErrorMessage);
                                        }
                                        catch (WebException ex)
                                        {
                                            if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.BadRequest)
                                            {
                                                Console.WriteLine("Error 400 detected!!");
                                            }
                                            response = (IRestResponse)ex.Response;
                                            Console.WriteLine("Error: " + response);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!updategame");
                                    }
                                }

                                if (message.StartsWith("!tweet"))
                                {
                                    try
                                    {
                                        if (!hasTwitterInfo)
                                            _irc.sendPublicChatMessage("You are missing twitter info @" + _strBroadcasterName);
                                        else
                                        {
                                            string command = message;
                                            SendTweet(message, command);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!tweet");
                                    }
                                }

                                if (message.StartsWith("!displaysongs on"))
                                {
                                    try
                                    {
                                        _isAutoDisplaySong = true;
                                        Properties.Settings.Default.enableDisplaySong = _isAutoDisplaySong;
                                        Properties.Settings.Default.Save();

                                        Console.WriteLine("Auto display songs is set to [" + _isAutoDisplaySong + "]");
                                        _irc.sendPublicChatMessage(_strBroadcasterName + ": Automatic display songs is set to \"" + _isAutoDisplaySong + "\"");
                                    }
                                    catch (Exception ex)
                                    {
                                        LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!displaysongs on");
                                    }
                                }

                                if (message.StartsWith("!displaysongs off"))
                                {
                                    try
                                    {
                                        _isAutoDisplaySong = false;
                                        Properties.Settings.Default.enableDisplaySong = _isAutoDisplaySong;
                                        Properties.Settings.Default.Save();

                                        Console.WriteLine("Auto display songs is set to [" + _isAutoDisplaySong + "]");
                                        _irc.sendPublicChatMessage(_strBroadcasterName + ": Automatic display songs is set to \"" + _isAutoDisplaySong + "\"");
                                    }
                                    catch (Exception ex)
                                    {
                                        LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!displaysongs off");
                                    }
                                }

                                if (message.StartsWith("!addmod") && message.Contains("@"))
                                {
                                    try
                                    {
                                        string strRecipient = message.Substring(message.IndexOf("@") + 1);

                                        _mod.addNewModToLst(strRecipient.ToLower(), 1, _connStr);

                                        _irc.sendPublicChatMessage("@" + strRecipient + " is now able to use moderator features within MrSandmanBot");
                                    }
                                    catch (Exception ex)
                                    {
                                        LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!addmod");
                                    }
                                }

                                if (message.StartsWith("!delmod") && message.Contains("@"))
                                {
                                    try
                                    {
                                        string strRecipient = message.Substring(message.IndexOf("@") + 1);

                                        _mod.delOldModFromLst(strRecipient.ToLower(), 1, _connStr);

                                        _irc.sendPublicChatMessage("@" + strRecipient + " is not able to use moderator features within MrSandmanBot any longer");
                                    }
                                    catch (Exception ex)
                                    {
                                        LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!delmod");
                                    }
                                }

                                if (message.Equals("!listmod"))
                                {
                                    try
                                    {
                                        string strListModMsg = "";

                                        if (_mod.getLstMod().Count > 0)
                                        {
                                            foreach (string name in _mod.getLstMod())
                                                strListModMsg += name + " | ";

                                            strListModMsg = strListModMsg.Remove(strListModMsg.Length - 3); // removed extra " | "
                                            _irc.sendPublicChatMessage("List of bot moderators: " + strListModMsg);
                                        }
                                        else
                                            _irc.sendPublicChatMessage("No one is ruling over me other than you @" + _strBroadcasterName);
                                    }
                                    catch (Exception ex)
                                    {
                                        LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!listmod");
                                    }
                                }

                                if (message.StartsWith("!addcountdown"))
                                {
                                    try
                                    {
                                        // get due date of countdown
                                        string strCountdownDT = message.Substring(14, 20); // MM-DD-YY hh:mm:ss AM
                                        DateTime dtCountdown = Convert.ToDateTime(strCountdownDT);

                                        // get message of countdown
                                        string strCountdownMsg = message.Substring(34);

                                        // log new countdown into db
                                        string query = "INSERT INTO tblCountdown (dueDate, message, broadcaster) VALUES (@dueDate, @message, @broadcaster)";

                                        using (SqlConnection conn = new SqlConnection(_connStr))
                                        using (SqlCommand cmd = new SqlCommand(query, conn))
                                        {
                                            cmd.Parameters.Add("@dueDate", SqlDbType.DateTime).Value = dtCountdown;
                                            cmd.Parameters.Add("@message", SqlDbType.VarChar, 50).Value = strCountdownMsg;
                                            cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;

                                            conn.Open();
                                            cmd.ExecuteNonQuery();
                                        }

                                        Console.WriteLine("Countdown added!");
                                        _irc.sendPublicChatMessage("");
                                    }
                                    catch (Exception ex)
                                    {
                                        LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!addcountdown");
                                    }
                                }

                                if (message.StartsWith("!editcountdown"))
                                {
                                    try
                                    {
                                        int intReqCountdownID = -1;
                                        bool bolValidCountdownID = int.TryParse(message.Substring(18, GetNthIndex(message, ' ', 2) - GetNthIndex(message, ' ', 1)), out intReqCountdownID);

                                        // validate requested countdown ID
                                        if (!bolValidCountdownID || intReqCountdownID < 0)
                                            _irc.sendPublicChatMessage("Please use a positive whole number to find your countdown ID");
                                        else
                                        {
                                            // check if countdown ID exists
                                            int intCountdownID = -1;
                                            using (SqlConnection conn = new SqlConnection(_connStr))
                                            {
                                                conn.Open();
                                                using (SqlCommand cmd = new SqlCommand("SELECT id, broadcaster FROM tblCountdown " 
                                                    + "WHERE broadcaster = @broadcaster", conn))
                                                {
                                                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;
                                                    using (SqlDataReader reader = cmd.ExecuteReader())
                                                    {
                                                        if (reader.HasRows)
                                                        {
                                                            while (reader.Read())
                                                            {
                                                                if (intReqCountdownID.ToString().Equals(reader["id"].ToString()))
                                                                {
                                                                    intCountdownID = int.Parse(reader["id"].ToString());
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            // check if countdown ID was retrieved
                                            if (intCountdownID == -1)
                                                _irc.sendPublicChatMessage($"Cannot find the countdown ID: {intReqCountdownID}");
                                            else
                                            {
                                                int intInputType = -1; // check if input is in the correct format
                                                DateTime dtCountdown = new DateTime();
                                                string strCountdownInput = message.Substring(20);

                                                // check if user wants to edit the DateTime or message
                                                if (message.StartsWith("!editcountdownDTE"))
                                                {
                                                    // get new due date of countdown
                                                    bool bolValidCountdownDT = DateTime.TryParse(strCountdownInput, out dtCountdown);

                                                    if (!bolValidCountdownDT)
                                                        _irc.sendPublicChatMessage("Please enter a valid date and time @" + strUserName);
                                                    else
                                                        intInputType = 1;
                                                }
                                                else if (message.StartsWith("!editcountdownMSG"))
                                                {
                                                    // get new message of countdown
                                                    if (string.IsNullOrWhiteSpace(strCountdownInput))
                                                        _irc.sendPublicChatMessage("Please enter a valid message @" + strUserName);
                                                    else
                                                        intInputType = 2;
                                                }

                                                // if input is correct update db
                                                if (intInputType > 0)
                                                {
                                                    string strQuery = "";

                                                    if (intInputType == 1)
                                                        strQuery = "UPDATE dbo.tblCountdown SET dueDate = @dueDate WHERE (Id = @id AND broadcaster = @broadcaster)";
                                                    else if (intInputType == 2)
                                                        strQuery = "UPDATE dbo.tblCountdown SET message = @message WHERE (Id = @id AND broadcaster = @broadcaster)";

                                                    using (SqlConnection conn = new SqlConnection(_connStr))
                                                    using (SqlCommand cmd = new SqlCommand(strQuery, conn))
                                                    {
                                                        // append proper parameter
                                                        if (intInputType == 1)
                                                            cmd.Parameters.Add("@dueDate", SqlDbType.DateTime).Value = dtCountdown;
                                                        else if (intInputType == 2)
                                                            cmd.Parameters.Add("@message", SqlDbType.VarChar, 50).Value = strCountdownInput;

                                                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = intCountdownID;   
                                                        cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;

                                                        conn.Open();
                                                        cmd.ExecuteNonQuery();
                                                    }

                                                    Console.WriteLine($"Changes to countdown ID: {intReqCountdownID} have been made @{strUserName}");
                                                    _irc.sendPublicChatMessage($"Changes to countdown ID: {intReqCountdownID} have been made @{strUserName}");
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!editcountdown");
                                    }
                                }

                                if (message.Equals("!listcountdown"))
                                {
                                    try
                                    {
                                        string strCountdownList = "";

                                        using (SqlConnection conn = new SqlConnection(_connStr))
                                        {
                                            conn.Open();
                                            using (SqlCommand cmd = new SqlCommand("SELECT Id, dueDate, message, broadcaster FROM tblCountdown "
                                                + "WHERE broadcaster = @broadcaster ORDER BY Id", conn))
                                            {
                                                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;
                                                using (SqlDataReader reader = cmd.ExecuteReader())
                                                {
                                                    if (reader.HasRows)
                                                    {
                                                        while (reader.Read())
                                                        {
                                                            strCountdownList += "ID: " +  reader["Id"].ToString() 
                                                                + " Message: \"" + reader["message"].ToString() 
                                                                + "\" Time: \"" + reader["dueDate"].ToString()
                                                                + "\" || ";
                                                        }
                                                        StringBuilder strBdrPartyList = new StringBuilder(strCountdownList);
                                                        strBdrPartyList.Remove(strCountdownList.Length - 4, 4); // remove extra " || "
                                                        strCountdownList = strBdrPartyList.ToString(); // replace old countdown list string with new
                                                        _irc.sendPublicChatMessage(strCountdownList);
                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine("No countdown messages are set at the moment");
                                                        _irc.sendPublicChatMessage("No countdown messages are set at the moment @" + strUserName);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!listcountdown");
                                    }
                                }

                                /* insert more broadcaster commands here */
                            }

                            /*
                             * Moderator commands
                             */
                            if (strUserName.Equals(_strBroadcasterName) || _mod.getLstMod().Contains(strUserName.ToLower()))
                            {
                                if (message.Equals("!discord") && !isUserTimedout(strUserName))
                                {
                                    try
                                    {
                                        _irc.sendPublicChatMessage("Come be a potato with us on our own Discord server! " + _strDiscordLink);
                                    }
                                    catch (Exception ex)
                                    {
                                        LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!discord");
                                    }
                                }

                                if (message.StartsWith("!charge") && message.Contains("@") && !isUserTimedout(strUserName))
                                {
                                    try
                                    {
                                        if (message.StartsWith("!charge @"))
                                            _irc.sendPublicChatMessage("Please enter a valid amount to a user @" + strUserName);
                                        else
                                        {
                                            int intIndexAction = 8;
                                            int intFee = -1;
                                            bool validFee = int.TryParse(message.Substring(intIndexAction, message.IndexOf("@") - intIndexAction - 1), out intFee);
                                            string strRecipient = message.Substring(message.IndexOf("@") + 1).ToLower();
                                            int intWallet = currencyBalance(strRecipient);

                                            // Check user's bank account
                                            if (intWallet == -1)
                                                _irc.sendPublicChatMessage("The user '" + strRecipient + "' is not currently banking with us @" + strUserName);
                                            else if (intWallet == 0)
                                                _irc.sendPublicChatMessage("'" + strRecipient + "' is out of " + _strCurrencyType + " @" + strUserName);
                                            // Check if fee can be accepted
                                            else if (intFee > 0)
                                                _irc.sendPublicChatMessage("Please insert a negative amount or use the !deposit command to add " + _strCurrencyType + " to a user");
                                            else if (!validFee)
                                                _irc.sendPublicChatMessage("The fee wasn't accepted. Please try again with negative whole numbers only");
                                            else /* Insert fee from wallet */
                                            {
                                                intWallet = intWallet + intFee;

                                                // Zero out account balance if user is being charged more than they have
                                                if (intWallet < 0)
                                                    intWallet = 0;

                                                string query = "UPDATE dbo.tblBank SET wallet = @wallet WHERE (username = @username AND broadcaster = @broadcaster)";

                                                using (SqlConnection conn = new SqlConnection(_connStr))
                                                using (SqlCommand cmd = new SqlCommand(query, conn))
                                                {
                                                    cmd.Parameters.Add("@wallet", SqlDbType.Int).Value = intWallet;
                                                    cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = strRecipient;
                                                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;

                                                    conn.Open();
                                                    cmd.ExecuteNonQuery();
                                                }

                                                // prompt user's balance
                                                if (intWallet == 0)
                                                    _irc.sendPublicChatMessage("Charged " + intFee.ToString().Replace("-", "") + " " + _strCurrencyType + " to " + strRecipient
                                                        + "'s account! They are out of " + _strCurrencyType + " to spend");
                                                else
                                                    _irc.sendPublicChatMessage("Charged " + intFee.ToString().Replace("-", "") + " " + _strCurrencyType + " to " + strRecipient
                                                        + "'s account! They only have " + intWallet + " " + _strCurrencyType + " to spend");
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!charge");
                                    }
                                }

                                if (message.StartsWith("!deposit") && message.Contains("@") && !isUserTimedout(strUserName))
                                {
                                    try
                                    {
                                        if (message.StartsWith("!deposit @"))
                                            _irc.sendPublicChatMessage("Please enter a valid amount to a user @" + strUserName);
                                        else
                                        {
                                            int intIndexAction = 9;
                                            int intDeposit = -1;
                                            bool validDeposit = int.TryParse(message.Substring(intIndexAction, message.IndexOf("@") - intIndexAction - 1), out intDeposit);
                                            string strRecipient = message.Substring(message.IndexOf("@") + 1).ToLower();
                                            int intWallet = currencyBalance(strRecipient);

                                            // check if deposit amount is valid
                                            if (intDeposit < 0)
                                                _irc.sendPublicChatMessage("Please insert a positive amount or use the !charge command to remove " + _strCurrencyType + " from a user");
                                            else if (!validDeposit)
                                                _irc.sendPublicChatMessage("The deposit wasn't accepted. Please try again with positive whole numbers only");
                                            else
                                            {
                                                // check if user has a bank account
                                                if (intWallet == -1)
                                                {
                                                    string query = "INSERT INTO tblBank (username, wallet, broadcaster) VALUES (@username, @wallet, @broadcaster)";

                                                    using (SqlConnection conn = new SqlConnection(_connStr))
                                                    using (SqlCommand cmd = new SqlCommand(query, conn))
                                                    {
                                                        cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = strRecipient;
                                                        cmd.Parameters.Add("@wallet", SqlDbType.Int).Value = intDeposit;
                                                        cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;

                                                        conn.Open();
                                                        cmd.ExecuteNonQuery();
                                                    }

                                                    _irc.sendPublicChatMessage(strUserName + " has created a new account for @" + strRecipient
                                                        + " with " + intDeposit + " " + _strCurrencyType + " to spend");
                                                }
                                                else // deposit money into wallet
                                                {
                                                    intWallet = intWallet + intDeposit;

                                                    string query = "UPDATE dbo.tblBank SET wallet = @wallet WHERE (username = @username AND broadcaster = @broadcaster)";

                                                    using (SqlConnection conn = new SqlConnection(_connStr))
                                                    using (SqlCommand cmd = new SqlCommand(query, conn))
                                                    {
                                                        cmd.Parameters.Add("@wallet", SqlDbType.Int).Value = intWallet;
                                                        cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = strRecipient;
                                                        cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;

                                                        conn.Open();
                                                        cmd.ExecuteNonQuery();
                                                    }

                                                    // prompt user's balance
                                                    _irc.sendPublicChatMessage("Deposited " + intDeposit.ToString() + " " + _strCurrencyType + " to @" + strRecipient
                                                        + "'s account! They now have " + intWallet + " " + _strCurrencyType + " to spend");
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!deposit");
                                    }
                                }

                                if (message.Equals("!popsr") && !isUserTimedout(strUserName))
                                {
                                    string strRemovedSong = "";

                                    try
                                    {
                                        using (SqlConnection conn = new SqlConnection(_connStr))
                                        {
                                            conn.Open();
                                            using (SqlCommand cmd = new SqlCommand("SELECT TOP(1) songRequests FROM tblSongRequests WHERE broadcaster = @broadcaster ORDER BY id", conn))
                                            {
                                                cmd.Parameters.AddWithValue("@broadcaster", _intBroadcasterID);
                                                using (SqlDataReader reader = cmd.ExecuteReader())
                                                {
                                                    if (reader.HasRows)
                                                    {
                                                        while (reader.Read())
                                                        {
                                                            strRemovedSong = reader["songRequests"].ToString();
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        if (!string.IsNullOrWhiteSpace(strRemovedSong))
                                        {
                                            string query = "WITH T AS (SELECT TOP(1) * FROM tblSongRequests WHERE broadcaster = @broadcaster ORDER BY id) DELETE FROM T";

                                            // Create connection and command
                                            using (SqlConnection conn = new SqlConnection(_connStr))
                                            using (SqlCommand cmd = new SqlCommand(query, conn))
                                            {
                                                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;

                                                conn.Open();
                                                cmd.ExecuteNonQuery();
                                                conn.Close();
                                            }

                                            _irc.sendPublicChatMessage("The first song in queue, '" + strRemovedSong + "' has been removed from the request list");
                                        }
                                        else
                                            _irc.sendPublicChatMessage("There are no songs that can be removed from the song request list");
                                    }
                                    catch (Exception ex)
                                    {
                                        LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!popsr");
                                    }
                                }

                                if (message.Equals("!poppartyuprequest") && !isUserTimedout(strUserName))
                                {
                                    string strRemovedPartyMember = "";

                                    try
                                    {
                                        using (SqlConnection conn = new SqlConnection(_connStr))
                                        {
                                            conn.Open();
                                            using (SqlCommand cmd = new SqlCommand("SELECT TOP(1) partyMember, username FROM tblPartyUpRequests WHERE broadcaster = @broadcaster ORDER BY id", conn))
                                            {
                                                cmd.Parameters.AddWithValue("@broadcaster", _intBroadcasterID);
                                                using (SqlDataReader reader = cmd.ExecuteReader())
                                                {
                                                    if (reader.HasRows)
                                                    {
                                                        while (reader.Read())
                                                        {
                                                            strRemovedPartyMember = reader["partyMember"].ToString();
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        if (!string.IsNullOrWhiteSpace(strRemovedPartyMember))
                                        {
                                            string query = "WITH T AS (SELECT TOP(1) * FROM tblPartyUpRequests WHERE broadcaster = @broadcaster ORDER BY id) DELETE FROM T";

                                            // Create connection and command
                                            using (SqlConnection conn = new SqlConnection(_connStr))
                                            using (SqlCommand cmd = new SqlCommand(query, conn))
                                            {
                                                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;

                                                conn.Open();
                                                cmd.ExecuteNonQuery();
                                                conn.Close();
                                            }

                                            _irc.sendPublicChatMessage("The first party member in queue, '" + strRemovedPartyMember + "' has been removed from the request list");
                                        }
                                        else
                                            _irc.sendPublicChatMessage("There are no songs that can be removed from the song request list");
                                    }
                                    catch (Exception ex)
                                    {
                                        LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!poppartyuprequest");
                                    }
                                }

                                if (message.StartsWith("!addtimeout") && message.Contains("@") && !isUserTimedout(strUserName))
                                {
                                    try
                                    {
                                        if (message.StartsWith("!addtimeout @"))
                                            _irc.sendPublicChatMessage("I cannot make a user not talk to me without this format '!addtimeout [seconds] @[username]'");
                                        else if (message.ToLower().Contains(_strBroadcasterName.ToLower()))
                                            _irc.sendPublicChatMessage("I cannot betray @" + _strBroadcasterName + " by not allowing him to communicate with me @" + strUserName);
                                        else
                                        {
                                            int intIndexAction = 12;
                                            string strRecipient = message.Substring(message.IndexOf("@") + 1).ToLower();
                                            double dblSec = -1;
                                            bool validDeposit = double.TryParse(message.Substring(intIndexAction, message.IndexOf("@") - intIndexAction - 1), out dblSec);

                                            if (!validDeposit || dblSec < 0.00)
                                                _irc.sendPublicChatMessage("The timeout amount wasn't accepted. Please try again with positive seconds only");
                                            else if (dblSec < 15.00)
                                                _irc.sendPublicChatMessage("The duration needs to be at least 15 seconds long. Please try again");
                                            else
                                            {
                                                _timeout.addTimeoutToLst(strRecipient, _intBroadcasterID, dblSec, _connStr);

                                                _irc.sendPublicChatMessage(strRecipient + ", I don't want to talk to you for " + dblSec + " seconds");
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!addtimeout");
                                    }
                                }

                                if (message.StartsWith("!deltimeout @") && !isUserTimedout(strUserName))
                                {
                                    try
                                    {
                                        string strRecipient = message.Substring(message.IndexOf("@") + 1).ToLower();

                                        _timeout.delTimeoutFromLst(strRecipient, _intBroadcasterID, _connStr);

                                        _irc.sendPublicChatMessage(strRecipient + " can now interact with me again because of @" + strUserName);
                                    }
                                    catch (Exception ex)
                                    {
                                        LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!deltimeout");
                                    }
                                }

                                if (message.StartsWith("!setlatency") && !isUserTimedout(strUserName))
                                {
                                    int intLatency = -1;
                                    bool validInput = int.TryParse(message.Substring(12), out intLatency);
                                    if (!validInput || intLatency < 0)
                                        _irc.sendPublicChatMessage("Please insert a valid positive alloted amount of time (in seconds)");
                                    else
                                    {
                                        // set and save latency
                                        _intStreamLatency = intLatency;
                                        Properties.Settings.Default.streamLatency = _intStreamLatency;
                                        Properties.Settings.Default.Save();

                                        Console.WriteLine("Stream latency set to " + _intStreamLatency + " second(s)");
                                        _irc.sendPublicChatMessage("Bot settings for stream latency set to " + _intStreamLatency + " second(s) @" + strUserName);
                                    }
                                }

                                /* insert moderator commands here */
                            }

                            /* 
                             * General commands 
                             */
                            if (message.Equals("!commands") && !isUserTimedout(strUserName))
                            {
                                try
                                {
                                    _irc.sendPublicChatMessage("--- !hello | !slap @[username] | !stab @[username] | !throw [item] @[username] | !shoot @[username]"
                                        + "| !currentsong | !srlist | !sr [artist] - [song title] | !utctime | !hosttime | !partyup [party member name] ---"
                                        + " Link to full list of commands: "
                                        + "https://github.com/SimpleSandman/TwitchBot/wiki/List-of-Commands");
                                }
                                catch (Exception ex)
                                {
                                    LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!commands");
                                }
                            }

                            if (message.Equals("!hello") && !isUserTimedout(strUserName))
                            {
                                try
                                {
                                    _irc.sendPublicChatMessage("Hey " + strUserName + "! Thanks for talking to me.");
                                }
                                catch (Exception ex)
                                {
                                    LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!hello");
                                }
                            }

                            if (message.Equals("!utctime") && !isUserTimedout(strUserName))
                            {
                                try
                                {
                                    _irc.sendPublicChatMessage("UTC Time: " + DateTime.UtcNow.ToString());
                                }
                                catch (Exception ex)
                                {
                                    LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!utctime");
                                }
                            }

                            if (message.Equals("!hosttime") && !isUserTimedout(strUserName))
                            {
                                try
                                {
                                    _irc.sendPublicChatMessage(_strBroadcasterName + "'s Current Time: " + DateTime.Now.ToString() + " (" + _localZone.StandardName + ")");
                                }
                                catch (Exception ex)
                                {
                                    LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!hosttime");
                                }
                            }

                            if (message.Equals("!uptime") && !isUserTimedout(strUserName)) // need to check if channel is currently streaming
                            {
                                try
                                {
                                    var upTimeRes = GetChannel();
                                    TimeSpan ts = DateTime.UtcNow - DateTime.Parse(upTimeRes.Result.updated_at);
                                    string upTime = String.Format("{0:h\\:mm\\:ss}", ts);
                                    _irc.sendPublicChatMessage("This channel's current uptime (length of current stream) is " + upTime);
                                }
                                catch (Exception ex)
                                {
                                    LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!uptime");
                                }
                            }

                            /* List song requests from database */
                            if (message.Equals("!srlist") && !isUserTimedout(strUserName))
                            {
                                try
                                {
                                    string songList = "";

                                    using (SqlConnection conn = new SqlConnection(_connStr))
                                    {
                                        conn.Open();
                                        using (SqlCommand cmd = new SqlCommand("SELECT songRequests FROM tblSongRequests WHERE broadcaster = @broadcaster", conn))
                                        {
                                            cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;
                                            using (SqlDataReader reader = cmd.ExecuteReader())
                                            {
                                                if (reader.HasRows)
                                                {
                                                    DataTable schemaTable = reader.GetSchemaTable();
                                                    DataTable data = new DataTable();
                                                    foreach (DataRow row in schemaTable.Rows)
                                                    {
                                                        string colName = row.Field<string>("ColumnName");
                                                        Type t = row.Field<Type>("DataType");
                                                        data.Columns.Add(colName, t);
                                                    }
                                                    while (reader.Read())
                                                    {
                                                        var newRow = data.Rows.Add();
                                                        foreach (DataColumn col in data.Columns)
                                                        {
                                                            newRow[col.ColumnName] = reader[col.ColumnName];
                                                            Console.WriteLine(newRow[col.ColumnName].ToString());
                                                            songList = songList + newRow[col.ColumnName].ToString() + " || ";
                                                        }
                                                    }
                                                    StringBuilder strBdrSongList = new StringBuilder(songList);
                                                    strBdrSongList.Remove(songList.Length - 4, 4); // remove extra " || "
                                                    songList = strBdrSongList.ToString(); // replace old song list string with new
                                                    _irc.sendPublicChatMessage("Current List of Requested Songs: " + songList);
                                                }
                                                else
                                                {
                                                    Console.WriteLine("No requests have been made");
                                                    _irc.sendPublicChatMessage("No requests have been made");
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!srlist");
                                }
                            }

                            /* Insert requested song into database */
                            if (message.StartsWith("!sr ") && !isUserTimedout(strUserName))
                            {
                                try
                                {
                                    if (isSongRequestAvail)
                                    {
                                        // Grab the song name from the request
                                        int index = message.IndexOf("!sr");
                                        string songRequest = message.Substring(index, message.Length - index);
                                        songRequest = songRequest.Replace("!sr ", "");
                                        Console.WriteLine("New song request: " + songRequest);

                                        // Check if song request has more than letters, numbers, and hyphens
                                        if (!Regex.IsMatch(songRequest, @"^[a-zA-Z0-9 \-]+$"))
                                        {
                                            _irc.sendPublicChatMessage("Only letters, numbers, and hyphens (-) are allowed. Please try again. "
                                                + "If the problem persists, please contact my creator");
                                        }
                                        else
                                        {
                                            /* Add song request to database */
                                            string query = "INSERT INTO tblSongRequests (songRequests, broadcaster, chatter) VALUES (@song, @broadcaster, @chatter)";

                                            // Create connection and command
                                            using (SqlConnection conn = new SqlConnection(_connStr))
                                            using (SqlCommand cmd = new SqlCommand(query, conn))
                                            {
                                                cmd.Parameters.Add("@song", SqlDbType.VarChar, 200).Value = songRequest;
                                                cmd.Parameters.Add("@broadcaster", SqlDbType.VarChar, 200).Value = _intBroadcasterID;
                                                cmd.Parameters.Add("@chatter", SqlDbType.VarChar, 200).Value = strUserName;

                                                conn.Open();
                                                cmd.ExecuteNonQuery();
                                                conn.Close();
                                            }

                                            _irc.sendPublicChatMessage("The song \"" + songRequest + "\" has been successfully requested!");
                                        }
                                    }
                                    else
                                        _irc.sendPublicChatMessage("Song requests are disabled at the moment");
                                }
                                catch (Exception ex)
                                {
                                    LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!sr");
                                }
                            }

                            if (message.Equals("!currentsong") && !isUserTimedout(strUserName))
                            {
                                try
                                {
                                    StatusResponse status = _spotify.GetStatus();
                                    if (status != null)
                                    {
                                        _irc.sendPublicChatMessage("Current Song: " + status.Track.TrackResource.Name
                                            + " || Artist: " + status.Track.ArtistResource.Name
                                            + " || Album: " + status.Track.AlbumResource.Name);
                                    }
                                    else
                                        _irc.sendPublicChatMessage("The broadcaster is not playing a song at the moment");
                                }
                                catch (Exception ex)
                                {
                                    LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!currentsong");
                                }
                            }

                            /* Action commands */
                            if (message.StartsWith("!slap @") && !isUserTimedout(strUserName))
                            {
                                try
                                {
                                    string strRecipient = message.Substring(message.IndexOf("@") + 1).ToLower();
                                    reactionCmd(message, strUserName, strRecipient, "Stop smacking yourself", "slaps");
                                }
                                catch (Exception ex)
                                {
                                    LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!slap");
                                }
                            }

                            if (message.StartsWith("!stab @") && !isUserTimedout(strUserName))
                            {
                                try
                                {
                                    string strRecipient = message.Substring(message.IndexOf("@") + 1).ToLower();
                                    reactionCmd(message, strUserName, strRecipient, "Stop stabbing yourself", "stabs", " to death!");
                                }
                                catch (Exception ex)
                                {
                                    LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!stab");
                                }
                            }

                            if (message.StartsWith("!shoot @") && !isUserTimedout(strUserName))
                            {
                                try
                                {
                                    string strBodyPart = "'s ";
                                    string strRecipient = message.Substring(message.IndexOf("@") + 1).ToLower();
                                    Random rnd = new Random(DateTime.Now.Millisecond);
                                    int intBodyPart = rnd.Next(8); // between 0 and 7

                                    if (intBodyPart == 0)
                                        strBodyPart += "head";
                                    else if (intBodyPart == 1)
                                        strBodyPart += "left leg";
                                    else if (intBodyPart == 2)
                                        strBodyPart += "right leg";
                                    else if (intBodyPart == 3)
                                        strBodyPart += "left arm";
                                    else if (intBodyPart == 4)
                                        strBodyPart += "right arm";
                                    else if (intBodyPart == 5)
                                        strBodyPart += "stomach";
                                    else if (intBodyPart == 6)
                                        strBodyPart += "neck";
                                    else // found largest random value
                                        strBodyPart = " but missed";

                                    reactionCmd(message, strUserName, strRecipient, "You just shot your " + strBodyPart.Replace("'s ", ""), "shoots", strBodyPart);

                                    // bot responds if targeted
                                    if (strRecipient.Equals(_strBotName.ToLower()))
                                    {
                                        if (strBodyPart.Equals(" but missed"))
                                            _irc.sendPublicChatMessage("Ha! You missed @" + strUserName);
                                        else
                                            _irc.sendPublicChatMessage("You think shooting me in the " + strBodyPart.Replace("'s ", "") + " would hurt me? I am a bot!");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!shoot");
                                }
                            }

                            if (message.StartsWith("!throw ") && message.Contains("@") && !isUserTimedout(strUserName))
                            {
                                try
                                {
                                    int intIndexAction = 7;

                                    if (message.StartsWith("!throw @"))
                                        _irc.sendPublicChatMessage("Please throw an item to a user @" + strUserName);
                                    else
                                    {
                                        string strRecipient = message.Substring(message.IndexOf("@") + 1).ToLower();
                                        string item = message.Substring(intIndexAction, message.IndexOf("@") - intIndexAction - 1);

                                        Random rnd = new Random(DateTime.Now.Millisecond);
                                        int intEffectiveLvl = rnd.Next(3); // between 0 and 2
                                        string strEffectiveness = "";

                                        if (intEffectiveLvl == 0)
                                            strEffectiveness = "It's super effective!";
                                        else if (intEffectiveLvl == 1)
                                            strEffectiveness = "It wasn't very effective";
                                        else
                                            strEffectiveness = "It had no effect";

                                        reactionCmd(message, strUserName, strRecipient, "Stop throwing " + item + " at yourself", "throws " + item + " at", ". " + strEffectiveness);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!throw");
                                }
                            }

                            if (message.StartsWith("!requestpartymember") && !isUserTimedout(strUserName))
                            {
                                try
                                {
                                    string strPartyMember = "";
                                    int intInputIndex = 20;
                                    int intGameID = 0;
                                    bool isPartyMemebrFound = false;
                                    bool isDuplicateRequestor = false;

                                    // Get current game
                                    _strBroadcasterGame = GetChannel().Result.game;

                                    // check if user entered something
                                    if (message.Length < intInputIndex)
                                        _irc.sendPublicChatMessage("Please enter a party member @" + strUserName);
                                    else
                                        strPartyMember = message.Substring(intInputIndex);

                                    // grab game id in order to find party member
                                    using (SqlConnection conn = new SqlConnection(_connStr))
                                    {
                                        conn.Open();
                                        using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblGameList", conn))
                                        using (SqlDataReader reader = cmd.ExecuteReader())
                                        {
                                            if (reader.HasRows)
                                            {
                                                while (reader.Read())
                                                {
                                                    if (_strBroadcasterGame.Equals(reader["name"].ToString()))
                                                    {
                                                        intGameID = int.Parse(reader["id"].ToString());
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    // if the game is not found
                                    // tell users this game is not accepting party up requests
                                    if (intGameID == 0)
                                        _irc.sendPublicChatMessage("This game is currently not a part of the 'Party Up' system");
                                    else // check if user has already requested a party member
                                    {
                                        using (SqlConnection conn = new SqlConnection(_connStr))
                                        {
                                            conn.Open();
                                            using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblPartyUpRequests " 
                                                + "WHERE broadcaster = @broadcaster AND game = @game AND username = @username", conn))
                                            {
                                                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;
                                                cmd.Parameters.Add("@game", SqlDbType.Int).Value = intGameID;
                                                cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = strUserName;
                                                using (SqlDataReader reader = cmd.ExecuteReader())
                                                {
                                                    if (reader.HasRows)
                                                    {
                                                        while (reader.Read())
                                                        {
                                                            if (strUserName.ToLower().Equals(reader["username"].ToString().ToLower()))
                                                            {
                                                                isDuplicateRequestor = true;
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        if (isDuplicateRequestor)
                                            _irc.sendPublicChatMessage("You have already requested a party member. Please wait until your request has been completed @" + strUserName);
                                        else // search for party member user is requesting
                                        {
                                            using (SqlConnection conn = new SqlConnection(_connStr))
                                            {
                                                conn.Open();
                                                using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblPartyUp WHERE broadcaster = @broadcaster AND game = @game", conn))
                                                {
                                                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;
                                                    cmd.Parameters.Add("@game", SqlDbType.Int).Value = intGameID;
                                                    using (SqlDataReader reader = cmd.ExecuteReader())
                                                    {
                                                        if (reader.HasRows)
                                                        {
                                                            while (reader.Read())
                                                            {
                                                                if (strPartyMember.ToLower().Equals(reader["partyMember"].ToString().ToLower()))
                                                                {
                                                                    strPartyMember = reader["partyMember"].ToString();
                                                                    isPartyMemebrFound = true;
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            // insert party member if they exists from database
                                            if (!isPartyMemebrFound)
                                                _irc.sendPublicChatMessage("I couldn't find the requested party member '" + strPartyMember + "' @" + strUserName
                                                    + ". Please check with the broadcaster for possible spelling errors");
                                            else
                                            {
                                                string query = "INSERT INTO tblPartyUpRequests (username, partyMember, timeRequested, broadcaster, game) "
                                                    + "VALUES (@username, @partyMember, @timeRequested, @broadcaster, @game)";

                                                // Create connection and command
                                                using (SqlConnection conn = new SqlConnection(_connStr))
                                                using (SqlCommand cmd = new SqlCommand(query, conn))
                                                {
                                                    cmd.Parameters.Add("@username", SqlDbType.VarChar, 50).Value = strUserName;
                                                    cmd.Parameters.Add("@partyMember", SqlDbType.VarChar, 50).Value = strPartyMember;
                                                    cmd.Parameters.Add("@timeRequested", SqlDbType.DateTime).Value = DateTime.Now;
                                                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;
                                                    cmd.Parameters.Add("@game", SqlDbType.Int).Value = intGameID;

                                                    conn.Open();
                                                    cmd.ExecuteNonQuery();
                                                    conn.Close();
                                                }

                                                _irc.sendPublicChatMessage("@" + strUserName + ": " + strPartyMember + " has been added to the party queue");
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!requestpartymember");
                                }
                            }

                            if (message.Equals("!partyuprequestlist") && !isUserTimedout(strUserName))
                            {
                                try
                                {
                                    string strPartyList = "Here are the requested party members: ";
                                    int intGameID = 0;

                                    // Get current game
                                    _strBroadcasterGame = GetChannel().Result.game;

                                    // grab game id in order to find party member
                                    using (SqlConnection conn = new SqlConnection(_connStr))
                                    {
                                        conn.Open();
                                        using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblGameList", conn))
                                        using (SqlDataReader reader = cmd.ExecuteReader())
                                        {
                                            if (reader.HasRows)
                                            {
                                                while (reader.Read())
                                                {
                                                    if (_strBroadcasterGame.Equals(reader["name"].ToString()))
                                                    {
                                                        intGameID = int.Parse(reader["id"].ToString());
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    // if the game is not found
                                    // tell users this game is not part of the party up system
                                    if (intGameID == 0)
                                        _irc.sendPublicChatMessage("This game is currently not a part of the 'Party Up' system");
                                    else
                                    {
                                        using (SqlConnection conn = new SqlConnection(_connStr))
                                        {
                                            conn.Open();
                                            using (SqlCommand cmd = new SqlCommand("SELECT username, partyMember FROM tblPartyUpRequests " 
                                                + "WHERE game = @game AND broadcaster = @broadcaster ORDER BY Id", conn))
                                            {
                                                cmd.Parameters.Add("@game", SqlDbType.Int).Value = intGameID;
                                                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;
                                                using (SqlDataReader reader = cmd.ExecuteReader())
                                                {
                                                    if (reader.HasRows)
                                                    {
                                                        while (reader.Read())
                                                        {
                                                            strPartyList += reader["partyMember"].ToString() + " <-- " + reader["username"].ToString() + " || ";
                                                        }
                                                        StringBuilder strBdrPartyList = new StringBuilder(strPartyList);
                                                        strBdrPartyList.Remove(strPartyList.Length - 4, 4); // remove extra " || "
                                                        strPartyList = strBdrPartyList.ToString(); // replace old party member list string with new
                                                        _irc.sendPublicChatMessage(strPartyList);
                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine("No party members are set for this game");
                                                        _irc.sendPublicChatMessage("No party members are set for this game");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!partyuprequestlist");
                                }
                            }

                            if (message.Equals("!partyuplist") && !isUserTimedout(strUserName))
                            {
                                try
                                {
                                    string strPartyList = "The available party members are: ";
                                    int intGameID = 0;

                                    // Get current game
                                    _strBroadcasterGame = GetChannel().Result.game;

                                    // grab game id in order to find party member
                                    using (SqlConnection conn = new SqlConnection(_connStr))
                                    {
                                        conn.Open();
                                        using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblGameList", conn))
                                        using (SqlDataReader reader = cmd.ExecuteReader())
                                        {
                                            if (reader.HasRows)
                                            {
                                                while (reader.Read())
                                                {
                                                    if (_strBroadcasterGame.Equals(reader["name"].ToString()))
                                                    {
                                                        intGameID = int.Parse(reader["id"].ToString());
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    // if the game is not found
                                    // tell users this game is not part of the party up system
                                    if (intGameID == 0)
                                        _irc.sendPublicChatMessage("This game is currently not a part of the 'Party Up' system");
                                    else
                                    {
                                        using (SqlConnection conn = new SqlConnection(_connStr))
                                        {
                                            conn.Open();
                                            using (SqlCommand cmd = new SqlCommand("SELECT partyMember FROM tblPartyUp WHERE game = @game AND broadcaster = @broadcaster ORDER BY partyMember", conn))
                                            {
                                                cmd.Parameters.Add("@game", SqlDbType.Int).Value = intGameID;
                                                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;
                                                using (SqlDataReader reader = cmd.ExecuteReader())
                                                {
                                                    if (reader.HasRows)
                                                    {
                                                        while (reader.Read())
                                                        {
                                                            strPartyList += reader["partyMember"].ToString() + " || ";
                                                        }
                                                        StringBuilder strBdrPartyList = new StringBuilder(strPartyList);
                                                        strBdrPartyList.Remove(strPartyList.Length - 4, 4); // remove extra " || "
                                                        strPartyList = strBdrPartyList.ToString(); // replace old party member list string with new
                                                        _irc.sendPublicChatMessage(strPartyList);
                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine("No party members are set for this game");
                                                        _irc.sendPublicChatMessage("No party members are set for this game");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!partyuplist");
                                }
                            }

                            if (message.Equals("!myfunds") && !isUserTimedout(strUserName))
                            {
                                try
                                {
                                    int intBalance = currencyBalance(strUserName);

                                    if (intBalance == -1)
                                        _irc.sendPublicChatMessage("You are not currently banking with us at the moment. Please talk to a moderator about acquiring " + _strCurrencyType);
                                    else
                                        _irc.sendPublicChatMessage("@" + strUserName + " currently has " + intBalance.ToString() + " " + _strCurrencyType);
                                }
                                catch (Exception ex)
                                {
                                    LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!myfunds");
                                }
                            }

                            /* add more general commands here */
                        }
                    }
                } // end master while loop
            }
            catch (Exception ex)
            {
                LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, string)", true);
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

        static async Task<ChannelJSON> GetChannel()
        {
            using (var client = new HttpClient())
            {
                var body = await client.GetStringAsync("https://api.twitch.tv/kraken/channels/" + _strBroadcasterName);
                var response = JsonConvert.DeserializeObject<ChannelJSON>(body);
                return response;
            }
        }

        static async Task<FollowerInfo> GetBroadcasterInfo()
        {
            using (var client = new HttpClient())
            {
                var body = await client.GetStringAsync("https://api.twitch.tv/kraken/channels/" + _strBroadcasterName + "/follows?limit=" + _intFollowers);
                var response = JsonConvert.DeserializeObject<FollowerInfo>(body);
                return response;
            }
        }

        static async Task<ChatterInfo> GetChatters()
        {
            using (var client = new HttpClient())
            {
                var body = await client.GetStringAsync("https://tmi.twitch.tv/group/user/" + _strBroadcasterName + "/chatters");
                var response = JsonConvert.DeserializeObject<ChatterInfo>(body);
                return response;
            }
        }

        /// <summary>
        /// Find the Nth index of a character
        /// </summary>
        /// <param name="s"></param>
        /// <param name="findChar"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        private static int GetNthIndex(string s, char findChar, int n)
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

        private static void SendTweet(string pendingMessage, string command)
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

        public static void LogError(Exception ex, string strClass, string strMethod, bool hasToExit, string strCmd = "N/A")
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
            string query = "INSERT INTO tblErrorLog (errorTime, errorLine, errorClass, errorMethod, errorMsg, broadcaster, command) "
                + "VALUES (@time, @lineNum, @class, @method, @msg, @broadcaster, @command)";

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
                Thread.Sleep(3000);
                Environment.Exit(0);
            }
        }

        private static string chatterValid(string strOrigUser, string strRecipient, string strSearchCriteria = "")
        {
            Chatters chatters = GetChatters().Result.chatters;

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

        private static bool reactionCmd(string message, string strOrigUser, string strRecipient, string strMsgToSelf, string strAction, string strAddlMsg = "")
        {
            string strRoleType = chatterValid(strOrigUser, strRecipient);

            // check if user currently watching the channel
            if (!string.IsNullOrEmpty(strRoleType))
            {
                if (strOrigUser.Equals(strRecipient))
                    _irc.sendPublicChatMessage(strMsgToSelf + " @" + strOrigUser);
                else
                    _irc.sendPublicChatMessage(strOrigUser + " " + strAction + " @" + strRecipient + strAddlMsg);

                return true;
            }
            else
                return false;
        }
        
        private static int currencyBalance(string username)
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

        private static bool isUserTimedout(string strUserName)
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

    }
}
