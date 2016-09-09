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
using TwitchBot.Configuration;
using System.Collections;

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
            var appConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var twitchBotConfigurationSection = appConfig.GetSection("TwitchBotConfiguration") as TwitchBotConfigurationSection;

            if(twitchBotConfigurationSection == null)
            {
                //section not in app.config create a default, add it to the config, and save
                twitchBotConfigurationSection = new TwitchBotConfigurationSection();
                appConfig.Sections.Add("TwitchBotConfiguration", twitchBotConfigurationSection);
                appConfig.Save(ConfigurationSaveMode.Full);
                ConfigurationManager.RefreshSection("TwitchBotConfiguration");

                //Since not previously configured, configure bot and save changes using configuration wizard
                TwitchBotConfigurator.ConfigureBot(twitchBotConfigurationSection);
                appConfig.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("TwitchBotConfiguration");
            }

            //Bot already configured, do stuff
            //Lets get the connection string, and if it doesn't exist lets run the configuration wizard to add it to the config
            var connectionStringSetting = appConfig.ConnectionStrings.ConnectionStrings["TwitchBotConnectionString"];
            if(connectionStringSetting == null)
            {
                connectionStringSetting = new ConnectionStringSettings();
                TwitchBotConfigurator.ConfigureConnectionString("TwitchBotConnectionString", connectionStringSetting);
                appConfig.ConnectionStrings.ConnectionStrings.Add(connectionStringSetting);
                appConfig.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("connectionStrings");
            }
            var connectionString = connectionStringSetting.ConnectionString;

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
//                if (!string.IsNullOrWhiteSpace(connectionString))
//                {
//                    _connStr = connectionString; // production database only
//                    Console.WriteLine("Connecting to database...");
//                }
//                else if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.connTest))
//                {
                    _connStr = connectionString; // test database only
                    Console.WriteLine("<<<< WARNING: Connecting to local database (testing only) >>>>");
