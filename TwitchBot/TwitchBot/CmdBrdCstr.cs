using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot
{
    public class CmdBrdCstr
    {
        /// <summary>
        /// Display bot settings
        /// </summary>
        public void CmdBotSettings()
        {
            try
            {
                Program._irc.sendPublicChatMessage("Auto tweets set to \"" + Program._isAutoPublishTweet + "\" "
                    + "|| Auto display songs set to \"" + Program._isAutoDisplaySong + "\" "
                    + "|| Currency set to \"" + Program._strCurrencyType + "\"");
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "CmdBrdCstr", "CmdBotSettings()", false, "!botsettings");
            }
        }

        /// <summary>
        /// Stop running the bot
        /// </summary>
        public void CmdExitBot()
        {
            try
            {
                Program._irc.sendPublicChatMessage("Bye! Have a beautiful time!");
                Environment.Exit(0); // exit program
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "CmdBrdCstr", "CmdExitBot()", false, "!exitbot");
            }
        }

        /// <summary>
        /// Enables tweets to be sent out from this bot (both auto publish tweets and manual tweets)
        /// </summary>
        /// <param name="bolHasTwitterInfo">Check for Twitter credentials</param>
        public void CmdEnableTweet(bool bolHasTwitterInfo)
        {
            try
            {
                if (!bolHasTwitterInfo)
                    Program._irc.sendPublicChatMessage("You are missing twitter info @" + Program._strBroadcasterName);
                else
                {
                    Program._isAutoPublishTweet = true;
                    Properties.Settings.Default.enableTweet = Program._isAutoPublishTweet;
                    Properties.Settings.Default.Save();

                    Console.WriteLine("Auto publish tweets is set to [" + Program._isAutoPublishTweet + "]");
                    Program._irc.sendPublicChatMessage(Program._strBroadcasterName + ": Automatic tweets is set to \"" + Program._isAutoPublishTweet + "\"");
                }
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "CmdBrdCstr", "CmdEnableTweet(bool)", false, "!sendtweet on");
            }
        }

        /// <summary>
        /// Disables tweets to be sent out from this bot (both auto publish tweets and manual tweets)
        /// </summary>
        /// <param name="bolHasTwitterInfo">Check for Twitter credentials</param>
        public void CmdDisableTweet(bool bolHasTwitterInfo)
        {
            try
            {
                if (!bolHasTwitterInfo)
                    Program._irc.sendPublicChatMessage("You are missing twitter info @" + Program._strBroadcasterName);
                else
                {
                    Program._isAutoPublishTweet = false;
                    Properties.Settings.Default.enableTweet = Program._isAutoPublishTweet;
                    Properties.Settings.Default.Save();

                    Console.WriteLine("Auto publish tweets is set to [" + Program._isAutoPublishTweet + "]");
                    Program._irc.sendPublicChatMessage(Program._strBroadcasterName + ": Automatic tweets is set to \"" + Program._isAutoPublishTweet + "\"");
                }
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "CmdBrdCstr", "CmdDisableTweet(bool)", false, "!sendtweet off");
            }
        }

        /// <summary>
        /// Enable song request mode
        /// </summary>
        /// <param name="isSongRequestAvail">Set song request mode</param>
        public void CmdEnableSRMode(ref bool isSongRequestAvail)
        {
            try
            {
                isSongRequestAvail = true;
                Program._irc.sendPublicChatMessage("Song requests enabled");
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "CmdBrdCstr", "CmdEnableSRMode(ref bool)", false, "!srmode on");
            }
        }

        /// <summary>
        /// Disable song request mode
        /// </summary>
        /// <param name="isSongRequestAvail">Set song request mode</param>
        public void CmdDisableSRMode(ref bool isSongRequestAvail)
        {
            try
            {
                isSongRequestAvail = false;
                Program._irc.sendPublicChatMessage("Song requests disabled");
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "CmdBrdCstr", "CmdDisableSRMode(ref bool)", false, "!srmode off");
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
                RestClient client = new RestClient("https://api.twitch.tv/kraken/channels/" + Program._strBroadcasterName);
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
                        Program._irc.sendPublicChatMessage("Twitch channel title updated to \"" + title +
                            "\" || Refresh your browser [F5] or twitch app to see the change");
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
                Program.LogError(ex, "CmdBrdCstr", "CmdUpdateTitle(string, string, string)", false, "!updatetitle");
            }
        }

        /// <summary>
        /// Updates the game being played on the Twitch channel
        /// </summary>
        /// <param name="message">Chat message from the user</param>
        /// <param name="twitchAccessToken">Token needed to change channel info</param>
        /// <param name="bolHasTwitterInfo">Check for Twitter credentials</param>
        public void CmdUpdateGame(string message, string twitchAccessToken, bool bolHasTwitterInfo)
        {
            try
            {
                // Get game from command parameter
                string game = message.Substring(message.IndexOf(" ") + 1);

                // Send HTTP method PUT to base URI in order to change the game
                RestClient client = new RestClient("https://api.twitch.tv/kraken/channels/" + Program._strBroadcasterName);
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
                        Program._irc.sendPublicChatMessage("Twitch channel game status updated to \"" + game +
                            "\" || Restart your connection to the stream or twitch app to see the change");
                        if (Program._isAutoPublishTweet && bolHasTwitterInfo)
                        {
                            Program.SendTweet("Watch me stream " + game + " on Twitch" + Environment.NewLine
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
                Program.LogError(ex, "CmdBrdCstr", "CmdUpdateGame(string, string, string, bool, bool)", false, "!updategame");
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
                    Program._irc.sendPublicChatMessage("You are missing twitter info @" + Program._strBroadcasterName);
                else
                {
                    string command = message;
                    Program.SendTweet(message, command);
                }
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "CmdBrdCstr", "CmdTweet(bool, string, string)", false, "!tweet");
            }
        }

        /// <summary>
        /// Enables displaying songs from Spotify into the IRC chat
        /// </summary>
        public void CmdEnableDisplaySongs()
        {
            try
            {
                Program._isAutoDisplaySong = true;
                Properties.Settings.Default.enableDisplaySong = Program._isAutoDisplaySong;
                Properties.Settings.Default.Save();

                Console.WriteLine("Auto display songs is set to [" + Program._isAutoDisplaySong + "]");
                Program._irc.sendPublicChatMessage(Program._strBroadcasterName + ": Automatic display songs is set to \"" + Program._isAutoDisplaySong + "\"");
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "CmdBrdCstr", "CmdEnableDisplaySongs()", false, "!displaysongs on");
            }
        }

        /// <summary>
        /// Disables displaying songs from Spotify into the IRC chat
        /// </summary>
        public void CmdDisableDisplaySongs()
        {
            try
            {
                Program._isAutoDisplaySong = false;
                Properties.Settings.Default.enableDisplaySong = Program._isAutoDisplaySong;
                Properties.Settings.Default.Save();

                Console.WriteLine("Auto display songs is set to [" + Program._isAutoDisplaySong + "]");
                Program._irc.sendPublicChatMessage(Program._strBroadcasterName + ": Automatic display songs is set to \"" + Program._isAutoDisplaySong + "\"");
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "CmdBrdCstr", "CmdDisableDisplaySongs()", false, "!displaysongs off");
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
                string strRecipient = message.Substring(message.IndexOf("@") + 1); // grab user from message
                Program._mod.addNewModToLst(strRecipient.ToLower(), Program._intBroadcasterID, Program._connStr); // add user to mod list and add to db
                Program._irc.sendPublicChatMessage("@" + strRecipient + " is now able to use moderator features within " + Program._strBotName);
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "CmdBrdCstr", "CmdAddBotMod(string)", false, "!addmod");
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
                string strRecipient = message.Substring(message.IndexOf("@") + 1); // grab user from message
                Program._mod.delOldModFromLst(strRecipient.ToLower(), Program._intBroadcasterID, Program._connStr); // delete user from mod list and remove from db
                Program._irc.sendPublicChatMessage("@" + strRecipient + " is not able to use moderator features within " + Program._strBotName + " any longer");
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "CmdBrdCstr", "CmdDelBotMod(string)", false, "!delmod");
            }
        }

        /// <summary>
        /// List bot moderators
        /// </summary>
        public void CmdListMod()
        {
            try
            {
                string strListModMsg = "";

                if (Program._mod.getLstMod().Count > 0)
                {
                    foreach (string name in Program._mod.getLstMod())
                        strListModMsg += name + " | ";

                    strListModMsg = strListModMsg.Remove(strListModMsg.Length - 3); // removed extra " | "
                    Program._irc.sendPublicChatMessage("List of bot moderators: " + strListModMsg);
                }
                else
                    Program._irc.sendPublicChatMessage("No one is ruling over me other than you @" + Program._strBroadcasterName);
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "CmdBrdCstr", "CmdListMod()", false, "!listmod");
            }
        }

        /// <summary>
        /// Add a custom countdown for a user to post in the chat
        /// </summary>
        /// <param name="message">Chat message from the user</param>
        /// <param name="strUserName">User that sent the message</param>
        public void CmdAddCountdown(string message, string strUserName)
        {
            try
            {
                // get due date of countdown
                string strCountdownDT = message.Substring(14, 20); // MM-DD-YY hh:mm:ss [AM/PM]
                DateTime dtCountdown = Convert.ToDateTime(strCountdownDT);

                // get message of countdown
                string strCountdownMsg = message.Substring(34);

                // log new countdown into db
                string query = "INSERT INTO tblCountdown (dueDate, message, broadcaster) VALUES (@dueDate, @message, @broadcaster)";

                using (SqlConnection conn = new SqlConnection(Program._connStr))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@dueDate", SqlDbType.DateTime).Value = dtCountdown;
                    cmd.Parameters.Add("@message", SqlDbType.VarChar, 50).Value = strCountdownMsg;
                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = Program._intBroadcasterID;

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                Console.WriteLine("Countdown added!");
                Program._irc.sendPublicChatMessage($"Countdown added @{strUserName}");
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "CmdBrdCstr", "CmdAddCountdown(string, string)", false, "!addcountdown");
            }
        }

        /// <summary>
        /// Edit countdown details (for either date and time or message)
        /// </summary>
        /// <param name="message">Chat message from the user</param>
        /// <param name="strUserName">User that sent the message</param>
        public void CmdEditCountdown(string message, string strUserName)
        {
            try
            {
                int intReqCountdownID = -1;
                string strReqCountdownID = message.Substring(18, Program.GetNthIndex(message, ' ', 2) - Program.GetNthIndex(message, ' ', 1) - 1);
                bool bolValidCountdownID = int.TryParse(strReqCountdownID, out intReqCountdownID);

                // validate requested countdown ID
                if (!bolValidCountdownID || intReqCountdownID < 0)
                    Program._irc.sendPublicChatMessage("Please use a positive whole number to find your countdown ID");
                else
                {
                    // check if countdown ID exists
                    int intCountdownID = -1;
                    using (SqlConnection conn = new SqlConnection(Program._connStr))
                    {
                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand("SELECT id, broadcaster FROM tblCountdown "
                            + "WHERE broadcaster = @broadcaster", conn))
                        {
                            cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = Program._intBroadcasterID;
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
                        Program._irc.sendPublicChatMessage($"Cannot find the countdown ID: {intReqCountdownID}");
                    else
                    {
                        int intInputType = -1; // check if input is in the correct format
                        DateTime dtCountdown = new DateTime();
                        string strCountdownInput = message.Substring(Program.GetNthIndex(message, ' ', 2) + 1);

                        /* Check if user wants to edit the date and time or message */
                        if (message.StartsWith("!editcountdownDTE"))
                        {
                            // get new due date of countdown
                            bool bolValidCountdownDT = DateTime.TryParse(strCountdownInput, out dtCountdown);

                            if (!bolValidCountdownDT)
                                Program._irc.sendPublicChatMessage("Please enter a valid date and time @" + strUserName);
                            else
                                intInputType = 1;
                        }
                        else if (message.StartsWith("!editcountdownMSG"))
                        {
                            // get new message of countdown
                            if (string.IsNullOrWhiteSpace(strCountdownInput))
                                Program._irc.sendPublicChatMessage("Please enter a valid message @" + strUserName);
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

                            using (SqlConnection conn = new SqlConnection(Program._connStr))
                            using (SqlCommand cmd = new SqlCommand(strQuery, conn))
                            {
                                // append proper parameter
                                if (intInputType == 1)
                                    cmd.Parameters.Add("@dueDate", SqlDbType.DateTime).Value = dtCountdown;
                                else if (intInputType == 2)
                                    cmd.Parameters.Add("@message", SqlDbType.VarChar, 50).Value = strCountdownInput;

                                cmd.Parameters.Add("@id", SqlDbType.Int).Value = intCountdownID;
                                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = Program._intBroadcasterID;

                                conn.Open();
                                cmd.ExecuteNonQuery();
                            }

                            Console.WriteLine($"Changes to countdown ID: {intReqCountdownID} have been made @{strUserName}");
                            Program._irc.sendPublicChatMessage($"Changes to countdown ID: {intReqCountdownID} have been made @{strUserName}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "CmdBrdCstr", "CmdEditCountdown(string, string)", false, "!editcountdown");
            }
        }

        /// <summary>
        /// List all of the countdowns the broadcaster has set
        /// </summary>
        /// <param name="strUserName"></param>
        public void CmdListCountdown(string strUserName)
        {
            try
            {
                string strCountdownList = "";

                using (SqlConnection conn = new SqlConnection(Program._connStr))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT Id, dueDate, message, broadcaster FROM tblCountdown "
                        + "WHERE broadcaster = @broadcaster ORDER BY Id", conn))
                    {
                        cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = Program._intBroadcasterID;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    strCountdownList += "ID: " + reader["Id"].ToString()
                                        + " Message: \"" + reader["message"].ToString()
                                        + "\" Time: \"" + reader["dueDate"].ToString()
                                        + "\" || ";
                                }
                                StringBuilder strBdrPartyList = new StringBuilder(strCountdownList);
                                strBdrPartyList.Remove(strCountdownList.Length - 4, 4); // remove extra " || "
                                strCountdownList = strBdrPartyList.ToString(); // replace old countdown list string with new
                                Program._irc.sendPublicChatMessage(strCountdownList);
                            }
                            else
                            {
                                Console.WriteLine("No countdown messages are set at the moment");
                                Program._irc.sendPublicChatMessage("No countdown messages are set at the moment @" + strUserName);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "CmdBrdCstr", "CmdListCountdown()", false, "!listcountdown");
            }
        }
    }
}
