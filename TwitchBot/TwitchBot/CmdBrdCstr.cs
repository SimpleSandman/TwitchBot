using RestSharp;
using System;
using System.Collections.Generic;
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
    }
}