//                }
//                else
//                {
//                    Console.WriteLine("Internal Error: Connection string to database not provided!");
//                    Console.WriteLine("Shutting down now...");
//                    Thread.Sleep(3500);
//                    Environment.Exit(1);
//                }

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
                //Console.WriteLine("Database connection successful!");
                //Console.WriteLine();
                //Console.WriteLine("Checking if user settings has all necessary info...");

                //// Grab settings
                //_strBotName = Properties.Settings.Default.botName;
                //_strBroadcasterName = Properties.Settings.Default.broadcaster;
                //twitchOAuth = Properties.Settings.Default.twitchOAuth;
                //twitchClientID = Properties.Settings.Default.twitchClientID;
                //twitchAccessToken = Properties.Settings.Default.twitchAccessToken;
                //twitterConsumerKey = Properties.Settings.Default.twitterConsumerKey;
                //twitterConsumerSecret = Properties.Settings.Default.twitterConsumerSecret;
                //twitterAccessToken = Properties.Settings.Default.twitterAccessToken;
                //twitterAccessSecret = Properties.Settings.Default.twitterAccessSecret;
                //_strDiscordLink = Properties.Settings.Default.discordLink;
                //_strCurrencyType = Properties.Settings.Default.currencyType;
                //_isAutoDisplaySong = Properties.Settings.Default.enableDisplaySong;
                //_isAutoPublishTweet = Properties.Settings.Default.enableTweet;
                //_intStreamLatency = Properties.Settings.Default.streamLatency;

                //// Check if program has client ID (developer needs to provide this inside the settings)
                //if (string.IsNullOrWhiteSpace(twitchClientID))
                //{
                //    Console.WriteLine("Error: MISSING Twitch Client ID");
                //    Console.WriteLine("Please contact the author of this bot to re-release this application with the client ID");
                //    Console.WriteLine();
                //    Console.WriteLine("Shutting down now...");
                //    Thread.Sleep(3000);
                //    Environment.Exit(1);
                //}

                //// Check if user has the minimum info in order to run the bot
                //// Tell user to input essential info
                //while (string.IsNullOrWhiteSpace(_strBotName)
                //    && string.IsNullOrWhiteSpace(_strBroadcasterName)
                //    && string.IsNullOrWhiteSpace(twitchOAuth)
                //    && string.IsNullOrWhiteSpace(twitchAccessToken))
                //{
                //    Console.WriteLine("You are missing essential info");
                //    if (string.IsNullOrWhiteSpace(_strBotName))
                //    {
                //        Console.WriteLine("Enter your bot's username: ");
                //        Properties.Settings.Default.botName = Console.ReadLine();
                //        _strBotName = Properties.Settings.Default.botName;
                //    }

                //    if (string.IsNullOrWhiteSpace(_strBroadcasterName))
                //    {
                //        Console.WriteLine("Enter your Twitch username: ");
                //        Properties.Settings.Default.broadcaster = Console.ReadLine();
                //        _strBroadcasterName = Properties.Settings.Default.broadcaster;
                //    }

                //    if (string.IsNullOrWhiteSpace(twitchOAuth))
                //    {
                //        Console.WriteLine("Enter your Twitch OAuth: ");
                //        Properties.Settings.Default.twitchOAuth = Console.ReadLine();
                //        twitchOAuth = Properties.Settings.Default.twitchOAuth;
                //    }

                //    if (string.IsNullOrWhiteSpace(twitchAccessToken))
                //    {
                //        Console.WriteLine("Enter your Twitch Access Token: ");
                //        Properties.Settings.Default.twitchAccessToken = Console.ReadLine();
                //        twitchAccessToken = Properties.Settings.Default.twitchAccessToken;
                //    }

                //    Properties.Settings.Default.Save();
                //    Console.WriteLine("Saved Settings!");
                //    Console.WriteLine();
                //}

                //// Option to edit settings before running it
                //Console.WriteLine("Do you want to edit your essential bot settings (y/n or yes/no)?");
                //string strResponse = Console.ReadLine().ToLower();

                //// Check if user inserted a valid option
                //while (!(strResponse.Equals("y") || strResponse.Equals("yes")
                //    || strResponse.Equals("n") || strResponse.Equals("no")))
                //{
                //    Console.WriteLine("Please insert a valid option (y/n or yes/no)");
                //    strResponse = Console.ReadLine().ToLower();
                //}

                //// Change some settings
                //if (strResponse.Equals("y") || strResponse.Equals("yes"))
                //{
                //    int intOption = 0;
                //    Console.WriteLine();

                //    /* Loop until user is finished making changes */
                //    do
                //    {
                //        // Display bot settings
                //        Console.WriteLine("---> Here are your essential bot settings <---");
                //        Console.WriteLine("1. Bot's username: " + Properties.Settings.Default.botName);
                //        Console.WriteLine("2. Your main Twitch username: " + Properties.Settings.Default.broadcaster);
                //        Console.WriteLine("3. Twitch OAuth: " + Properties.Settings.Default.twitchOAuth);
                //        Console.WriteLine("4. Twitch Access Token: " + Properties.Settings.Default.twitchAccessToken);

                //        Console.WriteLine();
                //        Console.WriteLine("From the options 1-4 (or 0 to exit editing), which option do you want to edit?");

                //        // Edit an option
                //        if (int.TryParse(Console.ReadLine(), out intOption) && intOption < 5 && intOption >= 0)
                //        {
                //            Console.WriteLine();
                //            switch (intOption)
                //            {
                //                case 1:
                //                    Console.WriteLine("Enter your bot's new username: ");
                //                    Properties.Settings.Default.botName = Console.ReadLine();
                //                    _strBotName = Properties.Settings.Default.botName;
                //                    break;
                //                case 2:
                //                    Console.WriteLine("Enter your new Twitch username: ");
                //                    Properties.Settings.Default.broadcaster = Console.ReadLine();
                //                    _strBroadcasterName = Properties.Settings.Default.broadcaster;
                //                    break;
                //                case 3:
                //                    Console.WriteLine("Enter your new Twitch OAuth (include 'oauth:' along the 30 character phrase): ");
                //                    Properties.Settings.Default.twitchOAuth = Console.ReadLine();
                //                    twitchOAuth = Properties.Settings.Default.twitchOAuth;
                //                    break;
                //                case 4:
                //                    Console.WriteLine("Enter your new Twitch access token: ");
                //                    Properties.Settings.Default.twitchAccessToken = Console.ReadLine();
                //                    twitchAccessToken = Properties.Settings.Default.twitchAccessToken;
                //                    break;
                //            }

                //            Properties.Settings.Default.Save();
                //            Console.WriteLine("Saved Settings!");
                //            Console.WriteLine();
                //        }
                //        else
                //        {
                //            Console.WriteLine("Please write a valid option between 1-4 (or 0 to exit editing)");
                //            Console.WriteLine();
                //        }
                //    } while (intOption != 0);

                //    Console.WriteLine("Finished with editing settings");
                //}
                //else // No need to change settings
                //    Console.WriteLine("Essential settings confirmed!");

                //Console.WriteLine();

                //// Extra settings menu
                //Console.WriteLine("Do you want to edit your extra bot settings [twitter/discord/currency] (y/n or yes/no)?");
                //strResponse = Console.ReadLine().ToLower();

                //// Check if user inserted a valid option
                //while (!(strResponse.Equals("y") || strResponse.Equals("yes")
                //    || strResponse.Equals("n") || strResponse.Equals("no")))
                //{
                //    Console.WriteLine("Please insert a valid option (y/n or yes/no)");
                //    strResponse = Console.ReadLine().ToLower();
                //}

                //// Change some settings
                //if (strResponse.Equals("y") || strResponse.Equals("yes"))
                //{
                //    int intOption = 0;
                //    Console.WriteLine();

                //    /* Loop until user is finished making changes */
                //    do
                //    {
                //        // Display bot settings
                //        Console.WriteLine("---> Here are your extra bot settings <---");
                //        Console.WriteLine("1. Twitter consumer key: " + Properties.Settings.Default.twitterConsumerKey);
                //        Console.WriteLine("2. Twitter consumer secret: " + Properties.Settings.Default.twitterConsumerSecret);
                //        Console.WriteLine("3. Twitter access token: " + Properties.Settings.Default.twitterAccessToken);
                //        Console.WriteLine("4. Twitter access secret: " + Properties.Settings.Default.twitterAccessSecret);
                //        Console.WriteLine("5. Discord link: " + Properties.Settings.Default.discordLink);
                //        Console.WriteLine("6. Currency type: " + Properties.Settings.Default.currencyType);
                //        Console.WriteLine("7. Enable Auto Tweets: " + Properties.Settings.Default.enableTweet);
                //        Console.WriteLine("8. Enable Auto Display Songs: " + Properties.Settings.Default.enableDisplaySong);
                //        Console.WriteLine("9. Stream Latency: " + Properties.Settings.Default.streamLatency);

                //        Console.WriteLine();
                //        Console.WriteLine("From the options 1-9 (or 0 to exit editing), which option do you want to edit?");

                //        // Edit an option
                //        string strOption = Console.ReadLine();
                //        if (int.TryParse(strOption, out intOption) && intOption < 10 && intOption >= 0)
                //        {
                //            Console.WriteLine();
                //            switch (intOption)
                //            {
                //                case 1:
                //                    Console.WriteLine("Enter your new Twitter consumer key: ");
                //                    Properties.Settings.Default.twitterConsumerKey = Console.ReadLine();
                //                    twitterConsumerKey = Properties.Settings.Default.twitterConsumerKey;
                //                    break;
                //                case 2:
                //                    Console.WriteLine("Enter your new Twitter consumer secret: ");
                //                    Properties.Settings.Default.twitterConsumerSecret = Console.ReadLine();
                //                    twitterConsumerSecret = Properties.Settings.Default.twitterConsumerSecret;
                //                    break;
                //                case 3:
                //                    Console.WriteLine("Enter your new Twitter access token: ");
                //                    Properties.Settings.Default.twitterAccessToken = Console.ReadLine();
                //                    twitterAccessToken = Properties.Settings.Default.twitterAccessToken;
                //                    break;
                //                case 4:
                //                    Console.WriteLine("Enter your new Twitter access secret: ");
                //                    Properties.Settings.Default.twitterAccessSecret = Console.ReadLine();
                //                    twitterAccessSecret = Properties.Settings.Default.twitterAccessSecret;
                //                    break;
                //                case 5:
                //                    Console.WriteLine("Enter your new Discord link: ");
                //                    Properties.Settings.Default.discordLink = Console.ReadLine();
                //                    _strDiscordLink = Properties.Settings.Default.discordLink;
                //                    break;
                //                case 6:
                //                    Console.WriteLine("Enter your new currency type: ");
                //                    Properties.Settings.Default.currencyType = Console.ReadLine();
                //                    _strCurrencyType = Properties.Settings.Default.currencyType;
                //                    break;
                //                case 7:
                //                    Console.WriteLine("Want to enable (true) or disable (false) auto tweets: ");
                //                    Properties.Settings.Default.enableTweet = Convert.ToBoolean(Console.ReadLine());
                //                    _isAutoPublishTweet = Properties.Settings.Default.enableTweet;
                //                    break;
                //                case 8:
                //                    Console.WriteLine("Want to enable (true) or disable (false) display songs from Spotify: ");
                //                    Properties.Settings.Default.enableDisplaySong = Convert.ToBoolean(Console.ReadLine());
                //                    _isAutoDisplaySong = Properties.Settings.Default.enableDisplaySong;
                //                    break;
                //                case 9:
                //                    Console.WriteLine("Enter your new stream latency (in seconds): ");
                //                    Properties.Settings.Default.streamLatency = int.Parse(Console.ReadLine());
                //                    _intStreamLatency = Properties.Settings.Default.streamLatency;
                //                    break;
                //            }

                //            Properties.Settings.Default.Save();
                //            Console.WriteLine("Saved Settings!");
                //            Console.WriteLine();
                //        }
                //        else
                //        {
                //            Console.WriteLine("Please write a valid option between 1-9 (or 0 to exit editing)");
                //            Console.WriteLine();
                //        }
                //    } while (intOption != 0);

                //    Console.WriteLine("Finished with editing settings");
                //}
                //else // No need to change settings
                //    Console.WriteLine("Extra settings confirmed!");

                //Console.WriteLine();

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
                Thread thdIrcClient = new Thread(() => GetChatBox(isSongRequestAvail, twitchAccessToken, hasTwitterInfo));
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
        /// Monitor chat box for commands
        /// </summary>
        /// <param name="isSongRequestAvail"></param>
        /// <param name="twitchAccessToken"></param>
        /// <param name="hasTwitterInfo"></param>
        private static void GetChatBox(bool isSongRequestAvail, string twitchAccessToken, bool hasTwitterInfo)
        {
            try
            {
                /* Master loop */
                while (true)
                {
                    // Read any message inside the chat room
                    string message = _irc.readMessage();
                    Console.WriteLine(message); // Print raw irc message

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
                                /* Display bot settings */
                                if (message.Equals("!botsettings"))
                                    _cmdBrdCstr.CmdBotSettings();

                                /* Stop running the bot */
                                else if (message.Equals("!exitbot"))
                                    _cmdBrdCstr.CmdExitBot();

                                /* Manually connect to Spotify */
                                else if (message.Equals("!spotifyconnect"))
                                    _spotify.Connect();

                                /* Press local Spotify play button [>] */
                                else if (message.Equals("!spotifyplay"))
                                    _spotify.playBtn_Click();

                                /* Press local Spotify pause button [||] */
                                else if (message.Equals("!spotifypause"))
                                    _spotify.pauseBtn_Click();

                                /* Press local Spotify previous button [|<] */
                                else if (message.Equals("!spotifyprev"))
                                    _spotify.prevBtn_Click();

                                /* Press local Spotify next (skip) button [>|] */
                                else if (message.Equals("!spotifynext"))
                                    _spotify.skipBtn_Click();

                                /* Enables tweets to be sent out from this bot (both auto publish tweets and manual tweets) */
                                else if (message.Equals("!sendtweet on"))
                                    _cmdBrdCstr.CmdEnableTweet(hasTwitterInfo);

                                /* Disables tweets from being sent out from this bot */
                                else if (message.Equals("!sendtweet off"))
                                    _cmdBrdCstr.CmdDisableTweet(hasTwitterInfo);

                                /* Enables viewers to request songs (default off) */
                                else if (message.Equals("!srmode on"))
                                    _cmdBrdCstr.CmdEnableSRMode(ref isSongRequestAvail);

                                /* Disables viewers to request songs (default off) */
                                else if (message.Equals("!srmode off"))
                                    _cmdBrdCstr.CmdDisableSRMode(ref isSongRequestAvail);

                                /* Updates the title of the Twitch channel */
                                // Usage: !updatetitle [title]
                                else if (message.StartsWith("!updatetitle "))
                                    _cmdBrdCstr.CmdUpdateTitle(message, twitchAccessToken);

                                /* Updates the game of the Twitch channel */
                                // Usage: !updategame "[game]" (with quotation marks)
                                else if (message.StartsWith("!updategame "))
                                    _cmdBrdCstr.CmdUpdateGame(message, twitchAccessToken, hasTwitterInfo);

                                /* Sends a manual tweet (if credentials have been provided) */
                                // Usage: !tweet "[message]" (use quotation marks)
                                else if (message.StartsWith("!tweet "))
                                    _cmdBrdCstr.CmdTweet(hasTwitterInfo, message);

                                /* Enables songs from local Spotify to be displayed inside the chat */
                                else if (message.Equals("!displaysongs on"))
                                    _cmdBrdCstr.CmdEnableDisplaySongs();

                                /* Disables songs from local Spotify to be displayed inside the chat */
                                else if (message.Equals("!displaysongs off"))
                                    _cmdBrdCstr.CmdDisableDisplaySongs();

                                /* Add viewer to moderator list so they can have access to bot moderator commands */
                                // Usage: !addmod @[username]
                                else if (message.StartsWith("!addmod ") && message.Contains("@"))
                                    _cmdBrdCstr.CmdAddBotMod(message);

                                /* Remove moderator from list so they can't access the bot moderator commands */
                                // Usage: !delmod @[username]
                                else if (message.StartsWith("!delmod ") && message.Contains("@"))
                                    _cmdBrdCstr.CmdDelBotMod(message);

                                /* List bot moderators */
                                else if (message.Equals("!listmod"))
                                    _cmdBrdCstr.CmdListMod();

                                /* Add countdown */
                                // Usage: !addcountdown [MM-DD-YY] [hh:mm:ss] [AM/PM] [message]
                                else if (message.StartsWith("!addcountdown "))
                                    _cmdBrdCstr.CmdAddCountdown(message, strUserName);

                                /* Edit countdown details (for either date and time or message) */
                                // Usage (message): !editcountdownMSG [countdown id] [message]
                                // Usage (date and time): !editcountdownDTE [countdown id] [MM-DD-YY] [hh:mm:ss] [AM/PM]
                                else if (message.StartsWith("!editcountdown"))
                                    _cmdBrdCstr.CmdEditCountdown(message, strUserName);

                                /* List all of the countdowns the broadcaster has set */
                                else if (message.Equals("!listcountdown"))
                                    _cmdBrdCstr.CmdListCountdown(strUserName);

                                /* insert more broadcaster commands here */
                            }

                            /*
                             * Moderator commands (also checks if user has been timed out from using a command)
                             */
                            if (strUserName.Equals(_strBroadcasterName) || _mod.getLstMod().Contains(strUserName.ToLower()))
                            {
                                /* Displays Discord link into chat (if available) */
                                if (message.Equals("!discord") && !isUserTimedout(strUserName))
                                    _cmdMod.CmdDiscord();

                                /* Takes money away from a user */
                                // Usage: !charge [-amount] @[username]
                                else if (message.StartsWith("!charge ") && message.Contains("@") && !isUserTimedout(strUserName))
                                    _cmdMod.CmdCharge(message, strUserName);

                                /* Gives money to user */
                                // Usage: !deposit [amount] @[username]
                                else if (message.StartsWith("!deposit ") && message.Contains("@") && !isUserTimedout(strUserName))
                                    _cmdMod.CmdDeposit(message, strUserName);

                                /* Removes the first song in the queue of song requests */
                                else if (message.Equals("!popsr") && !isUserTimedout(strUserName))
                                    _cmdMod.CmdPopSongRequest();

                                /* Removes first party memeber in queue of party up requests */
                                else if (message.Equals("!poppartyuprequest") && !isUserTimedout(strUserName))
                                    _cmdMod.CmdPopPartyUpRequest();

                                /* Bot-specific timeout on a user for a set amount of time */
                                // Usage: !addtimeout [seconds] @[username]
                                else if (message.StartsWith("!addtimeout ") && message.Contains("@") && !isUserTimedout(strUserName))
                                    _cmdMod.CmdAddTimeout(message, strUserName);

                                /* Remove bot-specific timeout on a user for a set amount of time */
                                // Usage: !deltimeout @[username]
                                else if (message.StartsWith("!deltimeout @") && !isUserTimedout(strUserName))
                                    _cmdMod.CmdDelTimeout(message, strUserName);

                                /* Set delay for messages based on the latency of the stream */
                                // Usage: !setlatency [seconds]
                                else if (message.StartsWith("!setlatency ") && !isUserTimedout(strUserName))
                                    _cmdMod.CmdSetLatency(message, strUserName);

                                /* Add a mod/broadcaster quote */
                                // Usage: !addquote [quote]
                                else if (message.StartsWith("!addquote ") && !isUserTimedout(strUserName))
                                    _cmdMod.CmdAddQuote(message, strUserName);

                                /* insert moderator commands here */
                            }

                            /* 
                             * General commands 
                             */
                            /* Display some viewer commands a link to command documentation */
                            if (message.Equals("!cmds") && !isUserTimedout(strUserName))
                                _cmdGen.CmdCmds();

                            /* Display a static greeting */
                            else if (message.Equals("!hello") && !isUserTimedout(strUserName))
                                _cmdGen.CmdHello(strUserName);

                            /* Display the current time in UTC (Coordinated Universal Time) */
                            else if (message.Equals("!utctime") && !isUserTimedout(strUserName))
                                _cmdGen.CmdUtcTime();

                            /* Display the current time in the time zone the host is located */
                            else if (message.Equals("!hosttime") && !isUserTimedout(strUserName))
                                _cmdGen.CmdHostTime(_strBroadcasterName);

                            /* Shows how long the broadcaster has been streaming */
                            else if (message.Equals("!duration") && !isUserTimedout(strUserName))
                                _cmdGen.CmdDuration();

                            /* Display list of requested songs */
                            else if (message.Equals("!srlist") && !isUserTimedout(strUserName))
                                _cmdGen.CmdListSR();

                            /* Request a song for the host to play */
                            // Usage: !sr [artist] - [song title]
                            else if (message.StartsWith("!sr ") && !isUserTimedout(strUserName))
                                _cmdGen.CmdSR(isSongRequestAvail, message, strUserName);

                            /* Displays the current song being played from Spotify */
                            else if (message.Equals("!spotifycurr") && !isUserTimedout(strUserName))
                                _cmdGen.CmdSpotifyCurr();

                            /* Slaps a user and rates its effectiveness */
                            // Usage: !slap @[username]
                            else if (message.StartsWith("!slap @") && !isUserTimedout(strUserName))
                                _cmdGen.CmdSlap(message, strUserName);

                            /* Stabs a user and rates its effectiveness */
                            // Usage: !stab @[username]
                            else if (message.StartsWith("!stab @") && !isUserTimedout(strUserName))
                                _cmdGen.CmdStab(message, strUserName);

                            /* Shoots a viewer's random body part */
                            // Usage !shoot @[username]
                            else if (message.StartsWith("!shoot @") && !isUserTimedout(strUserName))
                                _cmdGen.CmdShoot(message, strUserName);

                            /* Throws an item at a viewer and rates its effectiveness against the victim */
                            // Usage: !throw [item] @username
                            else if (message.StartsWith("!throw ") && message.Contains("@") && !isUserTimedout(strUserName))
                                _cmdGen.CmdThrow(message, strUserName);

                            /* Request party member if game and character exists in party up system */
                            // Usage: !partyup [party member name]
                            else if (message.StartsWith("!partyup ") && !isUserTimedout(strUserName))
                                _cmdGen.CmdPartyUp(message, strUserName);

                            /* Check what other user's have requested */
                            else if (message.Equals("!partyuprequestlist") && !isUserTimedout(strUserName))
                                _cmdGen.CmdPartyUpRequestList();

                            /* Check what party members are available (if game is part of the party up system) */
                            else if (message.Equals("!partyuplist") && !isUserTimedout(strUserName))
                                _cmdGen.CmdPartyUpList();

                            /* Check user's account balance */
                            else if (message.Equals("!myfunds") && !isUserTimedout(strUserName))
                                _cmdGen.CmdCheckFunds(strUserName);

                            /* Gamble money away */
                            // Usage: !gamble [money]
                            else if (message.StartsWith("!gamble ") && !isUserTimedout(strUserName))
                                _cmdGen.CmdGamble(message, strUserName);

                            /* Display random mod/broadcaster quote */
                            else if (message.Equals("!quote") && !isUserTimedout(strUserName))
                                _cmdGen.CmdQuote();

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
