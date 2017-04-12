using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

using RestSharp;
using Tweetinvi;

using TwitchBot.Configuration;
using TwitchBot.Extensions;
using TwitchBot.Libraries;

namespace TwitchBot.Commands
{
    public class CmdBrdCstr
    {
        private IrcClient _irc;
        private Moderator _modInstance = Moderator.Instance;
        private System.Configuration.Configuration _appConfig;
        private TwitchBotConfigurationSection _botConfig;
        private string _connStr;
        private int _broadcasterId;
        private ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        public CmdBrdCstr(IrcClient irc, TwitchBotConfigurationSection botConfig, string connStr, int broadcasterId, System.Configuration.Configuration appConfig)
        {
            _irc = irc;
            _botConfig = botConfig;
            _connStr = connStr;
            _broadcasterId = broadcasterId;
            _appConfig = appConfig;
        }

        /// <summary>
        /// Display bot settings
        /// </summary>
        public void CmdBotSettings()
        {
            try
            {
                _irc.SendPublicChatMessage("Auto tweets set to \"" + _botConfig.EnableTweets + "\" "
                    + ">< Auto display songs set to \"" + _botConfig.EnableDisplaySong + "\" "
                    + ">< Currency set to \"" + _botConfig.CurrencyType + "\" "
                    + ">< Stream Latency set to \"" + _botConfig.StreamLatency + " second(s)\"");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdBotSettings()", false, "!botsettings");
            }
        }

        /// <summary>
        /// Stop running the bot
        /// </summary>
        public void CmdExitBot()
        {
            try
            {
                _irc.SendPublicChatMessage("Bye! Have a beautiful time!");
                Environment.Exit(0); // exit program
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdExitBot()", false, "!exitbot");
            }
        }

