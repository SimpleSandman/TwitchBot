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
        public static SpotifyLocalAPI spotify;
        public static IrcClient irc;
        public static string strBroadcasterName = "simple_sandman";
        public static string strBroadcasterGame = "";
        public static string strBotName = "MrSandmanBot";
        public static string strCurrencyType = "coins";
        public static string connStr = ""; // connection string
        public static TimeZone localZone = TimeZone.CurrentTimeZone;
        public static int intFollowers = 0;
        public static string strDiscordLink = "Link unavailable at the moment"; // provide discord server link if available
        public static bool isAutoPublishTweet = false; // set to auto publish tweets (disabled by default)
        public static bool isAutoDisplaySong = false; // set to auto song status (disabled by default)

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
            string twitterConsumerKey = "";
            string twitterConsumerSecret = "";
            string twitterAccessToken = "";
            string twitterAccessSecret = "";

            bool isSongRequest = false;  // check song request status (disabled by default)
            bool isConnected = false;    // check azure connection

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
                    connStr = ConfigurationManager.ConnectionStrings["TwitchBot.Properties.Settings.conn"].ConnectionString;
                    connStr = connStr + ";User ID=" + userID + ";Password=" + pass;

                    // Check if server is connected
                    if (!IsServerConnected(connStr))
                    {
                        // clear sensitive data
                        pass = null;
                        connStr = null;

                        Console.WriteLine("Azure connection failed. Username or password are incorrect. Please try again.");
                        Console.WriteLine();
                        Console.WriteLine("-- Common technical issues: --");
                        Console.WriteLine("1: Check if Azure firewall settings has your client IP address.");
                        Console.WriteLine("2: Double check the connection string under 'Properties' and 'Settings'");
                        Console.WriteLine("<<<< To exit this program at any time, press 'CTRL' + 'C' >>>>");
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

                /* Get sensitive info from tblSettings */
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblSettings", conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                twitchOAuth = reader["twitchOAuth"].ToString();
                                twitchClientID = reader["twitchClientID"].ToString();
                                twitchAccessToken = reader["twitchAccessToken"].ToString();

                                twitterConsumerKey = reader["twitterConsumerKey"].ToString();
                                twitterConsumerSecret = reader["twitterConsumerSecret"].ToString();
                                twitterAccessToken = reader["twitterAccessToken"].ToString();
                                twitterAccessSecret = reader["twitterAccessSecret"].ToString();

                                if (!String.IsNullOrEmpty(reader["discordLink"].ToString()))
                                    strDiscordLink = reader["discordLink"].ToString();

                                if (!String.IsNullOrEmpty(reader["currencyType"].ToString()))
                                    strCurrencyType = reader["currencyType"].ToString();

                                isAutoPublishTweet = (bool)reader["enableTweet"];
                                isAutoDisplaySong = (bool)reader["enableDisplaySong"];
                            }
                        }
                        else
                        {
                            conn.Close();
                            Console.WriteLine("Check tblSettings for twitch or twitter variables");
                            Thread.Sleep(3000);
                            return;
                        }
                    }
                }

                /* Connect to local spotify client */
                spotify = new SpotifyLocalAPI();
                SpotifyControl spotifyCtrl = new SpotifyControl();
                spotify.OnPlayStateChange += spotifyCtrl.spotify_OnPlayStateChange;
                spotify.OnTrackChange += spotifyCtrl.spotify_OnTrackChange;

                // Password from www.twitchapps.com/tmi/
                // include the "oauth:" portion
                // Use chat bot's oauth
                /* main server: irc.twitch.tv, 6667 */
                irc = new IrcClient("irc.twitch.tv", 6667, strBotName, twitchOAuth);
                irc.joinRoom(strBroadcasterName);

                // Update channel info
                intFollowers = GetChannel().Result.followers;
                strBroadcasterGame = GetChannel().Result.game;

                /* Make new thread to get messages */
                Thread thdIrcClient = new Thread(() => GetChatBox(spotifyCtrl, isSongRequest, twitchAccessToken, connStr));
                thdIrcClient.Start();

                spotifyCtrl.Connect(); // attempt to connect to local Spotify client

                /* Whisper broadcaster bot settings */
                Console.WriteLine("Bot settings: Automatic tweets is set to [" + isAutoPublishTweet + "]");
                Console.WriteLine("Automatic display songs is set to [" + isAutoDisplaySong + "]");
                Console.WriteLine();

                /* Ping to twitch server to prevent auto-disconnect */
                PingSender ping = new PingSender();
                ping.Start();

                PresenceReminder preRmd = new PresenceReminder();
                preRmd.Start();

                /* Authenticate to Twitter */
                Auth.ApplicationCredentials = new TwitterCredentials(
                    twitterConsumerKey, twitterConsumerSecret,
                    twitterAccessToken, twitterAccessSecret
                );
            }
            catch (Exception ex)
            {
                LogError(ex, "Program", "Main(string[])");
            }
        }

        /// <summary>
        /// Monitor chat box for commands
        /// </summary>
        /// <param name="spotifyCtrl"></param>
        /// <param name="isSongRequest"></param>
        /// <param name="twitchAccessToken"></param>
        /// <param name="connStr"></param>
        private static void GetChatBox(SpotifyControl spotifyCtrl, bool isSongRequest, string twitchAccessToken, string connStr)
        {
            try
            {
                /* Master loop */
                while (true)
                {
                    // Read any message inside the chat room
                    string message = irc.readMessage();
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
                            if (strUserName.Equals(strBroadcasterName))
                            {
                                if (message.Equals("!botsettings"))
                                {
                                    irc.sendPublicChatMessage("Auto tweets set to \"" + isAutoPublishTweet + "\" " 
                                        + "|| Auto display songs set to \"" + isAutoDisplaySong + "\"");
                                }

                                if (message.Equals("!exitbot"))
                                {
                                    irc.sendPublicChatMessage("Bye! Have a beautiful time!");
                                    Environment.Exit(0); // exit program
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

                                if (message.Equals("!discord"))
                                    irc.sendPublicChatMessage("Come be a potato with us on our own Discord server! " + strDiscordLink);

                                if (message.Equals("!enabletweet"))
                                {
                                    isAutoPublishTweet = true;

                                    /* Update auto tweet to database */
                                    string query = "UPDATE tblSettings SET enableTweet = @enableTweet";

                                    // Create connection and command
                                    using (SqlConnection conn = new SqlConnection(connStr))
                                    using (SqlCommand cmd = new SqlCommand(query, conn))
                                    {
                                        cmd.Parameters.Add("@enableTweet", SqlDbType.Bit).Value = isAutoPublishTweet;

                                        conn.Open();
                                        cmd.ExecuteNonQuery();
                                        conn.Close();
                                    }

                                    Console.WriteLine("Auto publish tweets is set to [" + isAutoPublishTweet + "]");
                                    irc.sendPublicChatMessage(strBroadcasterName + ": Automatic tweets is set to \"" + isAutoPublishTweet + "\"");
                                }

                                if (message.Equals("!disabletweet"))
                                {
                                    isAutoPublishTweet = false;

                                    /* Update auto tweet to database */
                                    string query = "UPDATE tblSettings SET enableTweet = @enableTweet";

                                    // Create connection and command
                                    using (SqlConnection conn = new SqlConnection(connStr))
                                    using (SqlCommand cmd = new SqlCommand(query, conn))
                                    {
                                        cmd.Parameters.Add("@enableTweet", SqlDbType.Bit).Value = isAutoPublishTweet;

                                        conn.Open();
                                        cmd.ExecuteNonQuery();
                                        conn.Close();
                                    }

                                    Console.WriteLine("Auto publish tweets is set to [" + isAutoPublishTweet + "]");
                                    irc.sendPublicChatMessage(strBroadcasterName + ": Automatic tweets is set to \"" + isAutoPublishTweet + "\"");
                                }

                                if (message.Equals("!songrequests on"))
                                {
                                    isSongRequest = true;
                                    irc.sendPublicChatMessage("Song requests enabled");
                                }

                                if (message.Equals("!songrequests off"))
                                {
                                    isSongRequest = false;
                                    irc.sendPublicChatMessage("Song requests disabled");
                                }

                                if (message.StartsWith("!updatetitle"))
                                {
                                    // Get title from command parameter
                                    string title = string.Empty;
                                    int lengthParam1 = (GetNthIndex(message, '"', 2) - message.IndexOf('"')) - 1;
                                    int startIndexParam1 = message.IndexOf('"') + 1;
                                    title = message.Substring(startIndexParam1, lengthParam1);

                                    // Send HTTP method PUT to base URI in order to change the title
                                    var client = new RestClient("https://api.twitch.tv/kraken/channels/" + strBroadcasterName);
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
                                            irc.sendPublicChatMessage("Twitch channel title updated to \"" + title +
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

                                if (message.StartsWith("!updategame"))
                                {
                                    // Get title from command parameter
                                    string game = string.Empty;
                                    int lengthParam1 = (message.LastIndexOf('"') - message.IndexOf('"')) - 1;
                                    int startIndexParam1 = message.IndexOf('"') + 1;
                                    game = message.Substring(startIndexParam1, lengthParam1);

                                    // Send HTTP method PUT to base URI in order to change the game
                                    var client = new RestClient("https://api.twitch.tv/kraken/channels/" + strBroadcasterName);
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
                                            irc.sendPublicChatMessage("Twitch channel game status updated to \"" + game +
                                                "\" || Restart your connection to the stream or twitch app in order to see the change");
                                            if (isAutoPublishTweet)
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

                                if (message.StartsWith("!tweet"))
                                {
                                    string command = message;
                                    SendTweet(message, command);
                                }

                                if (message.StartsWith("!displaysongs on"))
                                {
                                    isAutoDisplaySong = true;

                                    /* Update auto tweet to database */
                                    string query = "UPDATE tblSettings SET enableDisplaySong = @enableDisplaySong";

                                    // Create connection and command
                                    using (SqlConnection conn = new SqlConnection(connStr))
                                    using (SqlCommand cmd = new SqlCommand(query, conn))
                                    {
                                        cmd.Parameters.Add("@enableDisplaySong", SqlDbType.Bit).Value = isAutoDisplaySong;

                                        conn.Open();
                                        cmd.ExecuteNonQuery();
                                        conn.Close();
                                    }

                                    Console.WriteLine("Auto display songs is set to [" + isAutoDisplaySong + "]");
                                    irc.sendPublicChatMessage(strBroadcasterName + ": Automatic display songs is set to \"" + isAutoDisplaySong + "\"");
                                }

                                if (message.StartsWith("!displaysongs off"))
                                {
                                    isAutoDisplaySong = false;

                                    /* Update auto song display to database */
                                    string query = "UPDATE tblSettings SET enableDisplaySong = @enableDisplaySong";

                                    // Create connection and command
                                    using (SqlConnection conn = new SqlConnection(connStr))
                                    using (SqlCommand cmd = new SqlCommand(query, conn))
                                    {
                                        cmd.Parameters.Add("@enableDisplaySong", SqlDbType.Bit).Value = isAutoDisplaySong;

                                        conn.Open();
                                        cmd.ExecuteNonQuery();
                                        conn.Close();
                                    }

                                    Console.WriteLine("Auto display songs is set to [" + isAutoDisplaySong + "]");
                                    irc.sendPublicChatMessage(strBroadcasterName + ": Automatic display songs is set to \"" + isAutoDisplaySong + "\"");
                                }

                                if (message.StartsWith("!charge") && message.Contains("@"))
                                {
                                    if (message.StartsWith("!charge @"))
                                        irc.sendPublicChatMessage("Please enter a valid amount to a user @" + strUserName);
                                    else
                                    {
                                        int intIndexAction = 8;
                                        int intFee = -1;
                                        bool validFee = int.TryParse(message.Substring(intIndexAction, message.IndexOf("@") - intIndexAction - 1), out intFee);
                                        string strRecipient = message.Substring(message.IndexOf("@") + 1).ToLower();
                                        int intWallet = currencyBalance(strRecipient);

                                        // Check user's bank account
                                        if (intWallet == -1)
                                            irc.sendPublicChatMessage("The user '" + strRecipient + "' is not currently banking with us @" + strUserName);
                                        else if (intWallet == 0)
                                            irc.sendPublicChatMessage("'" + strRecipient + "' is out of " + strCurrencyType + " @" + strUserName);
                                        // Check if fee can be accepted
                                        else if (intFee > 0)
                                            irc.sendPublicChatMessage("Please insert a negative amount or use the !deposit command to add " + strCurrencyType + " to a user");
                                        else if (!validFee)
                                            irc.sendPublicChatMessage("The fee wasn't accepted. Please try again with negative whole numbers only");
                                        else /* Insert fee into wallet */
                                        {
                                            intWallet = intWallet + intFee;

                                            // Zero out account balance if user is being charged more than they have
                                            if (intWallet < 0)
                                                intWallet = 0;

                                            string query = "UPDATE dbo.tblBank SET wallet = @wallet WHERE (username = @username AND broadcaster = @broadcaster)";

                                            using (SqlConnection conn = new SqlConnection(connStr))
                                            using (SqlCommand cmd = new SqlCommand(query, conn))
                                            {
                                                cmd.Parameters.Add("@wallet", SqlDbType.Int).Value = intWallet;
                                                cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = strRecipient;
                                                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = 1; // ToDo: Make dynamic (debugging only)

                                                conn.Open();
                                                cmd.ExecuteNonQuery();
                                                conn.Close();
                                            }

                                            // prompt user's balance
                                            if (intWallet == 0)
                                                irc.sendPublicChatMessage("Charged " + intFee.ToString() + " " + strCurrencyType + " to " + strRecipient
                                                    + "'s account! They are out of " + strCurrencyType + " to spend");
                                            else
                                                irc.sendPublicChatMessage("Charged " + intFee.ToString().Replace("-", "") + " " + strCurrencyType + " to " + strRecipient
                                                    + "'s account! They only have " + intWallet + " " + strCurrencyType + " to spend");
                                        }
                                    }
                                }

                                if (message.StartsWith("!deposit") && message.Contains("@"))
                                {
                                    if (message.StartsWith("!deposit @"))
                                        irc.sendPublicChatMessage("Please enter a valid amount to a user @" + strUserName);
                                    else
                                    {
                                        int intIndexAction = 9;
                                        int intDeposit = -1;
                                        bool validDeposit = int.TryParse(message.Substring(intIndexAction, message.IndexOf("@") - intIndexAction - 1), out intDeposit);
                                        string strRecipient = message.Substring(message.IndexOf("@") + 1).ToLower();
                                        int intWallet = currencyBalance(strRecipient);

                                        // check if deposit amount is valid
                                        if (intDeposit < 0)
                                            irc.sendPublicChatMessage("Please insert a positive amount or use the !charge command to remove " + strCurrencyType + " from a user");
                                        else if (!validDeposit)
                                            irc.sendPublicChatMessage("The deposit wasn't accepted. Please try again with positive whole numbers only");
                                        else
                                        {
                                            // check if user has a bank account
                                            if (intWallet == -1)
                                            {
                                                string query = "INSERT INTO tblBank (username, wallet, broadcaster) VALUES (@username, @wallet, @broadcaster)";

                                                using (SqlConnection conn = new SqlConnection(connStr))
                                                using (SqlCommand cmd = new SqlCommand(query, conn))
                                                {
                                                    cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = strRecipient;
                                                    cmd.Parameters.Add("@wallet", SqlDbType.Int).Value = intDeposit;
                                                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = 1; // ToDo: Make dynamic (debugging only)

                                                    conn.Open();
                                                    cmd.ExecuteNonQuery();
                                                    conn.Close();
                                                }

                                                irc.sendPublicChatMessage(strUserName + " has created a new account for @" + strRecipient
                                                    + " with " + intDeposit + " " + strCurrencyType + " to spend");
                                            }
                                            else // deposit money into wallet
                                            {
                                                intWallet = intWallet + intDeposit;

                                                string query = "UPDATE dbo.tblBank SET wallet = @wallet WHERE (username = @username AND broadcaster = @broadcaster)";

                                                using (SqlConnection conn = new SqlConnection(connStr))
                                                using (SqlCommand cmd = new SqlCommand(query, conn))
                                                {
                                                    cmd.Parameters.Add("@wallet", SqlDbType.Int).Value = intWallet;
                                                    cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = strRecipient;
                                                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = 1; // ToDo: Make dynamic (debugging only)

                                                    conn.Open();
                                                    cmd.ExecuteNonQuery();
                                                    conn.Close();
                                                }

                                                // prompt user's balance
                                                irc.sendPublicChatMessage("Deposited " + intDeposit.ToString() + " " + strCurrencyType + " to @" + strRecipient
                                                    + "'s account! They now have " + intWallet + " " + strCurrencyType + " to spend");
                                            }
                                        }
                                    }
                                }
                            }

                            /*
                             * Moderator commands
                             * ToDo: check for moderators
                             */

                            /* insert moderator commands here */

                            /* 
                             * General commands 
                             */
                            if (message.Equals("!commands"))
                            {
                                irc.sendPublicChatMessage("!hello | !slap @[username] | !stab @[username] | !throw [item] @[username] | !shoot @[username]"
                                    + "| !currentsong | !songrequestlist | !requestsong [artist] - [song title] | !utctime | !hosttime | !partyup [party member name] ---"
                                    + " Link to full list of commands: "
                                    + "https://github.com/SimpleSandman/TwitchBot/wiki/List-of-Commands");
                            }

                            if (message.Equals("!hello"))
                                irc.sendPublicChatMessage("Hey " + strUserName + "! Thanks for talking to me.");

                            if (message.Equals("!utctime"))
                                irc.sendPublicChatMessage("UTC Time: " + DateTime.UtcNow.ToString());

                            if (message.Equals("!hosttime"))
                                irc.sendPublicChatMessage(strBroadcasterName + "'s Current Time: " + DateTime.Now.ToString() + " (" + localZone.StandardName + ")");

                            if (message.Equals("!uptime")) // need to check if channel is currently streaming
                            {
                                var upTimeRes = GetChannel();
                                TimeSpan ts = DateTime.UtcNow - DateTime.Parse(upTimeRes.Result.updated_at);
                                string upTime = String.Format("{0:h\\:mm\\:ss}", ts);
                                irc.sendPublicChatMessage("This channel's current uptime (length of current stream) is " + upTime);
                            }

                            /* List song requests from database */
                            if (message.Equals("!songrequestlist"))
                            {
                                string songList = "";

                                using (SqlConnection conn = new SqlConnection(connStr))
                                {
                                    conn.Open();
                                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblSongRequests", conn))
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
                                                    // grab only the real data
                                                    if (!col.ColumnName.Equals("Id"))
                                                    {
                                                        newRow[col.ColumnName] = reader[col.ColumnName];
                                                        Console.WriteLine(newRow[col.ColumnName].ToString());
                                                        songList = songList + newRow[col.ColumnName].ToString() + " || ";
                                                    }
                                                }
                                            }
                                            StringBuilder strBdrSongList = new StringBuilder(songList);
                                            strBdrSongList.Remove(songList.Length - 4, 4); // remove extra " || "
                                            songList = strBdrSongList.ToString(); // replace old song list string with new
                                            irc.sendPublicChatMessage("Current List of Requested Songs: " + songList);
                                        }
                                        else
                                        {
                                            Console.WriteLine("No requests have been made");
                                            irc.sendPublicChatMessage("No requests have been made");
                                        }
                                    }
                                }
                            }

                            /* Insert requested song into database */
                            if (message.StartsWith("!requestsong"))
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
                                        irc.sendPublicChatMessage("Only letters, numbers, and hyphens (-) are allowed. Please try again. "
                                         + "If the problem persists, please contact the author of this bot");
                                    }
                                    else
                                    {
                                        /* Add song request to database */
                                        string query = "INSERT INTO tblSongRequests (songRequests) VALUES (@song)";

                                        // Create connection and command
                                        using (SqlConnection conn = new SqlConnection(connStr))
                                        using (SqlCommand cmd = new SqlCommand(query, conn))
                                        {
                                            cmd.Parameters.Add("@song", SqlDbType.VarChar, 200).Value = songRequest;

                                            conn.Open();
                                            cmd.ExecuteNonQuery();
                                            conn.Close();
                                        }

                                        irc.sendPublicChatMessage("The song \"" + songRequest + "\" has been successfully requested!");
                                    }
                                }
                                else
                                    irc.sendPublicChatMessage("Song requests are disabled at the moment");
                            }

                            if (message.Equals("!currentsong"))
                            {
                                StatusResponse status = spotify.GetStatus();
                                if (status != null)
                                {
                                    irc.sendPublicChatMessage("Current Song: " + status.Track.TrackResource.Name
                                        + " || Artist: " + status.Track.ArtistResource.Name
                                        + " || Album: " + status.Track.AlbumResource.Name);
                                }
                                else
                                    irc.sendPublicChatMessage("The broadcaster is not playing a song at the moment");
                            }

                            /* Action commands */
                            if (message.StartsWith("!slap @"))
                            {
                                string strRecipient = message.Substring(message.IndexOf("@") + 1).ToLower();
                                reactionCmd(message, strUserName, strRecipient, "Stop smacking yourself", "slaps");
                            }

                            if (message.StartsWith("!stab @"))
                            {
                                string strRecipient = message.Substring(message.IndexOf("@") + 1).ToLower();
                                reactionCmd(message, strUserName, strRecipient, "Stop stabbing yourself", "stabs", " to death!");
                            }

                            if (message.StartsWith("!shoot @"))
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
                                if (strRecipient.Equals(strBotName.ToLower()))
                                {
                                    if (strBodyPart.Equals(" but missed"))
                                        irc.sendPublicChatMessage("Ha! You missed @" + strUserName);
                                    else
                                        irc.sendPublicChatMessage("You think shooting me in the " + strBodyPart.Replace("'s ", "") + " would hurt me? I am a bot!");
                                }
                            }

                            if (message.StartsWith("!throw ") && message.Contains("@"))
                            {
                                int intIndexAction = 7;

                                if (message.StartsWith("!throw @"))
                                    irc.sendPublicChatMessage("Please throw an item to a user @" + strUserName);
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

                            if (message.StartsWith("!partyup"))
                            {
                                string strPartyMember = "";
                                int intGameID = 0;
                                bool isPartyMemebrFound = false;

                                // Get current game
                                strBroadcasterGame = GetChannel().Result.game;

                                // check if user entered something
                                if (message.Length < 9)
                                    irc.sendPublicChatMessage("Please enter a party member @" + strUserName);
                                else
                                    strPartyMember = message.Substring(9);

                                // grab game id in order to find party member
                                using (SqlConnection conn = new SqlConnection(connStr))
                                {
                                    conn.Open();
                                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblGameList", conn))
                                    using (SqlDataReader reader = cmd.ExecuteReader())
                                    {
                                        if (reader.HasRows)
                                        {
                                            while (reader.Read())
                                            {
                                                if (strBroadcasterGame.Equals(reader["name"].ToString()))
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
                                    irc.sendPublicChatMessage("This game is currently not a part of the 'Party Up' command");
                                else // check if requested party member is valid
                                {
                                    using (SqlConnection conn = new SqlConnection(connStr))
                                    {
                                        conn.Open();
                                        using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblPartyUp", conn))
                                        using (SqlDataReader reader = cmd.ExecuteReader())
                                        {
                                            if (reader.HasRows)
                                            {
                                                while (reader.Read())
                                                {
                                                    if (strPartyMember.ToLower().Equals(reader["partyMember"].ToString()))
                                                    {
                                                        isPartyMemebrFound = true;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    // insert party member if member exists from database
                                    if (!isPartyMemebrFound)
                                        irc.sendPublicChatMessage("I couldn't find the requested party memebr '" + strPartyMember + "' @" + strUserName
                                            + ". Please check with the broadcaster for possible spelling errors");
                                    else
                                    {
                                        string query = "INSERT INTO tblPartyUpRequests (username, partyMember, timeRequested) VALUES (@username, @partyMember, @timeRequested)";

                                        // Create connection and command
                                        using (SqlConnection conn = new SqlConnection(connStr))
                                        using (SqlCommand cmd = new SqlCommand(query, conn))
                                        {
                                            cmd.Parameters.Add("@username", SqlDbType.VarChar, 50).Value = strUserName;
                                            cmd.Parameters.Add("@partyMember", SqlDbType.VarChar, 50).Value = strPartyMember;
                                            cmd.Parameters.Add("@timeRequested", SqlDbType.DateTime).Value = DateTime.Now;

                                            conn.Open();
                                            cmd.ExecuteNonQuery();
                                            conn.Close();
                                        }

                                        irc.sendPublicChatMessage("@" + strUserName + ": " + strPartyMember + " has been added to the party queue");
                                    }
                                }
                            }

                            if (message.Equals("!myfunds"))
                            {
                                int intBalance = currencyBalance(strUserName);

                                if (intBalance == -1)
                                    irc.sendPublicChatMessage("You are not currently banking with us at the moment. Please talk to a moderator about acquiring " + strCurrencyType);
                                else
                                    irc.sendPublicChatMessage("@" + strUserName + " currently has " + intBalance.ToString() + " " + strCurrencyType);
                            }

                            /* add more general commands here */
                        }
                    }
                } // end master while loop
            }
            catch (Exception ex)
            {
                LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, string)");
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
                var body = await client.GetStringAsync("https://api.twitch.tv/kraken/channels/" + strBroadcasterName);
                var response = JsonConvert.DeserializeObject<ChannelJSON>(body);
                return response;
            }
        }

        static async Task<FollowerInfo> GetBroadcasterInfo()
        {
            using (var client = new HttpClient())
            {
                var body = await client.GetStringAsync("https://api.twitch.tv/kraken/channels/" + strBroadcasterName + "/follows?limit=" + intFollowers);
                var response = JsonConvert.DeserializeObject<FollowerInfo>(body);
                return response;
            }
        }

        static async Task<ChatterInfo> GetChatters()
        {
            using (var client = new HttpClient())
            {
                var body = await client.GetStringAsync("https://tmi.twitch.tv/group/user/" + strBroadcasterName + "/chatters");
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
                irc.sendPublicChatMessage(resultMessage);
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
                irc.sendPublicChatMessage(resultMessage);
            }
            else
            {
                int overCharLimit = tweetMessage.Length - 140;
                resultMessage = "The message you attempted to tweet had " + overCharLimit +
                    " characters more than the 140 character limit. Please shorten your message and try again";
                Console.WriteLine(resultMessage);
                irc.sendPublicChatMessage(resultMessage);
            }
        }

        private static void LogError(Exception ex, string strClass, string strMethod)
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
            string query = "INSERT INTO tblErrorLog (errorTime, errorLine, errorClass, errorMethod, errorMsg, broadcasterName) "
                + "VALUES (@time, @lineNum, @class, @method, @msg, @broadcasterName)";

            // Create connection and command
            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add("@time", SqlDbType.DateTime).Value = DateTime.Now;
                cmd.Parameters.Add("@lineNum", SqlDbType.Int).Value = lineNumber;
                cmd.Parameters.Add("@class", SqlDbType.VarChar, 100).Value = strClass;
                cmd.Parameters.Add("@method", SqlDbType.VarChar, 100).Value = strMethod;
                cmd.Parameters.Add("@msg", SqlDbType.VarChar, 4000).Value = ex.Message;
                cmd.Parameters.Add("@broadcasterName", SqlDbType.VarChar, 50).Value = strBroadcasterName;

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }

            Thread.Sleep(3000);

            if (irc != null)
                irc.sendPublicChatMessage("I ran into an unexpected internal error! I have to leave the chat now. "
                    + "@" + strBroadcasterName + " please look into the error log when you have time");

            Environment.Exit(0);
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
            irc.sendPublicChatMessage("@" + strOrigUser + ": I cannot find the user you wanted to interact with. Perhaps the user left us?");
            return "";
        }

        public static bool reactionCmd(string message, string strOrigUser, string strRecipient, string strMsgToSelf, string strAction, string strAddlMsg = "")
        {
            string strRoleType = chatterValid(strOrigUser, strRecipient);

            // check if user currently watching the channel
            if (!string.IsNullOrEmpty(strRoleType))
            {
                if (strOrigUser.Equals(strRecipient))
                    irc.sendPublicChatMessage(strMsgToSelf + " @" + strOrigUser);
                else
                    irc.sendPublicChatMessage(strOrigUser + " " + strAction + " @" + strRecipient + strAddlMsg);

                return true;
            }
            else
                return false;
        }
        
        public static int currencyBalance(string username)
        {
            int intBalance = -1;

            // check if user already has a bank account
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblBank", conn))
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

            return intBalance;
        }

    }
}
