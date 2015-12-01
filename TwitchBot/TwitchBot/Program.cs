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

        static void Main(string[] args)
        {            
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

            bool isSongRequest = false; // check song request status
            bool isConnected = false; // check azure connection

            TimeZone localZone = TimeZone.CurrentTimeZone;

            /* Retry password until success */
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

                // append password to connection string
                connStr = ConfigurationManager.ConnectionStrings["TwitchBot.Properties.Settings.conn"].ConnectionString;
                connStr = connStr + ";Password=" + pass;

                // check if server is connected
                if (!IsServerConnected(connStr))
                {
                    // clear sensitive data
                    pass = null;
                    connStr = null;

                    Console.WriteLine("Azure connection failed. Check connection string or password");
                }
                else
                    isConnected = true;
            }
            while (!isConnected);

            // clear sensitive data
            pass = null;

            Console.WriteLine("Azure server connection successful");

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
                        }
                    }
                    else
                    {
                        conn.Close();
                        Console.WriteLine("Check database for twitch or twitter variables");
                        Thread.Sleep(1000);
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
            irc = new IrcClient("irc.twitch.tv", 6667,
                strBotName, twitchOAuth);
            irc.joinRoom(strBroadcasterName);

            spotifyCtrl.Connect(); // initial connect to spotify local client

            /* Ping to twitch server to prevent auto-disconnect */
            PingSender ping = new PingSender();
            ping.Start();

            /* Authenticate to Twitter */
            Auth.ApplicationCredentials = new TwitterCredentials(
                twitterConsumerKey, twitterConsumerSecret, 
                twitterAccessToken, twitterAccessSecret
            );

            /* Master loop */
            while (true)
            {
                string message = irc.readMessage();
                Console.WriteLine(message);

                if (!string.IsNullOrEmpty(message))
                {
                    /* 
                    * Get username and message from chat 
                    * and check if user has access to functions
                    */
                    if (message.Contains("PRIVMSG"))
                    {
                        // Modify message to only show user and message
                        int indexFirstPoundSign = message.IndexOf('#');
                        StringBuilder strBdrMessage = new StringBuilder(message);
                        strBdrMessage.Remove(0, indexFirstPoundSign - 1); // remove unnecessary info before pound sign
                        message = strBdrMessage.ToString(); // replace old message string with new

                        // Get username from PRIVMSG
                        int indexFirstSpace = message.IndexOf(" :");
                        string strUserName = message.Substring(0, indexFirstSpace);

                        /* Broadcaster privileges */
                        if (strUserName.Contains(strBroadcasterName))
                        {
                            if (message.Contains("!spotifyconnect"))
                                spotifyCtrl.Connect(); // manually connect to spotify

                            if (message.Contains("!spotifyplay"))
                                spotifyCtrl.playBtn_Click();

                            if (message.Contains("!spotifypause"))
                                spotifyCtrl.pauseBtn_Click();

                            if (message.Contains("!spotifyprev"))
                                spotifyCtrl.prevBtn_Click();

                            if (message.Contains("!spotifynext"))
                                spotifyCtrl.skipBtn_Click();

                            if (message.Contains("!songs on"))
                            {
                                isSongRequest = true;
                                irc.sendPublicChatMessage("Song requests enabled");
                            }

                            if (message.Contains("!songs off"))
                            {
                                isSongRequest = false;
                                irc.sendPublicChatMessage("Song requests disabled");
                            }

                            if (message.Contains("!updatetitle"))
                            {
                                // Get title from command parameter
                                string title = string.Empty;
                                int lengthParam1 = (GetNthIndex(message, '"', 2) - message.IndexOf('"')) - 1;
                                int startIndexParam1 = message.IndexOf('"') + 1;
                                title = message.Substring(startIndexParam1, lengthParam1);

                                var client = new RestClient("https://api.twitch.tv/kraken/channels/" + strBroadcasterName);
                                var request = new RestRequest(Method.PUT);
                                request.AddHeader("cache-control", "no-cache");
                                request.AddHeader("content-type", "application/json");
                                request.AddHeader("authorization", "OAuth " + twitchAccessToken);
                                request.AddHeader("accept", "application/vnd.twitchtv.v3+json");
                                request.AddParameter("application/json", "{\"channel\":{\"status\":\"" + title + "\"}}", ParameterType.RequestBody);

                                IRestResponse response = null;
                                try
                                {
                                    response = client.Execute(request);
                                    string statResponse = response.StatusCode.ToString();
                                    if (statResponse.Contains("OK"))
                                        irc.sendPublicChatMessage("Twitch channel title updated to " + title + "! Refresh your browser to see the change");
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

                            if (message.Contains("!updategame"))
                            {
                                // Get title from command parameter
                                string game = string.Empty;
                                int lengthParam1 = (GetNthIndex(message, '"', 2) - message.IndexOf('"')) - 1;
                                int startIndexParam1 = message.IndexOf('"') + 1;
                                game = message.Substring(startIndexParam1, lengthParam1);

                                var client = new RestClient("https://api.twitch.tv/kraken/channels/" + strBroadcasterName);
                                var request = new RestRequest(Method.PUT);
                                request.AddHeader("cache-control", "no-cache");
                                request.AddHeader("content-type", "application/json");
                                request.AddHeader("authorization", "OAuth " + twitchAccessToken);
                                request.AddHeader("accept", "application/vnd.twitchtv.v3+json");
                                request.AddParameter("application/json", "{\"channel\":{\"game\":\"" + game + "\"}}", ParameterType.RequestBody);

                                IRestResponse response = null;
                                try
                                {
                                    response = client.Execute(request);
                                    string statResponse = response.StatusCode.ToString();
                                    if (statResponse.Contains("OK"))
                                        irc.sendPublicChatMessage("Twitch channel game status updated to " + game + "! Refresh your browser to see the change");
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

                            if (message.Contains("!tweet"))
                            {
                                // Get message from command parameter
                                string tweetMessage = string.Empty;
                                int lengthParam1 = (GetNthIndex(message, '"', 2) - message.IndexOf('"')) - 1;
                                int startIndexParam1 = message.IndexOf('"') + 1;
                                tweetMessage = message.Substring(startIndexParam1, lengthParam1);

                                var basicTweet = Tweet.PublishTweet(tweetMessage);
                            }
                        }

                        /* General commands */
                        if (message.Contains("!commands"))
                        {
                            // work in progress
                            irc.sendPublicChatMessage("Check out all you can tell me to do here: " 
                                + "https://github.com/SimpleSandman/TwitchBot/wiki/List-of-Commands");
                        }

                        if (message.Contains("!hello"))
                            irc.sendPublicChatMessage("Hey there! Thanks for talking to me.");

                        if (message.Contains("!utctime"))
                            irc.sendPublicChatMessage("UTC Time: " + DateTime.UtcNow.ToString());

                        if (message.Contains("!localtime"))
                            irc.sendPublicChatMessage("Local Time: " + DateTime.Now.ToString() + " (" + localZone.StandardName + ")");

                        if (message.Contains("!uptime")) // need to check if channel is currently streaming
                        {
                            var upTimeRes = GetChannel();
                            TimeSpan ts = DateTime.UtcNow - DateTime.Parse(upTimeRes.Result.updated_at);
                            string upTime = String.Format("{0:h\\:mm\\:ss}", ts);
                            irc.sendPublicChatMessage("This channel's current uptime (length of current stream) is " + upTime);
                        }

                        // add song request to database
                        if (message.Contains("!requestlist"))
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
                                        songList = strBdrSongList.ToString(); // replace old songlist string with new
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

                        // insert requested song into database
                        if (message.Contains("!requestsong"))
                        {
                            if (isSongRequest)
                            {
                                // grab the song name from the request
                                int index = message.IndexOf("!requestsong");
                                string songRequest = message.Substring(index, message.Length - index);
                                songRequest = songRequest.Replace("!requestsong ", "");
                                Console.WriteLine("New song request: " + songRequest);

                                // check if song request has more than letters, numbers, and hyphens
                                if (!Regex.IsMatch(songRequest, @"^[a-zA-Z0-9 \-]+$"))
                                {
                                    irc.sendPublicChatMessage("Only letters, numbers, and hyphens (-) are allowed. Please try again. "
                                     + "If the problem persists, please contact the author of this bot");
                                }
                                else
                                {
                                    // add song request to database
                                    string query = "INSERT INTO tblSongRequests (songRequests) VALUES (@song)";

                                    // create connection and command
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

                        if (message.Contains("!currentsong")) 
                        {
                            StatusResponse status = spotify.GetStatus();
                            if (status == null) return;

                            irc.sendPublicChatMessage("Current Song: " + status.Track.TrackResource.Name
                                + " || Artist: " + status.Track.ArtistResource.Name
                                + " || Album: " + status.Track.AlbumResource.Name);
                        }

                        /* add more general commands here */
                    }
                }
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
        
    }
}