        /// <summary>
        /// Enables tweets to be sent out from this bot (both auto publish tweets and manual tweets)
        /// </summary>
        /// <param name="hasTwitterInfo">Check for Twitter credentials</param>
        public void CmdEnableTweet(bool hasTwitterInfo)
        {
            try
            {
                if (!hasTwitterInfo)
                    _irc.SendPublicChatMessage("You are missing twitter info @" + _botConfig.Broadcaster);
                else
                {
                    _botConfig.EnableTweets = true;
                    _appConfig.Save();

                    Console.WriteLine("Auto publish tweets is set to [" + _botConfig.EnableTweets + "]");
                    _irc.SendPublicChatMessage(_botConfig.Broadcaster + ": Automatic tweets is set to \"" + _botConfig.EnableTweets + "\"");
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdEnableTweet(bool)", false, "!sendtweet on");
            }
        }

        /// <summary>
        /// Disables tweets to be sent out from this bot (both auto publish tweets and manual tweets)
        /// </summary>
        /// <param name="hasTwitterInfo">Check for Twitter credentials</param>
        public void CmdDisableTweet(bool hasTwitterInfo)
        {
            try
            {
                if (!hasTwitterInfo)
                    _irc.SendPublicChatMessage("You are missing twitter info @" + _botConfig.Broadcaster);
                else
                {
                    _botConfig.EnableTweets = false;
                    _appConfig.Save();

                    Console.WriteLine("Auto publish tweets is set to [" + _botConfig.EnableTweets + "]");
                    _irc.SendPublicChatMessage(_botConfig.Broadcaster + ": Automatic tweets is set to \"" + _botConfig.EnableTweets + "\"");
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdDisableTweet(bool)", false, "!sendtweet off");
            }
        }

        /// <summary>
        /// Enable manual song request mode
        /// </summary>
        /// <param name="isSongRequestAvail">Set song request mode</param>
        public void CmdEnableManualSrMode(ref bool isSongRequestAvail)
        {
            try
            {
                isSongRequestAvail = true;
                _irc.SendPublicChatMessage("Song requests enabled");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdEnableManualSrMode(ref bool)", false, "!rbsrmode on");
            }
        }

        /// <summary>
        /// Disable manual song request mode
        /// </summary>
        /// <param name="isSongRequestAvail">Set song request mode</param>
        public void CmdDisableManualSrMode(ref bool isSongRequestAvail)
        {
            try
            {
                isSongRequestAvail = false;
                _irc.SendPublicChatMessage("Song requests disabled");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdDisableSRMode(ref bool)", false, "!rbsrmode off");
            }
        }

        /// <summary>
        /// Enable manual song request mode
        /// </summary>
        /// <param name="isSongRequestAvail">Set song request mode</param>
        public void CmdEnableYouTubeSrMode(ref bool isSongRequestAvail)
        {
            try
            {
                isSongRequestAvail = true;
                _irc.SendPublicChatMessage("YouTube song requests enabled");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdEnableYouTubeSrMode(ref bool)", false, "!ytsrmode on");
            }
        }

        /// <summary>
        /// Disable manual song request mode
        /// </summary>
        /// <param name="isSongRequestAvail">Set song request mode</param>
        public void CmdDisableYouTubeSrMode(ref bool isSongRequestAvail)
        {
            try
            {
                isSongRequestAvail = false;
                _irc.SendPublicChatMessage("YouTube song requests disabled");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdDisableYouTubeSrMode(ref bool)", false, "!ytsrmode off");
            }
        }

        /// <summary>
        /// Update the title of the Twitch channel
        /// </summary>
        /// <param name="message">Chat message from the user</param>
        /// <param name="twitchAccessToken">Token needed to change channel info</param>
        public void CmdUpdateTitle(string message, string twitchAccessToken)
        {
            try
            {
                // Get title from command parameter
                string title = message.Substring(message.IndexOf(" ") + 1);

                // Send HTTP method PUT to base URI in order to change the title
                RestClient client = new RestClient("https://api.twitch.tv/kraken/channels/" + _botConfig.Broadcaster);
                RestRequest request = new RestRequest(Method.PUT);
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
                        _irc.SendPublicChatMessage("Twitch channel title updated to \"" + title +
                            "\" >< Refresh your browser [F5] or twitch app to see the change");
                    }
                    else
                        Console.WriteLine(response.ErrorMessage);
                }
                catch (WebException ex)
                {
                    if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.BadRequest)
                    {
                        Console.WriteLine("Error 400 detected!");
                    }
                    response = (IRestResponse)ex.Response;
                    Console.WriteLine("Error: " + response);
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdUpdateTitle(string, string)", false, "!updatetitle");
            }
        }

        /// <summary>
        /// Updates the game being played on the Twitch channel
        /// </summary>
        /// <param name="message">Chat message from the user</param>
        /// <param name="twitchAccessToken">Token needed to change channel info</param>
        /// <param name="hasTwitterInfo">Check for Twitter credentials</param>
        public void CmdUpdateGame(string message, string twitchAccessToken, bool hasTwitterInfo)
        {
            try
            {
                // Get game from command parameter
                string game = message.Substring(message.IndexOf(" ") + 1);

                // Send HTTP method PUT to base URI in order to change the game
                RestClient client = new RestClient("https://api.twitch.tv/kraken/channels/" + _botConfig.Broadcaster);
                RestRequest request = new RestRequest(Method.PUT);
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
                        _irc.SendPublicChatMessage("Twitch channel game status updated to \"" + game +
                            "\" >< Restart your connection to the stream or twitch app to see the change");
                        if (_botConfig.EnableTweets && hasTwitterInfo)
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
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdUpdateGame(string, string, bool)", false, "!updategame");
            }
        }

