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
        public static int _intBroadcasterID = 0;
        public static string _strBroadcasterGame = "";
        public static string _strBotName = "";
        public static string _strCurrencyType = "coins";
        public static string _connStr = ""; // connection string
        public static TimeZone _localZone = TimeZone.CurrentTimeZone;
        public static int _intFollowers = 0;
        public static string _strDiscordLink = "Link unavailable at the moment"; // provide discord server link if available
        public static bool _isAutoPublishTweet = false; // set to auto publish tweets (disabled by default)
        public static bool _isAutoDisplaySong = false; // set to auto song status (disabled by default)

        static void Main(string[] args)
        {
            string userID = "";       // username
            string pass = "";         // password
            ConsoleKeyInfo key;       // keystroke            

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

            bool isSongRequest = false;  // check song request status (disabled by default)
            bool isConnected = false;    // check azure connection
            bool hasExtraInfo = false;   // check broadcaster has existing extra settings

            /* Insert user ID for database */
            Console.Write("Azure database username required: ");
            userID = Console.ReadLine();

            try
            {
                /* 
                 * Connect to database and retry password until success 
                 */
                do
                {
                    Console.Write("Azure database password required: ");
                    do
                    {
                        key = Console.ReadKey(true); // enter masked password here from user input

                        // backspace should not work
                        if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                        {
                            pass += key.KeyChar;
                            Console.Write("*");
                        }
                        else
                        {
                            if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                            {
                                pass = pass.Substring(0, (pass.Length - 1));
                                Console.Write("\b \b");
                            }
                        }
                    }
                    // Stops receiving keys once enter is pressed
                    while (key.Key != ConsoleKey.Enter);

                    Console.WriteLine();

                    // Append username and password to connection string
                    _connStr = ConfigurationManager.ConnectionStrings["TwitchBot.Properties.Settings.conn"].ConnectionString;
                    _connStr = _connStr + ";User ID=" + userID + ";Password=" + pass;

                    // Check if server is connected
                    if (!IsServerConnected(_connStr))
                    {
                        // clear sensitive data
                        pass = null;
                        _connStr = null;

                        Console.WriteLine("Azure connection failed. Username or password are incorrect. Please try again.");
                        Console.WriteLine();
                        Console.WriteLine("-- Common technical issues: --");
                        Console.WriteLine("1: Check if Azure firewall settings has your client IP address.");
                        Console.WriteLine("2: Double check the connection string under 'Properties' and 'Settings'");
                        Console.WriteLine("<<<< To exit this program at any time, press 'CTRL' + 'C' inside this terminal >>>>");
                        Console.WriteLine();
                    }
                    else
                        isConnected = true;
                }
                while (!isConnected);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("**** Local troubleshooting needed ****");
                Thread.Sleep(3000);
                Environment.Exit(0);
            }

            /* Try to grab the info needed for the bot to connect to the channel */
            try {
                // Clear sensitive data
                pass = null;
                userID = null;

                Console.WriteLine("Azure server connection successful!");

                using (StreamReader sr = File.OpenText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Bot-Settings.txt"))
                {
                    string s = String.Empty;
                    while ((s = sr.ReadLine()) != null)
                    {
                        if (s.StartsWith("botName")) _strBotName = s.Substring(s.IndexOf("=") + 2);
                        else if (s.StartsWith("broadcaster")) _strBroadcasterName = s.Substring(s.IndexOf("=") + 2);
                        else if (s.StartsWith("twitchOAuth")) twitchOAuth = s.Substring(s.IndexOf("oauth:"));
                        else if (s.StartsWith("twitchClientID")) twitchClientID = s.Substring(s.IndexOf("=") + 2);
                        else if (s.StartsWith("twitchAccessToken")) twitchAccessToken = s.Substring(s.IndexOf("=") + 2);
                        else if (s.StartsWith("twitterConsumerKey")) twitterConsumerKey = s.Substring(s.IndexOf("=") + 2);
                        else if (s.StartsWith("twitterConsumerSecret")) twitterConsumerSecret = s.Substring(s.IndexOf("=") + 2);
                        else if (s.StartsWith("twitterAccessToken")) twitterAccessToken = s.Substring(s.IndexOf("=") + 2);
                        else if (s.StartsWith("twitterAccessSecret")) twitterAccessSecret = s.Substring(s.IndexOf("=") + 2);
                        else if (s.StartsWith("discordLink")) _strDiscordLink = s.Substring(s.IndexOf("=") + 2);
                    }
                }

                // Check if required options are filled in
                if (string.IsNullOrWhiteSpace(_strBotName)
                    && string.IsNullOrWhiteSpace(_strBroadcasterName)
                    && string.IsNullOrWhiteSpace(twitchOAuth)
                    && string.IsNullOrWhiteSpace(twitchClientID)
                    && string.IsNullOrWhiteSpace(twitchAccessToken))
                {
                    Console.WriteLine("Check your Bot-Settings.txt on your desktop for missing info pertaining to: ''");
                    Thread.Sleep(3000);
                    Environment.Exit(0);
                }

                // Get broadcaster ID
                using (SqlConnection conn = new SqlConnection(_connStr))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblBroadcasters WHERE username = @username", conn))
                    {
                        cmd.Parameters.AddWithValue("@username", _strBroadcasterName);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    if (_strBroadcasterName.Equals(reader["username"].ToString().ToLower()))
                                    {
                                        _intBroadcasterID = int.Parse(reader["id"].ToString());
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

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
                }

                /* Connect to local spotify client */
                _spotify = new SpotifyLocalAPI();
                SpotifyControl spotifyCtrl = new SpotifyControl();
                _spotify.OnPlayStateChange += spotifyCtrl.spotify_OnPlayStateChange;
                _spotify.OnTrackChange += spotifyCtrl.spotify_OnTrackChange;

                // Password from www.twitchapps.com/tmi/
                // include the "oauth:" portion
                // Use chat bot's oauth
                /* main server: irc.twitch.tv, 6667 */
                _irc = new IrcClient("irc.twitch.tv", 6667, _strBotName, twitchOAuth, _strBroadcasterName);

                // Update channel info
                _intFollowers = GetChannel().Result.followers;
                _strBroadcasterGame = GetChannel().Result.game;

                /* Make new thread to get messages */
                Thread thdIrcClient = new Thread(() => GetChatBox(spotifyCtrl, isSongRequest, twitchAccessToken, hasTwitterInfo));
                thdIrcClient.Start();

                spotifyCtrl.Connect(); // attempt to connect to local Spotify client

                // Grab non-essential settings
                using (SqlConnection conn = new SqlConnection(_connStr))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblChannelSettings WHERE broadcaster = @broadcaster", conn))
                    {
                        cmd.Parameters.AddWithValue("@broadcaster", _intBroadcasterID);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    _isAutoPublishTweet = Convert.ToBoolean(reader["enableTweet"]);
                                    _isAutoDisplaySong = Convert.ToBoolean(reader["enableDisplaySong"]);
                                    _strCurrencyType = reader["currencyType"].ToString();
                                    hasExtraInfo = true;
                                    break;
                                }
                            }
                        }
                    }
                }

                // Insert default values for extra settings for the broadcaster
                if (!hasExtraInfo)
                {
                    string query = "INSERT INTO tblChannelSettings (enableTweet, enableDisplaySong, currencyType, broadcaster) " + 
                        "VALUES (@enableTweet, @enableDisplaySong, @currencyType, @broadcaster)";

                    using (SqlConnection conn = new SqlConnection(_connStr))
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.Add("@enableTweet", SqlDbType.Bit).Value = _isAutoPublishTweet;
                        cmd.Parameters.Add("@enableDisplaySong", SqlDbType.Bit).Value = _isAutoDisplaySong;
                        cmd.Parameters.Add("@currencyType", SqlDbType.VarChar, 50).Value = _strCurrencyType;
                        cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;                        
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                }

                /* Whisper broadcaster bot settings */
                Console.WriteLine("Bot settings: Automatic tweets is set to [" + _isAutoPublishTweet + "]");
                Console.WriteLine("Automatic display songs is set to [" + _isAutoDisplaySong + "]");
                Console.WriteLine("Currency type is set to [" + _strCurrencyType + "]");
                Console.WriteLine();

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
        /// <param name="isSongRequest"></param>
        /// <param name="twitchAccessToken"></param>
        private static void GetChatBox(SpotifyControl spotifyCtrl, bool isSongRequest, string twitchAccessToken, bool hasTwitterInfo)
        {
            try
            {
                /* Master loop */
                while (true)
                {
                    // Read any message inside the chat room
                    string message = _irc.readMessage();
                    bool hideExtraTimoutReminder = false; // handle duplicate timeout reminder message
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
                                            + "|| Auto display songs set to \"" + _isAutoDisplaySong + "\"" 
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

                                            /* Update auto tweet to database */
                                            string query = "UPDATE tblChannelSettings SET enableTweet = @enableTweet WHERE broadcaster = @broadcaster";

                                            // Create connection and command
                                            using (SqlConnection conn = new SqlConnection(_connStr))
                                            using (SqlCommand cmd = new SqlCommand(query, conn))
                                            {
                                                cmd.Parameters.Add("@enableTweet", SqlDbType.Bit).Value = _isAutoPublishTweet;
                                                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;

                                                conn.Open();
                                                cmd.ExecuteNonQuery();
                                                conn.Close();
                                            }

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

                                            /* Update auto tweet to database */
                                            string query = "UPDATE tblChannelSettings SET enableTweet = @enableTweet WHERE broadcaster = @broadcaster";

                                            // Create connection and command
                                            using (SqlConnection conn = new SqlConnection(_connStr))
                                            using (SqlCommand cmd = new SqlCommand(query, conn))
                                            {
                                                cmd.Parameters.Add("@enableTweet", SqlDbType.Bit).Value = _isAutoPublishTweet;
                                                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;

                                                conn.Open();
                                                cmd.ExecuteNonQuery();
                                                conn.Close();
                                            }

                                            Console.WriteLine("Auto publish tweets is set to [" + _isAutoPublishTweet + "]");
                                            _irc.sendPublicChatMessage(_strBroadcasterName + ": Automatic tweets is set to \"" + _isAutoPublishTweet + "\"");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!disabletweet");
                                    }
                                }

                                if (message.Equals("!songrequests on"))
                                {
                                    try
                                    {
                                        isSongRequest = true;
                                        _irc.sendPublicChatMessage("Song requests enabled");
                                    }
                                    catch (Exception ex)
                                    {
                                        LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!songrequests on");
                                    }
                                }

                                if (message.Equals("!songrequests off"))
                                {
                                    try
                                    {
                                        isSongRequest = false;
                                        _irc.sendPublicChatMessage("Song requests disabled");
                                    }
                                    catch (Exception ex)
                                    {
                                        LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!songrequests off");
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

                                        /* Update auto tweet to database */
                                        string query = "UPDATE tblChannelSettings SET enableDisplaySong = @enableDisplaySong WHERE broadcaster = @broadcaster";

                                        // Create connection and command
                                        using (SqlConnection conn = new SqlConnection(_connStr))
                                        using (SqlCommand cmd = new SqlCommand(query, conn))
                                        {
                                            cmd.Parameters.Add("@enableDisplaySong", SqlDbType.Bit).Value = _isAutoDisplaySong;
                                            cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;

                                            conn.Open();
                                            cmd.ExecuteNonQuery();
                                            conn.Close();
                                        }

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

                                        /* Update auto song display to database */
                                        string query = "UPDATE tblChannelSettings SET enableDisplaySong = @enableDisplaySong WHERE broadcaster = @broadcaster";

                                        // Create connection and command
                                        using (SqlConnection conn = new SqlConnection(_connStr))
                                        using (SqlCommand cmd = new SqlCommand(query, conn))
                                        {
                                            cmd.Parameters.Add("@enableDisplaySong", SqlDbType.Bit).Value = _isAutoDisplaySong;
                                            cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;

                                            conn.Open();
                                            cmd.ExecuteNonQuery();
                                            conn.Close();
                                        }

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

                                /* insert more broadcaster commands here */
                            }

                            /*
                             * Moderator commands
                             */
                            if (strUserName.Equals(_strBroadcasterName) || _mod.getLstMod().Contains(strUserName.ToLower()))
                            {
                                // check if moderator is still timed out from using this bot
                                if (_timeout.getLstTimeout().ContainsKey(strUserName))
                                {
                                    string timeout = _timeout.getTimoutFromUser(strUserName, _intBroadcasterID, _connStr);

                                    if (timeout.Equals("0 seconds"))
                                        _irc.sendPublicChatMessage("You are now allowed to talk to me again @" + strUserName 
                                            + ". Please try the requested command once more (if necessary)");
                                    else
                                        _irc.sendPublicChatMessage("I am not allowed to talk to you for " + timeout);

                                    hideExtraTimoutReminder = true;
                                }
                                else
                                {
                                    if (message.Equals("!discord"))
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

                                    if (message.StartsWith("!charge") && message.Contains("@"))
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
                                                else /* Insert fee into wallet */
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
                                                        conn.Close();
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

                                    if (message.StartsWith("!deposit") && message.Contains("@"))
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
                                                            conn.Close();
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
                                                            conn.Close();
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

                                    if (message.StartsWith("!popsonglist"))
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
                                            LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!charge");
                                        }
                                    }

                                    if (message.StartsWith("!addtimeout") && message.Contains("@"))
                                    {
                                        try
                                        {
                                            if (message.StartsWith("!addtimeout @"))
                                                _irc.sendPublicChatMessage("I cannot make a user not talk to me without this format '!addtimeout [seconds] @[username]'");
                                            //else if (message.ToLower().Contains(_strBroadcasterName.ToLower()))
                                            //_irc.sendPublicChatMessage("I cannot betray @" + _strBroadcasterName + " by not allowing him to communicate with me @" + strUserName);
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

                                                    hideExtraTimoutReminder = true;
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!addtimeout");
                                        }
                                    }

                                    if (message.StartsWith("!deltimeout @"))
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

                                    /* insert moderator commands here */
                                } // end else not timeout check
                            }

                            /* 
                             * General commands 
                             */
                            if (_timeout.getLstTimeout().ContainsKey(strUserName))
                            {
                                string timeout = _timeout.getTimoutFromUser(strUserName, _intBroadcasterID, _connStr);

                                if (timeout.Equals("0 seconds"))
                                    _irc.sendPublicChatMessage("You are now allowed to talk to me again @" + strUserName);
                                else if (!hideExtraTimoutReminder)
                                    _irc.sendPublicChatMessage("I am not allowed to talk to you for " + timeout);
                            }
                            else
                            {
                                if (message.Equals("!commands"))
                                {
                                    try
                                    {
                                        _irc.sendPublicChatMessage("--- !hello | !slap @[username] | !stab @[username] | !throw [item] @[username] | !shoot @[username]"
                                            + "| !currentsong | !songrequestlist | !requestsong [artist] - [song title] | !utctime | !hosttime | !partyup [party member name] ---"
                                            + " Link to full list of commands: "
                                            + "https://github.com/SimpleSandman/TwitchBot/wiki/List-of-Commands");
                                    }
                                    catch (Exception ex)
                                    {
                                        LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!commands");
                                    }
                                }

                                if (message.Equals("!hello"))
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

                                if (message.Equals("!utctime"))
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

                                if (message.Equals("!hosttime"))
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

                                if (message.Equals("!uptime")) // need to check if channel is currently streaming
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
                                if (message.Equals("!songrequestlist"))
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
                                        LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!songrequestlist");
                                    }
                                }

                                /* Insert requested song into database */
                                if (message.StartsWith("!requestsong"))
                                {
                                    try
                                    {
                                        if (isSongRequest)
                                        {
                                            // Grab the song name from the request
                                            int index = message.IndexOf("!requestsong");
                                            string songRequest = message.Substring(index, message.Length - index);
                                            songRequest = songRequest.Replace("!requestsong ", "");
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
                                        LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!requestsong");
                                    }
                                }

                                if (message.Equals("!currentsong"))
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
                                if (message.StartsWith("!slap @"))
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

                                if (message.StartsWith("!stab @"))
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

                                if (message.StartsWith("!shoot @"))
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

                                if (message.StartsWith("!throw ") && message.Contains("@"))
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

                                if (message.StartsWith("!partyuprequest"))
                                {
                                    try
                                    {
                                        string strPartyMember = "";
                                        int intInputIndex = 16;
                                        int intGameID = 0;
                                        bool isPartyMemebrFound = false;

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
                                            _irc.sendPublicChatMessage("This game is currently not a part of the 'Party Up' command");
                                        else // check if requested party member is valid
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

                                            // insert party member if member exists from database
                                            if (!isPartyMemebrFound)
                                                _irc.sendPublicChatMessage("I couldn't find the requested party memebr '" + strPartyMember + "' @" + strUserName
                                                    + ". Please check with the broadcaster for possible spelling errors");
                                            else
                                            {
                                                string query = "INSERT INTO tblPartyUpRequests (username, partyMember, timeRequested, broadcaster) " 
                                                    + "VALUES (@username, @partyMember, @timeRequested, @broadcaster)";

                                                // Create connection and command
                                                using (SqlConnection conn = new SqlConnection(_connStr))
                                                using (SqlCommand cmd = new SqlCommand(query, conn))
                                                {
                                                    cmd.Parameters.Add("@username", SqlDbType.VarChar, 50).Value = strUserName;
                                                    cmd.Parameters.Add("@partyMember", SqlDbType.VarChar, 50).Value = strPartyMember;
                                                    cmd.Parameters.Add("@timeRequested", SqlDbType.DateTime).Value = DateTime.Now;
                                                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _intBroadcasterID;

                                                    conn.Open();
                                                    cmd.ExecuteNonQuery();
                                                    conn.Close();
                                                }

                                                _irc.sendPublicChatMessage("@" + strUserName + ": " + strPartyMember + " has been added to the party queue");
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!partyup");
                                    }
                                }

                                if (message.Equals("!partyuplist"))
                                {
                                    try
                                    {
                                        string strPartyList = "The available party members are ";
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
                                            _irc.sendPublicChatMessage("This game is currently not a part of the 'Party Up' command");
                                        else
                                        {
                                            using (SqlConnection conn = new SqlConnection(_connStr))
                                            {
                                                conn.Open();
                                                using (SqlCommand cmd = new SqlCommand("SELECT partyMember FROM tblPartyUp WHERE game = @game AND broadcaster = @broadcaster", conn))
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

                                if (message.Equals("!myfunds"))
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

        private static void LogError(Exception ex, string strClass, string strMethod, bool hasToExit, string strCmd = "N/A")
        {
            Console.WriteLine(ex.Message);

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

        public static string chatterValid(string strOrigUser, string strRecipient, string strSearchCriteria = "")
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

        public static bool reactionCmd(string message, string strOrigUser, string strRecipient, string strMsgToSelf, string strAction, string strAddlMsg = "")
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

        public static void setListMods()
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

        public static void setListTimeouts()
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

        public void isUserTimedout(string strUserName)
        {
            if (_timeout.getLstTimeout().ContainsKey(strUserName))
            {
                string timeout = _timeout.getTimoutFromUser(strUserName, _intBroadcasterID, _connStr);

                if (timeout.Equals("0 seconds"))
                    _irc.sendPublicChatMessage("You are now allowed to talk to me again @" + strUserName
                        + ". Please try the requested command once more");
                else
                    _irc.sendPublicChatMessage("I am not allowed to talk to you for " + timeout);
            }
        }

    }
}
