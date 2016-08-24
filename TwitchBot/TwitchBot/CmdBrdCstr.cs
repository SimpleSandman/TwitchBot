using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot
{
    public class CmdBrdCstr
    {
        /// <summary>
        /// Display bot settings
        /// </summary>
        /// <param name="isAutoPublishTweet">Auto publish tweets within the bot</param>
        /// <param name="isAutoDisplaySong">Display songs from Spotify in chat</param>
        /// <param name="strCurrencyType">Used for the currency system</param>
        public void CmdBotSettings(bool isAutoPublishTweet, bool isAutoDisplaySong, string strCurrencyType)
        {
            try
            {
                Program._irc.sendPublicChatMessage("Auto tweets set to \"" + isAutoPublishTweet + "\" "
                    + "|| Auto display songs set to \"" + isAutoDisplaySong + "\" "
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
                Program.LogError(ex, "Program", "GetChatBox(SpotifyControl, bool, string, bool)", false, "!exitbot");
            }
        }
    }
}