        /// <summary>
        /// Manually send a tweet
        /// </summary>
        /// <param name="bolHasTwitterInfo">Check if user has provided the specific twitter credentials</param>
        /// <param name="message">Chat message from the user</param>
        public void CmdTweet(bool bolHasTwitterInfo, string message)
        {
            try
            {
                if (!bolHasTwitterInfo)
                    _irc.SendPublicChatMessage("You are missing twitter info @" + _botConfig.Broadcaster);
                else
                {
                    string command = message;
                    SendTweet(message, command);
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdTweet(bool, string)", false, "!tweet");
            }
        }

        /// <summary>
        /// Enables displaying songs from Spotify into the IRC chat
        /// </summary>
        public void CmdEnableDisplaySongs()
        {
            try
            {
                _botConfig.EnableDisplaySong = true;
                _appConfig.Save();

                Console.WriteLine("Auto display songs is set to [" + _botConfig.EnableDisplaySong + "]");
                _irc.SendPublicChatMessage(_botConfig.Broadcaster + ": Automatic display Spotify songs is set to \"" + _botConfig.EnableDisplaySong + "\"");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdEnableDisplaySongs()", false, "!displaysongs on");
            }
        }

        /// <summary>
        /// Disables displaying songs from Spotify into the IRC chat
        /// </summary>
        public void CmdDisableDisplaySongs()
        {
            try
            {
                _botConfig.EnableDisplaySong = false;
                _appConfig.Save();

                Console.WriteLine("Auto display songs is set to [" + _botConfig.EnableDisplaySong + "]");
                _irc.SendPublicChatMessage(_botConfig.Broadcaster + ": Automatic display Spotify songs is set to \"" + _botConfig.EnableDisplaySong + "\"");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdDisableDisplaySongs()", false, "!displaysongs off");
            }
        }

        /// <summary>
        /// Grant viewer to moderator status for this bot's mod commands
        /// </summary>
        /// <param name="message">Chat message from the user</param>
        public void CmdAddBotMod(string message)
        {
            try
            {
                string recipient = message.Substring(message.IndexOf("@") + 1); // grab user from message
                _modInstance.AddNewModToList(recipient.ToLower(), _broadcasterId, _connStr); // add user to mod list and add to db
                _irc.SendPublicChatMessage("@" + recipient + " is now able to use moderator features within " + _botConfig.BotName);
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdAddBotMod(string)", false, "!addmod");
            }
        }

        /// <summary>
        /// Revoke moderator status from user for this bot's mods commands
        /// </summary>
        /// <param name="message">Chat message from the user</param>
        public void CmdDelBotMod(string message)
        {
            try
            {
                string recipient = message.Substring(message.IndexOf("@") + 1); // grab user from message
                _modInstance.DeleteOldModFromList(recipient.ToLower(), _broadcasterId, _connStr); // delete user from mod list and remove from db
                _irc.SendPublicChatMessage("@" + recipient + " is not able to use moderator features within " + _botConfig.BotName + " any longer");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdDelBotMod(string)", false, "!delmod");
            }
        }

        /// <summary>
        /// List bot moderators
        /// </summary>
        public void CmdListMod()
        {
            try
            {
                string listModMsg = "";

                if (_modInstance.ListMods.Count > 0)
                {
                    foreach (string name in _modInstance.ListMods)
                        listModMsg += name + " >< ";

                    listModMsg = listModMsg.Remove(listModMsg.Length - 3); // removed extra " >< "
                    _irc.SendPublicChatMessage("List of bot moderators (separate from channel mods): " + listModMsg);
                }
                else
                    _irc.SendPublicChatMessage("No one is ruling over me other than you @" + _botConfig.Broadcaster);
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdListMod()", false, "!listmod");
            }
        }

        /// <summary>
        /// Add a custom countdown for a user to post in the chat
        /// </summary>
        /// <param name="message">Chat message from the user</param>
        /// <param name="username">User that sent the message</param>
        public void CmdAddCountdown(string message, string username)
        {
            try
            {
                // get due date of countdown
                string countdownInput = message.Substring(14, 20); // MM-DD-YY hh:mm:ss [AM/PM]
                DateTime countdownDuration = Convert.ToDateTime(countdownInput);

                // get message of countdown
                string countdownMsg = message.Substring(34);

                // log new countdown into db
                string query = "INSERT INTO tblCountdown (dueDate, message, broadcaster) VALUES (@dueDate, @message, @broadcaster)";

                using (SqlConnection conn = new SqlConnection(_connStr))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@dueDate", SqlDbType.DateTime).Value = countdownDuration;
                    cmd.Parameters.Add("@message", SqlDbType.VarChar, 50).Value = countdownMsg;
                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _broadcasterId;

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                Console.WriteLine("Countdown added!");
                _irc.SendPublicChatMessage($"Countdown added @{username}");
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdAddCountdown(string, string)", false, "!addcountdown");
            }
        }

        /// <summary>
        /// Edit countdown details (for either date and time or message)
        /// </summary>
        /// <param name="message">Chat message from the user</param>
        /// <param name="username">User that sent the message</param>
        public void CmdEditCountdown(string message, string username)
        {
            try
            {
                int reqCountdownId = -1;
                string msgCountdownId = message.Substring(18, message.GetNthCharIndex(' ', 2) - message.GetNthCharIndex(' ', 1) - 1);
                bool isValidCountdownId = int.TryParse(msgCountdownId, out reqCountdownId);

                // validate requested countdown ID
                if (!isValidCountdownId || reqCountdownId < 0)
                    _irc.SendPublicChatMessage("Please use a positive whole number to find your countdown ID");
                else
                {
                    // check if countdown ID exists
                    int responseCountdownId = -1;
                    using (SqlConnection conn = new SqlConnection(_connStr))
                    {
                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand("SELECT id, broadcaster FROM tblCountdown "
                            + "WHERE broadcaster = @broadcaster", conn))
                        {
                            cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _broadcasterId;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        if (reqCountdownId.ToString().Equals(reader["id"].ToString()))
                                        {
                                            responseCountdownId = int.Parse(reader["id"].ToString());
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // check if countdown ID was retrieved
                    if (responseCountdownId == -1)
                        _irc.SendPublicChatMessage($"Cannot find the countdown ID: {reqCountdownId}");
                    else
                    {
                        int inputType = -1; // check if input is in the correct format
                        DateTime countdownDuration = new DateTime();
                        string countdownInput = message.Substring(message.GetNthCharIndex(' ', 2) + 1);

                        /* Check if user wants to edit the date and time or message */
                        if (message.StartsWith("!editcountdownDTE"))
                        {
                            // get new due date of countdown
                            bool hasValidCountdownDuration = DateTime.TryParse(countdownInput, out countdownDuration);

                            if (!hasValidCountdownDuration)
                                _irc.SendPublicChatMessage("Please enter a valid date and time @" + username);
                            else
                                inputType = 1;
                        }
                        else if (message.StartsWith("!editcountdownMSG"))
                        {
                            // get new message of countdown
                            if (string.IsNullOrWhiteSpace(countdownInput))
                                _irc.SendPublicChatMessage("Please enter a valid message @" + username);
                            else
                                inputType = 2;
                        }

                        // if input is correct update db
                        if (inputType > 0)
                        {
                            string strQuery = "";

                            if (inputType == 1)
                                strQuery = "UPDATE dbo.tblCountdown SET dueDate = @dueDate WHERE (Id = @id AND broadcaster = @broadcaster)";
                            else if (inputType == 2)
                                strQuery = "UPDATE dbo.tblCountdown SET message = @message WHERE (Id = @id AND broadcaster = @broadcaster)";

                            using (SqlConnection conn = new SqlConnection(_connStr))
                            using (SqlCommand cmd = new SqlCommand(strQuery, conn))
                            {
                                // append proper parameter
                                if (inputType == 1)
                                    cmd.Parameters.Add("@dueDate", SqlDbType.DateTime).Value = countdownDuration;
                                else if (inputType == 2)
                                    cmd.Parameters.Add("@message", SqlDbType.VarChar, 50).Value = countdownInput;

                                cmd.Parameters.Add("@id", SqlDbType.Int).Value = responseCountdownId;
                                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _broadcasterId;

                                conn.Open();
                                cmd.ExecuteNonQuery();
                            }

                            Console.WriteLine($"Changes to countdown ID: {reqCountdownId} have been made @{username}");
                            _irc.SendPublicChatMessage($"Changes to countdown ID: {reqCountdownId} have been made @{username}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdEditCountdown(string, string)", false, "!editcountdown");
            }
        }

        /// <summary>
        /// List all of the countdowns the broadcaster has set
        /// </summary>
        /// <param name="username">User that sent the message</param>
        public void CmdListCountdown(string username)
        {
            try
            {
                string countdownListMsg = "";

                using (SqlConnection conn = new SqlConnection(_connStr))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT Id, dueDate, message, broadcaster FROM tblCountdown "
                        + "WHERE broadcaster = @broadcaster ORDER BY Id", conn))
                    {
                        cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _broadcasterId;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    countdownListMsg += "ID: " + reader["Id"].ToString()
                                        + " Message: \"" + reader["message"].ToString()
                                        + "\" Time: \"" + reader["dueDate"].ToString()
                                        + "\" // ";
                                }
                                StringBuilder modCountdownListMsg = new StringBuilder(countdownListMsg);
                                modCountdownListMsg.Remove(countdownListMsg.Length - 4, 4); // remove extra " >< "
                                countdownListMsg = modCountdownListMsg.ToString(); // replace old countdown list string with new
                                _irc.SendPublicChatMessage(countdownListMsg);
                            }
                            else
                            {
                                Console.WriteLine("No countdown messages are set at the moment");
                                _irc.SendPublicChatMessage("No countdown messages are set at the moment @" + username);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdListCountdown(string)", false, "!listcountdown");
            }
        }

        /// <summary>
        /// Add a giveaway for a user to post in the chat
        /// </summary>
        /// <param name="message">Chat message from the user</param>
        /// <param name="username">User that sent the message</param>
        public void CmdAddGiveaway(string message, string username)
        {
            int giveawayType = -1;
            DateTime giveawayDate;
            string minRandNum = "-1";
            string maxRandNum = "0";
            bool isValidated = true; // used for nested "if" validation

            try
            {
                // get due date of giveaway
                int dueDateMsgIndex = message.GetNthCharIndex(' ', 4);
                if (dueDateMsgIndex > 0)
                {
                    string giveawayDateMsg = message.Substring(13, dueDateMsgIndex - 13); // MM-DD-YY hh:mm:ss [AM/PM]
                    if (DateTime.TryParse(giveawayDateMsg, out giveawayDate))
                    {
                        // get eligibility parameters for user types (using boolean bits)
                        int elgMsgIndex = -1; // get the index of the space separating the message and the parameter
                        for (int i = 5; i < 8; i++)
                        {
                            elgMsgIndex = message.GetNthCharIndex(' ', i);
                            if (elgMsgIndex == -1)
                                break;
                        }

                        if (elgMsgIndex > 0)
                        {
                            string giveawayElgMsg = message.Substring(message.GetNthCharIndex(' ', 4) + 1, 7); // [mods] [regulars] [subscribers] [users]
                            if (giveawayElgMsg.Replace(" ", "").IsInt()
                                && giveawayElgMsg.Replace(" ", "").Length == 4
                                && !Regex.IsMatch(giveawayElgMsg, @"[2-9]"))
                            {
                                int[] elgList =
                                {
                                    int.Parse(giveawayElgMsg.Substring(0, 1)),
                                    int.Parse(giveawayElgMsg.Substring(2, 1)),
                                    int.Parse(giveawayElgMsg.Substring(4, 1)),
                                    int.Parse(giveawayElgMsg.Substring(6, 1))
                                };

                                // get giveaway type (1 = Keyword, 2 = Random Number)
                                if (int.TryParse(message.Substring(message.GetNthCharIndex(' ', 8) + 1, 1), out giveawayType))
                                {
                                    // get parameter of new giveaway (1 = [keyword], 2 = [min]-[max])
                                    int paramMsgIndex = message.GetNthCharIndex(' ', 10); // get the index of the space separating the message and the parameter
                                    if (paramMsgIndex > 0)
                                    {
                                        string giveawayParam = message.Substring(44, paramMsgIndex - 44);

                                        if (giveawayType == 2)
                                        {
                                            // check if min-max range can be validated
                                            if (Regex.IsMatch(giveawayParam, @"^\d{1,4}-\d{1,4}"))
                                            {
                                                int dashIndex = giveawayParam.IndexOf('-');

                                                minRandNum = giveawayParam.Substring(0, dashIndex); // min
                                                maxRandNum = giveawayParam.Substring(dashIndex + 1); // max

                                                if (int.Parse(minRandNum) > int.Parse(maxRandNum))
                                                    isValidated = false;
                                            }
                                            else
                                            {
                                                isValidated = false;
                                            }
                                        }

                                        if (isValidated)
                                        {
                                            // get message of new giveaway
                                            string giveawayText = message.Substring(paramMsgIndex + 1);

                                            // log new giveaway into db
                                            string query = "INSERT INTO tblGiveaway (dueDate, message, broadcaster, elgMod, elgReg, elgSub, elgUsr, giveType, giveParam1, giveParam2) " +
                                                            "VALUES (@dueDate, @message, @broadcaster, @elgMod, @elgReg, @elgSub, @elgUsr, @giveType, @giveParam1, @giveParam2)";

                                            using (SqlConnection conn = new SqlConnection(_connStr))
                                            using (SqlCommand cmd = new SqlCommand(query, conn))
                                            {
                                                cmd.Parameters.Add("@dueDate", SqlDbType.DateTime).Value = giveawayDate;
                                                cmd.Parameters.Add("@message", SqlDbType.VarChar, 75).Value = giveawayText;
                                                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _broadcasterId;
                                                cmd.Parameters.Add("@elgMod", SqlDbType.Bit).Value = elgList[0];
                                                cmd.Parameters.Add("@elgReg", SqlDbType.Bit).Value = elgList[1];
                                                cmd.Parameters.Add("@elgSub", SqlDbType.Bit).Value = elgList[2];
                                                cmd.Parameters.Add("@elgUsr", SqlDbType.Bit).Value = elgList[3];
                                                cmd.Parameters.Add("@giveType", SqlDbType.Int).Value = giveawayType;

                                                if (giveawayType == 1) // keyword
                                                {
                                                    cmd.Parameters.Add("@giveParam1", SqlDbType.VarChar, 50).Value = giveawayParam;
                                                    cmd.Parameters.Add("@giveParam2", SqlDbType.VarChar, 50).Value = DBNull.Value;
                                                }
                                                else if (giveawayType == 2) // random number
                                                {
                                                    cmd.Parameters.Add("@giveParam1", SqlDbType.VarChar, 50).Value = minRandNum;
                                                    cmd.Parameters.Add("@giveParam2", SqlDbType.VarChar, 50).Value = maxRandNum;
                                                }

                                                conn.Open();
                                                cmd.ExecuteNonQuery();
                                            }

                                            Console.WriteLine("Giveaway started!");
                                            _irc.SendPublicChatMessage($"Giveaway \"{giveawayText}\" has started @{username}");
                                        }
                                        else
                                        {
                                            if (int.Parse(minRandNum) > int.Parse(maxRandNum))
                                                _irc.SendPublicChatMessage($"Giveaway random number parameter values were flipped @{username}");
                                            else
                                                _irc.SendPublicChatMessage($"Giveaway random number parameter [min-max] was not given correctly @{username}");
                                        }
                                    }
                                    else
                                    {
                                        if (giveawayType == 1)
                                            _irc.SendPublicChatMessage($"Giveaway keyword parameter was not given @{username}");
                                        else if (giveawayType == 2)
                                            _irc.SendPublicChatMessage($"Giveaway random number parameter was not given @{username}");
                                    }
                                }
                                else
                                {
                                    if (giveawayType == -1)
                                        _irc.SendPublicChatMessage($"Giveaway type was not given correctly @{username}");
                                    else if (giveawayType != 1 || giveawayType != 2)
                                        _irc.SendPublicChatMessage($"Please use giveaway type (1 = Keyword or 2 = Random Number) @{username}");
                                }
                            }
                            else
                            {
                                _irc.SendPublicChatMessage($"Eligibility parameters were not given correctly @{username}");
                            }
                        }
                        else
                        {
                            _irc.SendPublicChatMessage($"Couldn't find space between eligibility parameters @{username}");
                        }
                    }
                    else
                    {
                        _irc.SendPublicChatMessage($"Date and time parameters were not given correctly @{username}");
                    }
                }
                else
                {
                    _irc.SendPublicChatMessage($"Couldn't find eligibility parameters @{username}");
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdAddGiveaway(string, string)", false, "!addgiveaway");
            }
        }

        /// <summary>
        /// Edit giveaway details (date/time, message, giveaway type, or eligibility)
        /// </summary>
        /// <param name="message">Chat message from the user</param>
        /// <param name="username">User that sent the message</param>
        public void CmdEditGiveaway(string message, string username)
        {
            try
            {
                int reqGiveawayId = -1;
                string reqGiveawayIdMsg = message.Substring(18, message.GetNthCharIndex(' ', 2) - message.GetNthCharIndex(' ', 1) - 1);
                bool isValidGiveawayId = int.TryParse(reqGiveawayIdMsg, out reqGiveawayId);

                // validate requested giveaway ID
                if (!isValidGiveawayId || reqGiveawayId < 0)
                    _irc.SendPublicChatMessage("Please use a positive whole number to find your giveaway ID");
                else
                {
                    // check if giveaway ID exists
                    int giveawayId = -1;
                    using (SqlConnection conn = new SqlConnection(_connStr))
                    {
                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand("SELECT id, broadcaster FROM tblGiveaway "
                            + "WHERE broadcaster = @broadcaster", conn))
                        {
                            cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _broadcasterId;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        if (reqGiveawayId.ToString().Equals(reader["id"].ToString()))
                                        {
                                            giveawayId = int.Parse(reader["id"].ToString());
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // check if giveaway ID was retrieved
                    if (giveawayId == -1)
                        _irc.SendPublicChatMessage($"Cannot find the giveaway ID: {reqGiveawayId}");
                    else
                    {
                        int inputType = -1; // check if input is in the correct format
                        DateTime giveawayDate = new DateTime();
                        int[] elgList = { };

                        string giveawayInput = message.Substring(message.GetNthCharIndex(' ', 2) + 1);

                        /* Check if user wants to edit the date/time, message, giveaway type, or eligibility */
                        if (message.StartsWith("!editgiveawayDTE"))
                        {
                            // get new due date of giveaway
                            if (!DateTime.TryParse(giveawayInput, out giveawayDate))
                                _irc.SendPublicChatMessage("Please enter a valid date and time @" + username);
                            else
                                inputType = 1;
                        }
                        else if (message.StartsWith("!editgiveawayMSG"))
                        {
                            // get new message for giveaway
                            if (string.IsNullOrWhiteSpace(giveawayInput))
                                _irc.SendPublicChatMessage("Please enter a valid message @" + username);
                            else
                                inputType = 2;
                        }
                        else if (message.StartsWith("!editgiveawayELG"))
                        {
                            // ToDo: Test edit eligibility edit
                            // get new eligibility list for giveaway
                            string giveawayElg = message.Substring(message.GetNthCharIndex(' ', 1) + 1, 7); // [mods] [regulars] [subscribers] [users]
                            if (giveawayElg.Replace(" ", "").IsInt()
                                && giveawayElg.Replace(" ", "").Length == 4
                                && !Regex.IsMatch(giveawayElg, @"[2-9]"))
                            {
                                elgList = new int[] 
                                {
                                    int.Parse(giveawayElg.Substring(0, 1)),
                                    int.Parse(giveawayElg.Substring(2, 1)),
                                    int.Parse(giveawayElg.Substring(4, 1)),
                                    int.Parse(giveawayElg.Substring(6, 1))
                                };

                                inputType = 3;
                            }
                            else
                            {
                                _irc.SendPublicChatMessage("Please enter a valid message @" + username);
                            }
                        }
                        else if (message.StartsWith("!editgiveawayTYP"))
                        {
                            // ToDo: Implement giveaway type/param(s) change
                            // get new giveaway type for giveaway
                            if (string.IsNullOrWhiteSpace(giveawayInput))
                                _irc.SendPublicChatMessage("Please enter a valid message @" + username);
                            else
                                inputType = 4;
                        }

                        // update info based on input type
                        // ToDo: Implement database updates for giveway type/param(s) and eligibility changes
                        if (inputType > 0)
                        {
                            string strQuery = "";

                            if (inputType == 1)
                                strQuery = "UPDATE dbo.tblGiveaway SET dueDate = @dueDate WHERE (Id = @id AND broadcaster = @broadcaster)";
                            else if (inputType == 2)
                                strQuery = "UPDATE dbo.tblGiveaway SET message = @message WHERE (Id = @id AND broadcaster = @broadcaster)";

                            using (SqlConnection conn = new SqlConnection(_connStr))
                            using (SqlCommand cmd = new SqlCommand(strQuery, conn))
                            {
                                // append proper parameter(s)
                                if (inputType == 1)
                                    cmd.Parameters.Add("@dueDate", SqlDbType.DateTime).Value = giveawayDate;
                                else if (inputType == 2)
                                    cmd.Parameters.Add("@message", SqlDbType.VarChar, 50).Value = giveawayInput;

                                cmd.Parameters.Add("@id", SqlDbType.Int).Value = giveawayId;
                                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = _broadcasterId;

                                conn.Open();
                                cmd.ExecuteNonQuery();
                            }

                            Console.WriteLine($"Changes to giveaway ID: {reqGiveawayId} have been made @{username}");
                            _irc.SendPublicChatMessage($"Changes to giveaway ID: {reqGiveawayId} have been made @{username}");
                        }
                        else
                        {
                            _irc.SendPublicChatMessage($"Please specify an option to edit a giveaway @{username}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _errHndlrInstance.LogError(ex, "CmdBrdCstr", "CmdEditCountdown(string, string)", false, "!editcountdown");
            }
        }

        public void SendTweet(string pendingMessage, string command)
        {
            // Check if there are at least two quotation marks before sending message using LINQ
            string resultMessage = "";
            if (command.Count(c => c == '"') < 2)
            {
                resultMessage = "Please use at least two quotation marks (\") before sending a tweet. " +
                    "Quotations are used to find the start and end of a message wanting to be sent";
                Console.WriteLine(resultMessage);
                _irc.SendPublicChatMessage(resultMessage);
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
                _irc.SendPublicChatMessage(resultMessage);
            }
            else
            {
                int overCharLimit = tweetMessage.Length - 140;
                resultMessage = "The message you attempted to tweet had " + overCharLimit +
                    " characters more than the 140 character limit. Please shorten your message and try again";
                Console.WriteLine(resultMessage);
                _irc.SendPublicChatMessage(resultMessage);
            }
        }
    }
}
