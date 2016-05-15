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

namespace TwitchBot
{
    class Program
    {
        public static SpotifyLocalAPI spotify;
        public static IrcClient irc;
        public static string strBroadcasterName = "simple_sandman";
        public static string strBotName = "MrSandmanBot";
        public static TimeZone localZone = TimeZone.CurrentTimeZone;
        public static bool isAutoPublishTweet = false; // set to auto publish tweets (disabled by default)
        public static bool isAutoDisplaySong = false; // set to auto song status (disabled by default)

        static void Main(string[] args)
        {
            string userID = "";       // username
            string pass = "";         // password
            string connStr = "";      // connection string
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

                        Console.WriteLine("Azure connection failed. Username or password are incorrect. Please try again");
                    }
                    else
                        isConnected = true;
                }
                while (!isConnected);

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
                Console.WriteLine(ex.Message);
                Thread.Sleep(3000);
                return;
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
                        * and check if user has access to functions
                        */
                        if (message.Contains("PRIVMSG"))
                        {
                            // Modify message to only show user and message
                            int indexFirstPoundSign = message.IndexOf('#');
                            StringBuilder strBdrMessage = new StringBuilder(message);
                            strBdrMessage.Remove(0, indexFirstPoundSign + 1); // remove unnecessary info before and including the pound sign
                            message = strBdrMessage.ToString(); // replace old message string with new

                            // Get user name from PRIVMSG
                            int indexFirstSpace = message.IndexOf(" :");
                            string strUserName = message.Substring(0, indexFirstSpace);

                            strBdrMessage.Remove(0, indexFirstSpace + 2); // remove user name info before message
                            message = strBdrMessage.ToString(); // replace old message with new

                            /* Broadcaster privileges */
                            if (strUserName.Equals(strBroadcasterName))
                            {
                                if (message.Equals("!botsettings"))
                                {
                                    irc.sendPublicChatMessage("Auto tweets set to \"" + isAutoPublishTweet + "\" " 
                                        + "|| Auto display song set to \"" + isAutoDisplaySong + "\"");
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
                            }

                            /* General commands */
                            if (message.Equals("!commands"))
                            {
                                irc.sendPublicChatMessage("Link to list of commands: "
                                    + "https://github.com/SimpleSandman/TwitchBot/wiki/List-of-Commands");
                            }

                            if (message.Equals("!hello"))
                                irc.sendPublicChatMessage("Hey " + strUserName + "! Thanks for talking to me.");

                            if (message.Equals("!utctime"))
                                irc.sendPublicChatMessage("UTC Time: " + DateTime.UtcNow.ToString());

                            if (message.Equals("!localtime"))
                                irc.sendPublicChatMessage("Local Time: " + DateTime.Now.ToString() + " (" + localZone.StandardName + ")");

                            if (message.Equals("!uptime")) // need to check if channel is currently streaming
                            {
                                var upTimeRes = GetChannel();
                                TimeSpan ts = DateTime.UtcNow - DateTime.Parse(upTimeRes.Result.updated_at);
                                string upTime = String.Format("{0:h\\:mm\\:ss}", ts);
                                irc.sendPublicChatMessage("This channel's current uptime (length of current stream) is " + upTime);
                            }

                            /* Add song request to database */
                            if (message.Equals("!requestlist"))
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

                            /* add more general commands here */
                        }
                    }
                } // end master while loop
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                irc.sendPublicChatMessage("I ran into an internal error! I am leaving the chat now. " 
                    + "Please notify the broadcaster of this issue");
                Thread.Sleep(3000);
                Environment.Exit(0);
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
        
    }
}
