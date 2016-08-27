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
        /// <param name="bolAutoPublishTweet">Auto publish tweets within the bot</param>
        /// <param name="bolAutoDisplaySong">Display songs from Spotify in chat</param>
        /// <param name="strCurrencyType">Used for the currency system</param>
        public void CmdBotSettings(bool bolAutoPublishTweet, bool bolAutoDisplaySong, string strCurrencyType)
        {
            try
            {
                Program._irc.sendPublicChatMessage("Auto tweets set to \"" + bolAutoPublishTweet + "\" "
                    + "|| Auto display songs set to \"" + bolAutoDisplaySong + "\" "
                    + "|| Currency set to \"" + strCurrencyType + "\"");
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
        /// <param name="bolAutoPublishTweet">Auto publish tweets within the bot</param>
        /// <param name="strBroadcasterName">Name of the channel's broadcaster</param>
        /// <param name="bolHasTwitterInfo">Check for Twitter credentials</param>
        public void CmdEnableTweet(ref bool bolAutoPublishTweet, string strBroadcasterName, bool bolHasTwitterInfo)
        {
            try
            {
                if (!bolHasTwitterInfo)
                    Program._irc.sendPublicChatMessage("You are missing twitter info @" + strBroadcasterName);
                else
                {
                    bolAutoPublishTweet = true;
                    Properties.Settings.Default.enableTweet = bolAutoPublishTweet;
                    Properties.Settings.Default.Save();

                    Console.WriteLine("Auto publish tweets is set to [" + bolAutoPublishTweet + "]");
                    Program._irc.sendPublicChatMessage(strBroadcasterName + ": Automatic tweets is set to \"" + bolAutoPublishTweet + "\"");
                }
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "CmdBrdCstr", "CmdEnableTweet(bool, string, bool)", false, "!settweet on");
            }
        }

        /// <summary>
        /// Disables tweets to be sent out from this bot (both auto publish tweets and manual tweets)
        /// </summary>
        /// <param name="bolAutoPublishTweet">Auto publish tweets within the bot</param>
        /// <param name="strBroadcasterName">Name of the channel's broadcaster</param>
        /// <param name="bolHasTwitterInfo">Check for Twitter credentials</param>
        public void CmdDisableTweet(ref bool bolAutoPublishTweet, string strBroadcasterName, bool bolHasTwitterInfo)
        {
            try
            {
                if (!bolHasTwitterInfo)
                    Program._irc.sendPublicChatMessage("You are missing twitter info @" + strBroadcasterName);
                else
                {
                    bolAutoPublishTweet = false;
                    Properties.Settings.Default.enableTweet = bolAutoPublishTweet;
                    Properties.Settings.Default.Save();

                    Console.WriteLine("Auto publish tweets is set to [" + bolAutoPublishTweet + "]");
                    Program._irc.sendPublicChatMessage(strBroadcasterName + ": Automatic tweets is set to \"" + bolAutoPublishTweet + "\"");
                }
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "CmdBrdCstr", "CmdEnableTweet(ref bool, string, bool)", false, "!settweet off");
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
        /// <param name="_strBroadcasterName">Name of the broadcaster</param>
        public void CmdUpdateTitle(string message, string twitchAccessToken, string _strBroadcasterName)
        {
            try
            {
                // Get title from command parameter
                string title = message.Substring(message.IndexOf(" ") + 1);

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
    }
}
