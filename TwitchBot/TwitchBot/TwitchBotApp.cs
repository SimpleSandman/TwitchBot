using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot
{
    public class TwitchBotApp
    {
        /// <summary>
        /// Monitor chat box for commands
        /// </summary>
        /// <param name="isSongRequestAvail"></param>
        /// <param name="twitchAccessToken"></param>
        /// <param name="hasTwitterInfo"></param>
        public static void GetChatBox(bool isSongRequestAvail, string twitchAccessToken, bool hasTwitterInfo)
        {
            try
            {
                /* Master loop */
                while (true)
                {
                    // Read any message inside the chat room
                    string message = Program._irc.readMessage();
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
                            if (strUserName.Equals(Program._strBroadcasterName))
                            {
                                /* Display bot settings */
                                if (message.Equals("!botsettings"))
                                    Program._cmdBrdCstr.CmdBotSettings();

                                /* Stop running the bot */
                                else if (message.Equals("!exitbot"))
                                    Program._cmdBrdCstr.CmdExitBot();

                                /* Manually connect to Spotify */
                                else if (message.Equals("!spotifyconnect"))
                                    Program._spotify.Connect();

                                /* Press local Spotify play button [>] */
                                else if (message.Equals("!spotifyplay"))
                                    Program._spotify.playBtn_Click();

                                /* Press local Spotify pause button [||] */
                                else if (message.Equals("!spotifypause"))
                                    Program._spotify.pauseBtn_Click();

                                /* Press local Spotify previous button [|<] */
                                else if (message.Equals("!spotifyprev"))
                                    Program._spotify.prevBtn_Click();

                                /* Press local Spotify next (skip) button [>|] */
                                else if (message.Equals("!spotifynext"))
                                    Program._spotify.skipBtn_Click();

                                /* Enables tweets to be sent out from this bot (both auto publish tweets and manual tweets) */
                                else if (message.Equals("!sendtweet on"))
                                    Program._cmdBrdCstr.CmdEnableTweet(hasTwitterInfo);

                                /* Disables tweets from being sent out from this bot */
                                else if (message.Equals("!sendtweet off"))
                                    Program._cmdBrdCstr.CmdDisableTweet(hasTwitterInfo);

                                /* Enables viewers to request songs (default off) */
                                else if (message.Equals("!srmode on"))
                                    Program._cmdBrdCstr.CmdEnableSRMode(ref isSongRequestAvail);

                                /* Disables viewers to request songs (default off) */
                                else if (message.Equals("!srmode off"))
                                    Program._cmdBrdCstr.CmdDisableSRMode(ref isSongRequestAvail);

                                /* Updates the title of the Twitch channel */
                                // Usage: !updatetitle [title]
                                else if (message.StartsWith("!updatetitle "))
                                    Program._cmdBrdCstr.CmdUpdateTitle(message, twitchAccessToken);

                                /* Updates the game of the Twitch channel */
                                // Usage: !updategame "[game]" (with quotation marks)
                                else if (message.StartsWith("!updategame "))
                                    Program._cmdBrdCstr.CmdUpdateGame(message, twitchAccessToken, hasTwitterInfo);

                                /* Sends a manual tweet (if credentials have been provided) */
                                // Usage: !tweet "[message]" (use quotation marks)
                                else if (message.StartsWith("!tweet "))
                                    Program._cmdBrdCstr.CmdTweet(hasTwitterInfo, message);

                                /* Enables songs from local Spotify to be displayed inside the chat */
                                else if (message.Equals("!displaysongs on"))
                                    Program._cmdBrdCstr.CmdEnableDisplaySongs();

                                /* Disables songs from local Spotify to be displayed inside the chat */
                                else if (message.Equals("!displaysongs off"))
                                    Program._cmdBrdCstr.CmdDisableDisplaySongs();

                                /* Add viewer to moderator list so they can have access to bot moderator commands */
                                // Usage: !addmod @[username]
                                else if (message.StartsWith("!addmod ") && message.Contains("@"))
                                    Program._cmdBrdCstr.CmdAddBotMod(message);

                                /* Remove moderator from list so they can't access the bot moderator commands */
                                // Usage: !delmod @[username]
                                else if (message.StartsWith("!delmod ") && message.Contains("@"))
                                    Program._cmdBrdCstr.CmdDelBotMod(message);

                                /* List bot moderators */
                                else if (message.Equals("!listmod"))
                                    Program._cmdBrdCstr.CmdListMod();

                                /* Add countdown */
                                // Usage: !addcountdown [MM-DD-YY] [hh:mm:ss] [AM/PM] [message]
                                else if (message.StartsWith("!addcountdown "))
                                    Program._cmdBrdCstr.CmdAddCountdown(message, strUserName);

                                /* Edit countdown details (for either date and time or message) */
                                // Usage (message): !editcountdownMSG [countdown id] [message]
                                // Usage (date and time): !editcountdownDTE [countdown id] [MM-DD-YY] [hh:mm:ss] [AM/PM]
                                else if (message.StartsWith("!editcountdown"))
                                    Program._cmdBrdCstr.CmdEditCountdown(message, strUserName);

                                /* List all of the countdowns the broadcaster has set */
                                else if (message.Equals("!listcountdown"))
                                    Program._cmdBrdCstr.CmdListCountdown(strUserName);

                                /* insert more broadcaster commands here */
                            }

                            /*
                             * Moderator commands (also checks if user has been timed out from using a command)
                             */
                            if (strUserName.Equals(Program._strBroadcasterName) || Program._mod.getLstMod().Contains(strUserName.ToLower()))
                            {
                                /* Displays Discord link into chat (if available) */
                                if (message.Equals("!discord") && !Program.isUserTimedout(strUserName))
                                    Program._cmdMod.CmdDiscord();

                                /* Takes money away from a user */
                                // Usage: !charge [-amount] @[username]
                                else if (message.StartsWith("!charge ") && message.Contains("@") && !Program.isUserTimedout(strUserName))
                                    Program._cmdMod.CmdCharge(message, strUserName);

                                /* Gives money to user */
                                // Usage: !deposit [amount] @[username]
                                else if (message.StartsWith("!deposit ") && message.Contains("@") && !Program.isUserTimedout(strUserName))
                                    Program._cmdMod.CmdDeposit(message, strUserName);

                                /* Removes the first song in the queue of song requests */
                                else if (message.Equals("!popsr") && !Program.isUserTimedout(strUserName))
                                    Program._cmdMod.CmdPopSongRequest();

                                /* Removes first party memeber in queue of party up requests */
                                else if (message.Equals("!poppartyuprequest") && !Program.isUserTimedout(strUserName))
                                    Program._cmdMod.CmdPopPartyUpRequest();

                                /* Bot-specific timeout on a user for a set amount of time */
                                // Usage: !addtimeout [seconds] @[username]
                                else if (message.StartsWith("!addtimeout ") && message.Contains("@") && !Program.isUserTimedout(strUserName))
                                    Program._cmdMod.CmdAddTimeout(message, strUserName);

                                /* Remove bot-specific timeout on a user for a set amount of time */
                                // Usage: !deltimeout @[username]
                                else if (message.StartsWith("!deltimeout @") && !Program.isUserTimedout(strUserName))
                                    Program._cmdMod.CmdDelTimeout(message, strUserName);

                                /* Set delay for messages based on the latency of the stream */
                                // Usage: !setlatency [seconds]
                                else if (message.StartsWith("!setlatency ") && !Program.isUserTimedout(strUserName))
                                    Program._cmdMod.CmdSetLatency(message, strUserName);

                                /* Add a mod/broadcaster quote */
                                // Usage: !addquote [quote]
                                else if (message.StartsWith("!addquote ") && !Program.isUserTimedout(strUserName))
                                    Program._cmdMod.CmdAddQuote(message, strUserName);

                                /* insert moderator commands here */
                            }

                            /* 
                             * General commands 
                             */
                            /* Display some viewer commands a link to command documentation */
                            if (message.Equals("!cmds") && !Program.isUserTimedout(strUserName))
                                Program._cmdGen.CmdCmds();

                            /* Display a static greeting */
                            else if (message.Equals("!hello") && !Program.isUserTimedout(strUserName))
                                Program._cmdGen.CmdHello(strUserName);

                            /* Display the current time in UTC (Coordinated Universal Time) */
                            else if (message.Equals("!utctime") && !Program.isUserTimedout(strUserName))
                                Program._cmdGen.CmdUtcTime();

                            /* Display the current time in the time zone the host is located */
                            else if (message.Equals("!hosttime") && !Program.isUserTimedout(strUserName))
                                Program._cmdGen.CmdHostTime(Program._strBroadcasterName);

                            /* Shows how long the broadcaster has been streaming */
                            else if (message.Equals("!duration") && !Program.isUserTimedout(strUserName))
                                Program._cmdGen.CmdDuration();

                            /* Display list of requested songs */
                            else if (message.Equals("!srlist") && !Program.isUserTimedout(strUserName))
                                Program._cmdGen.CmdListSR();

                            /* Request a song for the host to play */
                            // Usage: !sr [artist] - [song title]
                            else if (message.StartsWith("!sr ") && !Program.isUserTimedout(strUserName))
                                Program._cmdGen.CmdSR(isSongRequestAvail, message, strUserName);

                            /* Displays the current song being played from Spotify */
                            else if (message.Equals("!spotifycurr") && !Program.isUserTimedout(strUserName))
                                Program._cmdGen.CmdSpotifyCurr();

                            /* Slaps a user and rates its effectiveness */
                            // Usage: !slap @[username]
                            else if (message.StartsWith("!slap @") && !Program.isUserTimedout(strUserName))
                                Program._cmdGen.CmdSlap(message, strUserName);

                            /* Stabs a user and rates its effectiveness */
                            // Usage: !stab @[username]
                            else if (message.StartsWith("!stab @") && !Program.isUserTimedout(strUserName))
                                Program._cmdGen.CmdStab(message, strUserName);

                            /* Shoots a viewer's random body part */
                            // Usage !shoot @[username]
                            else if (message.StartsWith("!shoot @") && !Program.isUserTimedout(strUserName))
                                Program._cmdGen.CmdShoot(message, strUserName);

                            /* Throws an item at a viewer and rates its effectiveness against the victim */
                            // Usage: !throw [item] @username
                            else if (message.StartsWith("!throw ") && message.Contains("@") && !Program.isUserTimedout(strUserName))
                                Program._cmdGen.CmdThrow(message, strUserName);

                            /* Request party member if game and character exists in party up system */
                            // Usage: !partyup [party member name]
                            else if (message.StartsWith("!partyup ") && !Program.isUserTimedout(strUserName))
                                Program._cmdGen.CmdPartyUp(message, strUserName);

                            /* Check what other user's have requested */
                            else if (message.Equals("!partyuprequestlist") && !Program.isUserTimedout(strUserName))
                                Program._cmdGen.CmdPartyUpRequestList();

                            /* Check what party members are available (if game is part of the party up system) */
                            else if (message.Equals("!partyuplist") && !Program.isUserTimedout(strUserName))
                                Program._cmdGen.CmdPartyUpList();

                            /* Check user's account balance */
                            else if (message.Equals("!myfunds") && !Program.isUserTimedout(strUserName))
                                Program._cmdGen.CmdCheckFunds(strUserName);

                            /* Gamble money away */
                            // Usage: !gamble [money]
                            else if (message.StartsWith("!gamble ") && !Program.isUserTimedout(strUserName))
                                Program._cmdGen.CmdGamble(message, strUserName);

                            /* Display random mod/broadcaster quote */
                            else if (message.Equals("!quote") && !Program.isUserTimedout(strUserName))
                                Program._cmdGen.CmdQuote();

                            /* add more general commands here */
                        }
                    }
                } // end master while loop
            }
            catch (Exception ex)
            {
                Program.LogError(ex, "TwitchBotApp", "GetChatBox(bool, string, string)", true);
            }
        }
    }
}
